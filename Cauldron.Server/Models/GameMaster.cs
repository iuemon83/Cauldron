using Cauldron.Server.Models.Effect;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Cauldron_Test")]
namespace Cauldron.Server.Models
{
    public class GameMaster
    {
        public static bool CanAttack(Card attackCard)
        {
            return attackCard != null
                // クリーチャーでなければ攻撃できない
                && attackCard.Type == CardType.Creature
                // 召喚酔いでない
                && attackCard.TurnCountToCanAttack <= attackCard.TurnCountInField
                // 攻撃不能状態でない
                && !attackCard.Abilities.Contains(CreatureAbility.CantAttack)
                ;
        }

        public static bool CanAttack(Card attackCard, Player guardPlayer)
        {
            var existsCover = guardPlayer.Field.AllCards
                .Any(c => c.Abilities.Contains(CreatureAbility.Cover)
                    && !c.Abilities.Contains(CreatureAbility.Stealth));

            return
                // 攻撃可能なカード
                CanAttack(attackCard)
                // 持ち主には攻撃できない
                && attackCard.OwnerId != guardPlayer.Id
                // カバーされていない
                && !existsCover
                ;
        }

        public static bool CanAttack(Card attackCard, Card guardCard, GameEnvironment environment)
        {
            var guardPlayer = environment.You.Id == guardCard.OwnerId
                ? environment.You
                : environment.Opponent;
            var existsCover = guardPlayer.Field.AllCards
                .Any(c => c.Abilities.Contains(CreatureAbility.Cover)
                    && !c.Abilities.Contains(CreatureAbility.Stealth));

            var coverCheck = guardCard.Abilities.Contains(CreatureAbility.Cover)
                || !existsCover;

            return
                // 攻撃可能なカード
                CanAttack(attackCard)
                // 自分自信のカードには攻撃できない
                && attackCard.OwnerId != guardCard.OwnerId
                // クリーチャー以外には攻撃できない
                && guardCard.Type == CardType.Creature
                // ステルス状態は攻撃対象にならない
                && !guardCard.Abilities.Contains(CreatureAbility.Stealth)
                // カバー関連のチェック
                && coverCheck
                ;
        }

        public RuleBook RuleBook { get; }

        private readonly ILogger logger;

        private readonly CardFactory cardFactory;

        public readonly Dictionary<Guid, Player> PlayersById = new Dictionary<Guid, Player>();

        public Player CurrentPlayer { get; set; }

        public Player NextPlayer { get; set; }

        public bool GameOver => this.PlayersById.Values.Any(player => player.Hp <= 0);

        public Dictionary<Guid, int> PlayerTurnCountById { get; set; } = new Dictionary<Guid, int>();

        public int AllTurnCount => this.PlayerTurnCountById.Sum(x => x.Value);

        // イベント
        private readonly Subject<EffectEventArgs> PlayEvent = new Subject<EffectEventArgs>();
        private readonly Subject<EffectEventArgs> DestroyEvent = new Subject<EffectEventArgs>();
        private readonly Subject<EffectEventArgs> StartTurnEvent = new Subject<EffectEventArgs>();
        private readonly Subject<EffectEventArgs> EndTurnEvent = new Subject<EffectEventArgs>();

        /// <summary>
        /// カードの効果の解除リスト。キーはカードのID
        /// </summary>
        private readonly Dictionary<Guid, List<IDisposable>> EffectDisposerListByCardId = new Dictionary<Guid, List<IDisposable>>();

        public IEnumerable<CardDef> CardPool => this.cardFactory.CardPool;

        private readonly Func<Guid, ChoiceResult, int, ChoiceResult> AskCardAction;

        public GameMaster(RuleBook ruleBook, CardFactory cardFactory, ILogger logger,
            Func<Guid, ChoiceResult, int, ChoiceResult> askCardAction)
        {
            this.RuleBook = ruleBook;
            this.cardFactory = cardFactory;
            this.logger = logger;
            this.AskCardAction = askCardAction;
        }

        public Guid CreateNewPlayer(string name, IEnumerable<Guid> deckCardDefIdList)
        {
            var newId = Guid.NewGuid();
            var deckCards = deckCardDefIdList.Select(id => this.cardFactory.CreateNew(id)).ToArray();
            var player = new Player(newId, name, this.RuleBook.InitialPlayerHp, this.RuleBook, deckCards);
            this.PlayersById.Add(newId, player);

            this.PlayerTurnCountById.Add(newId, 0);

            return newId;
        }

