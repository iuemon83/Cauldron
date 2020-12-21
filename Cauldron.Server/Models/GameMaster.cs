using Cauldron.Server.Models.Effect;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
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

        public static bool CanAttack(Card attackCard, Card guardCard, GameContext environment)
        {
            var guardPlayer = environment.You.PublicPlayerInfo.Id == guardCard.OwnerId
                ? environment.You.PublicPlayerInfo
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

        public readonly ConcurrentDictionary<Guid, Player> PlayersById = new();

        public Player ActivePlayer { get; set; }

        public Player NextPlayer { get; set; }

        public IEnumerable<Player> NonActivePlayers => this.PlayersById.Values.Where(player => player.Id != this.ActivePlayer.Id);

        public bool GameOver => this.PlayersById.Values.Any(player => player.CurrentHp <= 0);

        public ConcurrentDictionary<Guid, int> PlayerTurnCountById { get; set; } = new();

        public int AllTurnCount => this.PlayerTurnCountById.Sum(x => x.Value);

        public IEnumerable<CardDef> CardPool => this.cardFactory.CardPool;

        private readonly Func<Guid, ChoiceResult, int, ChoiceResult> AskCardAction;

        private readonly EffectManager effectManager;

        private readonly Action<Guid, Grpc.Api.ReadyGameReply> notifyClient;

        public GameMaster(RuleBook ruleBook, CardFactory cardFactory, ILogger logger,
            Func<Guid, ChoiceResult, int, ChoiceResult> askCardAction, Action<Guid, Grpc.Api.ReadyGameReply> notifyClient)
        {
            this.RuleBook = ruleBook;
            this.cardFactory = cardFactory;
            this.logger = logger;
            this.AskCardAction = askCardAction;
            this.effectManager = new EffectManager(logger);
            this.notifyClient = notifyClient;
        }

        public (GameMasterStatusCode, Guid) CreateNewPlayer(string name, IEnumerable<Guid> deckCardDefIdList)
        {
            var newId = Guid.NewGuid();
            var deckCards = deckCardDefIdList.Select(id => this.cardFactory.CreateNew(id)).ToArray();

            // 提示されたデッキにトークンが含まれていてはいけない
            if (deckCards.Any(c => c.IsToken))
            {
                return (GameMasterStatusCode.IsIncludedTokensInDeck, default);
            }

            var player = new Player(newId, name, this.RuleBook, deckCards);
            this.PlayersById.TryAdd(newId, player);

            this.PlayerTurnCountById.TryAdd(newId, 0);

            return (GameMasterStatusCode.OK, newId);
        }


        public void Start(Guid firstPlayerId)
        {
            this.ActivePlayer = this.PlayersById[firstPlayerId];
            this.NextPlayer = this.GetOpponent(this.ActivePlayer.Id);

            foreach (var player in this.PlayersById.Values)
            {
                this.Draw(player.Id, this.RuleBook.InitialNumHands);
            }

            this.notifyClient(this.ActivePlayer.Id, new Grpc.Api.ReadyGameReply()
            {
                Code = Grpc.Api.ReadyGameReply.Types.Code.StartTurn,
            });
        }

        public Player GetWinner()
        {
            // 引き分けならターンのプレイヤーの負け
            var alives = this.PlayersById.Values.Where(p => p.CurrentHp > 0).ToArray();
            return alives.Length == 0
                ? this.PlayersById.Values.First(p => p.Id != this.ActivePlayer.Id)
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
            this.logger.LogInformation($"破壊：{cardToDestroy}({player.Name})");

            this.MoveCard(cardToDestroy.Id, new(ZoneType.YouField, ZoneType.YouCemetery));

            this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnDestroy, this, SourceCard: cardToDestroy));
        }

        public void Buff(Card creatureCard, int powerBuff, int toughnessBuff)
        {
            this.logger.LogInformation($"修整：{creatureCard}({this.PlayersById[creatureCard.OwnerId].Name})-[{powerBuff},{toughnessBuff}]");

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
            if (player.CurrentMp < card.Cost)
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

            this.logger.LogInformation($"手札に追加: {addCard.Name}({player.Name})");
            var handsLog = string.Join(",", player.Hands.AllCards.Select(c => c.Name));
            this.logger.LogInformation($"手札: {handsLog}({player.Name})");
        }

        public void RemoveHand(Player player, Card removeCard)
        {
            player.Hands.Remove(removeCard);

            this.logger.LogInformation($"手札を捨てる: {removeCard.Name}({player.Name})");
            var handsLog = string.Join(",", player.Hands.AllCards.Select(c => c.Name));
            this.logger.LogInformation($"手札: {handsLog}({player.Name})");
        }

        public void MoveCard(Guid cardId, MoveCardContext moveCardContext)
        {
            var card = this.cardFactory.GetById(cardId);

            switch (moveCardContext.From)
            {
                case ZoneType.OpponentDeck:
                    this.GetOpponent(card.OwnerId).Deck.Remove(card);
                    break;

                case ZoneType.OpponentField:
                    this.GetOpponent(card.OwnerId).Field.Remove(card);
                    break;

                case ZoneType.OpponentHand:
                    this.GetOpponent(card.OwnerId).Hands.Remove(card);
                    break;

                case ZoneType.YouDeck:
                    this.PlayersById[card.OwnerId].Deck.Remove(card);
                    break;

                case ZoneType.YouField:
                    this.PlayersById[card.OwnerId].Field.Remove(card);
                    break;

                case ZoneType.YouHand:
                    this.PlayersById[card.OwnerId].Hands.Remove(card);
                    break;

                default:
                    throw new InvalidOperationException();
            }

            switch (moveCardContext.To)
            {
                case ZoneType.OpponentDeck:
                    //this.GetOpponent(card.OwnerId).Deck.Add(card);
                    break;

                case ZoneType.OpponentField:
                    this.GetOpponent(card.OwnerId).Field.Add(card);
                    break;

                case ZoneType.OpponentHand:
                    this.GetOpponent(card.OwnerId).Hands.Add(card);
                    break;

                case ZoneType.OpponentCemetery:
                    this.GetOpponent(card.OwnerId).Cemetery.Add(card);
                    break;

                case ZoneType.YouDeck:
                    //this.PlayersById[card.OwnerId].Deck.Add(card);
                    break;

                case ZoneType.YouField:
                    this.PlayersById[card.OwnerId].Field.Add(card);
                    break;

                case ZoneType.YouHand:
                    this.PlayersById[card.OwnerId].Hands.Add(card);
                    break;

                case ZoneType.YouCemetery:
                    this.PlayersById[card.OwnerId].Cemetery.Add(card);
                    break;

                default:
                    throw new InvalidOperationException();
            }

            card.Zone = moveCardContext.To;

            var publicZones = new[] { ZoneType.YouField, ZoneType.OpponentField, ZoneType.YouCemetery, ZoneType.OpponentCemetery };
            var isPublic = publicZones.Contains(moveCardContext.From)
                || publicZones.Contains(moveCardContext.To);

            // カードの持ち主には無条件に通知する
            //this.notifyClient(card.OwnerId, new ClientNotify(MoveCardNotify: new MoveCardNotify(card.Id, moveCardContext.To)));
            this.notifyClient(card.OwnerId, new Grpc.Api.ReadyGameReply()
            {
                Code = Grpc.Api.ReadyGameReply.Types.Code.MoveCard,
                MoveCardNotify = new Grpc.Api.MoveCardNotify()
                {
                    CardId = card.Id.ToString(),
                    ToZone = moveCardContext.To.ToString()
                }
            });

            // カードの持ち主以外への通知は
            // 移動元か移動後どちらかの領域が公開領域の場合のみ
            if (isPublic)
            {
                //this.notifyClient(this.GetOpponent(card.OwnerId).Id, new ClientNotify(MoveCardNotify: new MoveCardNotify(card.Id, moveCardContext.To)));
                this.notifyClient(this.GetOpponent(card.OwnerId).Id, new Grpc.Api.ReadyGameReply()
                {
                    Code = Grpc.Api.ReadyGameReply.Types.Code.MoveCard,
                    MoveCardNotify = new Grpc.Api.MoveCardNotify()
                    {
                        CardId = card.Id.ToString(),
                        ToZone = moveCardContext.To.ToString()
                    }
                });
            }

            // カードの移動イベント
            this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnMoveCard, this, SourceCard: card, MoveCardContext: moveCardContext));
        }

        public GameContext CreateGameContext(Guid playerId)
        {
            return new GameContext()
            {
                GameOver = this.GameOver,
                WinnerPlayerId = this.GetWinner()?.Id ?? Guid.Empty,
                You = new PrivatePlayerInfo(this.PlayersById[playerId]),
                Opponent = new PublicPlayerInfo(this.GetOpponent(playerId)),
                RuleBook = this.RuleBook
            };
        }

        public GameMasterStatusCode Resolve(Guid playerId, Func<Guid, GameMasterStatusCode> action)
        {
            if (this.GameOver)
            {
                return GameMasterStatusCode.GameOver;
            }

            if (playerId != this.ActivePlayer.Id)
            {
                return GameMasterStatusCode.NotActivePlayer;
            }

            return action(playerId);
        }

        public (bool IsSucceeded, string errorMessage) StartTurn(Guid playerId)
        {
            if (this.GameOver)
            {
                return (false, "すでにゲームが終了しています。");
            }

            if (playerId != this.ActivePlayer.Id)
            {
                return (false, "このプレイヤーのターンではありません。");
            }

            this.PlayerTurnCountById[this.ActivePlayer.Id]++;

            this.ActivePlayer.AddMaxMp(this.RuleBook.MpByStep);
            this.ActivePlayer.FullMp();
            foreach (var card in this.ActivePlayer.Field.AllCards)
            {
                card.TurnCountInField++;
            }

            this.logger.LogInformation(
                $"ターン開始: {this.ActivePlayer.Name}-[HP:{this.ActivePlayer.CurrentHp}/{this.ActivePlayer.MaxHp}][MP:{this.ActivePlayer.MaxMp}][ターン:{this.PlayerTurnCountById[this.ActivePlayer.Id]}({this.AllTurnCount})]----------------------------");
            this.logger.LogInformation(
                $"フィールド: {string.Join(",", this.ActivePlayer.Field.AllCards.Select(c => c.Name))}");

            this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnStartTurn, this));

            this.Draw(this.ActivePlayer.Id, 1);

            return (true, "");
        }

        public (bool IsSucceeded, string errorMessage) EndTurn(Guid playerId)
        {
            if (this.GameOver)
            {
                return (false, "すでにゲームが終了しています。");
            }

            if (playerId != this.ActivePlayer.Id)
            {
                return (false, "このプレイヤーのターンではありません。");
            }

            var endTurnPlayer = this.ActivePlayer;

            this.logger.LogInformation($"ターンエンド：{endTurnPlayer.Name}");

            this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnEndTurn, this));

            this.ActivePlayer = this.NextPlayer;
            this.NextPlayer = this.GetOpponent(this.ActivePlayer.Id);
            //this.notifyClient(this.ActivePlayer.Id, new ClientNotify(StartTurnNotify: new StartTurnNotify()));
            this.notifyClient(this.ActivePlayer.Id, new Grpc.Api.ReadyGameReply()
            {
                Code = Grpc.Api.ReadyGameReply.Types.Code.StartTurn,
            });

            return (true, "");
        }

        public (bool IsSucceeded, string errorMessage) Draw(Guid playerId, int numCards)
        {
            if (this.PlayersById.TryGetValue(playerId, out var player))
            {
                foreach (var _ in Enumerable.Range(0, numCards))
                {
                    var drawCard = player.Draw();

                    this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnDraw, this, SourceCard: drawCard));
                    this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnMoveCard, this, SourceCard: drawCard,
                        MoveCardContext: new(ZoneType.Deck, ZoneType.Hand)));
                }

                return (true, "");
            }
            else
            {
                return (false, "指定されたID を持つプレイヤーがいません");
            }
        }

        public (bool IsSucceeded, string errorMessage) Discard(Guid playerId, IEnumerable<Guid> handCardId)
        {
            if (this.PlayersById.TryGetValue(playerId, out var player))
            {
                //TODO 本当に手札にあるのか確認する必要あり
                var handCards = handCardId.Select(cid => this.cardFactory.GetById(cid));
                foreach (var card in handCards)
                {
                    this.MoveCard(card.Id, new(ZoneType.YouHand, ZoneType.YouCemetery));
                }

                return (true, "");
            }
            else
            {
                return (false, "指定されたID を持つプレイヤーがいません");
            }
        }

        /// <summary>
        /// 手札のカードをプレイします
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="handCardId"></param>
        /// <returns></returns>
        public GameMasterStatusCode PlayFromHand(Guid playerId, Guid handCardId)
        {
            if (this.GameOver)
            {
                return GameMasterStatusCode.GameOver;
            }

            if (playerId != this.ActivePlayer.Id)
            {
                return GameMasterStatusCode.NotActivePlayer;
            }

            var player = this.PlayersById[playerId];
            var (success, playingCard) = player.Hands.TryGetById(handCardId);

            if (!success)
            {
                return GameMasterStatusCode.CardNotExists;
            }

            this.logger.LogInformation($"プレイ：{playingCard}({player.Name})");

            // プレイ不能
            if (!this.IsPlayable(player, playingCard))
            {
                return GameMasterStatusCode.CardCantPlay;
            }

            this.ActivePlayer.UseMp(playingCard.Cost);

            // イベント発行
            this.MoveCard(handCardId, new MoveCardContext(ZoneType.YouHand, ZoneType.YouField));

            this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnPlay, this, SourceCard: playingCard));

            switch (playingCard.Type)
            {
                case CardType.Creature:
                case CardType.Artifact:

                    //player.Field.Add(playingCard);
                    break;

                case CardType.Sorcery:
                    this.MoveCard(handCardId, new MoveCardContext(ZoneType.YouField, ZoneType.YouCemetery));

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

            if (playerId != this.ActivePlayer.Id)
            {
                return (false, "このプレイヤーのターンではありません。");
            }

            var player = this.PlayersById[playerId];
            var playingCard = this.cardFactory.GetById(cardId);

            this.logger.LogInformation($"特殊プレイ：{playingCard}({player.Name})");

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

            if (playerId != this.ActivePlayer.Id)
            {
                return (false, "このプレイヤーのターンではありません。");
            }

            var player = this.PlayersById[playerId];
            var card = player.Field.GetById(cardId);
            var damagePlayer = this.PlayersById[damagePlayerId];

            var eventArgs = new EffectEventArgs(
                GameEvent.OnBattleBefore,
                this,
                BattleContext: new BattleContext(
                    AttackCard: card,
                    GuardPlayer: damagePlayer,
                    Value: card.Power
                    )
                );

            this.effectManager.DoEffect(eventArgs);

            this.logger.LogInformation($"アタック（プレイヤー）：{card}({player.Name}) > {damagePlayer.Name}");

            if (!CanAttack(eventArgs.BattleContext.AttackCard, eventArgs.BattleContext.GuardPlayer))
            {
                return (false, "攻撃不能");
            }

            var damageContext = new DamageContext(
                DamageSourceCard: eventArgs.BattleContext.AttackCard,
                GuardPlayer: eventArgs.BattleContext.GuardPlayer,
                Value: eventArgs.BattleContext.AttackCard.Power
            );
            this.HitPlayer(damageContext);

            var eventArgs2 = eventArgs with { GameEvent = GameEvent.OnBattle };

            this.effectManager.DoEffect(eventArgs2);

            return (true, "");
        }

        public void HitPlayer(DamageContext damageContext)
        {
            var eventArgs = new EffectEventArgs(
                GameEvent.OnDamageBefore,
                this,
                DamageContext: damageContext
            );
            var newEventArgs = this.effectManager.DoEffect(eventArgs);

            this.logger.LogInformation($"ダメージ：{newEventArgs.DamageContext.Value} > {newEventArgs.DamageContext.GuardPlayer.Name}");

            newEventArgs.DamageContext.GuardPlayer.Damage(newEventArgs.DamageContext.Value);

            // 各プレイヤーに通知
            foreach (var playerId in this.PlayersById.Keys)
            {
                this.notifyClient(playerId, new Grpc.Api.ReadyGameReply()
                {
                    Code = Grpc.Api.ReadyGameReply.Types.Code.Damage,
                    DamageNotify = new Grpc.Api.DamageNotify()
                    {
                        Reason = Grpc.Api.DamageNotify.Types.Reason.Attack,
                        Damage = newEventArgs.DamageContext.Value,
                        SourceCardId = newEventArgs.DamageContext.DamageSourceCard.Id.ToString(),
                        GuardPlayerId = newEventArgs.DamageContext.GuardPlayer.Id.ToString()
                    }
                });
            }

            this.effectManager.DoEffect(newEventArgs with { GameEvent = GameEvent.OnDamage });
        }

        public (bool IsSucceeded, string errorMessage) AttackToCreature(Guid playerId, Guid attackCardId, Guid guardCardId)
        {
            if (this.GameOver)
            {
                return (false, "すでにゲームが終了しています。");
            }

            if (playerId != this.ActivePlayer.Id)
            {
                return (false, "このプレイヤーのターンではありません。");

            }

            var attackCard = this.cardFactory.GetById(attackCardId);
            var guardCard = this.cardFactory.GetById(guardCardId);

            var attackPlayer = this.PlayersById[attackCard.OwnerId];
            var guardPlayer = this.PlayersById[guardCard.OwnerId];

            var eventArgs = new EffectEventArgs(
                GameEvent.OnBattleBefore,
                this,
                BattleContext: new BattleContext(
                    AttackCard: attackCard,
                    GuardCard: guardCard,
                    Value: attackCard.Power
                ));

            this.effectManager.DoEffect(eventArgs);

            this.logger.LogInformation($"アタック（クリーチャー）：{eventArgs.BattleContext.AttackCard}({attackPlayer.Name}) > {eventArgs.BattleContext.GuardCard}({guardPlayer.Name})");

            if (!CanAttack(eventArgs.BattleContext.AttackCard, eventArgs.BattleContext.GuardCard, this.CreateGameContext(playerId)))
            {
                return (false, "攻撃不能");
            }

            // 攻撃するとステルスを失う
            if (eventArgs.BattleContext.AttackCard.Abilities.Contains(CreatureAbility.Stealth))
            {
                eventArgs.BattleContext.AttackCard.Abilities.Remove(CreatureAbility.Stealth);
            }

            // お互いにダメージを受ける
            var damageContext = new DamageContext(
                DamageSourceCard: eventArgs.BattleContext.AttackCard,
                GuardCard: eventArgs.BattleContext.GuardCard,
                Value: eventArgs.BattleContext.AttackCard.Power
            );
            this.HitCreature(damageContext);

            var damageContext2 = new DamageContext(
                DamageSourceCard: eventArgs.BattleContext.GuardCard,
                GuardCard: eventArgs.BattleContext.AttackCard,
                Value: eventArgs.BattleContext.GuardCard.Power
            );
            this.HitCreature(damageContext2);

            var eventArgs2 = eventArgs with { GameEvent = GameEvent.OnBattle };
            this.effectManager.DoEffect(eventArgs2);

            return (true, "");
        }

        public void HitCreature(DamageContext damageContext)
        {
            var eventArgs = new EffectEventArgs(GameEvent.OnDamageBefore, this, DamageContext: damageContext);
            var newEventArgs = this.effectManager.DoEffect(eventArgs);

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
                this.notifyClient(playerId, new Grpc.Api.ReadyGameReply()
                {
                    Code = Grpc.Api.ReadyGameReply.Types.Code.Damage,
                    DamageNotify = new Grpc.Api.DamageNotify()
                    {
                        Reason = Grpc.Api.DamageNotify.Types.Reason.Attack,
                        Damage = newEventArgs.DamageContext.Value,
                        SourceCardId = newEventArgs.DamageContext.DamageSourceCard.Id.ToString(),
                        GuardCardId = newEventArgs.DamageContext.GuardCard.Id.ToString()
                    }
                });
            }

            this.effectManager.DoEffect(newEventArgs with { GameEvent = GameEvent.OnDamage });

            if (newEventArgs.DamageContext.GuardCard.Toughness <= 0)
            {
                this.DestroyCard(newEventArgs.DamageContext.GuardCard);
            }
        }

        public IReadOnlyList<Player> ChoiceCandidatePlayers(Card effectOwnerCard, Choice choice, EffectEventArgs eventArgs)
        {
            return this.PlayersById.Values
                .Where(p => choice.PlayerCondition.IsMatch(effectOwnerCard, p, eventArgs))
                .ToArray();
        }

        public IReadOnlyList<Card> ChoiceCandidateCards(Card effectOwnerCard, Choice choice, EffectEventArgs eventArgs)
        {
            return choice.CardCondition?.ZoneCondition switch
            {
                ZoneType.All => this.cardFactory.GetAllCards
                    .Where(c => choice.CardCondition.IsMatch(effectOwnerCard, c, eventArgs))
                    .ToArray(),
                ZoneType.Field => this.PlayersById.Values.SelectMany(p => p.Field.AllCards)
                    .Where(c => choice.CardCondition.IsMatch(effectOwnerCard, c, eventArgs))
                    .ToArray(),
                ZoneType.YouField => this.PlayersById[effectOwnerCard.OwnerId].Field.AllCards
                    .Where(c => choice.CardCondition.IsMatch(effectOwnerCard, c, eventArgs))
                    .ToArray(),
                ZoneType.OpponentField => this.GetOpponent(effectOwnerCard.OwnerId).Field.AllCards
                    .Where(c => choice.CardCondition.IsMatch(effectOwnerCard, c, eventArgs))
                    .ToArray(),
                _ => Array.Empty<Card>(),
            };
        }

        public IReadOnlyList<CardDef> ChoiceCandidateCardDefs(Choice choice)
        {
            return choice.NewCardCondition?.ZoneCondition switch
            {
                ZoneType.All => this.cardFactory.CardPool
                    .Where(cdef => choice.NewCardCondition.IsMatch(cdef))
                    .SelectMany(cdef => Enumerable.Repeat(cdef, choice.NumPicks))
                    .ToArray(),
                ZoneType.CardPool => this.cardFactory.CardPool
                    .Where(cdef => choice.NewCardCondition.IsMatch(cdef))
                    .SelectMany(cdef => Enumerable.Repeat(cdef, choice.NumPicks))
                    .ToArray(),
                _ => Array.Empty<CardDef>(),
            };
        }

        public ChoiceResult ChoiceCandidates(Card effectOwnerCard, Choice choice, EffectEventArgs eventArgs)
        {
            return new ChoiceResult()
            {
                PlayerList = choice.PlayerCondition == null ? Array.Empty<Player>() : this.ChoiceCandidatePlayers(effectOwnerCard, choice, eventArgs),
                CardList = choice.CardCondition == null ? Array.Empty<Card>() : this.ChoiceCandidateCards(effectOwnerCard, choice, eventArgs),
                CardDefList = choice.NewCardCondition == null ? Array.Empty<CardDef>() : this.ChoiceCandidateCardDefs(choice),
            };
        }

        public ChoiceResult ChoiceCards(Card ownerCard, Choice choice, EffectEventArgs eventArgs)
        {
            var choiceCandidates = this.ChoiceCandidates(ownerCard, choice, eventArgs);

            switch (choice.How)
            {
                case Choice.ChoiceHow.All:
                    return choiceCandidates;

                case Choice.ChoiceHow.Choose:
                    return this.AskCard(ownerCard.OwnerId, choiceCandidates, choice.NumPicks);

                case Choice.ChoiceHow.Random:
                    var totalCount = choiceCandidates.PlayerList.Count + choiceCandidates.CardList.Count + choiceCandidates.CardDefList.Count;
                    var totalIndexList = Enumerable.Range(0, totalCount).ToArray();
                    var pickedIndexList = Program.RandomPick(totalIndexList, choice.NumPicks);

                    var randomPickedPlayerList = new List<Player>();
                    var randomPickedCardList = new List<Card>();
                    var randomPickedCardDefList = new List<CardDef>();
                    foreach (var pickedIndex in pickedIndexList)
                    {
                        if (pickedIndex < choiceCandidates.PlayerList.Count)
                        {
                            randomPickedPlayerList.Add(choiceCandidates.PlayerList[pickedIndex]);
                        }
                        else if (pickedIndex < choiceCandidates.PlayerList.Count + choiceCandidates.CardList.Count)
                        {
                            var cardIndex = pickedIndex - choiceCandidates.PlayerList.Count;
                            randomPickedCardList.Add(choiceCandidates.CardList[cardIndex]);
                        }
                        else
                        {
                            var cardIndex = pickedIndex - choiceCandidates.PlayerList.Count - choiceCandidates.CardList.Count;
                            randomPickedCardDefList.Add(choiceCandidates.CardDefList[cardIndex]);
                        }
                    }

                    return new ChoiceResult()
                    {
                        PlayerList = randomPickedPlayerList,
                        CardList = randomPickedCardList,
                        CardDefList = randomPickedCardDefList
                    };

                default:
                    throw new Exception($"how={choice.How}");
            }
        }

        public (bool, string) ModifyPlayer(ModifyPlayerContext modifyPlayerContext)
        {
            if (!this.PlayersById.TryGetValue(modifyPlayerContext.PlayerId, out var player))
            {
                return (false, "指定のプレイヤーが存在しません");
            }

            player.Modify(modifyPlayerContext.PlayerModifier);

            return (true, "");
        }
    }
}
