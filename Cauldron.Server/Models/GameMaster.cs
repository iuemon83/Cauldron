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

        public readonly ConcurrentDictionary<PlayerId, PlayerDef> PlayerDefsById = new();
        public readonly ConcurrentDictionary<PlayerId, Player> PlayersById = new();

        public Player ActivePlayer { get; set; }

        public Player NextPlayer { get; set; }

        public IEnumerable<Player> NonActivePlayers => this.PlayersById.Values.Where(player => player.Id != this.ActivePlayer.Id);

        public bool GameOver => this.PlayersById.Values.Any(player => player.CurrentHp <= 0);

        public ConcurrentDictionary<PlayerId, int> PlayerTurnCountById { get; set; } = new();

        public int AllTurnCount => this.PlayerTurnCountById.Sum(x => x.Value);

        public IEnumerable<CardDef> CardPool => this.cardFactory.CardPool;

        private readonly Func<PlayerId, ChoiceResult, int, ChoiceResult> AskCardAction;

        private readonly EffectManager effectManager;

        private readonly Action<PlayerId, Grpc.Api.ReadyGameReply> notifyClient;

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

        public GameMaster(GameMasterOptions options)
        {
            this.RuleBook = options.RuleBook;
            this.cardFactory = options.CardFactory;
            this.logger = options.Logger;
            this.AskCardAction = options.AskCardAction;
            this.notifyClient = options.NotifyClient;

            this.effectManager = new EffectManager(logger);
        }

        public (GameMasterStatusCode, PlayerId) CreateNewPlayer(string name, IEnumerable<CardDefId> deckCardDefIdList)
        {
            var newId = PlayerId.NewId();
            var deckCards = deckCardDefIdList.Select(id => this.cardFactory.CreateNew(id)).ToArray();

            // 提示されたデッキにトークンが含まれていてはいけない
            if (deckCards.Any(c => c.IsToken))
            {
                return (GameMasterStatusCode.IsIncludedTokensInDeck, default);
            }

            var playerDef = new PlayerDef(newId, name, deckCardDefIdList.ToArray());
            this.PlayerDefsById.TryAdd(newId, playerDef);

            return (GameMasterStatusCode.OK, newId);
        }

        public void Start(PlayerId firstPlayerId)
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
                    var deckCards = playerDef.DeckIdList.Select(id => this.cardFactory.CreateNew(id)).ToArray();
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
                    this.Draw(player.Id, this.RuleBook.InitialNumHands);
                }

                this.notifyClient(this.ActivePlayer.Id, new Grpc.Api.ReadyGameReply()
                {
                    Code = Grpc.Api.ReadyGameReply.Types.Code.StartTurn,
                });
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
        public void DestroyCard(Card cardToDestroy)
        {
            var player = this.PlayersById[cardToDestroy.OwnerId];
            this.logger.LogInformation($"破壊：{cardToDestroy}({player.Name})");

            this.MoveCard(cardToDestroy.Id, new(new(cardToDestroy.OwnerId, ZoneName.Field), new(cardToDestroy.OwnerId, ZoneName.Cemetery)));

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

            // カードの持ち主には無条件に通知する
            this.notifyClient(creatureCard.OwnerId, new Grpc.Api.ReadyGameReply()
            {
                Code = Grpc.Api.ReadyGameReply.Types.Code.ModifyCard,
                ModifyCardNotify = new Grpc.Api.ModifyCardNotify()
                {
                    CardId = creatureCard.Id.ToString(),
                }
            });

            if (creatureCard.Toughness <= 0)
            {
                this.DestroyCard(creatureCard);
            }
        }

        public Card GenerateNewCard(CardDefId cardDefId, PlayerId ownerId)
        {
            var card = this.cardFactory.CreateNew(cardDefId);
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

        public void MoveCard(CardId cardId, MoveCardContext moveCardContext)
        {
            var card = this.cardFactory.GetById(cardId);

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
            this.notifyClient(card.OwnerId, new Grpc.Api.ReadyGameReply()
            {
                Code = Grpc.Api.ReadyGameReply.Types.Code.MoveCard,
                MoveCardNotify = new Grpc.Api.MoveCardNotify()
                {
                    CardId = card.Id.ToString(),
                    ToZone = new Grpc.Api.Zone()
                    {
                        PlayerId = moveCardContext.To.PlayerId.ToString(),
                        ZoneName = moveCardContext.To.ZoneName.ToString(),
                    }
                }
            });

            var isPublic = moveCardContext.From.IsPublic
                || moveCardContext.To.IsPublic;

            // カードの持ち主以外への通知は
            // 移動元か移動後どちらかの領域が公開領域の場合のみ
            this.notifyClient(this.GetOpponent(card.OwnerId).Id, new Grpc.Api.ReadyGameReply()
            {
                Code = Grpc.Api.ReadyGameReply.Types.Code.MoveCard,
                MoveCardNotify = new Grpc.Api.MoveCardNotify()
                {
                    CardId = isPublic ? card.Id.ToString() : "",
                    ToZone = new Grpc.Api.Zone()
                    {
                        PlayerId = moveCardContext.To.PlayerId.ToString(),
                        ZoneName = moveCardContext.To.ZoneName.ToString(),
                    }
                }
            });

            // カードの移動イベント
            this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnMoveCard, this, SourceCard: card, MoveCardContext: moveCardContext));
        }

        public GameContext CreateGameContext(PlayerId playerId)
        {
            return new GameContext()
            {
                GameOver = this.GameOver,
                WinnerPlayerId = this.GetWinner()?.Id ?? default,
                You = new PrivatePlayerInfo(this.PlayersById[playerId]),
                Opponent = new PublicPlayerInfo(this.GetOpponent(playerId)),
                RuleBook = this.RuleBook
            };
        }

        public GameMasterStatusCode Draw(PlayerId playerId, int numCards)
        {
            if (this.PlayersById.TryGetValue(playerId, out var player))
            {
                foreach (var _ in Enumerable.Range(0, numCards))
                {
                    var drawCard = player.Draw();

                    this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnDraw, this, SourceCard: drawCard));
                    this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnMoveCard, this, SourceCard: drawCard,
                        MoveCardContext: new(new(playerId, ZoneName.Deck), new(playerId, ZoneName.Hand))));
                }

                return GameMasterStatusCode.OK;
            }
            else
            {
                return GameMasterStatusCode.PlayerNotExists;
            }
        }

        public GameMasterStatusCode Discard(PlayerId playerId, IEnumerable<CardId> handCardId)
        {
            if (this.PlayersById.TryGetValue(playerId, out var player))
            {
                //TODO 本当に手札にあるのか確認する必要あり
                var handCards = handCardId.Select(cid => this.cardFactory.GetById(cid));
                foreach (var card in handCards)
                {
                    this.MoveCard(card.Id, new(new(playerId, ZoneName.Hand), new(playerId, ZoneName.Cemetery)));
                }

                return GameMasterStatusCode.OK;
            }
            else
            {
                return GameMasterStatusCode.PlayerNotExists;
            }
        }

        /// <summary>
        /// 新規に生成されるカードをプレイ（効果で召喚とか）
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public GameMasterStatusCode PlayDirect(PlayerId playerId, CardId cardId)
        {
            var player = this.PlayersById[playerId];
            var playingCard = this.cardFactory.GetById(cardId);

            this.logger.LogInformation($"特殊プレイ：{playingCard}({player.Name})");

            // プレイ不能
            if (!this.IsPlayableDirect(player, playingCard))
            {
                return GameMasterStatusCode.CardCantPlay;
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

            return GameMasterStatusCode.OK;
        }

        public GameMasterStatusCode StartTurn()
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

            this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnStartTurn, this));

            this.Draw(this.ActivePlayer.Id, 1);

            return GameMasterStatusCode.OK;
        }

        public GameMasterStatusCode EndTurn()
        {
            var endTurnPlayer = this.ActivePlayer;

            this.logger.LogInformation($"ターンエンド：{endTurnPlayer.Name}");

            this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnEndTurn, this));

            this.ActivePlayer = this.NextPlayer;
            this.NextPlayer = this.GetOpponent(this.ActivePlayer.Id);
            this.notifyClient(this.ActivePlayer.Id, new Grpc.Api.ReadyGameReply()
            {
                Code = Grpc.Api.ReadyGameReply.Types.Code.StartTurn,
            });

            return GameMasterStatusCode.OK;
        }

        /// <summary>
        /// 手札のカードをプレイします
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="handCardId"></param>
        /// <returns></returns>
        public GameMasterStatusCode PlayFromHand(PlayerId playerId, CardId handCardId)
        {
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

            this.MoveCard(handCardId, new(new(playerId, ZoneName.Hand), new(playerId, ZoneName.Field)));

            this.effectManager.DoEffect(new EffectEventArgs(GameEvent.OnPlay, this, SourceCard: playingCard));

            switch (playingCard.Type)
            {
                case CardType.Creature:
                case CardType.Artifact:

                    //player.Field.Add(playingCard);
                    break;

                case CardType.Sorcery:
                    this.MoveCard(handCardId, new(new(playerId, ZoneName.Field), new(playerId, ZoneName.Cemetery)));

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
        public ChoiceResult AskCard(PlayerId playerId, ChoiceResult choiceCandidates, int choiceNum)
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
        /// プレイヤーに攻撃します。
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="attackCardId"></param>
        /// <param name="damagePlayerId"></param>
        /// <returns></returns>
        public GameMasterStatusCode AttackToPlayer(PlayerId playerId, CardId attackCardId, PlayerId damagePlayerId)
        {
            var player = this.PlayersById[playerId];
            var attackCard = player.Field.GetById(attackCardId);
            var damagePlayer = this.PlayersById[damagePlayerId];

            var eventArgs = new EffectEventArgs(
                GameEvent.OnBattleBefore,
                this,
                BattleContext: new BattleContext(
                    AttackCard: attackCard,
                    GuardPlayer: damagePlayer,
                    Value: attackCard.Power
                    )
                );

            this.effectManager.DoEffect(eventArgs);

            this.logger.LogInformation($"アタック（プレイヤー）：{attackCard}({player.Name}) > {damagePlayer.Name}");

            if (!CanAttack(eventArgs.BattleContext.AttackCard, eventArgs.BattleContext.GuardPlayer))
            {
                return GameMasterStatusCode.CandAttack;
            }

            var damageContext = new DamageContext(
                DamageSourceCard: eventArgs.BattleContext.AttackCard,
                GuardPlayer: eventArgs.BattleContext.GuardPlayer,
                Value: eventArgs.BattleContext.AttackCard.Power
            );
            this.HitPlayer(damageContext);

            attackCard.NumAttacksInTurn++;

            var eventArgs2 = eventArgs with { GameEvent = GameEvent.OnBattle };

            this.effectManager.DoEffect(eventArgs2);

            return GameMasterStatusCode.OK;
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

        public GameMasterStatusCode AttackToCreature(PlayerId playerId, CardId attackCardId, CardId guardCardId)
        {
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
                return GameMasterStatusCode.CandAttack;
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

            attackCard.NumAttacksInTurn++;

            var damageContext2 = new DamageContext(
                DamageSourceCard: eventArgs.BattleContext.GuardCard,
                GuardCard: eventArgs.BattleContext.AttackCard,
                Value: eventArgs.BattleContext.GuardCard.Power
            );
            this.HitCreature(damageContext2);

            var eventArgs2 = eventArgs with { GameEvent = GameEvent.OnBattle };
            this.effectManager.DoEffect(eventArgs2);

            return GameMasterStatusCode.OK;
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
            if (choice.CardCondition?.ZoneCondition == null)
            {
                return this.cardFactory.GetAllCards
                    .Where(c => choice.CardCondition.IsMatch(effectOwnerCard, c, eventArgs))
                    .ToArray();
            }

            return choice.CardCondition.ZoneCondition.ZoneTypes
                .SelectMany(zoneType => zoneType switch
                {
                    ZoneType.YouField => this.PlayersById[effectOwnerCard.OwnerId].Field.AllCards
                        .Where(c => choice.CardCondition.IsMatch(effectOwnerCard, c, eventArgs)),
                    ZoneType.OpponentField => this.GetOpponent(effectOwnerCard.OwnerId).Field.AllCards
                        .Where(c => choice.CardCondition.IsMatch(effectOwnerCard, c, eventArgs)),
                    _ => Array.Empty<Card>(),
                })
                .ToArray();
        }

        public IReadOnlyList<CardDef> ChoiceCandidateCardDefs(Choice choice, IReadOnlyList<Card> candidateCards)
        {
            if (choice.CardCondition?.ZoneCondition == null)
            {
                return Array.Empty<CardDef>();
            }

            return choice.CardCondition.ZoneCondition.ZoneTypes
                .SelectMany(zoneType => zoneType switch
                {
                    ZoneType.CardPool => this.cardFactory.CardPool
                        .Where(cdef => choice.CardCondition.IsMatch(cdef))
                        .SelectMany(cdef => Enumerable.Repeat(cdef, choice.NumPicks)),
                    _ => Array.Empty<CardDef>(),
                })
                .Concat(candidateCards.Select(c => this.cardFactory.CardPool.First(cd => cd.Id == c.CardDefId)))
                .ToArray();
        }

        public ChoiceResult ChoiceCandidates(Card effectOwnerCard, Choice choice, EffectEventArgs eventArgs)
        {
            var CardList = choice.CardCondition == null ? Array.Empty<Card>() : this.ChoiceCandidateCards(effectOwnerCard, choice, eventArgs);
            var CardDefList = choice.CardCondition == null ? Array.Empty<CardDef>() : this.ChoiceCandidateCardDefs(choice, CardList);

            return new ChoiceResult()
            {
                PlayerList = choice.PlayerCondition == null ? Array.Empty<Player>() : this.ChoiceCandidatePlayers(effectOwnerCard, choice, eventArgs),
                CardList = CardList,
                CardDefList = CardDefList,
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
                    var totalCount = choiceCandidates.PlayerList.Count + choiceCandidates.CardList.Count;// + choiceCandidates.CardDefList.Count;
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

        public void ModifyPlayer(ModifyPlayerContext modifyPlayerContext)
        {
            if (!this.PlayersById.TryGetValue(modifyPlayerContext.PlayerId, out var player))
            {
                return;
            }

            player.Modify(modifyPlayerContext.PlayerModifier);

            foreach (var playerId in this.PlayersById.Keys)
            {
                this.notifyClient(playerId, new Grpc.Api.ReadyGameReply()
                {
                    Code = Grpc.Api.ReadyGameReply.Types.Code.ModifyPlayer,
                    ModifyPlayerNotify = new Grpc.Api.ModifyPlayerNotify()
                    {
                        PlayerId = modifyPlayerContext.PlayerId.ToString()
                    }
                });
            }
        }

        public Zone ConvertZone(PlayerId playerId, ZoneType zoneType)
        {
            return zoneType switch
            {
                ZoneType.CardPool => new Zone(default, ZoneName.CardPool),
                ZoneType.YouField => new Zone(playerId, ZoneName.Field),
                ZoneType.OpponentField => new Zone(this.GetOpponent(playerId).Id, ZoneName.Field),
                ZoneType.YouHand => new Zone(playerId, ZoneName.Hand),
                ZoneType.OpponentHand => new Zone(this.GetOpponent(playerId).Id, ZoneName.Hand),
                ZoneType.YouDeck => new Zone(playerId, ZoneName.Deck),
                ZoneType.OpponentDeck => new Zone(this.GetOpponent(playerId).Id, ZoneName.Deck),
                ZoneType.YouCemetery => new Zone(playerId, ZoneName.Cemetery),
                ZoneType.OpponentCemetery => new Zone(this.GetOpponent(playerId).Id, ZoneName.Cemetery),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