        public void Start(Guid firstPlayerId)
        {
            this.CurrentPlayer = this.PlayersById[firstPlayerId];
            this.NextPlayer = this.GetOpponent(this.CurrentPlayer.Id);

            foreach (var player in this.PlayersById.Values)
            {
                foreach (var _ in Enumerable.Range(0, this.RuleBook.InitialNumHands))
                {
                    player.Draw();
                }
            }
        }

        public Player GetWinner()
        {
            // 引き分けならターンのプレイヤーの負け
            var alives = this.PlayersById.Values.Where(p => p.Hp > 0).ToArray();
            return alives.Length == 0
                ? this.PlayersById.Values.First(p => p.Id != this.CurrentPlayer.Id)
                : alives.First();
        }

        public Player GetOpponent(Guid playerId)
        {
            return this.PlayersById.Values
                .First(player => player.Id != playerId);
        }

        /// <summary>
        /// 指定したカードを破壊します。
        /// </summary>
        /// <param name="cardToDestroy"></param>
        public void DestroyCard(Card cardToDestroy)
        {
            var player = this.PlayersById[cardToDestroy.OwnerId];
            this.logger.LogInformation($"破壊：{player.Name}-{cardToDestroy}");

            player.Field.Destroy(cardToDestroy);

            // カードの破壊イベント
            this.DestroyEvent.OnNext(new EffectEventArgs() { EffectType = GameEvent.OnDestroy, GameMaster = this, Source = cardToDestroy });

            // 破壊されたカードの効果を削除
            if (this.EffectDisposerListByCardId.TryGetValue(cardToDestroy.Id, out var dList))
            {
                foreach (var d in dList)
                {
                    d.Dispose();
                }
            }
        }

        public void Buff(Card creatureCard, int powerBuff, int toughnessBuff)
        {
            this.logger.LogInformation($"修整：{creatureCard}-[{powerBuff},{toughnessBuff}]");

            if (creatureCard.Type != CardType.Creature)
            {
                return;
            }

            creatureCard.PowerBuff += powerBuff;
            creatureCard.ToughnessBuff += toughnessBuff;

            if (creatureCard.Toughness <= 0)
            {
                this.DestroyCard(creatureCard);
            }
        }

        public bool IsPutFieldCard(Card card)
        {
            return card.Type == CardType.Artifact
                || card.Type == CardType.Creature
                || card.Type == CardType.Token
                ;
        }

        public bool IsPlayable(Player player, Card card)
        {
            // フィールドに出すカードはフィールドに空きがないとプレイできない
            if (IsPutFieldCard(card) && player.Field.Full)
            {
                return false;
            }

            // コストが払えないとプレイできない
            if (player.UsableMp < card.Cost)
            {
                return false;
            }

            return true;
        }

        public bool IsPlayableDirect(Player player, Card card)
        {
            // フィールドに出すカードはフィールドに空きがないとプレイできない
            if (IsPutFieldCard(card) && player.Field.Full)
            {
                return false;
            }

            return true;
        }

        public Card GenerateNewCard(Guid cardDefId, Guid ownerId)
        {
            var card = this.cardFactory.CreateNew(cardDefId);
            card.OwnerId = ownerId;

            return card;
        }

        public Card GenerateNewCard(string cardFullName, Guid ownerId)
        {
            var cardDef = this.cardFactory.GetByFullName(cardFullName);
            var card = this.cardFactory.CreateNew(cardDef.Id);
            card.OwnerId = ownerId;

            return card;
        }

        public void AddHand(Player player, Card addCard)
        {
            player.Hands.Add(addCard);

            this.logger.LogInformation($"手札に追加: {addCard.Name}");
            var handsLog = string.Join(",", player.Hands.AllCards.Select(c => c.Name));
            this.logger.LogInformation($"手札: {handsLog}");
        }

        public void RemoveHand(Player player, Card removeCard)
        {
            player.Hands.Remove(removeCard);

            this.logger.LogInformation($"手札を捨てる: {removeCard.Name}");
            var handsLog = string.Join(",", player.Hands.AllCards.Select(c => c.Name));
            this.logger.LogInformation($"手札: {handsLog}");
        }

