using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
                && !attackCard.IsSummoningSickness
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

        private List<Card> OnStartTurnEffectsCards { get; } = new List<Card>();
        private List<Card> OnEndTurnEffectsCards { get; } = new List<Card>();
        private List<Card> OnEveryPlayEffectsCards { get; } = new List<Card>();
        private List<Card> OnEveryDestroyEffectsCards { get; } = new List<Card>();

        public IEnumerable<CardDef> CardPool => this.cardFactory.CardPool;

        private readonly Func<Guid, IReadOnlyList<Guid>, Guid> AskCardAction;

        public GameMaster(RuleBook ruleBook, CardFactory cardFactory, ILogger logger,
            Func<Guid, IReadOnlyList<Guid>, Guid> askCardAction)
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

        public void DestroyCard(Card card)
        {
            var player = this.PlayersById[card.OwnerId];
            this.logger.LogInformation($"破壊：{player.Name}-{card}");

            this.DoEffect(card, CardEffectType.OnDestroy);

            player.Field.Destroy(card);

            // 各プレイ時の効果を処理
            var copyList = this.OnEveryDestroyEffectsCards.ToArray();
            foreach (var c in copyList)
            {
                if (this.OnEveryDestroyEffectsCards.Contains(c))
                {
                    this.DoEffect(c, CardEffectType.OnEveryDestroy);
                }
            }

            this.OnStartTurnEffectsCards.Remove(card);
            this.OnEndTurnEffectsCards.Remove(card);
            this.OnEveryPlayEffectsCards.Remove(card);
            this.OnEveryDestroyEffectsCards.Remove(card);
        }

        public void DoEffect(Card card, CardEffectType effectType)
        {
            if (card.EffectsByType.TryGetValue(effectType, out var effect))
            {
                this.logger.LogInformation($"効果：{card}-{effectType}");
                effect.Execute(this, card);
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
            //return this.Resolve(playerId, () => new CommandResult());

            return this.CreateEnvironment(playerId);
        }

        //public CommandResult Resolve(Guid playerId, Func<CommandResult> action)
        //{
        //    var result = action();

        //    if (this.GameOver)
        //    {
        //        var winner = this.GetWinner();
        //        this.logger.Information($"勝者：{winner.Name}");
        //    }

        //    result.GameEnvironment = this.CreateEnvironment(playerId);

        //    return result;
        //}

        public (bool IsSucceeded, string errorMessage) StartTurn(Guid playerId)
        {
            if (this.GameOver)
            {
                return (false, "すでにゲームが終了しています。");
            }

            //return this.Resolve(playerId, () =>
            //{
            if (playerId != this.CurrentPlayer.Id)
            {
                //return new CommandResult()
                //{
                //    IsSucceeded = false,
                //    ErrorMessage = "このプレイヤーのターンではありません。"
                //};

                return (false, "このプレイヤーのターンではありません。");
            }

            this.PlayerTurnCountById[this.CurrentPlayer.Id]++;

            this.CurrentPlayer.AddMaxMp(this.RuleBook.MpByStep);
            this.CurrentPlayer.FullMp();
            foreach (var card in this.CurrentPlayer.Field.AllCards)
            {
                card.TurnCountInField++;
            }

            this.logger.LogInformation($"ターン開始：{this.CurrentPlayer.Name}-[HP:{this.CurrentPlayer.Hp}/{this.CurrentPlayer.MaxHp}][MP:{this.CurrentPlayer.MaxMp}][ターン:{this.PlayerTurnCountById[this.CurrentPlayer.Id]}({this.AllTurnCount})]");

            // ターン開始時の効果を処理
            var copyList = this.OnStartTurnEffectsCards.ToArray();
            foreach (var card in copyList)
            {
                if (this.OnStartTurnEffectsCards.Contains(card))
                {
                    this.DoEffect(card, CardEffectType.OnStartTurn);
                }
            }

            this.CurrentPlayer.Draw();

            //return new CommandResult() { IsSucceeded = true };
            //});

            return (true, "");
        }

        public (bool IsSucceeded, string errorMessage) EndTurn(Guid playerId)
        {
            if (this.GameOver)
            {
                return (false, "すでにゲームが終了しています。");
            }

            //return this.Resolve(playerId, () =>
            //{
            if (playerId != this.CurrentPlayer.Id)
            {
                //return new CommandResult()
                //{
                //    IsSucceeded = false,
                //    ErrorMessage = "このプレイヤーのターンではありません。"
                //};

                return (false, "このプレイヤーのターンではありません。");

            }

            var endTurnPlayer = this.CurrentPlayer;

            this.logger.LogInformation($"ターンエンド：{endTurnPlayer.Name}");

            // ターン終了時の効果を処理
            // 処理中にOnEndTurnEffectsCards が変更される可能性がある
            var copyList = this.OnEndTurnEffectsCards.ToArray();
            foreach (var card in copyList)
            {
                if (this.OnEndTurnEffectsCards.Contains(card))
                {
                    this.DoEffect(card, CardEffectType.OnEndTurn);
                }
            }

            this.CurrentPlayer = this.NextPlayer;
            this.NextPlayer = this.GetOpponent(this.CurrentPlayer.Id);

            //    return new CommandResult() { IsSucceeded = true };
            //});

            return (true, "");
        }

        public (bool IsSucceeded, string errorMessage) PlayFromHand(Guid playerId, Guid handCardId)
        {
            if (this.GameOver)
            {
                return (false, "すでにゲームが終了しています。");
            }

            //return this.Resolve(playerId, () =>
            //{
            if (playerId != this.CurrentPlayer.Id)
            {
                //return new CommandResult()
                //{
                //    IsSucceeded = false,
                //    ErrorMessage = "このプレイヤーのターンではありません。"
                //};

                return (false, "このプレイヤーのターンではありません。");
            }

            var player = this.PlayersById[playerId];
            var playingCard = player.Hands.GetById(handCardId);

            this.logger.LogInformation($"プレイ：{player.Name}-{playingCard}");

            // プレイ不能
            if (!this.IsPlayable(player, playingCard))
            {
                //return new CommandResult() { IsSucceeded = false, ErrorMessage = "プレイ不能" };

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

            // 各プレイ時の効果を処理
            var copyList = this.OnEveryPlayEffectsCards.ToArray();
            foreach (var card in copyList)
            {
                if (this.OnEveryPlayEffectsCards.Contains(card))
                {
                    this.DoEffect(card, CardEffectType.OnEveryPlay);
                }
            }

            this.DoEffect(playingCard, CardEffectType.OnPlay);

            var onStartTurnEffect = playingCard.GetEffenct(CardEffectType.OnStartTurn);
            if (onStartTurnEffect != null)
            {
                this.OnStartTurnEffectsCards.Add(playingCard);
            }

            var onEndTurnEffect = playingCard.GetEffenct(CardEffectType.OnEndTurn);
            if (onEndTurnEffect != null)
            {
                this.OnEndTurnEffectsCards.Add(playingCard);
            }

            var onEveryPlayEffect = playingCard.GetEffenct(CardEffectType.OnEveryPlay);
            if (onEveryPlayEffect != null)
            {
                this.OnEveryPlayEffectsCards.Add(playingCard);
            }

            var onEveryDestroyEffect = playingCard.GetEffenct(CardEffectType.OnEveryDestroy);
            if (onEveryDestroyEffect != null)
            {
                this.OnEveryDestroyEffectsCards.Add(playingCard);
            }

            //return new CommandResult() { IsSucceeded = true };
            //});

            return (true, "");
        }

        private Guid[] FetchChoiceCandidates(Guid playerId, TargetCardType targetCardType)
        {
            return targetCardType switch
            {
                TargetCardType.OpponentCreature
                    => this.GetOpponent(playerId).Field.AllCards.Where(c => c.Type == CardType.Creature).Select(c => c.Id).ToArray(),
                TargetCardType.YourCreature
                    => this.PlayersById[playerId].Field.AllCards.Where(c => c.Type == CardType.Creature).Select(c => c.Id).ToArray(),
                _ => new Guid[0]
            };
        }

        public Card AskCard(Guid playerId, TargetCardType targetType)
        {
            var candidates = this.FetchChoiceCandidates(playerId, targetType);
            if (candidates.Length == 0)
            {
                return null;
            }

            var targetCardId = this.AskCardAction(playerId, candidates);

            switch (targetType)
            {
                case TargetCardType.YourCreature:
                    {
                        var you = this.PlayersById[playerId];
                        var targetCard = you.Field.GetById(targetCardId);
                        if (targetCard == null)
                        {
                            throw new Exception("指定されたカードが正しくない");
                        }

                        return targetCard;
                    }

                case TargetCardType.OpponentCreature:
                    {
                        var opponent = this.GetOpponent(playerId);
                        var targetCard = opponent.Field.GetById(targetCardId);
                        if (targetCard == null)
                        {
                            throw new Exception("指定されたカードが正しくない");
                        }

                        return targetCard;
                    }

                default:
                    throw new InvalidOperationException("サポートされていない");
            }
        }

        /// <summary>
        /// 新規に生成されるカードをプレイ（召喚時に効果で召喚とか）
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

            //return this.Resolve(playerId, () =>
            //{
            if (playerId != this.CurrentPlayer.Id)
            {
                //return new CommandResult()
                //{
                //    IsSucceeded = false,
                //    ErrorMessage = "このプレイヤーのターンではありません。"
                //};

                return (false, "このプレイヤーのターンではありません。");
            }

            var player = this.PlayersById[playerId];
            //var playingCard = this.CardFactory.CreateNew(cardId);
            //playingCard.OwnerId = player.Id;
            var playingCard = this.cardFactory.GetById(cardId);

            this.logger.LogInformation($"特殊プレイ：{player.Name}-{playingCard}");

            // プレイ不能
            if (!this.IsPlayableDirect(player, playingCard))
            {
                //return new CommandResult() { IsSucceeded = false, ErrorMessage = "プレイ不能" };

                return (false, "プレイ不能");
            }

            //this.CurrentPlayer.UseMp(playingCard.Cost);

            //player.Hands.Remove(playingCard);

            switch (playingCard.Type)
            {
                case CardType.Creature:
                case CardType.Artifact:
                    player.Field.Add(playingCard);
                    break;

                default:
                    break;
            }

            //this.DoEffect(playingCard, CardEffectType.OnPlay);

            //var onStartTurnEffect = playingCard.GetEffenct(CardEffectType.OnStartTurn);
            //if (onStartTurnEffect != null)
            //{
            //    this.OnStartTurnEffectsCards.Add(playingCard);
            //}

            //var onEndTurnEffect = playingCard.GetEffenct(CardEffectType.OnEndTurn);
            //if (onEndTurnEffect != null)
            //{
            //    this.OnEndTurnEffectsCards.Add(playingCard);
            //}

            //    return new CommandResult() { IsSucceeded = true };
            //});

            return (true, "");
        }

        public (bool IsSucceeded, string errorMessage) AttackToPlayer(Guid playerId, Guid cardId, Guid damagePlayerId)
        {
            if (this.GameOver)
            {
                return (false, "すでにゲームが終了しています。");
            }

            //return this.Resolve(playerId, () =>
            //{
            if (playerId != this.CurrentPlayer.Id)
            {
                //return new CommandResult()
                //{
                //    IsSucceeded = false,
                //    ErrorMessage = "このプレイヤーのターンではありません。"
                //};

                return (false, "このプレイヤーのターンではありません。");
            }

            var player = this.PlayersById[playerId];
            var card = player.Field.GetById(cardId);
            var damagePlayer = this.PlayersById[damagePlayerId];

            this.logger.LogInformation($"アタック（プレイヤー）：{player.Name}-{card} > {damagePlayer.Name}");

            if (!CanAttack(card, damagePlayer))
            {
                //return new CommandResult() { IsSucceeded = false, ErrorMessage = "攻撃不能" };

                return (false, "攻撃不能");
            }

            this.HitPlayer(damagePlayer.Id, card.Power);

            //return new CommandResult() { IsSucceeded = true };
            //});

            return (true, "");
        }

        public (bool IsSucceeded, string errorMessage) HitPlayer(Guid playerId, Guid damagePlayerId, int damage)
        {
            if (this.GameOver)
            {
                return (false, "すでにゲームが終了しています。");
            }

            //return this.Resolve(playerId, () =>
            //{
            if (playerId != this.CurrentPlayer.Id)
            {
                //return new CommandResult()
                //{
                //    IsSucceeded = false,
                //    ErrorMessage = "このプレイヤーのターンではありません。"
                //};

                return (false, "このプレイヤーのターンではありません。");
            }

            var damagePlayer = this.PlayersById[damagePlayerId];

            this.logger.LogInformation($"ダメージ：{damage} > {damagePlayer.Name}");

            damagePlayer.Damage(damage);

            //    return new CommandResult() { IsSucceeded = true };
            //});

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

            //return this.Resolve(playerId, () =>
            //{
            if (playerId != this.CurrentPlayer.Id)
            {
                //return new CommandResult()
                //{
                //    IsSucceeded = false,
                //    ErrorMessage = "このプレイヤーのターンではありません。"
                //};

                return (false, "このプレイヤーのターンではありません。");

            }

            var attackCard = this.cardFactory.GetById(attackCardId);
            var guardCard = this.cardFactory.GetById(guardCardId);

            var attackPlayer = this.PlayersById[attackCard.OwnerId];
            var guardPlayer = this.PlayersById[guardCard.OwnerId];

            this.logger.LogInformation($"アタック（クリーチャー）：{attackPlayer.Name}-{attackCard} > {guardPlayer.Name}-{guardCard}");

            if (!CanAttack(attackCard, guardCard, this.CreateEnvironment(playerId)))
            {
                //return new CommandResult() { IsSucceeded = false, ErrorMessage = "攻撃不能" };

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
            //});
        }

        public (bool IsSucceeded, string errorMessage) HitCreature(Guid playerId, Guid creatureCardId, int damage)
        {
            if (this.GameOver)
            {
                return (false, "すでにゲームが終了しています。");
            }

            //return this.Resolve(playerId, () =>
            //{
            if (playerId != this.CurrentPlayer.Id)
            {
                //return new CommandResult()
                //{
                //    IsSucceeded = false,
                //    ErrorMessage = "このプレイヤーのターンではありません。"
                //};

                return (false, "このプレイヤーのターンではありません。");
            }

            var creatureCard = this.cardFactory.GetById(creatureCardId);

            this.logger.LogInformation($"ダメージ：{damage} > {creatureCard}");

            creatureCard.Damage(damage);

            if (creatureCard.Toughness <= 0)
            {
                this.DestroyCard(creatureCard);
            }

            //    return new CommandResult() { IsSucceeded = true };
            //});

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
    }
}
