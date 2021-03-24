using Cauldron.Core.Entities.Effect;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Cauldron_Test")]
namespace Cauldron.Core.Entities
{
    public class GameMaster
    {
        public static bool CanAttack(Card attackCard)
        {
            return attackCard != null
                // クリーチャーでなければ攻撃できない
                && attackCard.Type == CardType.Creature
                // 召喚酔いでない
                && attackCard.NumTurnsToCanAttack <= attackCard.NumTurnsInField
                // 攻撃不能状態でない
                && !attackCard.Abilities.Contains(CreatureAbility.CantAttack)
                // 1ターン中に攻撃可能な回数を超えていない
                && attackCard.NumAttacksLimitInTurn > attackCard.NumAttacksInTurn
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

        public static bool CanAttack(Card attackCard, Card guardCard, GameContext environment)
        {
            var guardPlayer = environment.You.PublicPlayerInfo.Id == guardCard.OwnerId
                ? environment.You.PublicPlayerInfo
                : environment.Opponent;
            var existsCover = guardPlayer.Field
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

        private readonly CardRepository cardRepository;

        public readonly ConcurrentDictionary<PlayerId, PlayerDef> PlayerDefsById = new();
        public readonly ConcurrentDictionary<PlayerId, Player> PlayersById = new();

        public Player ActivePlayer { get; set; }

        public Player NextPlayer { get; set; }

        public IEnumerable<Player> NonActivePlayers => this.PlayersById.Values.Where(player => player.Id != this.ActivePlayer.Id);

        public bool GameOver => this.PlayersById.Values.Any(player => player.CurrentHp <= 0);

        public ConcurrentDictionary<PlayerId, int> PlayerTurnCountById { get; set; } = new();

        public int AllTurnCount => this.PlayerTurnCountById.Sum(x => x.Value);

        public IReadOnlyList<CardDef> CardPool => this.cardRepository.CardPool;

        private readonly EffectManager effectManager;

        public GameEventListener EventListener { get; }

        public bool IsStarted { get; private set; }

        public Player GetWinner()
        {
            // 引き分けならターンのプレイヤーの負け
            var alives = this.PlayersById.Values.Where(p => p.CurrentHp > 0).ToArray();
            return alives.Length == 0
                ? this.PlayersById.Values.First(p => p.Id != this.ActivePlayer.Id)
                : alives.First();
        }

        public Player GetOpponent(PlayerId playerId)
        {
            return this.PlayersById.Values
                .First(player => player.Id != playerId);
        }

        public (GameMasterStatusCode, CardId[]) ListPlayableCardId(PlayerId playerId)
        {
            if (!this.PlayersById.TryGetValue(playerId, out var player))
            {
                return (GameMasterStatusCode.PlayerNotExists, default);
            }

            var playableCardIdList = player.Hands.AllCards
                .Where(hand => IsPlayable(player, hand))
                .Select(c => c.Id)
                .ToArray();

            return (GameMasterStatusCode.OK, playableCardIdList);
        }

        public static bool IsPlayable(Player player, Card card)
        {
            // フィールドに出すカードはフィールドに空きがないとプレイできない
            if (player.Field.Full)
            {
                return false;
            }

            // コストが払えないとプレイできない
            if (player.CurrentMp < card.Cost)
            {
                return false;
            }

            return true;
        }

        public static bool IsPlayableDirect(Player player, Card card)
        {
            // フィールドに出すカードはフィールドに空きがないとプレイできない
            if (player.Field.Full)
            {
                return false;
            }

            return true;
        }

        public (GameMasterStatusCode, (PlayerId[], CardId[])) ListAttackTargets(CardId cardId)
        {
            var (exists, card) = this.cardRepository.TryGetById(cardId);
            if (!exists)
            {
                return (GameMasterStatusCode.CardNotExists, default);
            }

            var playerIdList = this.PlayersById.Values
                .Where(p => CanAttack(card, p))
                .Select(p => p.Id)
                .ToArray();

            var context = this.CreateGameContext(card.OwnerId);
            var cardIdList = this.GetOpponent(card.OwnerId).Field.AllCards
                .Where(c => CanAttack(card, c, context))
                .Select(c => c.Id)
                .ToArray();

            return (GameMasterStatusCode.OK, (playerIdList, cardIdList));
        }

        public GameMaster(GameMasterOptions options)
        {
            this.RuleBook = options.RuleBook;
            this.cardRepository = options.CardFactory;
            this.logger = options.Logger;
            this.EventListener = options.EventListener;

            this.effectManager = new EffectManager(logger);
        }

        public (GameMasterStatusCode, PlayerId) CreateNewPlayer(PlayerId newId, string name, IEnumerable<CardDefId> deckCardDefIdList)
        {
            //var newId = PlayerId.NewId();
            var deckCards = deckCardDefIdList.Select(id => this.cardRepository.CreateNew(id)).ToArray();

            if (deckCards.Any(c => c == null))
            {
                return (GameMasterStatusCode.CardNotExists, default);
            }

            // 提示されたデッキにトークンが含まれていてはいけない
            if (deckCards.Any(c => c.IsToken))
            {
                return (GameMasterStatusCode.IsIncludedTokensInDeck, default);
            }

            var playerDef = new PlayerDef(newId, name, deckCardDefIdList.ToArray());
            this.PlayerDefsById.TryAdd(newId, playerDef);

            return (GameMasterStatusCode.OK, newId);
        }

        public async ValueTask Start(PlayerId firstPlayerId)
        {
            if (this.IsStarted) return;

            this.IsStarted = true;

            try
            {
                if (!this.PlayerDefsById.ContainsKey(firstPlayerId))
                {
                    throw new InvalidOperationException("存在しないプレイヤーです。");
                }

                foreach (var playerDef in this.PlayerDefsById.Values)
                {
                    var deckCards = playerDef.DeckIdList.Select(id => this.cardRepository.CreateNew(id)).ToArray();
                    var player = new Player(playerDef.Id, playerDef.Name, this.RuleBook, deckCards);
                    this.PlayersById.TryAdd(player.Id, player);

                    this.PlayerTurnCountById.TryAdd(player.Id, 0);
                }

                this.ActivePlayer = this.PlayersById[firstPlayerId];
                this.NextPlayer = this.GetOpponent(this.ActivePlayer.Id);

                foreach (var player in this.PlayersById.Values)
                {
                    player.Deck.Shuffle();

                    // カードを配る
                    await this.Draw(player.Id, this.RuleBook.InitialNumHands);
                }

                this.EventListener?.OnStartTurn?.Invoke(this.ActivePlayer.Id, this.CreateGameContext(this.ActivePlayer.Id));
            }
            catch
            {
                this.IsStarted = false;
                throw;
            }
        }

        /// <summary>
        /// 指定したカードを破壊します。
        /// </summary>
        /// <param name="cardToDestroy"></param>
        public async ValueTask DestroyCard(Card cardToDestroy)
        {
            var player = this.PlayersById[cardToDestroy.OwnerId];
            this.logger.LogInformation($"破壊：{cardToDestroy}({player.Name})");

            await this.MoveCard(cardToDestroy.Id, new(new(cardToDestroy.OwnerId, ZoneName.Field), new(cardToDestroy.OwnerId, ZoneName.Cemetery)));

            await this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnDestroy, this, SourceCard: cardToDestroy));
        }