        public GameEnvironment CreateEnvironment(Guid playerId)
        {
            return new GameEnvironment()
            {
                GameOver = this.GameOver,
                WinnerPlayerId = this.GetWinner()?.Id ?? Guid.Empty,
                You = playerId == this.CurrentPlayer.Id
                    ? new PrivatePlayerInfo(this.CurrentPlayer)
                    : new PrivatePlayerInfo(this.GetOpponent(this.CurrentPlayer.Id)),

                Opponent = playerId == this.CurrentPlayer.Id
                    ? new PublicPlayerInfo(this.GetOpponent(this.CurrentPlayer.Id))
                    : new PublicPlayerInfo(this.CurrentPlayer),
                RuleBook = this.RuleBook
            };
        }

        public GameEnvironment GetEnvironment(Guid playerId)
        {
            return this.CreateEnvironment(playerId);
        }

        public (bool IsSucceeded, string errorMessage) StartTurn(Guid playerId)
        {
            if (this.GameOver)
            {
                return (false, "すでにゲームが終了しています。");
            }

            if (playerId != this.CurrentPlayer.Id)
            {
                return (false, "このプレイヤーのターンではありません。");
            }

            this.PlayerTurnCountById[this.CurrentPlayer.Id]++;

            this.CurrentPlayer.AddMaxMp(this.RuleBook.MpByStep);
            this.CurrentPlayer.FullMp();
            foreach (var card in this.CurrentPlayer.Field.AllCards)
            {
                card.TurnCountInField++;
            }

            this.logger.LogInformation($"ターン開始：{this.CurrentPlayer.Name}-[HP:{this.CurrentPlayer.Hp}/{this.CurrentPlayer.MaxHp}][MP:{this.CurrentPlayer.MaxMp}][ターン:{this.PlayerTurnCountById[this.CurrentPlayer.Id]}({this.AllTurnCount})]----------------------------");

            this.StartTurnEvent.OnNext(new EffectEventArgs() { EffectType = GameEvent.OnStartTurn, GameMaster = this });

            this.CurrentPlayer.Draw();

            return (true, "");
        }

        public (bool IsSucceeded, string errorMessage) EndTurn(Guid playerId)
        {
            if (this.GameOver)
            {
                return (false, "すでにゲームが終了しています。");
            }

            if (playerId != this.CurrentPlayer.Id)
            {
                return (false, "このプレイヤーのターンではありません。");
            }

            var endTurnPlayer = this.CurrentPlayer;

            this.logger.LogInformation($"ターンエンド：{endTurnPlayer.Name}");

            this.EndTurnEvent.OnNext(new EffectEventArgs() { EffectType = GameEvent.OnEndTurn, GameMaster = this });

            this.CurrentPlayer = this.NextPlayer;
            this.NextPlayer = this.GetOpponent(this.CurrentPlayer.Id);

            return (true, "");
        }

        /// <summary>
        /// 手札のカードをプレイします
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="handCardId"></param>
        /// <returns></returns>
        public (bool IsSucceeded, string errorMessage) PlayFromHand(Guid playerId, Guid handCardId)
        {
            if (this.GameOver)
            {
                return (false, "すでにゲームが終了しています。");
            }

            if (playerId != this.CurrentPlayer.Id)
            {
                return (false, "このプレイヤーのターンではありません。");
            }

            var player = this.PlayersById[playerId];
            var playingCard = player.Hands.GetById(handCardId);

            this.logger.LogInformation($"プレイ：{player.Name}-{playingCard}");

            // プレイ不能
            if (!this.IsPlayable(player, playingCard))
            {
                return (false, "プレイ不能");
            }

            this.CurrentPlayer.UseMp(playingCard.Cost);

            player.Hands.Remove(playingCard);

            switch (playingCard.Type)
            {
                case CardType.Creature:
                case CardType.Artifact:
                    player.Field.Add(playingCard);
                    break;

                default:
                    break;
            }

            // イベント登録
            var list = playingCard.Effects
                .SelectMany(effect =>
                {
                    var l = new List<IDisposable>();

                    if (effect.MatchTiming(GameEvent.OnStartTurn))
                    {
                        l.Add(this.StartTurnEvent.Subscribe(effect.Execute(playingCard)));
                    }

                    if (effect.MatchTiming(GameEvent.OnEndTurn))
                    {
                        l.Add(this.EndTurnEvent.Subscribe(effect.Execute(playingCard)));
                    }

                    if (effect.MatchTiming(GameEvent.OnPlay))
                    {
                        l.Add(this.PlayEvent.Subscribe(effect.Execute(playingCard)));
                    }

                    if (effect.MatchTiming(GameEvent.OnDestroy))
                    {
                        l.Add(this.DestroyEvent.Subscribe(effect.Execute(playingCard)));
                    }

                    return l;
                })
                .ToList();

            // 解除リストにも登録
            if (list.Any())
            {
                this.EffectDisposerListByCardId.Add(playingCard.Id, list);
            }

            // イベント発行
            this.PlayEvent.OnNext(new EffectEventArgs() { EffectType = GameEvent.OnPlay, GameMaster = this, Source = playingCard });

            return (true, "");
        }

