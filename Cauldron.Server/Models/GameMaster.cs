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

        public Player ActivePlayer { get; set; }

        public Player NextPlayer { get; set; }

        public bool GameOver => this.PlayersById.Values.Any(player => player.Hp <= 0);

        public Dictionary<Guid, int> PlayerTurnCountById { get; set; } = new Dictionary<Guid, int>();

        public int AllTurnCount => this.PlayerTurnCountById.Sum(x => x.Value);

        // イベント
        private readonly Dictionary<GameEvent, Subject<EffectEventArgs>> EffectListByEvent = new Dictionary<GameEvent, Subject<EffectEventArgs>>();

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

            foreach (var gameEvent in Enum.GetValues(typeof(GameEvent)).Cast<GameEvent>())
            {
                this.EffectListByEvent[gameEvent] = new Subject<EffectEventArgs>();
            }
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

            var player = new Player(newId, name, this.RuleBook.InitialPlayerHp, this.RuleBook, deckCards);
            this.PlayersById.Add(newId, player);

            this.PlayerTurnCountById.Add(newId, 0);

            return (GameMasterStatusCode.OK, newId);
        }

        public void Start(Guid firstPlayerId)
        {
            this.ActivePlayer = this.PlayersById[firstPlayerId];
            this.NextPlayer = this.GetOpponent(this.ActivePlayer.Id);

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

            player.Field.Destroy(cardToDestroy);

            // カードの破壊イベント
            this.EffectListByEvent[GameEvent.OnDestroy]
                .OnNext(new EffectEventArgs(GameEvent.OnDestroy, this, SourceCard: cardToDestroy));

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

        public GameEnvironment CreateEnvironment(Guid playerId)
        {
            return new GameEnvironment()
            {
                GameOver = this.GameOver,
                WinnerPlayerId = this.GetWinner()?.Id ?? Guid.Empty,
                You = playerId == this.ActivePlayer.Id
                    ? new PrivatePlayerInfo(this.ActivePlayer)
                    : new PrivatePlayerInfo(this.GetOpponent(this.ActivePlayer.Id)),

                Opponent = playerId == this.ActivePlayer.Id
                    ? new PublicPlayerInfo(this.GetOpponent(this.ActivePlayer.Id))
                    : new PublicPlayerInfo(this.ActivePlayer),
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
                $"ターン開始: {this.ActivePlayer.Name}-[HP:{this.ActivePlayer.Hp}/{this.ActivePlayer.MaxHp}][MP:{this.ActivePlayer.MaxMp}][ターン:{this.PlayerTurnCountById[this.ActivePlayer.Id]}({this.AllTurnCount})]----------------------------");
            this.logger.LogInformation(
                $"フィールド: {string.Join(",", this.ActivePlayer.Field.AllCards.Select(c => c.Name))}");

            this.EffectListByEvent[GameEvent.OnStartTurn]
                .OnNext(new EffectEventArgs(GameEvent.OnStartTurn, this));

            this.ActivePlayer.Draw();

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

            this.EffectListByEvent[GameEvent.OnEndTurn]
                .OnNext(new EffectEventArgs(GameEvent.OnEndTurn, this));

            this.ActivePlayer = this.NextPlayer;
            this.NextPlayer = this.GetOpponent(this.ActivePlayer.Id);

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

            if (playerId != this.ActivePlayer.Id)
            {
                return (false, "このプレイヤーのターンではありません。");
            }

            var player = this.PlayersById[playerId];
            var playingCard = player.Hands.GetById(handCardId);

            this.logger.LogInformation($"プレイ：{playingCard}({player.Name})");

            // プレイ不能
            if (!this.IsPlayable(player, playingCard))
            {
                return (false, "プレイ不能");
            }

            this.ActivePlayer.UseMp(playingCard.Cost);

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
                        l.Add(this.EffectListByEvent[GameEvent.OnStartTurn].Subscribe(args =>
                        {
                            if (effect.Execute(playingCard, args))
                            {
                                this.logger.LogInformation($"ターン開始時の効果: {playingCard.Name}({this.PlayersById[playingCard.OwnerId].Name})");
                            }
                        }));
                    }

                    if (effect.MatchTiming(GameEvent.OnEndTurn))
                    {
                        l.Add(this.EffectListByEvent[GameEvent.OnEndTurn].Subscribe(args =>
                        {
                            if (effect.Execute(playingCard, args))
                            {
                                this.logger.LogInformation($"ターン終了時の効果: {playingCard.Name}({this.PlayersById[playingCard.OwnerId].Name})");
                            }
                        }));
                    }

                    if (effect.MatchTiming(GameEvent.OnPlay))
                    {
                        IDisposable d = null;
                        d = this.EffectListByEvent[GameEvent.OnPlay].Subscribe(args =>
                        {
                            if (effect.Execute(playingCard, args))
                            {
                                this.logger.LogInformation($"プレイ時の効果: {playingCard.Name}({this.PlayersById[playingCard.OwnerId].Name})");
                            }

                            // 対象がこのカードだった場合に限り、一度実行したら解除する
                            if (effect.Timing.Play?.Source == EffectTimingPlayEvent.EventSource.This)
                            {
                                d?.Dispose();
                            }
                        });
                        l.Add(d);
                    }

                    if (effect.MatchTiming(GameEvent.OnDestroy))
                    {
                        IDisposable d = null;
                        d = this.EffectListByEvent[GameEvent.OnDestroy].Subscribe(args =>
                        {
                            if (effect.Execute(playingCard, args))
                            {
                                this.logger.LogInformation($"破壊時の効果: {playingCard.Name}({this.PlayersById[playingCard.OwnerId].Name})");
                            }

                            // 対象がこのカードだった場合に限り、一度実行したら解除する
                            if (effect.Timing.Destroy?.Source == EffectTimingDestroyEvent.EventSource.This)
                            {
                                d?.Dispose();
                            }
                        });
                        l.Add(d);
                    }

                    if (effect.MatchTiming(GameEvent.OnDamageBefore))
                    {
                        l.Add(this.EffectListByEvent[GameEvent.OnDamageBefore].Subscribe(args =>
                        {
                            if (effect.Execute(playingCard, args))
                            {
                                this.logger.LogInformation($"ダメージ前の効果: {playingCard.Name}({this.PlayersById[playingCard.OwnerId].Name})");
                            }
                        }));
                    }

                    if (effect.MatchTiming(GameEvent.OnDamage))
                    {
                        l.Add(this.EffectListByEvent[GameEvent.OnDamage].Subscribe(args =>
                        {
                            if (effect.Execute(playingCard, args))
                            {
                                this.logger.LogInformation($"ダメージ後の効果: {playingCard.Name}({this.PlayersById[playingCard.OwnerId].Name})");
                            }
                        }));
                    }

                    if (effect.MatchTiming(GameEvent.OnBattleBefore))
                    {
                        l.Add(this.EffectListByEvent[GameEvent.OnBattleBefore].Subscribe(args =>
                        {
                            if (effect.Execute(playingCard, args))
                            {
                                this.logger.LogInformation($"戦闘前の効果: {playingCard.Name}({this.PlayersById[playingCard.OwnerId].Name})");
                            }
                        }));
                    }

                    if (effect.MatchTiming(GameEvent.OnBattle))
                    {
                        l.Add(this.EffectListByEvent[GameEvent.OnBattle].Subscribe(args =>
                        {
                            if (effect.Execute(playingCard, args))
                            {
                                this.logger.LogInformation($"戦闘後の効果: {playingCard.Name}({this.PlayersById[playingCard.OwnerId].Name})");
                            }
                        }));
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
            this.EffectListByEvent[GameEvent.OnPlay]
                .OnNext(new EffectEventArgs(GameEvent.OnPlay, this, SourceCard: playingCard));

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
                BattleContext: new BattleContext()
                {
                    AttackCard = card,
                    GuardPlayer = damagePlayer,
                    Value = card.Power,
                }
                );

            this.EffectListByEvent[GameEvent.OnBattleBefore].OnNext(eventArgs);

            this.logger.LogInformation($"アタック（プレイヤー）：{card}({player.Name}) > {damagePlayer.Name}");

            if (!CanAttack(eventArgs.BattleContext.AttackCard, eventArgs.BattleContext.GuardPlayer))
            {
                return (false, "攻撃不能");
            }

            var damageContext = new DamageContext()
            {
                DamageSourceCard = eventArgs.BattleContext.AttackCard,
                GuardPlayer = eventArgs.BattleContext.GuardPlayer,
                Value = eventArgs.BattleContext.AttackCard.Power,
            };
            this.HitPlayer(damageContext);

            var eventArgs2 = eventArgs with { EffectType = GameEvent.OnBattle };
            this.EffectListByEvent[GameEvent.OnBattle].OnNext(eventArgs2);

            return (true, "");
        }

        //public (bool IsSucceeded, string errorMessage) HitPlayer(Guid playerId, Guid damagePlayerId, int damage)
        //{
        //    if (this.GameOver)
        //    {
        //        return (false, "すでにゲームが終了しています。");
        //    }

        //    if (playerId != this.CurrentPlayer.Id)
        //    {
        //        return (false, "このプレイヤーのターンではありません。");
        //    }

        //    var damagePlayer = this.PlayersById[damagePlayerId];

        //    this.logger.LogInformation($"ダメージ：{damage} > {damagePlayer.Name}");

        //    damagePlayer.Damage(damage);

        //    this.EffectListByEvent[GameEvent.OnDamage].OnNext(new EffectEventArgs() { EffectType = GameEvent.OnDamage, GameMaster = this, });

        //    return (true, "");
        //}

        public void HitPlayer(DamageContext damageContext)
        {
            //var damagePlayer = this.PlayersById[damagePlayerId];

            var eventArgs = new EffectEventArgs(
                GameEvent.OnDamageBefore,
                this,
                DamageContext: damageContext
            );
            this.EffectListByEvent[GameEvent.OnDamageBefore].OnNext(eventArgs);

            this.logger.LogInformation($"ダメージ：{damageContext.Value} > {damageContext.GuardPlayer.Name}");

            damageContext.GuardPlayer.Damage(damageContext.Value);

            this.EffectListByEvent[GameEvent.OnDamage].OnNext(eventArgs);
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
                BattleContext: new BattleContext()
                {
                    AttackCard = attackCard,
                    GuardCard = guardCard,
                    Value = attackCard.Power,
                });

            this.EffectListByEvent[GameEvent.OnBattleBefore].OnNext(eventArgs);

            this.logger.LogInformation($"アタック（クリーチャー）：{eventArgs.BattleContext.AttackCard}({attackPlayer.Name}) > {eventArgs.BattleContext.GuardCard}({guardPlayer.Name})");

            if (!CanAttack(eventArgs.BattleContext.AttackCard, eventArgs.BattleContext.GuardCard, this.CreateEnvironment(playerId)))
            {
                return (false, "攻撃不能");
            }

            // 攻撃するとステルスを失う
            if (eventArgs.BattleContext.AttackCard.Abilities.Contains(CreatureAbility.Stealth))
            {
                eventArgs.BattleContext.AttackCard.Abilities.Remove(CreatureAbility.Stealth);
            }

            // お互いにダメージを受ける
            var damageContext = new DamageContext()
            {
                DamageSourceCard = eventArgs.BattleContext.AttackCard,
                GuardCard = eventArgs.BattleContext.GuardCard,
                Value = eventArgs.BattleContext.AttackCard.Power,
            };
            this.HitCreature(damageContext);

            var damageContext2 = new DamageContext()
            {
                DamageSourceCard = eventArgs.BattleContext.GuardCard,
                GuardCard = eventArgs.BattleContext.AttackCard,
                Value = eventArgs.BattleContext.GuardCard.Power,
            };
            this.HitCreature(damageContext2);

            var eventArgs2 = eventArgs with { EffectType = GameEvent.OnBattle };
            this.EffectListByEvent[GameEvent.OnBattle].OnNext(eventArgs2);

            return (true, "");
        }

        //public (bool IsSucceeded, string errorMessage) HitCreature(Guid playerId, Guid guardCreatureCardId, int damage)
        //{
        //    if (this.GameOver)
        //    {
        //        return (false, "すでにゲームが終了しています。");
        //    }

        //    if (playerId != this.CurrentPlayer.Id)
        //    {
        //        return (false, "このプレイヤーのターンではありません。");
        //    }

        //    var creatureCard = this.cardFactory.GetById(guardCreatureCardId);

        //    this.logger.LogInformation($"ダメージ：{damage} > {creatureCard}");

        //    creatureCard.Damage(damage);

        //    this.EffectListByEvent[GameEvent.OnDamage].OnNext(new EffectEventArgs() { EffectType = GameEvent.OnDamage, GameMaster = this, Source = creatureCard });

        //    if (creatureCard.Toughness <= 0)
        //    {
        //        this.DestroyCard(creatureCard);
        //    }

        //    return (true, "");
        //}

        public void HitCreature(DamageContext damageContext)
        {
            //var guardCreatureCard = this.cardFactory.GetById(guardCreatureCardId);

            var eventArgs = new EffectEventArgs(GameEvent.OnDamageBefore, this, DamageContext: damageContext);
            this.EffectListByEvent[GameEvent.OnDamageBefore].OnNext(eventArgs);

            if (damageContext.GuardCard.Type != CardType.Creature)
            {
                throw new Exception($"指定されたカードはクリーチャーではありません。: {damageContext.GuardCard.Name}");
            }

            this.logger.LogInformation(
                $"ダメージ：{damageContext.Value} > {damageContext.GuardCard}({this.PlayersById[damageContext.GuardCard.OwnerId].Name})");

            damageContext.GuardCard.Damage(damageContext.Value);

            this.EffectListByEvent[GameEvent.OnDamage].OnNext(eventArgs);

            if (damageContext.GuardCard.Toughness <= 0)
            {
                this.DestroyCard(damageContext.GuardCard);
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
    }
}
