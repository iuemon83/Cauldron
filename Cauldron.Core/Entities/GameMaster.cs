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
        /// 指定したアビリティが有効になっている場合はtrue、そうでなければfalse
        /// </summary>
        /// <param name="card"></param>
        /// <param name="ability"></param>
        /// <returns></returns>
        public static bool EnableAbility(Card card, CreatureAbility ability)
        {
            static bool HasAbility(Card card, CreatureAbility ability)
                => card.Abilities.Contains(ability);

            if (HasAbility(card, CreatureAbility.Sealed))
            {
                return ability == CreatureAbility.Sealed;
            }

            return ability switch
            {
                CreatureAbility.Cover => HasAbility(card, ability) && !HasAbility(card, CreatureAbility.Stealth),
                _ => HasAbility(card, ability)
            };
        }

        /// <summary>
        /// 攻撃可能な状態か
        /// </summary>
        /// <param name="attackCard"></param>
        /// <returns></returns>
        public static bool CanAttack(Card attackCard)
        {
            return attackCard != null
                // クリーチャーでなければ攻撃できない
                && attackCard.Type == CardType.Creature
                // 召喚酔いでない
                && attackCard.NumTurnsToCanAttack <= attackCard.NumTurnsInField
                // 攻撃不能状態でない
                && !EnableAbility(attackCard, CreatureAbility.CantAttack)
                // 1ターン中に攻撃可能な回数を超えていない
                && attackCard.NumAttacksLimitInTurn > attackCard.NumAttacksInTurn
                ;
        }

        /// <summary>
        /// 指定したカードが、指定したプレイヤーに攻撃可能か
        /// </summary>
        /// <param name="attackCard"></param>
        /// <param name="guardPlayer"></param>
        /// <returns></returns>
        public static bool CanAttack(Card attackCard, Player guardPlayer)
        {
            var existsCover = guardPlayer.Field.AllCards
                .Any(c => EnableAbility(c, CreatureAbility.Cover));

            return
                // 攻撃可能なカード
                CanAttack(attackCard)
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

                return guardPlayer.Field.Any(c => EnableAbility(c, CreatureAbility.Cover));
            }

            return
                // 攻撃可能なカード
                CanAttack(attackCard)
                // 自分自信のカードには攻撃できない
                && attackCard.OwnerId != guardCard.OwnerId
                // クリーチャー以外には攻撃できない
                && guardCard.Type == CardType.Creature
                // ステルス状態は攻撃対象にならない
                && !EnableAbility(guardCard, CreatureAbility.Stealth)
                // 自分がカバー or 他にカバーがいない
                && (EnableAbility(guardCard, CreatureAbility.Cover)
                    || !ExistsOtherCover(guardCard, environment))
                ;
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

        public bool IsStarted { get; private set; }

        public Player GetWinner()
        {
            // 引き分けならターンのプレイヤーの負け
            var alives = this.playerRepository.Alives;
            return alives.Count == 0
                ? this.playerRepository.Opponents(this.ActivePlayer.Id)[0]
                : alives[0];
        }

        public Player GetOpponent(PlayerId playerId) => this.playerRepository.Opponents(playerId)[0];

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
            var deckCards = deckCardDefIdList.Select(id => this.cardRepository.CreateNew(id)).ToArray();

            if (deckCards.Any(c => c == null))
            {
                this.logger.LogError("includ invalid cards in deck");
                return false;
            }

            // 提示されたデッキにトークンが含まれていてはいけない
            if (deckCards.Any(c => c.IsToken))
            {
                this.logger.LogError("includ token cards in deck");
                return false;
            }

            var invalidNumCards = deckCards.Length < this.RuleBook.MinNumDeckCards
                || deckCards.Length > this.RuleBook.MaxNumDeckCards;

            if (invalidNumCards)
            {
                this.logger.LogError("invalid number of deck cards");
                return false;
            }

            return true;
        }

        public async ValueTask Start(PlayerId firstPlayerId)
        {
            if (this.IsStarted) return;

            this.IsStarted = true;

            try
            {
                if (!this.PlayerDefsById.ContainsKey(firstPlayerId))
                {
                    throw new InvalidOperationException($"player not exists. id={firstPlayerId}");
                }

                foreach (var playerDef in this.PlayerDefsById.Values)
                {
                    var deckCards = playerDef.DeckIdList.Select(id => this.cardRepository.CreateNew(id)).ToArray();

                    var player = this.playerRepository.CreateNew(playerDef, this.RuleBook, deckCards);

                    this.PlayerTurnCountById.TryAdd(player.Id, 0);

                    if (player.Id == firstPlayerId)
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
            var (exists, player) = this.playerRepository.TryGet(cardToDestroy.OwnerId);
            if (!exists)
            {
                throw new InvalidOperationException($"player not exists. id={cardToDestroy.OwnerId}");
            }

            this.logger.LogInformation($"破壊：{cardToDestroy}({player.Name})");

            await this.MoveCard(cardToDestroy.Id, new(new(cardToDestroy.OwnerId, ZoneName.Field), new(cardToDestroy.OwnerId, ZoneName.Cemetery)));

            await this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnDestroy, this, SourceCard: cardToDestroy));
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
                new ModifyCardNotifyMessage()
                {
                    CardId = card.Id,
                });

            if (card.Type == CardType.Creature && card.Toughness <= 0)
            {
                await this.DestroyCard(card);
            }
        }

        public async ValueTask<GameMasterStatusCode> Draw(PlayerId playerId, int numCards)
        {
            var (exists, player) = this.playerRepository.TryGet(playerId);
            if (!exists)
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

                foreach (var p in this.playerRepository.AllPlayers)
                {
                    this.EventListener?.OnMoveCard?.Invoke(p.Id,
                        this.CreateGameContext(p.Id),
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

            var (exists, player) = this.playerRepository.TryGet(zone.PlayerId);
            if (!exists)
            {
                throw new InvalidOperationException($"player not exists. id={zone.PlayerId}");
            }

            switch (zone.ZoneName)
            {
                case ZoneName.Cemetery:
                    player.Cemetery.Add(card);
                    break;

                case ZoneName.Deck:
                    //this.PlayersById[zone.playerid].Deck.Add(card);
                    break;

                case ZoneName.Field:
                    player.Field.Add(card);
                    break;

                case ZoneName.Hand:
                    player.Hands.Add(card);
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

            var (fromPlayerExists, fromPlayer) = this.playerRepository.TryGet(moveCardContext.From.PlayerId);
            if (!fromPlayerExists)
            {
                throw new InvalidOperationException($"player not exists. id={moveCardContext.From.PlayerId}");
            }

            var (toPlayerExists, toPlayer) = this.playerRepository.TryGet(moveCardContext.From.PlayerId);
            if (!toPlayerExists)
            {
                throw new InvalidOperationException($"player not exists. id={moveCardContext.From.PlayerId}");
            }

            switch (moveCardContext.From.ZoneName)
            {
                case ZoneName.Cemetery:
                    //this.PlayersById[moveCardContext.From.PlayerId].Cemetery.Remove(card);
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

                default:
                    throw new InvalidOperationException();
            }

            switch (moveCardContext.To.ZoneName)
            {
                case ZoneName.Cemetery:
                    toPlayer.Cemetery.Add(card);
                    break;

                case ZoneName.Deck:
                    //toPlayer.Deck.Add(card);
                    break;

                case ZoneName.Field:
                    toPlayer.Field.Add(card);
                    break;

                case ZoneName.Hand:
                    toPlayer.Hands.Add(card);
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
            var (exists, player) = this.playerRepository.TryGet(playerId);
            if (!exists)
            {
                throw new InvalidOperationException($"player not exists. id={playerId}");
            }

            return new GameContext()
            {
                GameOver = this.GameOver,
                WinnerPlayerId = this.GetWinner()?.Id ?? default,
                You = player.PrivatePlayerInfo,
                Opponent = this.GetOpponent(playerId).PublicPlayerInfo,
                RuleBook = this.RuleBook
            };
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
            foreach (var player in this.playerRepository.AllPlayers)
            {
                this.EventListener?.OnDamage?.Invoke(player.Id,
                    this.CreateGameContext(player.Id),
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

            // 攻撃するとステルスを失う
            if (EnableAbility(attackCard, CreatureAbility.Stealth))
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
                    new DamageNotifyMessage()
                    {
                        Reason = DamageNotifyMessage.ReasonCode.Attack,
                        Damage = newDamageContext.Value,
                        SourceCardId = newDamageContext.DamageSourceCard.Id,
                        GuardCardId = newDamageContext.GuardCard.Id
                    });
            }

            await this.effectManager.DoEffect(newEventArgs with { GameEvent = GameEvent.OnDamage });

            var isDead = (
                newDamageContext.Value > 0
                    && EnableAbility(newDamageContext.DamageSourceCard, CreatureAbility.Deadly)
                    )
                || newDamageContext.GuardCard.Toughness <= 0;

            if (isDead)
            {
                await this.DestroyCard(newDamageContext.GuardCard);
            }
        }

        public Player[] ChoiceCandidatePlayers(Card effectOwnerCard, Choice choice, EffectEventArgs eventArgs)
        {
            return this.playerRepository.AllPlayers
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
                return this.cardRepository.AllCards;
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
            var (exists, player) = this.playerRepository.TryGet(effectOwnerCard.OwnerId);

            var sourceByZone = zonPrettyNames
                .SelectMany(zoneType => zoneType switch
                {
                    ZonePrettyName.YouField => player.Field.AllCards,
                    ZonePrettyName.OpponentField => this.GetOpponent(effectOwnerCard.OwnerId).Field.AllCards,
                    ZonePrettyName.YouHand => player.Hands.AllCards,
                    ZonePrettyName.OpponentHand => this.GetOpponent(effectOwnerCard.OwnerId).Hands.AllCards,
                    ZonePrettyName.YouCemetery => player.Cemetery.AllCards,
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