        /// <summary>
        /// プレイヤーにカードを選択させます。
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="choiceCandidates"></param>
        /// <param name="choiceNum"></param>
        /// <returns></returns>
        public ChoiceResult AskCard(Guid playerId, ChoiceResult choiceCandidates, int choiceNum)
        {
            var choiceResult = this.AskCardAction(playerId, choiceCandidates, choiceNum);
            return choiceResult;

            //TODO ほんとは不正なIDが指定されていないかの確認が必要
            //switch (targetType)
            //{
            //    case TargetCardType.YourCreature:
            //        {
            //            var you = this.PlayersById[playerId];
            //            var targetCard = you.Field.GetById(targetCardId);
            //            if (targetCard == null)
            //            {
            //                throw new Exception("指定されたカードが正しくない");
            //            }

            //            return targetCard;
            //        }

            //    case TargetCardType.OpponentCreature:
            //        {
            //            var opponent = this.GetOpponent(playerId);
            //            var targetCard = opponent.Field.GetById(targetCardId);
            //            if (targetCard == null)
            //            {
            //                throw new Exception("指定されたカードが正しくない");
            //            }

            //            return targetCard;
            //        }

            //    default:
            //        throw new InvalidOperationException("サポートされていない");
            //}
        }

        /// <summary>
        /// 新規に生成されるカードをプレイ（効果で召喚とか）
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public (bool IsSucceeded, string errorMessage) PlayDirect(Guid playerId, Guid cardId)
        {
            if (this.GameOver)
            {
                return (false, "すでにゲームが終了しています。");
            }

            if (playerId != this.CurrentPlayer.Id)
            {
                return (false, "このプレイヤーのターンではありません。");
            }

            var player = this.PlayersById[playerId];
            var playingCard = this.cardFactory.GetById(cardId);

            this.logger.LogInformation($"特殊プレイ：{player.Name}-{playingCard}");

            // プレイ不能
            if (!this.IsPlayableDirect(player, playingCard))
            {
                return (false, "プレイ不能");
            }

            switch (playingCard.Type)
            {
                case CardType.Creature:
                case CardType.Artifact:
                    player.Field.Add(playingCard);
                    break;

                default:
                    break;
            }

            return (true, "");
        }

        /// <summary>
        /// プレイヤーに攻撃します。
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="cardId"></param>
        /// <param name="damagePlayerId"></param>
        /// <returns></returns>
        public (bool IsSucceeded, string errorMessage) AttackToPlayer(Guid playerId, Guid cardId, Guid damagePlayerId)
        {
            if (this.GameOver)
            {
                return (false, "すでにゲームが終了しています。");
            }

            if (playerId != this.CurrentPlayer.Id)
            {
                return (false, "このプレイヤーのターンではありません。");
            }

            var player = this.PlayersById[playerId];
            var card = player.Field.GetById(cardId);
            var damagePlayer = this.PlayersById[damagePlayerId];

            this.logger.LogInformation($"アタック（プレイヤー）：{player.Name}-{card} > {damagePlayer.Name}");

            if (!CanAttack(card, damagePlayer))
            {
                return (false, "攻撃不能");
            }

            this.HitPlayer(damagePlayer.Id, card.Power);

            return (true, "");
        }