        public async ValueTask Buff(Card creatureCard, int powerBuff, int toughnessBuff)
        {
            this.logger.LogInformation($"修整：{creatureCard}({this.PlayersById[creatureCard.OwnerId].Name})-[{powerBuff},{toughnessBuff}]");

            if (creatureCard.Type != CardType.Creature)
            {
                return;
            }

            creatureCard.PowerBuff += powerBuff;
            creatureCard.ToughnessBuff += toughnessBuff;

            // カードの持ち主には無条件に通知する
            this.EventListener?.OnModifyCard?.Invoke(creatureCard.OwnerId,
                this.CreateGameContext(creatureCard.OwnerId),
                new ModifyCardNotifyMessage()
                {
                    CardId = creatureCard.Id,
                });

            if (creatureCard.Toughness <= 0)
            {
                await this.DestroyCard(creatureCard);
            }
        }

        //public void AddHand(Player player, Card addCard)
        //{
        //    player.Hands.Add(addCard);

        //    this.logger.LogInformation($"手札に追加: {addCard.Name}({player.Name})");
        //    var handsLog = string.Join(",", player.Hands.AllCards.Select(c => c.Name));
        //    this.logger.LogInformation($"手札: {handsLog}({player.Name})");
        //}

        public async ValueTask<GameMasterStatusCode> Draw(PlayerId playerId, int numCards)
        {
            if (!this.PlayersById.TryGetValue(playerId, out var player))
            {
                return GameMasterStatusCode.PlayerNotExists;
            }

            foreach (var _ in Enumerable.Range(0, numCards))
            {
                var drawCard = player.Draw();

                this.logger.LogInformation($"ドロー: {player.Name}: {drawCard.Name}");

                await this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnDraw, this, SourceCard: drawCard));
                await this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnMoveCard, this, SourceCard: drawCard,
                      MoveCardContext: new(new(playerId, ZoneName.Deck), new(playerId, ZoneName.Hand))));

                foreach (var pid in this.PlayersById.Keys)
                {
                    this.EventListener?.OnMoveCard?.Invoke(pid,
                        this.CreateGameContext(pid),
                        new MoveCardNotifyMessage()
                        {
                            CardId = drawCard.Id,
                            ToZone = new Zone(
                                playerId,
                                ZoneName.Hand
                            )
                        });
                }
            }

            return GameMasterStatusCode.OK;
        }

        public Card GenerateNewCard(CardDefId cardDefId, Zone zone)
        {
            var card = this.cardRepository.CreateNew(cardDefId);
            card.OwnerId = zone.PlayerId;

            switch (zone.ZoneName)
            {
                case ZoneName.Cemetery:
                    this.PlayersById[zone.PlayerId].Cemetery.Add(card);
                    break;

                case ZoneName.Deck:
                    //this.PlayersById[zone.playerid].Deck.Add(card);
                    break;

                case ZoneName.Field:
                    //this.PlayDirect(zone.PlayerId, card.Id);
                    this.PlayersById[zone.PlayerId].Field.Add(card);
                    break;

                case ZoneName.Hand:
                    this.PlayersById[zone.PlayerId].Hands.Add(card);
                    break;

                default:
                    throw new InvalidOperationException();
            }

            card.Zone = zone;

            // カードの持ち主には無条件に通知する
            this.EventListener?.OnAddCard?.Invoke(card.OwnerId,
                this.CreateGameContext(card.OwnerId),
                new AddCardNotifyMessage()
                {
                    CardId = card.Id,
                    ToZone = zone
                });

            var isPublic = zone.IsPublic();

            // カードの持ち主以外への通知は
            // 移動元か移動後どちらかの領域が公開領域の場合のみ
            this.EventListener?.OnAddCard?.Invoke(this.GetOpponent(card.OwnerId).Id,
                this.CreateGameContext(this.GetOpponent(card.OwnerId).Id),
                new AddCardNotifyMessage()
                {
                    CardId = isPublic ? card.Id : default,
                    ToZone = zone
                });

            return card;
        }

        public async ValueTask MoveCard(CardId cardId, MoveCardContext moveCardContext)
        {
            var (exists, card) = this.cardRepository.TryGetById(cardId);
            if (!exists)
            {
                return;
            }

            switch (moveCardContext.From.ZoneName)
            {
                case ZoneName.Cemetery:
                    //this.PlayersById[moveCardContext.From.PlayerId].Cemetery.Remove(card);
                    break;

                case ZoneName.Deck:
                    this.PlayersById[moveCardContext.From.PlayerId].Deck.Remove(card);
                    break;

                case ZoneName.Field:
                    this.PlayersById[moveCardContext.From.PlayerId].Field.Remove(card);
                    break;

                case ZoneName.Hand:
                    this.PlayersById[moveCardContext.From.PlayerId].Hands.Remove(card);
                    break;

                default:
                    throw new InvalidOperationException();
            }

            switch (moveCardContext.To.ZoneName)
            {
                case ZoneName.Cemetery:
                    this.PlayersById[moveCardContext.To.PlayerId].Cemetery.Add(card);
                    break;

                case ZoneName.Deck:
                    //this.PlayersById[moveCardContext.To.PlayerId].Deck.Add(card);
                    break;

                case ZoneName.Field:
                    this.PlayersById[moveCardContext.To.PlayerId].Field.Add(card);
                    break;

                case ZoneName.Hand:
                    this.PlayersById[moveCardContext.To.PlayerId].Hands.Add(card);
                    break;

                default:
                    throw new InvalidOperationException();
            }

            card.Zone = moveCardContext.To;

            // カードの持ち主には無条件に通知する
            this.EventListener?.OnMoveCard?.Invoke(card.OwnerId,
                this.CreateGameContext(card.OwnerId),
                new MoveCardNotifyMessage()
                {
                    CardId = card.Id,
                    ToZone = new Zone(
                        moveCardContext.To.PlayerId,
                        moveCardContext.To.ZoneName
                    )
                });

            var isPublic = moveCardContext.From.IsPublic()
                || moveCardContext.To.IsPublic();

            // カードの持ち主以外への通知は
            // 移動元か移動後どちらかの領域が公開領域の場合のみ
            this.EventListener?.OnMoveCard?.Invoke(this.GetOpponent(card.OwnerId).Id,
                this.CreateGameContext(this.GetOpponent(card.OwnerId).Id),
                new MoveCardNotifyMessage()
                {
                    CardId = isPublic ? card.Id : default,
                    ToZone = new Zone(
                        moveCardContext.To.PlayerId,
                        moveCardContext.To.ZoneName
                    )
                });

            // カードの移動イベント
            await this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnMoveCard, this, SourceCard: card, MoveCardContext: moveCardContext));
        }

        public GameContext CreateGameContext(PlayerId playerId)
        {
            return new GameContext()
            {
                GameOver = this.GameOver,
                WinnerPlayerId = this.GetWinner()?.Id ?? default,
                You = this.PlayersById[playerId].PrivatePlayerInfo,
                Opponent = this.GetOpponent(playerId).PublicPlayerInfo,
                RuleBook = this.RuleBook
            };
        }

        public async ValueTask<GameMasterStatusCode> Discard(PlayerId playerId, IEnumerable<CardId> handCardId)
        {
            if (this.PlayersById.TryGetValue(playerId, out var player))
            {
                //TODO 本当に手札にあるのか確認する必要あり
                var handCards = handCardId
                    .Select(cid => this.cardRepository.TryGetById(cid))
                    .Where(x => x.Item1)
                    .Select(x => x.Item2);

                foreach (var card in handCards)
                {
                    await this.MoveCard(card.Id, new(new(playerId, ZoneName.Hand), new(playerId, ZoneName.Cemetery)));
                }

                return GameMasterStatusCode.OK;
            }
            else
            {
                return GameMasterStatusCode.PlayerNotExists;
            }
        }

        public async ValueTask<GameMasterStatusCode> StartTurn()
        {
            this.PlayerTurnCountById[this.ActivePlayer.Id]++;

            // 1ターン目はMP を増やさない
            if (this.PlayerTurnCountById[this.ActivePlayer.Id] != 1)
            {
                this.ActivePlayer.AddMaxMp(this.RuleBook.MpByStep);
            }

            this.ActivePlayer.FullMp();
            foreach (var card in this.ActivePlayer.Field.AllCards)
            {
                card.NumTurnsInField++;
                card.NumAttacksInTurn = 0;
            }

            this.logger.LogInformation(
                 $"ターン開始: {this.ActivePlayer.Name}-[HP:{this.ActivePlayer.CurrentHp}/{this.ActivePlayer.MaxHp}][MP:{this.ActivePlayer.MaxMp}][ターン:{this.PlayerTurnCountById[this.ActivePlayer.Id]}({this.AllTurnCount})]----------------------------");
            this.logger.LogInformation(
                 $"フィールド: {string.Join(",", this.ActivePlayer.Field.AllCards.Select(c => c.Name))}");

            await this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnStartTurn, this));

            await this.Draw(this.ActivePlayer.Id, 1);

            return GameMasterStatusCode.OK;
        }

        public async ValueTask<GameMasterStatusCode> EndTurn()
        {
            var endTurnPlayer = this.ActivePlayer;

            this.logger.LogInformation($"ターンエンド：{endTurnPlayer.Name}");

            await this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnEndTurn, this));

            this.ActivePlayer = this.NextPlayer;
            this.NextPlayer = this.GetOpponent(this.ActivePlayer.Id);
            this.EventListener?.OnStartTurn?.Invoke(this.ActivePlayer.Id, this.CreateGameContext(this.ActivePlayer.Id));

            return GameMasterStatusCode.OK;
        }

        /// <summary>
        /// 手札のカードをプレイします
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="handCardId"></param>
        /// <returns></returns>
        public async ValueTask<GameMasterStatusCode> PlayFromHand(PlayerId playerId, CardId handCardId)
        {
            if (!this.PlayersById.TryGetValue(playerId, out var player))
            {
                return GameMasterStatusCode.PlayerNotExists;
            }

            var (cardExists, playingCard) = this.cardRepository.TryGetById(handCardId, new Zone(playerId, ZoneName.Hand));

            if (!cardExists)
            {
                return GameMasterStatusCode.CardNotExists;
            }

            this.logger.LogInformation($"プレイ：{playingCard}({player.Name})");

            // プレイ不能
            if (!IsPlayable(player, playingCard))
            {
                return GameMasterStatusCode.CardCantPlay;
            }

            this.ActivePlayer.UseMp(playingCard.Cost);

            await this.MoveCard(handCardId, new(new(playerId, ZoneName.Hand), new(playerId, ZoneName.Field)));

            await this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnPlay, this, SourceCard: playingCard));

            switch (playingCard.Type)
            {
                case CardType.Creature:
                case CardType.Artifact:

                    //player.Field.Add(playingCard);
                    break;

                case CardType.Sorcery:
                    await this.MoveCard(handCardId, new(new(playerId, ZoneName.Field), new(playerId, ZoneName.Cemetery)));

                    break;

                default:
                    break;
            }

            return GameMasterStatusCode.OK;
        }

        /// <summary>
        /// プレイヤーにカードを選択させます。
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="choiceCandidates"></param>
        /// <param name="choiceNum"></param>
        /// <returns></returns>
        public async ValueTask<ChoiceResult2> AskCard(PlayerId playerId, ChoiceCandidates choiceCandidates, int choiceNum)
        {
            var askAction = this.EventListener?.AskCardAction;

            if (askAction == null)
            {
                throw new InvalidOperationException("ask action is undefined");
            }

            // answer が正しければtrue, そうでなければfalse
            bool ValidAnswer(ChoiceResult answer)
            {
                var numPicked = answer.PlayerIdList.Length
                    + answer.CardIdList.Length
                    + answer.CardDefIdList.Length;

                if (numPicked > choiceNum)
                {
                    return false;
                }

                var playerNotExists = answer.PlayerIdList
                    .Any(p => !choiceCandidates.PlayerIdList.Contains(p));
                if (playerNotExists)
                {
                    return false;
                }

                var cardNotExists = answer.CardIdList
                    .Any(c => !choiceCandidates.CardList.Select(c => c.Id).Contains(c));
                if (cardNotExists)
                {
                    return false;
                }

                var cardDefNotExists = answer.CardDefIdList
                    .Any(c => !choiceCandidates.CardDefList.Select(c => c.Id).Contains(c));
                if (cardDefNotExists)
                {
                    return false;
                }

                return true;
            }

            ChoiceResult answer = await Task.Run(async () =>
            {
                ChoiceResult tmpAnswer = default;
                do
                {
                    tmpAnswer = await askAction(playerId, choiceCandidates, choiceNum);
                } while (!ValidAnswer(tmpAnswer));

                return tmpAnswer;
            });

            var cards = answer.CardIdList
                .Select(id => this.cardRepository.TryGetById(id))
                .Where(x => x.Item1)
                .Select(x => x.Item2)
                .ToArray();

            var carddefs = answer.CardDefIdList
                .Select(id => this.cardRepository.TryGetCardDefById(id))
                .Where(x => x.Item1)
                .Select(x => x.Item2)
                .ToArray();

            return new ChoiceResult2(
                answer.PlayerIdList,
                cards,
                carddefs
                );
        }

        /// <summary>
        /// プレイヤーに攻撃します。
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="attackCardId"></param>
        /// <param name="damagePlayerId"></param>
        /// <returns></returns>
        public async ValueTask<GameMasterStatusCode> AttackToPlayer(PlayerId playerId, CardId attackCardId, PlayerId damagePlayerId)
        {
            //var player = this.PlayersById[playerId];
            if (!this.PlayersById.TryGetValue(playerId, out var player))
            {
                return GameMasterStatusCode.PlayerNotExists;
            }

            //var attackCard = player.Field.GetById(attackCardId);
            var (attackCardExists, attackCard) = this.cardRepository.TryGetById(attackCardId, new Zone(playerId, ZoneName.Field));
            if (!attackCardExists)
            {
                return GameMasterStatusCode.CardNotExists;
            }

            //player.Field.GetById(attackCardId);
            //var damagePlayer = this.PlayersById[damagePlayerId];
            if (!this.PlayersById.TryGetValue(damagePlayerId, out var damagePlayer))
            {
                return GameMasterStatusCode.PlayerNotExists;
            }

            this.logger.LogInformation($"アタック（プレイヤー）：{attackCard}({player.Name}) > {damagePlayer.Name}");

            if (!CanAttack(attackCard, damagePlayer))
            {
                return GameMasterStatusCode.CantAttack;
            }

            var damageContext = new DamageContext(
                DamageSourceCard: attackCard,
                GuardPlayer: damagePlayer,
                Value: attackCard.Power,
                IsBattle: true
            );
            await this.HitPlayer(damageContext);

            attackCard.NumAttacksInTurn++;

            return GameMasterStatusCode.OK;
        }

        public async ValueTask HitPlayer(DamageContext damageContext)
        {
            var eventArgs = new EffectEventArgs(
                GameEvent.OnDamageBefore,
                this,
                DamageContext: damageContext
            );
            var newEventArgs = await this.effectManager.DoEffect(eventArgs);

            this.logger.LogInformation($"ダメージ：{newEventArgs.DamageContext.Value} > {newEventArgs.DamageContext.GuardPlayer.Name}");

            newEventArgs.DamageContext.GuardPlayer.Damage(newEventArgs.DamageContext.Value);

            // 各プレイヤーに通知
            foreach (var playerId in this.PlayersById.Keys)
            {
                this.EventListener?.OnDamage?.Invoke(playerId,
                    this.CreateGameContext(playerId),
                    new DamageNotifyMessage()
                    {
                        Reason = DamageNotifyMessage.ReasonCode.Attack,
                        Damage = newEventArgs.DamageContext.Value,
                        SourceCardId = newEventArgs.DamageContext.DamageSourceCard.Id,
                        GuardPlayerId = newEventArgs.DamageContext.GuardPlayer.Id
                    });
            }

            await this.effectManager.DoEffect(newEventArgs with { GameEvent = GameEvent.OnDamage });
        }

        public async ValueTask<GameMasterStatusCode> AttackToCreature(PlayerId playerId, CardId attackCardId, CardId guardCardId)
        {
            var (attackCardExists, attackCard) = this.cardRepository.TryGetById(attackCardId);
            if (!attackCardExists)
            {
                return GameMasterStatusCode.CardNotExists;
            }

            var (guardCardExists, guardCard) = this.cardRepository.TryGetById(guardCardId);
            if (!guardCardExists)
            {
                return GameMasterStatusCode.CardNotExists;
            }

            var attackPlayer = this.PlayersById[attackCard.OwnerId];
            var guardPlayer = this.PlayersById[guardCard.OwnerId];

            this.logger.LogInformation($"アタック（クリーチャー）：{attackCard}({attackPlayer.Name}) > {guardCard}({guardPlayer.Name})");

            if (!CanAttack(attackCard, guardCard, this.CreateGameContext(playerId)))
            {
                return GameMasterStatusCode.CantAttack;
            }

            // 攻撃するとステルスを失う
            if (attackCard.Abilities.Contains(CreatureAbility.Stealth))
            {
                attackCard.Abilities.Remove(CreatureAbility.Stealth);
            }

            // お互いにダメージを受ける
            var damageContext = new DamageContext(
                DamageSourceCard: attackCard,
                GuardCard: guardCard,
                Value: attackCard.Power,
                IsBattle: true
            );
            await this.HitCreature(damageContext);

            attackCard.NumAttacksInTurn++;

            var damageContext2 = new DamageContext(
                DamageSourceCard: guardCard,
                GuardCard: attackCard,
                Value: guardCard.Power,
                IsBattle: true
            );
            await this.HitCreature(damageContext2);

            return GameMasterStatusCode.OK;
        }

        public async ValueTask HitCreature(DamageContext damageContext)
        {
            var eventArgs = new EffectEventArgs(GameEvent.OnDamageBefore, this, DamageContext: damageContext);
            var newEventArgs = await this.effectManager.DoEffect(eventArgs);

            if (newEventArgs.DamageContext.GuardCard.Type != CardType.Creature)
            {
                throw new Exception($"指定されたカードはクリーチャーではありません。: {newEventArgs.DamageContext.GuardCard.Name}");
            }

            this.logger.LogInformation(
                $"ダメージ：{newEventArgs.DamageContext.Value} > {newEventArgs.DamageContext.GuardCard}({this.PlayersById[newEventArgs.DamageContext.GuardCard.OwnerId].Name})");

            newEventArgs.DamageContext.GuardCard.Damage(newEventArgs.DamageContext.Value);

            // 各プレイヤーに通知
            foreach (var playerId in this.PlayersById.Keys)
            {
                this.EventListener?.OnDamage?.Invoke(playerId,
                    this.CreateGameContext(playerId),
                    new DamageNotifyMessage()
                    {
                        Reason = DamageNotifyMessage.ReasonCode.Attack,
                        Damage = newEventArgs.DamageContext.Value,
                        SourceCardId = newEventArgs.DamageContext.DamageSourceCard.Id,
                        GuardCardId = newEventArgs.DamageContext.GuardCard.Id
                    });
            }

            await this.effectManager.DoEffect(newEventArgs with { GameEvent = GameEvent.OnDamage });

            var isDead = (
                newEventArgs.DamageContext.Value > 0
                    && newEventArgs.DamageContext.DamageSourceCard.Abilities.Contains(CreatureAbility.Deadly)
                    )
                || newEventArgs.DamageContext.GuardCard.Toughness <= 0;

            if (isDead)
            {
                await this.DestroyCard(newEventArgs.DamageContext.GuardCard);
            }
        }

        public Player[] ChoiceCandidatePlayers(Card effectOwnerCard, Choice choice, EffectEventArgs eventArgs)
        {
            return this.PlayersById.Values
                .Where(p => choice.PlayerCondition.IsMatch(effectOwnerCard, p, eventArgs))
                .ToArray();
        }

        private IEnumerable<Card> ChoiceCandidateSourceCards(Card effectOwnerCard, Choice choice, EffectEventArgs eventArgs)
        {
            var actionContext = choice.CardCondition?.ActionContext;
            if (actionContext != null)
            {
                return actionContext.GetCards(effectOwnerCard, eventArgs);
            }
            else
            {
                return this.cardRepository.GetAllCards;
            }
        }

        public async ValueTask<Card[]> ChoiceCandidateCards(Card effectOwnerCard, Choice choice, EffectEventArgs effectEventArgs)
        {
            var tasks = this.ChoiceCandidateSourceCards(effectOwnerCard, choice, effectEventArgs)
                .Select(async c => (Card: c, IsMatch: await choice.CardCondition.IsMatch(effectOwnerCard, c, effectEventArgs)));

            var cardAndIsMatch = await Task.WhenAll(tasks);

            var source = cardAndIsMatch
                .Where(c => c.IsMatch)
                .Select(c => c.Card);

            if (choice.CardCondition.ZoneCondition == null)
            {
                return source.ToArray();
            }

            var zonPrettyNames = await choice.CardCondition.ZoneCondition.Value.Calculate(effectOwnerCard, effectEventArgs);

            var sourceByZone = zonPrettyNames
                .SelectMany(zoneType => zoneType switch
                {
                    ZonePrettyName.YouField => this.PlayersById[effectOwnerCard.OwnerId].Field.AllCards,
                    ZonePrettyName.OpponentField => this.GetOpponent(effectOwnerCard.OwnerId).Field.AllCards,
                    ZonePrettyName.YouHand => this.PlayersById[effectOwnerCard.OwnerId].Hands.AllCards,
                    ZonePrettyName.OpponentHand => this.GetOpponent(effectOwnerCard.OwnerId).Hands.AllCards,
                    ZonePrettyName.YouCemetery => this.PlayersById[effectOwnerCard.OwnerId].Cemetery.AllCards,
                    ZonePrettyName.OpponentCemetery => this.GetOpponent(effectOwnerCard.OwnerId).Cemetery.AllCards,
                    _ => Array.Empty<Card>(),
                })
                .ToArray();

            return source
                .Where(c => sourceByZone.Any(cz => cz.Id == c.Id))
                .ToArray();
        }

        public async ValueTask<CardDef[]> ChoiceCandidateCardDefs(Card effectOwnerCard, EffectEventArgs effectEventArgs, Choice choice)
        {
            if (choice.CardCondition?.ZoneCondition == null)
            {
                return Array.Empty<CardDef>();
            }

            var zonePrettyNames = await choice.CardCondition.ZoneCondition.Value.Calculate(effectOwnerCard, effectEventArgs);

            async ValueTask<IEnumerable<CardDef>> GetMatchedCarddefs()
            {
                var carddefAndIsMatchTasks = this.cardRepository.CardPool
                    .Select(async cdef => (Carddef: cdef, IsMatch: await choice.CardCondition.IsMatch(effectOwnerCard, effectEventArgs, cdef)));

                var carddefAndIsMatch = await Task.WhenAll(carddefAndIsMatchTasks);

                return carddefAndIsMatch
                    .Where(x => x.IsMatch)
                    .SelectMany(x => Enumerable.Repeat(x.Carddef, choice.NumPicks));
            }

            var carddefsTasks = zonePrettyNames
                .Select(async zoneType => zoneType switch
                {
                    ZonePrettyName.CardPool => await GetMatchedCarddefs(),
                    _ => Array.Empty<CardDef>(),
                });

            var carddefs = await Task.WhenAll(carddefsTasks);

            return carddefs
                .SelectMany(c => c)
                .ToArray();
        }

        public async ValueTask<ChoiceCandidates> ChoiceCandidates(Card effectOwnerCard, Choice choice, EffectEventArgs effectEventArgs)
        {
            var playerList = choice.PlayerCondition == null
                ? Array.Empty<PlayerId>()
                : this.ChoiceCandidatePlayers(effectOwnerCard, choice, effectEventArgs).Select(p => p.Id).ToArray();

            var CardList = choice.CardCondition == null
                ? Array.Empty<Card>()
                : await this.ChoiceCandidateCards(effectOwnerCard, choice, effectEventArgs);

            var CardDefList = choice.CardCondition == null
                ? Array.Empty<CardDef>()
                : await this.ChoiceCandidateCardDefs(effectOwnerCard, effectEventArgs, choice);

            return new ChoiceCandidates(
                playerList,
                CardList,
                CardDefList
            );
        }

        public async ValueTask<ChoiceResult2> ChoiceCards(Card effectOwnerCard, Choice choice, EffectEventArgs eventArgs)
        {
            var choiceCandidates = await this.ChoiceCandidates(effectOwnerCard, choice, eventArgs);

            switch (choice.How)
            {
                case Choice.ChoiceHow.All:
                    return new ChoiceResult2(
                        choiceCandidates.PlayerIdList,
                        choiceCandidates.CardList,
                        choiceCandidates.CardDefList
                        );

                case Choice.ChoiceHow.Choose:
                    return await this.AskCard(effectOwnerCard.OwnerId, choiceCandidates, choice.NumPicks);

                case Choice.ChoiceHow.Random:
                    var totalCount = choiceCandidates.PlayerIdList.Length + choiceCandidates.CardList.Length + choiceCandidates.CardDefList.Length;
                    var totalIndexList = Enumerable.Range(0, totalCount).ToArray();
                    var pickedIndexList = RandomUtil.RandomPick(totalIndexList, choice.NumPicks);

                    var randomPickedPlayerList = new List<PlayerId>();
                    var randomPickedCardList = new List<Card>();
                    var randomPickedCardDefList = new List<CardDef>();
                    foreach (var pickedIndex in pickedIndexList)
                    {
                        if (pickedIndex < choiceCandidates.PlayerIdList.Length)
                        {
                            randomPickedPlayerList.Add(choiceCandidates.PlayerIdList[pickedIndex]);
                        }
                        else if (pickedIndex < choiceCandidates.PlayerIdList.Length + choiceCandidates.CardList.Length)
                        {
                            var cardIndex = pickedIndex - choiceCandidates.PlayerIdList.Length;
                            randomPickedCardList.Add(choiceCandidates.CardList[cardIndex]);
                        }
                        else
                        {
                            var cardIndex = pickedIndex - choiceCandidates.PlayerIdList.Length - choiceCandidates.CardList.Length;
                            randomPickedCardDefList.Add(choiceCandidates.CardDefList[cardIndex]);
                        }
                    }

                    return new ChoiceResult2(
                        randomPickedPlayerList.ToArray(),
                        randomPickedCardList.ToArray(),
                        randomPickedCardDefList.ToArray()
                    );

                default:
                    throw new Exception($"how={choice.How}");
            }
        }

        public async ValueTask ModifyPlayer(ModifyPlayerContext modifyPlayerContext, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            if (!this.PlayersById.TryGetValue(modifyPlayerContext.PlayerId, out var player))
            {
                return;
            }

            await player.Modify(modifyPlayerContext.PlayerModifier, effectOwnerCard, effectEventArgs);

            foreach (var playerId in this.PlayersById.Keys)
            {
                this.EventListener?.OnModifyPlayer?.Invoke(playerId,
                    this.CreateGameContext(playerId),
                    new ModifyPlayerNotifyMessage()
                    {
                        PlayerId = modifyPlayerContext.PlayerId
                    });
            }
        }

        public GameMasterStatusCode AddEffect(Card card, IEnumerable<CardEffect> effectToAdd)
        {
            card.Effects.AddRange(effectToAdd);

            return GameMasterStatusCode.OK;
        }

        private readonly Dictionary<(CardId, string), (int Num, string Text)> variablesByName = new();

        public void SetVariable(CardId cardId, string name, int value)
            => this.SetVariable(cardId, name, (value, default));

        public void SetVariable(CardId cardId, string name, string value)
            => this.SetVariable(cardId, name, (default, value));

        public void SetVariable(CardId cardId, string name, (int, string) value)
        {
            var key = (cardId, name);
            if (this.variablesByName.ContainsKey(key))
            {
                this.variablesByName[key] = value;
            }
            else
            {
                this.variablesByName.Add(key, value);
            }
        }

        public bool TryGetNumVariable(CardId cardId, string name, out int value)
        {
            if (this.variablesByName.TryGetValue((cardId, name), out var v))
            {
                value = v.Num;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetTextVariable(CardId cardId, string name, out string value)
        {
            if (this.variablesByName.TryGetValue((cardId, name), out var v))
            {
                value = v.Text;
                return true;
            }

            value = default;
            return false;
        }


        private readonly Dictionary<(CardId, string), ActionContext> actionContextsByActionName = new();

        public void SetActionContext(CardId cardId, string name, ActionContext actionContext)
        {
            var key = (cardId, name);
            if (this.actionContextsByActionName.ContainsKey(key))
            {
                this.actionContextsByActionName[key] = actionContext;
            }
            else
            {
                this.actionContextsByActionName.Add(key, actionContext);
            }
        }

        public bool TryGetActionContext(CardId cardId, string name, out ActionContext value)
        {
            if (this.actionContextsByActionName.TryGetValue((cardId, name), out value))
            {
                return true;
            }

            value = default;
            return false;
        }
    }
}
