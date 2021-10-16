using Cauldron.Core.Entities.Effect;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
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
        /// <summary>
        /// 指定したカードが、指定したプレイヤーに攻撃可能か
        /// </summary>
        /// <param name="attackCard"></param>
        /// <param name="guardPlayer"></param>
        /// <returns></returns>
        public static bool CanAttack(Card attackCard, Player guardPlayer)
        {
            var existsCover = guardPlayer.Field.AllCards
                .Any(c => c.EnableAbility(CreatureAbility.Cover));

            return
                // 攻撃可能なカード
                attackCard.CanAttack
                // 持ち主には攻撃できない
                && attackCard.OwnerId != guardPlayer.Id
                // カバーされていない
                && !existsCover
                ;
        }

        /// <summary>
        /// 指定したカードが、指定したカードに攻撃可能か
        /// </summary>
        /// <param name="attackCard"></param>
        /// <param name="guardCard"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static bool CanAttack(Card attackCard, Card guardCard, GameContext environment)
        {
            static bool ExistsOtherCover(Card guardCard, GameContext environment)
            {
                var guardPlayer = environment.You.PublicPlayerInfo.Id == guardCard.OwnerId
                    ? environment.You.PublicPlayerInfo
                    : environment.Opponent;

                return guardPlayer.Field.Any(c => c.EnableAbility(CreatureAbility.Cover));
            }

            return
                // 攻撃可能なカード
                attackCard.CanAttack
                // 自分自信のカードには攻撃できない
                && attackCard.OwnerId != guardCard.OwnerId
                // クリーチャー以外には攻撃できない
                && guardCard.Type == CardType.Creature
                // ステルス状態は攻撃対象にならない
                && !guardCard.EnableAbility(CreatureAbility.Stealth)
                // 自分がカバー or 他にカバーがいない
                && (guardCard.EnableAbility(CreatureAbility.Cover)
                    || !ExistsOtherCover(guardCard, environment))
                ;
        }

        public static int CalcIndex(InsertCardPosition insertCardPosition, int min, int max)
        {
            if (insertCardPosition == null)
            {
                return max;
            }

            return insertCardPosition.PositionType switch
            {
                InsertCardPosition.PositionTypeValue.Top
                    => Math.Max(0, Math.Min(insertCardPosition.PositionIndex - 1, max)),
                InsertCardPosition.PositionTypeValue.Bottom
                    => max - Math.Max(0, Math.Min(insertCardPosition.PositionIndex - 1, max)),
                _ => RandomUtil.RandomPick(Enumerable.Range(min, max).ToArray()),
            };
        }

        public static bool IsDead(Card card)
        {
            return card.Type == CardType.Creature
                && card.Toughness <= 0;
        }

        public RuleBook RuleBook { get; }

        private readonly ILogger logger;

        private readonly CardRepository cardRepository;

        public readonly ConcurrentDictionary<PlayerId, PlayerDef> PlayerDefsById = new();

        public readonly PlayerRepository playerRepository = new();

        public Player ActivePlayer { get; set; }

        public Player NextPlayer { get; set; }

        public IEnumerable<Player> NonActivePlayers => this.playerRepository.Opponents(this.ActivePlayer.Id);

        public bool GameOver => this.playerRepository.AllPlayers.Any(player => player.CurrentHp <= 0);

        public ConcurrentDictionary<PlayerId, int> PlayerTurnCountById { get; set; } = new();

        public int AllTurnCount => this.PlayerTurnCountById.Sum(x => x.Value);

        public IReadOnlyList<CardDef> CardPool => this.cardRepository.CardPool;

        private readonly EffectManager effectManager;

        public GameEventListener EventListener { get; }

        public bool IsGameStarted { get; private set; }

        public bool IsTurnStarted { get; private set; }

        public Player GetWinner()
        {
            // 引き分けならターンのプレイヤーの負け
            var alives = this.playerRepository.Alives;
            return alives.Count == 0
                ? this.playerRepository.Opponents(this.ActivePlayer.Id)[0]
                : alives[0];
        }

        public Player Get(PlayerId playerId) => this.playerRepository.TryGet(playerId).value;

        public Player GetOpponent(PlayerId playerId) => this.playerRepository.Opponents(playerId)[0];

        public (bool Exists, CardDef CardDef) TryGet(CardDefId id) => this.cardRepository.TryGetCardDefById(id);

        public (GameMasterStatusCode, CardId[]) ListPlayableCardId(PlayerId playerId)
        {
            var (exists, player) = this.playerRepository.TryGet(playerId);
            if (!exists)
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
            if (card.Type != CardType.Sorcery
                && player.Field.Full)
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

        public (GameMasterStatusCode, (PlayerId[], CardId[])) ListAttackTargets(CardId cardId)
        {
            var (exists, card) = this.cardRepository.TryGetById(cardId);
            if (!exists)
            {
                return (GameMasterStatusCode.CardNotExists, default);
            }

            var playerIdList = this.playerRepository.AllPlayers
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
            this.cardRepository = options.CardRepository;
            this.logger = options.Logger;
            this.EventListener = options.EventListener;

            this.effectManager = new EffectManager(logger);
        }

        public GameMasterStatusCode Surrender(PlayerId playerId)
        {
            var (_, status) = this.Win(this.GetOpponent(playerId).Id);

            return status;
        }

        public (GameMasterStatusCode, PlayerId) CreateNewPlayer(PlayerId newId, string name, IEnumerable<CardDefId> deckCardDefIdList)
        {
            var isValidDeck = this.IsValidDeck(deckCardDefIdList);
            if (!isValidDeck)
            {
                return (GameMasterStatusCode.InvalidDeck, default);
            }

            var playerDef = new PlayerDef(newId, name, deckCardDefIdList.ToArray());
            this.PlayerDefsById.TryAdd(newId, playerDef);

            return (GameMasterStatusCode.OK, newId);
        }

        private bool IsValidDeck(IEnumerable<CardDefId> deckCardDefIdList)
        {
            var deckCardDefList = deckCardDefIdList
                .Select(id => this.cardRepository.TryGetCardDefById(id))
                .Where(x => x.Item1)
                .Select(x => x.Item2)
                .ToArray();

            if (deckCardDefList.Length != deckCardDefIdList.Count())
            {
                this.logger.LogError("includ invalid cards in deck");
                return false;
            }

            // 提示されたデッキにトークンが含まれていてはいけない
            if (deckCardDefList.Any(c => c.IsToken))
            {
                this.logger.LogError("includ token cards in deck");
                return false;
            }

            var invalidNumCards = deckCardDefList.Length < this.RuleBook.MinNumDeckCards
                || deckCardDefList.Length > this.RuleBook.MaxNumDeckCards;

            if (invalidNumCards)
            {
                this.logger.LogError("invalid number of deck cards");
                return false;
            }

            return true;
        }

        public async ValueTask StartGame(PlayerId firstPlayerId)
        {
            if (this.IsGameStarted) return;

            this.IsGameStarted = true;

            try
            {
                if (!this.PlayerDefsById.ContainsKey(firstPlayerId))
                {
                    throw new InvalidOperationException($"player not exists. id={firstPlayerId}");
                }

                foreach (var playerDef in this.PlayerDefsById.Values)
                {
                    var ifFirst = playerDef.Id == firstPlayerId;

                    var deckCards = playerDef.DeckIdList.Select(id => this.cardRepository.CreateNew(id)).ToArray();

                    var player = this.playerRepository.CreateNew(playerDef, this.RuleBook, deckCards, ifFirst);

                    this.PlayerTurnCountById.TryAdd(player.Id, 0);

                    if (ifFirst)
                    {
                        this.ActivePlayer = player;
                    }
                }

                this.NextPlayer = this.GetOpponent(this.ActivePlayer.Id);

                foreach (var player in this.playerRepository.AllPlayers)
                {
                    player.Deck.Shuffle();

                    // カードを配る
                    await this.Draw(player.Id, this.RuleBook.InitialNumHands);
                }

                this.EventListener?.OnStartTurn?.Invoke(this.ActivePlayer.Id, this.CreateGameContext(this.ActivePlayer.Id));
            }
            catch
            {
                this.IsGameStarted = false;
                throw;
            }
        }

        /// <summary>
        /// 指定したカードを破壊します。
        /// </summary>
        /// <param name="cardToDestroy"></param>
        public async ValueTask<bool> DestroyCard(Card cardToDestroy)
        {
            var (exists, player) = this.playerRepository.TryGet(cardToDestroy.OwnerId);
            if (!exists)
            {
                throw new InvalidOperationException($"player not exists. id={cardToDestroy.OwnerId}");
            }

            if (cardToDestroy.Zone.ZoneName != ZoneName.Field)
            {
                this.logger.LogWarning($"destroy card should be in the field. zone={cardToDestroy.Zone.ZoneName}");
                return false;
            }

            this.logger.LogInformation($"破壊：{cardToDestroy}({player.Name})");

            var moveContext = new MoveCardContext(
                new(cardToDestroy.OwnerId, ZoneName.Field),
                new(cardToDestroy.OwnerId, ZoneName.Cemetery));

            await this.MoveCard(cardToDestroy.Id, moveContext);

            await this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnDestroy, this, SourceCard: cardToDestroy));

            return true;
        }

        public async ValueTask ModifyCard(Card card, EffectActionModifyCard effectActionModifyCard, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var newCost = await (effectActionModifyCard.Cost?.Modify(effectOwnerCard, effectEventArgs, card.Cost)
                    ?? ValueTask.FromResult(card.Cost));

            //this.logger.LogInformation($"修整：{card}({this.PlayersById[card.OwnerId].Name})-[{newCost}, {newPower},{newToughness}]");

            card.CostBuff = newCost - card.BaseCost;

            if (card.Type == CardType.Creature)
            {
                var newPower = await (effectActionModifyCard.Power?.Modify(effectOwnerCard, effectEventArgs, card.Power)
                        ?? ValueTask.FromResult(card.Power));
                var newToughness = await (effectActionModifyCard.Toughness?.Modify(effectOwnerCard, effectEventArgs, card.Toughness)
                        ?? ValueTask.FromResult(card.Toughness));
                var newAbilities = effectActionModifyCard.Ability?.Modify(card.Abilities)
                        ?? card.Abilities.ToArray();

                card.PowerBuff = newPower - card.BasePower;
                card.ToughnessBuff = newToughness - card.BaseToughness;
                card.Abilities = newAbilities.ToList();
            }

            // カードの持ち主には無条件に通知する
            this.EventListener?.OnModifyCard?.Invoke(card.OwnerId,
                this.CreateGameContext(card.OwnerId),
                new ModifyCardNotifyMessage(card.Id));

            if (card.Type == CardType.Creature && card.Toughness <= 0)
            {
                await this.DestroyCard(card);
            }
        }

        public async ValueTask<(GameMasterStatusCode, IReadOnlyList<Card>)> Draw(PlayerId playerId, int numCards)
        {
            var (exists, player) = this.playerRepository.TryGet(playerId);
            if (!exists)
            {
                return (GameMasterStatusCode.PlayerNotExists, default);
            }

            var drawnCards = new List<Card>();
            foreach (var _ in Enumerable.Range(0, numCards))
            {
                var (success, drawCard) = player.Draw();
                var isDrawed = success;
                var isDiscarded = !success && drawCard != default;
                var deckIsEmpty = !success && drawCard == default;

                if (drawCard != default)
                {
                    drawnCards.Add(drawCard);
                }

                if (deckIsEmpty)
                {
                    this.logger.LogInformation($"デッキが0: {player.Name}");

                    // notify
                    foreach (var p in this.playerRepository.AllPlayers)
                    {
                        this.EventListener?.OnDamage?.Invoke(p.Id,
                            this.CreateGameContext(p.Id),
                            new DamageNotifyMessage(
                                DamageNotifyMessage.ReasonValue.DrawDeath,
                                1,
                                GuardPlayerId: playerId));
                    }
                }
                else if (isDiscarded)
                {
                    this.logger.LogInformation($"手札が一杯で墓地へ: {player.Name}: {drawCard.Name}");

                    // notify
                    foreach (var p in this.playerRepository.AllPlayers)
                    {
                        this.EventListener?.OnMoveCard?.Invoke(p.Id,
                            this.CreateGameContext(p.Id),
                            new MoveCardNotifyMessage(
                                drawCard.Id,
                                new Zone(
                                    playerId,
                                    ZoneName.Cemetery
                                ),
                                0));
                    }

                    // event
                    // デッキから直接墓地
                    await this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnMoveCard, this, SourceCard: drawCard,
                          MoveCardContext: new(new(playerId, ZoneName.Deck), new(playerId, ZoneName.Cemetery))));
                }
                else if (isDrawed)
                {
                    this.logger.LogInformation($"ドロー: {player.Name}: {drawCard.Name}");

                    foreach (var p in this.playerRepository.AllPlayers)
                    {
                        this.EventListener?.OnMoveCard?.Invoke(p.Id,
                            this.CreateGameContext(p.Id),
                            new MoveCardNotifyMessage(
                                drawCard.Id,
                                new Zone(
                                    playerId,
                                    ZoneName.Hand
                                ),
                                0));
                    }

                    await this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnDraw, this, SourceCard: drawCard));
                    await this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnMoveCard, this, SourceCard: drawCard,
                          MoveCardContext: new(new(playerId, ZoneName.Deck), new(playerId, ZoneName.Hand))));
                }
            }

            return (GameMasterStatusCode.OK, drawnCards);
        }

        public async ValueTask<Card> GenerateNewCard(CardDefId cardDefId, Zone zone, InsertCardPosition insertCardPosition)
        {
            var card = this.cardRepository.CreateNew(cardDefId);
            card.OwnerId = zone.PlayerId;

            var moveContext = new MoveCardContext(
                new Zone(zone.PlayerId, ZoneName.CardPool),
                zone,
                insertCardPosition);

            await this.MoveCard(card.Id, moveContext);

            return card;
        }

        public async ValueTask<bool> ExcludeCard(Card cardToExclude)
        {
            var excluded = false;
            Player player = default;
            switch (cardToExclude.Zone.ZoneName)
            {
                case ZoneName.Field:
                    {
                        bool exists;
                        (exists, player) = this.playerRepository.TryGet(cardToExclude.Zone.PlayerId);
                        if (exists)
                        {
                            player.Field.Remove(cardToExclude);
                            excluded = true;
                        }
                        break;
                    }
                case ZoneName.Hand:
                    {
                        bool exists;
                        (exists, player) = this.playerRepository.TryGet(cardToExclude.Zone.PlayerId);
                        if (exists)
                        {
                            player.Hands.Remove(cardToExclude);
                            excluded = true;
                        }
                        break;
                    }
                case ZoneName.Deck:
                    {
                        bool exists;
                        (exists, player) = this.playerRepository.TryGet(cardToExclude.Zone.PlayerId);
                        if (exists)
                        {
                            player.Deck.Remove(cardToExclude);
                            excluded = true;
                        }
                        break;
                    }
                case ZoneName.Cemetery:
                    {
                        bool exists;
                        (exists, player) = this.playerRepository.TryGet(cardToExclude.Zone.PlayerId);
                        if (exists)
                        {
                            player.Cemetery.Remove(cardToExclude);
                            excluded = true;
                        }
                        break;
                    }
                default:
                    break;
            }

            if (excluded)
            {
                cardToExclude.Zone = new(cardToExclude.OwnerId, ZoneName.Excluded);
                this.cardRepository.Remove(cardToExclude);

                var (existsDef, def) = this.cardRepository.TryGetCardDefById(cardToExclude.CardDefId);
                if (existsDef)
                {
                    player.Excludes.Add(def);
                }

                this.logger.LogInformation($"Exclude: {cardToExclude}");

                foreach (var p in this.playerRepository.AllPlayers)
                {
                    this.EventListener.OnExcludeCard?.Invoke(p.Id,
                        this.CreateGameContext(p.Id),
                        new ExcludeCardNotifyMessage(
                            cardToExclude.Id)
                        );
                }

                await this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnExclude, this,
                    SourceCard: cardToExclude));
            }

            return excluded;
        }

        public async ValueTask MoveCard(CardId cardId, MoveCardContext moveCardContext)
        {
            var (exists, card) = this.cardRepository.TryGetById(cardId);
            if (!exists)
            {
                throw new InvalidOperationException($"card not exists. id={card.Id}");
            }

            var (fromPlayerExists, fromPlayer) = this.playerRepository.TryGet(moveCardContext.From.PlayerId);
            if (!fromPlayerExists)
            {
                throw new InvalidOperationException($"player not exists. id={moveCardContext.From.PlayerId}");
            }

            var (toPlayerExists, toPlayer) = this.playerRepository.TryGet(moveCardContext.To.PlayerId);
            if (!toPlayerExists)
            {
                throw new InvalidOperationException($"player not exists. id={moveCardContext.To.PlayerId}");
            }

            switch (moveCardContext.From.ZoneName)
            {
                case ZoneName.Cemetery:
                    fromPlayer.Cemetery.Remove(card);
                    break;

                case ZoneName.Deck:
                    fromPlayer.Deck.Remove(card);
                    break;

                case ZoneName.Field:
                    fromPlayer.Field.Remove(card);
                    break;

                case ZoneName.Hand:
                    fromPlayer.Hands.Remove(card);
                    break;

                case ZoneName.CardPool:
                    // 追加の場合はCardPool でくる
                    break;

                case ZoneName.Temporary:
                    break;

                default:
                    throw new InvalidOperationException();
            }

            var toIndex = -1;
            switch (moveCardContext.To.ZoneName)
            {
                case ZoneName.Cemetery:
                    toPlayer.Cemetery.Add(card);
                    toIndex = toPlayer.Cemetery.Count - 1;
                    break;

                case ZoneName.Deck:
                    var index = CalcIndex(moveCardContext.InsertCardPosition, 0, toPlayer.Deck.Count);
                    toPlayer.Deck.Insert(index, card);
                    break;

                case ZoneName.Field:
                    // 場の枚数が上限なら、直接墓地に行く
                    if (toPlayer.Field.Full)
                    {
                        this.logger.LogInformation("field is full");
                        await this.MoveCard(card.Id, new(moveCardContext.From, new Zone(toPlayer.Id, ZoneName.Cemetery)));
                        return;
                    }

                    toPlayer.Field.Add(card);
                    toIndex = toPlayer.Field.Count - 1;
                    break;

                case ZoneName.Hand:
                    // 手札の枚数が上限なら、直接墓地に行く
                    if (toPlayer.Hands.Count == this.RuleBook.MaxNumHands)
                    {
                        this.logger.LogInformation("hand is full");
                        await this.MoveCard(card.Id, new(moveCardContext.From, new Zone(toPlayer.Id, ZoneName.Cemetery)));
                        return;
                    }

                    toPlayer.Hands.Add(card);
                    toIndex = toPlayer.Hands.Count - 1;
                    break;

                case ZoneName.Temporary:
                    break;

                default:
                    throw new InvalidOperationException();
            }

            card.Zone = moveCardContext.To;
            card.OwnerId = toPlayer.Id;

            this.logger
                .LogInformation($"move card. card: {card.Name}, from: {moveCardContext.From.ZoneName}, to: {moveCardContext.To.ZoneName}");

            var isAdd = moveCardContext.From.ZoneName == ZoneName.CardPool;

            if (isAdd)
            {
                // カードの持ち主には無条件に通知する
                this.EventListener?.OnAddCard?.Invoke(card.OwnerId,
                    this.CreateGameContext(card.OwnerId),
                    new AddCardNotifyMessage(
                        card.Id,
                        moveCardContext.To));

                var isPublic = moveCardContext.To.IsPublic();

                // カードの持ち主以外への通知は
                // 移動後の領域が公開領域の場合のみ
                this.EventListener?.OnAddCard?.Invoke(this.GetOpponent(card.OwnerId).Id,
                    this.CreateGameContext(this.GetOpponent(card.OwnerId).Id),
                    new AddCardNotifyMessage(
                        isPublic ? card.Id : default,
                        moveCardContext.To));
            }
            else
            {
                // カードの持ち主には無条件に通知する
                this.EventListener?.OnMoveCard?.Invoke(card.OwnerId,
                    this.CreateGameContext(card.OwnerId),
                    new MoveCardNotifyMessage(
                        card.Id,
                        moveCardContext.To,
                        toIndex
                        ));

                var isPublic = moveCardContext.From.IsPublic()
                    || moveCardContext.To.IsPublic();

                // カードの持ち主以外への通知は
                // 移動元か移動後どちらかの領域が公開領域の場合のみ
                this.EventListener?.OnMoveCard?.Invoke(this.GetOpponent(card.OwnerId).Id,
                    this.CreateGameContext(this.GetOpponent(card.OwnerId).Id),
                    new MoveCardNotifyMessage(
                        isPublic ? card.Id : default,
                        moveCardContext.To,
                        isPublic ? toIndex : -1
                        ));
            }

            // カードの移動イベント
            await this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnMoveCard, this, SourceCard: card, MoveCardContext: moveCardContext));

            // 移動先が場で、対象のカードが死亡していれば破壊する
            if (moveCardContext.To.ZoneName == ZoneName.Field
                && IsDead(card))
            {
                await this.DestroyCard(card);
                return;
            }
        }

        public GameContext CreateGameContext(PlayerId playerId)
        {
            var (exists, player) = this.playerRepository.TryGet(playerId);
            if (!exists)
            {
                throw new InvalidOperationException($"player not exists. id={playerId}");
            }

            return new GameContext(
                this.GetWinner()?.Id ?? default,
                this.ActivePlayer?.Id ?? default,
                this.GetOpponent(playerId).PublicPlayerInfo,
                player.PrivatePlayerInfo,
                this.RuleBook,
                this.GameOver
                );
        }

        public async ValueTask<GameMasterStatusCode> Discard(PlayerId playerId, IEnumerable<CardId> handCardId)
        {
            var (exists, player) = this.playerRepository.TryGet(playerId);
            if (!exists)
            {
                return GameMasterStatusCode.PlayerNotExists;
            }

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

        public async ValueTask<GameMasterStatusCode> StartTurn()
        {
            if (this.IsTurnStarted)
            {
                return GameMasterStatusCode.AlreadyTurnStarted;
            }

            this.IsTurnStarted = true;

            this.PlayerTurnCountById[this.ActivePlayer.Id]++;

            // 1ターン目はMP を増やさない
            if (this.PlayerTurnCountById[this.ActivePlayer.Id] != 1)
            {
                this.ActivePlayer.AddMaxMp(this.RuleBook.LimitMpToIncrease);
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

            await this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnStartTurn, this, SourcePlayer: this.ActivePlayer));

            await this.Draw(this.ActivePlayer.Id, 1);

            return GameMasterStatusCode.OK;
        }

        public async ValueTask<GameMasterStatusCode> EndTurn()
        {
            if (!this.IsTurnStarted)
            {
                return GameMasterStatusCode.NotTurnStart;
            }

            this.logger.LogInformation($"ターンエンド：{this.ActivePlayer.Name}");

            await this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnEndTurn, this, SourcePlayer: this.ActivePlayer));

            this.IsTurnStarted = false;
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
            var (exists, player) = this.playerRepository.TryGet(playerId);
            if (!exists)
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

            foreach (var p in this.playerRepository.AllPlayers)
            {
                this.EventListener?.OnPlay?.Invoke(p.Id,
                    this.CreateGameContext(p.Id),
                    new PlayCardNotifyMessage(playingCard.OwnerId, playingCard.CardDefId));
            }

            switch (playingCard.Type)
            {
                case CardType.Sorcery:
                    await this.MoveCard(handCardId, new(new(playerId, ZoneName.Hand), new(playerId, ZoneName.Temporary)));
                    break;

                default:
                    await this.MoveCard(handCardId, new(new(playerId, ZoneName.Hand), new(playerId, ZoneName.Field)));
                    break;
            }

            var effectArgs = await this.effectManager.DoEffectOnPlay(playingCard,
                new EffectEventArgs(GameEvent.OnPlay, this, SourceCard: playingCard));

            this.effectManager.RegisterEffectIfNeeded(playingCard);
            await this.effectManager.DoEffect(effectArgs);

            switch (playingCard.Type)
            {
                case CardType.Sorcery:
                    await this.MoveCard(handCardId, new(new(playerId, ZoneName.Temporary), new(playerId, ZoneName.Cemetery)));
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
        public async ValueTask<ChoiceResult> AskCard(PlayerId playerId, ChoiceCandidates choiceCandidates, int choiceNum)
        {
            var askAction = this.EventListener?.AskCardAction;

            if (askAction == null)
            {
                throw new InvalidOperationException("ask action is undefined");
            }

            // 選択肢がゼロなら聞かずに終わり
            if (choiceCandidates.Count == 0)
            {
                return new ChoiceResult(
                    Array.Empty<PlayerId>(),
                    Array.Empty<Card>(),
                    Array.Empty<CardDef>()
                    );
            }

            // answer が正しければtrue, そうでなければfalse
            bool ValidAnswer(ChoiceAnswer answer)
            {
                if (answer.Count() > choiceNum)
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

            ChoiceAnswer answer = await Task.Run(async () =>
            {
                ChoiceAnswer tmpAnswer = default;
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

            return new ChoiceResult(
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
            var (exists, player) = this.playerRepository.TryGet(playerId);
            if (!exists)
            {
                return GameMasterStatusCode.PlayerNotExists;
            }

            var (attackCardExists, attackCard) = this.cardRepository.TryGetById(attackCardId, new Zone(playerId, ZoneName.Field));
            if (!attackCardExists)
            {
                return GameMasterStatusCode.CardNotExists;
            }

            var (damagePlayerExists, damagePlayer) = this.playerRepository.TryGet(damagePlayerId);
            if (!damagePlayerExists)
            {
                return GameMasterStatusCode.PlayerNotExists;
            }

            this.logger.LogInformation($"アタック（プレイヤー）：{attackCard}({player.Name}) > {damagePlayer.Name}");

            if (!CanAttack(attackCard, damagePlayer))
            {
                return GameMasterStatusCode.CantAttack;
            }

            // 各プレイヤーに通知
            foreach (var p in this.playerRepository.AllPlayers)
            {
                this.EventListener?.OnBattleStart?.Invoke(p.Id,
                    this.CreateGameContext(p.Id),
                    new BattleNotifyMessage(attackCard.Id, GuardPlayerId: damagePlayer.Id));
            }

            // 攻撃するとステルスを失う
            if (attackCard.EnableAbility(CreatureAbility.Stealth))
            {
                attackCard.Abilities.Remove(CreatureAbility.Stealth);
            }

            // 戦闘前のイベント
            var newArgs = await this.effectManager.DoEffect(
                new EffectEventArgs(GameEvent.OnAttackBefore, this, SourceCard: attackCard,
                    BattleContext: new(attackCard, null, damagePlayer)));

            // 攻撃カードが場から離れていたら戦闘しない
            if (newArgs.BattleContext.AttackCard.Zone.ZoneName != ZoneName.Field)
            {
                return GameMasterStatusCode.OK;
            }

            var damageContext = new DamageContext(
                DamageSourceCard: newArgs.BattleContext.AttackCard,
                GuardPlayer: newArgs.BattleContext.GuardPlayer,
                Value: newArgs.BattleContext.AttackCard.Power,
                IsBattle: true
            );
            await this.HitPlayer(damageContext);

            attackCard.NumAttacksInTurn++;

            // 各プレイヤーに通知
            foreach (var p in this.playerRepository.AllPlayers)
            {
                this.EventListener?.OnBattleEnd?.Invoke(p.Id,
                    this.CreateGameContext(p.Id),
                    new BattleNotifyMessage(attackCard.Id, GuardPlayerId: damagePlayer.Id));
            }

            // 戦闘後のイベント
            await this.effectManager.DoEffect(newArgs with { GameEvent = GameEvent.OnAttack });

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

            var newDamageContext = newEventArgs.DamageContext;

            newDamageContext.GuardPlayer.Damage(newDamageContext.Value);

            // 各プレイヤーに通知
            foreach (var player in this.playerRepository.AllPlayers)
            {
                this.EventListener?.OnDamage?.Invoke(player.Id,
                    this.CreateGameContext(player.Id),
                    new DamageNotifyMessage(
                        newDamageContext.IsBattle
                            ? DamageNotifyMessage.ReasonValue.Attack
                            : DamageNotifyMessage.ReasonValue.Effect,
                        newDamageContext.Value,
                        SourceCardId: newEventArgs.DamageContext.DamageSourceCard.Id,
                        GuardPlayerId: newEventArgs.DamageContext.GuardPlayer.Id
                        ));
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

            var (attackPlayerExists, attackPlayer) = this.playerRepository.TryGet(attackCard.OwnerId);
            if (!attackPlayerExists)
            {
                return GameMasterStatusCode.PlayerNotExists;
            }

            var (guardPlayerExists, guardPlayer) = this.playerRepository.TryGet(guardCard.OwnerId);
            if (!guardPlayerExists)
            {
                return GameMasterStatusCode.PlayerNotExists;
            }

            this.logger.LogInformation($"アタック（クリーチャー）：{attackCard}({attackPlayer.Name}) > {guardCard}({guardPlayer.Name})");

            if (!CanAttack(attackCard, guardCard, this.CreateGameContext(playerId)))
            {
                return GameMasterStatusCode.CantAttack;
            }

            // 各プレイヤーに通知
            foreach (var player in this.playerRepository.AllPlayers)
            {
                this.EventListener?.OnBattleStart?.Invoke(player.Id,
                    this.CreateGameContext(player.Id),
                    new BattleNotifyMessage(attackCard.Id, GuardCardId: guardCard.Id));
            }

            // 攻撃するとステルスを失う
            if (attackCard.EnableAbility(CreatureAbility.Stealth))
            {
                attackCard.Abilities.Remove(CreatureAbility.Stealth);
            }

            // 戦闘前のイベント
            var newArgs = await this.effectManager.DoEffect(
                new EffectEventArgs(GameEvent.OnAttackBefore, this, SourceCard: attackCard,
                    BattleContext: new(attackCard, guardCard, null)));

            // どちらかが場から離れていたらダメージ計算しない
            if (newArgs.BattleContext.AttackCard.Zone.ZoneName == ZoneName.Field
                && newArgs.BattleContext.GuardCard.Zone.ZoneName == ZoneName.Field)
            {

                // お互いにダメージを受ける
                var damageContext = new DamageContext(
                    DamageSourceCard: newArgs.BattleContext.AttackCard,
                    GuardCard: newArgs.BattleContext.GuardCard,
                    Value: newArgs.BattleContext.AttackCard.Power,
                    IsBattle: true
                );
                await this.HitCreature(damageContext);

                // 反撃ダメージは戦闘イベントとして扱わない
                var damageContext2 = new DamageContext(
                    DamageSourceCard: newArgs.BattleContext.GuardCard,
                    GuardCard: newArgs.BattleContext.AttackCard,
                    Value: newArgs.BattleContext.GuardCard.Power,
                    IsBattle: true
                );
                await this.HitCreature(damageContext2);
            }

            attackCard.NumAttacksInTurn++;

            // 各プレイヤーに通知
            foreach (var player in this.playerRepository.AllPlayers)
            {
                this.EventListener?.OnBattleEnd?.Invoke(player.Id,
                    this.CreateGameContext(player.Id),
                    new BattleNotifyMessage(attackCard.Id, GuardCardId: guardCard.Id));
            }

            // 戦闘後のイベント
            await this.effectManager.DoEffect(newArgs with { GameEvent = GameEvent.OnAttack });

            return GameMasterStatusCode.OK;
        }

        public async ValueTask HitCreature(DamageContext damageContext)
        {
            var eventArgs = new EffectEventArgs(GameEvent.OnDamageBefore, this, DamageContext: damageContext);
            var newEventArgs = await this.effectManager.DoEffect(eventArgs);

            var newDamageContext = newEventArgs.DamageContext;

            if (newDamageContext.GuardCard.Type != CardType.Creature)
            {
                throw new Exception($"指定されたカードはクリーチャーではありません。: {newDamageContext.GuardCard.Name}");
            }

            {
                var (exists, player) = this.playerRepository.TryGet(newDamageContext.GuardCard.OwnerId);
                var playerName = exists ? player.Name : "";
                this.logger.LogInformation(
                    $"ダメージ：{newDamageContext.Value} > {newDamageContext.GuardCard}({playerName})");
            }

            newDamageContext.GuardCard.Damage(newDamageContext.Value);

            // 各プレイヤーに通知
            foreach (var player in this.playerRepository.AllPlayers)
            {
                this.EventListener?.OnDamage?.Invoke(player.Id,
                    this.CreateGameContext(player.Id),
                    new DamageNotifyMessage(
                        newDamageContext.IsBattle
                            ? DamageNotifyMessage.ReasonValue.Attack
                            : DamageNotifyMessage.ReasonValue.Effect,
                        newDamageContext.Value,
                        SourceCardId: newDamageContext.DamageSourceCard.Id,
                        GuardCardId: newDamageContext.GuardCard.Id
                        ));
            }

            await this.effectManager.DoEffect(newEventArgs with { GameEvent = GameEvent.OnDamage });

            var isDead = (
                newDamageContext.Value > 0
                    && newDamageContext.DamageSourceCard.EnableAbility(CreatureAbility.Deadly)
                    )
                || newDamageContext.GuardCard.Toughness <= 0;

            if (isDead)
            {
                await this.DestroyCard(newDamageContext.GuardCard);
            }
        }

        public async ValueTask<ChoiceResult> Choice(Card effectOwnerCard, Choice choice, EffectEventArgs eventArgs)
        {
            var numPicks = await choice.NumPicks.Calculate(effectOwnerCard, eventArgs);

            var choiceCandidates = await choice.Source
                .ChoiceCandidates(effectOwnerCard, eventArgs, this.playerRepository,
                this.cardRepository, choice.How, numPicks);

            ChoiceResult All() => new(
                choiceCandidates.PlayerIdList,
                choiceCandidates.CardList,
                choiceCandidates.CardDefList
                );

            async ValueTask<ChoiceResult> Choose()
                => await this.AskCard(effectOwnerCard.OwnerId, choiceCandidates, numPicks);

            ChoiceResult Random()
            {
                var totalCount = choiceCandidates.PlayerIdList.Length + choiceCandidates.CardList.Length + choiceCandidates.CardDefList.Length;
                var totalIndexList = Enumerable.Range(0, totalCount).ToArray();
                var pickedIndexList = RandomUtil.RandomPick(totalIndexList, numPicks);

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

                return new ChoiceResult(
                    randomPickedPlayerList.ToArray(),
                    randomPickedCardList.ToArray(),
                    randomPickedCardDefList.ToArray()
                );
            }

            var choiceResult = choice.How switch
            {
                Shared.MessagePackObjects.Choice.HowValue.All => All(),
                Shared.MessagePackObjects.Choice.HowValue.Choose => await Choose(),
                Shared.MessagePackObjects.Choice.HowValue.Random => Random(),
                _ => throw new Exception($"how={choice.How}")
            };

            return choiceResult;
        }

        public async ValueTask ModifyPlayer(ModifyPlayerContext modifyPlayerContext, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var (exists, player) = this.playerRepository.TryGet(modifyPlayerContext.PlayerId);
            if (!exists)
            {
                return;
            }

            await player.Modify(modifyPlayerContext.PlayerModifier, effectOwnerCard, effectEventArgs);

            foreach (var p in this.playerRepository.AllPlayers)
            {
                this.EventListener?.OnModifyPlayer?.Invoke(p.Id,
                    this.CreateGameContext(p.Id),
                    new ModifyPlayerNotifyMessage(modifyPlayerContext.PlayerId));
            }
        }

        public GameMasterStatusCode AddEffect(Card card, IEnumerable<CardEffect> effectToAdd)
        {
            card.Effects.AddRange(effectToAdd);

            return GameMasterStatusCode.OK;
        }

        public async ValueTask ModifyCounter(PlayerId targetPlayerId, string counterName, int numCounters)
        {
            this.logger.LogInformation($"start set counter.");

            var (exists, targetPlayer) = this.playerRepository.TryGet(targetPlayerId);
            if (!exists)
            {
                this.logger.LogError($"player not exists. player id={targetPlayerId}");
                return;
            }

            targetPlayer.ModifyCounter(counterName, numCounters);

            this.logger.LogInformation($"set counter. player={targetPlayer.Name}, counter={counterName}:{numCounters}");

            foreach (var p in this.playerRepository.AllPlayers)
            {
                this.EventListener?.OnModityCounter?.Invoke(
                    p.Id,
                    this.CreateGameContext(p.Id),
                    new ModifyCounterNotifyMessage(
                        counterName, numCounters, TargetPlayerId: targetPlayerId));
            }

            var addOrRemove = numCounters > 0
                ? EffectTimingModifyCounterOnCardEvent.OperatorValue.Add
                : EffectTimingModifyCounterOnCardEvent.OperatorValue.Remove;

            var abs = Math.Abs(numCounters);
            for (var i = 0; i < abs; i++)
            {
                await this.effectManager.DoEffect(new EffectEventArgs(
                    GameEvent.OnModifyCounter, this,
                    SourcePlayer: targetPlayer,
                    ModifyCounterContext: new(
                        counterName,
                        addOrRemove
                    )));
            }
        }

        public async ValueTask ModifyCounter(Card targetCard, string counterName, int numCounters)
        {
            this.logger.LogInformation($"start set counter.");

            targetCard.ModifyCounter(counterName, numCounters);

            this.logger.LogInformation($"set counter. card={targetCard}, counter={counterName}:{numCounters}");

            foreach (var p in this.playerRepository.AllPlayers)
            {
                // 非公開領域なら、カードの持ち主以外にはカードを知らせない
                var targetCardId = !targetCard.Zone.IsPublic() && p.Id != targetCard.OwnerId
                    ? default
                    : targetCard.Id;

                this.EventListener?.OnModityCounter?.Invoke(
                    p.Id,
                    this.CreateGameContext(p.Id),
                    new ModifyCounterNotifyMessage(
                        counterName, numCounters,
                        TargetCardId: targetCardId));
            }

            var addOrRemove = numCounters > 0
                ? EffectTimingModifyCounterOnCardEvent.OperatorValue.Add
                : EffectTimingModifyCounterOnCardEvent.OperatorValue.Remove;

            var abs = Math.Abs(numCounters);
            for (var i = 0; i < abs; i++)
            {
                await this.effectManager.DoEffect(new EffectEventArgs(
                    GameEvent.OnModifyCounter, this,
                    SourceCard: targetCard,
                    ModifyCounterContext: new(
                        counterName,
                        addOrRemove
                    )));
            }
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

        public (bool, GameMasterStatusCode) Win(PlayerId playerId)
        {
            var (exists, _) = this.playerRepository.TryGet(playerId);
            if (!exists)
            {
                return (false, GameMasterStatusCode.PlayerNotExists);
            }

            // 対戦相手のHPをゼロにする
            foreach (var p in this.playerRepository.Opponents(playerId))
            {
                p.Damage(p.MaxHp);
            }

            foreach (var p in this.playerRepository.AllPlayers)
            {
                this.EventListener?.OnEndGame?.Invoke(p.Id, this.CreateGameContext(p.Id));
            }

            return (true, GameMasterStatusCode.OK);
        }
    }
}