        public (bool IsSucceeded, string errorMessage) HitPlayer(Guid playerId, Guid damagePlayerId, int damage)
        {
            if (this.GameOver)
            {
                return (false, "すでにゲームが終了しています。");
            }

            if (playerId != this.CurrentPlayer.Id)
            {
                return (false, "このプレイヤーのターンではありません。");
            }

            var damagePlayer = this.PlayersById[damagePlayerId];

            this.logger.LogInformation($"ダメージ：{damage} > {damagePlayer.Name}");

            damagePlayer.Damage(damage);

            return (true, "");
        }

        public void HitPlayer(Guid damagePlayerId, int damage)
        {
            var damagePlayer = this.PlayersById[damagePlayerId];

            this.logger.LogInformation($"ダメージ：{damage} > {damagePlayer.Name}");

            damagePlayer.Damage(damage);
        }

        public (bool IsSucceeded, string errorMessage) AttackToCreature(Guid playerId, Guid attackCardId, Guid guardCardId)
        {
            if (this.GameOver)
            {
                return (false, "すでにゲームが終了しています。");
            }

            if (playerId != this.CurrentPlayer.Id)
            {
                return (false, "このプレイヤーのターンではありません。");

            }

            var attackCard = this.cardFactory.GetById(attackCardId);
            var guardCard = this.cardFactory.GetById(guardCardId);

            var attackPlayer = this.PlayersById[attackCard.OwnerId];
            var guardPlayer = this.PlayersById[guardCard.OwnerId];

            this.logger.LogInformation($"アタック（クリーチャー）：{attackPlayer.Name}-{attackCard} > {guardPlayer.Name}-{guardCard}");

            if (!CanAttack(attackCard, guardCard, this.CreateEnvironment(playerId)))
            {
                return (false, "攻撃不能");
            }

            // 攻撃するとステルスを失う
            if (attackCard.Abilities.Contains(CreatureAbility.Stealth))
            {
                attackCard.Abilities.Remove(CreatureAbility.Stealth);
            }

            // お互いにダメージを受ける
            var result = this.HitCreature(playerId, guardCard.Id, attackCard.Power);
            if (!result.IsSucceeded) return result;

            result = this.HitCreature(playerId, attackCard.Id, guardCard.Power);
            return result;
        }

        public (bool IsSucceeded, string errorMessage) HitCreature(Guid playerId, Guid creatureCardId, int damage)
        {
            if (this.GameOver)
            {
                return (false, "すでにゲームが終了しています。");
            }

            if (playerId != this.CurrentPlayer.Id)
            {
                return (false, "このプレイヤーのターンではありません。");
            }

            var creatureCard = this.cardFactory.GetById(creatureCardId);

            this.logger.LogInformation($"ダメージ：{damage} > {creatureCard}");

            creatureCard.Damage(damage);

            if (creatureCard.Toughness <= 0)
            {
                this.DestroyCard(creatureCard);
            }

            return (true, "");
        }

        public void HitCreature(Guid creatureCardId, int damage)
        {
            var creatureCard = this.cardFactory.GetById(creatureCardId);

            if (creatureCard.Type != CardType.Creature)
            {
                throw new Exception($"指定されたカードはクリーチャーではありません。: {creatureCard.Name}");
            }

            this.logger.LogInformation($"ダメージ：{damage} > {creatureCard}");

            creatureCard.Damage(damage);

            if (creatureCard.Toughness <= 0)
            {
                this.DestroyCard(creatureCard);
            }
        }

        public IReadOnlyList<Card> ChoiceCandidateCards(Card ownerCard, Choice choice, Card eventSource)
        {
            return choice.CardCondition.ZoneCondition switch
            {
                ZoneType.All => this.cardFactory.GetAllCards
                    .Where(c => choice.CardCondition.IsMatch(ownerCard, c, eventSource))
                    .ToArray(),
                ZoneType.Field => this.PlayersById.Values.SelectMany(p => p.Field.AllCards)
                    .Where(c => choice.CardCondition.IsMatch(ownerCard, c, eventSource))
                    .ToArray(),
                ZoneType.YouField => this.PlayersById[ownerCard.OwnerId].Field.AllCards
                    .Where(c => choice.CardCondition.IsMatch(ownerCard, c, eventSource))
                    .ToArray(),
                ZoneType.OpponentField => this.GetOpponent(ownerCard.OwnerId).Field.AllCards
                    .Where(c => choice.CardCondition.IsMatch(ownerCard, c, eventSource))
                    .ToArray(),
                _ => new Card[0],
            };
        }

        public IReadOnlyList<CardDef> ChoiceCandidateCardDefs(Choice choice)
        {
            return choice.CardCondition.ZoneCondition switch
            {
                ZoneType.All => this.cardFactory.CardPool
                    .Where(cdef => choice.CardCondition.IsMatch(cdef))
                    .SelectMany(cdef => Enumerable.Repeat(cdef, choice.NumPicks))
                    .ToArray(),
                ZoneType.CardPool => this.cardFactory.CardPool
                    .Where(cdef => choice.CardCondition.IsMatch(cdef))
                    .SelectMany(cdef => Enumerable.Repeat(cdef, choice.NumPicks))
                    .ToArray(),
                _ => new CardDef[0],
            };
        }

        public ChoiceResult ChoiceCandidates(Card ownerCard, Choice choice, Card eventSource)
        {
            var playerIdList = new List<Guid>();
            var cardList = new List<Card>();
            var cardDefList = new List<CardDef>();
            foreach (var c in choice.Candidates)
            {
                switch (c)
                {
                    case Choice.ChoiceCandidateType.AllPlayer:
                        playerIdList.AddRange(this.PlayersById.Values.Select(p => p.Id));
                        break;

                    case Choice.ChoiceCandidateType.OwnerPlayer:
                        playerIdList.Add(ownerCard.OwnerId);
                        break;

                    case Choice.ChoiceCandidateType.OtherOwnerPlayer:
                        playerIdList.Add(this.GetOpponent(ownerCard.OwnerId).Id);
                        break;

                    case Choice.ChoiceCandidateType.TurnPlayer:
                        playerIdList.Add(this.CurrentPlayer.Id);
                        break;

                    case Choice.ChoiceCandidateType.OtherTurnPlayer:
                        playerIdList.Add(this.NextPlayer.Id);
                        break;

                    case Choice.ChoiceCandidateType.Card:
                        cardList.AddRange(this.ChoiceCandidateCards(ownerCard, choice, eventSource));
                        cardDefList.AddRange(this.ChoiceCandidateCardDefs(choice));
                        break;
                }
            }

            return new ChoiceResult()
            {
                PlayerIdList = playerIdList,
                CardList = cardList,
                CardDefList = cardDefList,
            };
        }

        public ChoiceResult ChoiceCards(Card ownerCard, Choice choice, Card eventSource)
        {
            var choiceCandidates = this.ChoiceCandidates(ownerCard, choice, eventSource);

            switch (choice.How)
            {
                case Choice.ChoiceHow.All:
                    return choiceCandidates;

                case Choice.ChoiceHow.Choose:
                    return this.AskCard(ownerCard.OwnerId, choiceCandidates, choice.NumPicks);

                case Choice.ChoiceHow.Random:
                    var totalCount = choiceCandidates.PlayerIdList.Count + choiceCandidates.CardList.Count + choiceCandidates.CardDefList.Count;
                    var totalIndexList = Enumerable.Range(0, totalCount).ToArray();
                    var pickedIndexList = Program.RandomPick(totalIndexList, choice.NumPicks);

                    var randomPickedPlayerIdList = new List<Guid>();
                    var randomPickedCardList = new List<Card>();
                    var randomPickedCardDefList = new List<CardDef>();
                    foreach (var pickedIndex in pickedIndexList)
                    {
                        if (pickedIndex < choiceCandidates.PlayerIdList.Count)
                        {
                            randomPickedPlayerIdList.Add(choiceCandidates.PlayerIdList[pickedIndex]);
                        }
                        else if (pickedIndex < choiceCandidates.PlayerIdList.Count + choiceCandidates.CardList.Count)
                        {
                            var cardIndex = pickedIndex - choiceCandidates.PlayerIdList.Count;
                            randomPickedCardList.Add(choiceCandidates.CardList[cardIndex]);
                        }
                        else
                        {
                            var cardIndex = pickedIndex - choiceCandidates.PlayerIdList.Count - choiceCandidates.CardList.Count;
                            randomPickedCardDefList.Add(choiceCandidates.CardDefList[cardIndex]);
                        }
                    }

                    return new ChoiceResult()
                    {
                        PlayerIdList = randomPickedPlayerIdList,
                        CardList = randomPickedCardList,
                        CardDefList = randomPickedCardDefList
                    };

                default:
                    throw new Exception($"how={choice.How}");
            }
        }
    }
}
