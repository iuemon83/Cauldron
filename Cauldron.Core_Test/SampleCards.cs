using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Core_Test
{
    public class SampleCards
    {
        public static readonly string CardsetName = "Sample";

        public static CardDef Creature(int cost, string name, int power, int toughness,
            IEnumerable<string> annotations = null,
            string flavorText = "",
            int? numTurnsToCanAttack = null, int? numAttacksInTurn = null, bool isToken = false,
            IEnumerable<CreatureAbility> abilities = null,
            IEnumerable<CardEffect> effects = null)
        {
            var t = CardDef.Empty;
            t.Cost = cost;
            t.Name = name;
            t.Power = power;
            t.Toughness = toughness;
            t.Annotations = annotations?.ToArray() ?? Array.Empty<string>();
            t.FlavorText = flavorText;
            t.NumTurnsToCanAttack = numTurnsToCanAttack;
            t.NumAttacksLimitInTurn = numAttacksInTurn;
            t.IsToken = isToken;
            t.Abilities = abilities?.ToArray() ?? Array.Empty<CreatureAbility>();
            t.Effects = effects?.ToArray() ?? Array.Empty<CardEffect>();

            t.Type = CardType.Creature;

            return t;
        }

        public static CardDef Artifact(int cost, string name, bool isToken = false,
            IEnumerable<string> annotations = null,
            string flavorText = "",
            IEnumerable<CardEffect> effects = null)
        {
            var t = CardDef.Empty;
            t.Cost = cost;
            t.IsToken = isToken;
            t.Type = CardType.Artifact;
            t.Name = name;
            t.Annotations = annotations?.ToArray() ?? Array.Empty<string>();
            t.FlavorText = flavorText;
            t.Effects = effects?.ToArray() ?? Array.Empty<CardEffect>();

            return t;
        }

        public static CardDef Sorcery(int cost, string name, bool isToken = false,
            IEnumerable<string> annotations = null,
            string flavorText = "",
            IEnumerable<CardEffect> effects = null)
        {
            var t = CardDef.Empty;
            t.Cost = cost;
            t.IsToken = isToken;
            t.Type = CardType.Sorcery;
            t.Name = name;
            t.Annotations = annotations?.ToArray() ?? Array.Empty<string>();
            t.FlavorText = flavorText;
            t.Effects = effects?.ToArray() ?? Array.Empty<CardEffect>();

            return t;
        }

        public static CardDef KarakuriGoblin
            => SampleCards.Creature(1, "からくりゴブリン", 1, 1, flavorText: "トークン", isToken: true);

        public static CardDef Goblin
            => SampleCards.Creature(1, "ゴブリン", 1, 2, flavorText: "ただのゴブリン");

        public static CardDef QuickGoblin
            => SampleCards.Creature(1, "素早いゴブリン", 1, 1, numTurnsToCanAttack: 0);

        public static CardDef ShieldGoblin
            => SampleCards.Creature(2, "盾持ちゴブリン", 1, 2, flavorText: "盾になる",
                abilities: new[] { CreatureAbility.Cover });

        public static CardDef TwinGoblin
            => SampleCards.Creature(2, "双子のゴブリン", 1, 1,
                numAttacksInTurn: 2);

        public static CardDef SlowGoblin
            => SampleCards.Creature(4, "遅いゴブリン", 7, 4,
                numTurnsToCanAttack: 2);

        public static CardDef DoubleStrikeGoblin
            => SampleCards.Creature(3, "二刀流のゴブリン",
                2, 1,
                effects: new[]
                {
                    new CardEffect(
                        "このカードの戦闘時、戦闘開始前に相手クリーチャーに、Xのダメージを与える。X=このカードの攻撃力",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                AttackBefore: new(
                                    AttackCardCondition: new(
                                        CardCondition.ContextConditionValue.This)))))),
                        new[]{
                            new EffectAction(
                                Damage: new(
                                    new NumValue(
                                        NumValueCalculator: new(
                                            ForCard: new(
                                                NumValueCalculatorForCard.TypeValue.CardPower,
                                                new Choice(
                                                    new ChoiceSource(
                                                        orCardConditions: new[]
                                                        {
                                                            new CardCondition(
                                                                CardCondition.ContextConditionValue.This)
                                                        }))))),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    BattleEventContextCondition: CardCondition.BattleEventContextConditionValue.Guard)
                                            }))))
                        }),
                    new CardEffect(
                        "",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                AttackBefore: new(
                                    GuardCardCondition: new(
                                        CardCondition.ContextConditionValue.This)))))),
                        new[]{
                            new EffectAction(
                                Damage: new(
                                    new NumValue(
                                        NumValueCalculator: new(
                                            ForCard: new(
                                                NumValueCalculatorForCard.TypeValue.CardPower,
                                                new Choice(
                                                    new ChoiceSource(
                                                        orCardConditions: new[]
                                                        {
                                                            new CardCondition(
                                                                CardCondition.ContextConditionValue.This)
                                                        }))))),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    BattleEventContextCondition: CardCondition.BattleEventContextConditionValue.Attack)
                                            }))))
                        })
                });

        public static CardDef DeadlyGoblin
            => SampleCards.Creature(3, "暗殺ゴブリン", 1, 1, flavorText: "暗殺者",
                abilities: new[] { CreatureAbility.Stealth, CreatureAbility.Deadly });

        public static CardDef MechanicGoblin
            => SampleCards.Creature(1, "ゴブリンの技師",
                1, 1,
                effects: new[]
                {
                    // 破壊時、からくりゴブリン１枚を手札に加える
                    new CardEffect(
                        "このカードが破壊されたとき、手札に「からくりゴブリン」1枚を加える。",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouCemetery,
                            new(new(Destroy: new (
                                OrCardCondition: new[]
                                {
                                    new CardCondition(CardCondition.ContextConditionValue.This)
                                })))
                        )),
                        new[]
                        {
                            new EffectAction(
                                AddCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            OrCardDefConditions: new[]
                                            {
                                                new CardDefCondition(
                                                    OutZoneCondition: new(new[]{ OutZonePrettyName.CardPool }),
                                                    CardSetCondition: new(CardSetCondition.TypeValue.This),
                                                    NameCondition: new(
                                                        new TextValue(KarakuriGoblin.Name),
                                                        TextCompare.CompareValue.Equality
                                                    )
                                                ),
                                            })),
                                    new ZoneValue(new[]{ ZonePrettyName.YouHand })
                                )
                            )
                        }
                    )
                });

        public static CardDef MagicBook
            => SampleCards.Creature(1, "魔法の本",
                1, 1,
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、ランダムな魔法カード1枚をあなたの手札に追加する。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]{
                            new EffectAction(
                                AddCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            OrCardDefConditions: new[]
                                            {
                                                new CardDefCondition(
                                                    OutZoneCondition: new(new[]{
                                                        OutZonePrettyName.CardPool
                                                    }),
                                                    TypeCondition: new(new[]{ CardType.Sorcery })
                                                )
                                            }),
                                        how: Choice.HowValue.Random,
                                        new NumValue(1)),
                                    new ZoneValue(new[]{ ZonePrettyName.YouHand })))
                        })
                });

        public static CardDef GoblinFollower
            => SampleCards.Creature(1, "ゴブリンフォロワー",
                1, 1,
                effects: new[]
                {
                    new CardEffect(
                        "あなたが「ゴブリン」と名のつくクリーチャーカードをプレイしたとき、このカードをデッキから場に出す。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouDeck,
                            new EffectWhen(new EffectTiming(
                                Play: new(
                                    OrCardConditions: new[]
                                    {
                                        new CardCondition(
                                            ContextCondition: CardCondition.ContextConditionValue.Others,
                                            NameCondition: new(
                                                new TextValue("ゴブリン"),
                                                TextCompare.CompareValue.Contains),
                                            ZoneCondition: new(new ZoneValue(new[]{ ZonePrettyName.YouField })),
                                            TypeCondition: new(new[]{ CardType.Creature })
                                        )
                                    }))))),
                        new[]{
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ContextCondition: CardCondition.ContextConditionValue.This
                                                )
                                            })),
                                    ZonePrettyName.YouField))
                        })
                });

        public static CardDef GoblinsPet
            => SampleCards.Creature(2, "ゴブリンのペット",
                2, 6, abilities: new[] { CreatureAbility.Cover },
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、相手の手札からランダムなクリーチャー1枚を相手の場に出す。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]{
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]{
                                                new CardCondition(
                                                    ZoneCondition: new(new ZoneValue(new[]
                                                    {
                                                        ZonePrettyName.OpponentHand
                                                    })),
                                                    TypeCondition: new(new[]{ CardType.Creature })
                                                )
                                            }),
                                        how: Choice.HowValue.Random,
                                        new NumValue(1)),
                                    ZonePrettyName.OpponentField))
                        })
                });

        public static CardDef MindController
            => SampleCards.Creature(3, "催眠術師",
                3, 3,
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、敵のフィールドにクリーチャーが4体以上いるなら、ランダムに1体を選択し、それをあなたの場に移動する。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                Play: new(
                                    OrCardConditions: new[]
                                    {
                                        new CardCondition(CardCondition.ContextConditionValue.This)
                                    }))),
                            If: new(new ConditionWrap(NumCondition: new(
                                    new NumValue(NumValueCalculator: new(
                                        ForCard: new(
                                            NumValueCalculatorForCard.TypeValue.Count,
                                            new Choice(
                                                new ChoiceSource(orCardConditions: new[]{
                                                    new CardCondition(
                                                        ZoneCondition: new(new ZoneValue(new[]
                                                        {
                                                            ZonePrettyName.OpponentField
                                                        })),
                                                        TypeCondition: new(new[]
                                                        {
                                                            CardType.Creature
                                                        })
                                                    )
                                                }))))),
                                    new NumCompare(
                                        4,
                                        NumCompare.CompareValue.GreaterThan)
                                        ))))),
                        new[]{
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]{
                                                new CardCondition(
                                                    ZoneCondition: new(new ZoneValue(new[]
                                                    {
                                                        ZonePrettyName.OpponentField
                                                    })),
                                                    TypeCondition: new(new[]{ CardType.Creature })
                                                )
                                            }),
                                        how: Choice.HowValue.Random,
                                        new NumValue(1)),
                                    ZonePrettyName.YouField))
                        })
                });

        public static CardDef NinjaGoblin
            => SampleCards.Creature(3, "分身ゴブリン",
                1, 2,
                effects: new[] {
                    new CardEffect(
                        "このカードをプレイしたとき、「分身ゴブリン」一体を場に出す。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                AddCard:new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ContextCondition: CardCondition.ContextConditionValue.This
                                                )
                                            })),
                                    new ZoneValue(new[]{ZonePrettyName.YouField })
                                    ))
                        })
                });

        public static CardDef SuperNinjaGoblin
            => SampleCards.Creature(3, "多重分身ゴブリン",
                1, 1,
                effects: new[] {
                    new CardEffect(
                        "このカードをプレイしたとき、「多重分身ゴブリン」2体を場に出す。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                AddCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ContextCondition: CardCondition.ContextConditionValue.This
                                                )
                                            })),
                                    new ZoneValue(new[]{ZonePrettyName.YouField }),
                                    NumOfAddCards: 2
                                    ))
                        })
                });

        public static CardDef Greed
            => SampleCards.Sorcery(2, "強欲",
                effects: new[]
                {
                    // カードを2枚引く
                    new CardEffect(
                        "あなたはカードを2枚ドローする。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                DrawCard: new(new NumValue(2),
                                    new PlayerCondition(PlayerCondition.ContextValue.You))
                            )
                        }
                    ),
                    // このカードが手札から捨てられたなら、1枚引く
                    new CardEffect(
                        "このカードを手札から捨てるとき、あなたはカードを1枚ドローする。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouCemetery,
                            new(new(MoveCard: new(
                                OrCardConditions: new[]
                                {
                                    new CardCondition(CardCondition.ContextConditionValue.This)
                                },
                                ZonePrettyName.YouHand, ZonePrettyName.YouCemetery))))),
                        new[]
                        {
                            new EffectAction(
                                DrawCard: new(new NumValue(1),
                                    new PlayerCondition(PlayerCondition.ContextValue.You))
                            )
                        }
                    )
                });

        public static CardDef Hope
            => SampleCards.Sorcery(1, "希望",
                effects: new[]
                {
                    new CardEffect(
                        "あなたはカードをX枚ドローする。X=相手のHPとあなたのHPの差",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                DrawCard: new(
                                    new NumValue(
                                        NumValueCalculator: new(ForPlayer: new(
                                            NumValueCalculatorForPlayer.TypeValue.PlayerCurrentHp,
                                            new Choice(
                                                new ChoiceSource(
                                                    orPlayerConditions: new[]
                                                    {
                                                        new PlayerCondition(PlayerCondition.ContextValue.Opponent)
                                                    })
                                                )
                                            )),
                                        NumValueModifier: new(
                                            NumValueModifier.OperatorValue.Sub,
                                            new NumValue(
                                                NumValueCalculator: new(ForPlayer: new(
                                                    NumValueCalculatorForPlayer.TypeValue.PlayerCurrentHp,
                                                    new Choice(
                                                        new ChoiceSource(
                                                            orPlayerConditions: new[]
                                                            {
                                                                new PlayerCondition(PlayerCondition.ContextValue.You)
                                                            })
                                                        )
                                                    ))
                                                )
                                            )
                                        ),
                                    new PlayerCondition(PlayerCondition.ContextValue.You)
                                    )
                            )
                        }
                    ),
                });

        public static CardDef RunawayMagic
            => SampleCards.Sorcery(2, "暴走する魔力",
                effects: new[]
                {
                    new CardEffect(
                        "場のすべてのクリーチャーにXのダメージを与える。" +
                            "その後、場のクリーチャーから魔導カウンターをすべて取り除く。" +
                            "X=そのクリーチャーに乗っている魔導カウンターの数",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(Damage: new(
                                    new NumValue(
                                        NumValueCalculator: new(ForCounter: new(
                                            "魔導",
                                            new Choice(
                                                new ChoiceSource(
                                                    orCardConditions: new[]
                                                    {
                                                        new CardCondition(CardCondition.ContextConditionValue.ActionTarget)
                                                    }
                                                    )
                                                )
                                            ))
                                        ),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new ZoneValue(new[]
                                                    {
                                                        ZonePrettyName.YouField,
                                                        ZonePrettyName.OpponentField,
                                                    })),
                                                    TypeCondition: new(new[]
                                                    {
                                                        CardType.Creature
                                                    })
                                                    )
                                            }
                                            )
                                        )
                                    )
                            ),
                            new EffectAction(ModifyCounter: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ActionContext: new(Damage: new(
                                                        "damage",
                                                        ActionContextCardsOfDamage.TypeValue.DamagedCards
                                                        ))
                                                    )
                                            }
                                            )
                                        ),
                                    "魔導",
                                    new NumValueModifier(
                                        NumValueModifier.OperatorValue.Replace,
                                        new NumValue(0)
                                        )
                                ))
                        }
                    ),
                });

        public static CardDef ShamanGoblin
            => SampleCards.Creature(3, "呪術師ゴブリン",
                1, 1,
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、相手の場にあるランダムなクリーチャーカード1枚を破壊する。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                DestroyCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    TypeCondition: new(new[]{CardType.Creature}),
                                                    ZoneCondition: new(new ZoneValue(new[]{ZonePrettyName.OpponentField}))
                                                )
                                            }),
                                        Choice.HowValue.Random,
                                        new NumValue(1))
                                    )
                            )
                        }
                    )
                });

        public static CardDef HealerGoblin
            => SampleCards.Creature(3, "ゴブリンの衛生兵",
                1, 2,
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、あなたのHPを2回復する。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                ModifyPlayer: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition(PlayerCondition.ContextValue.You)
                                            })),
                                    new PlayerModifier(
                                        Hp: new NumValueModifier(
                                            NumValueModifier.OperatorValue.Add,
                                            new NumValue(2)))
                                )
                                )
                        }
                    )
                });

        public static CardDef FireGoblin
            => SampleCards.Creature(4, "火炎のゴブリン",
                4, 2,
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、相手プレイヤーか、相手の場にいるクリーチャーカード1枚に2ダメージを与える。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                Damage: new EffectActionDamage(
                                    new NumValue(2),
                                    new Choice(
                                        new ChoiceSource(
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition(
                                                    PlayerCondition.ContextValue.Opponent
                                                ),
                                            },
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ContextCondition: CardCondition.ContextConditionValue.Others,
                                                    TypeCondition: new CardTypeCondition(new[]{ CardType.Creature }),
                                                    ZoneCondition: new(new(new[]{ ZonePrettyName.OpponentField }))
                                                )
                                            }),
                                        Choice.HowValue.Choose,
                                        new NumValue(1))
                                )
                            )
                        }
                    )
                });

        public static CardDef BeginnerSummoner
            => SampleCards.Creature(4, "初心者召喚士",
                3, 3,
                effects: new[]
                {
                    new CardEffect(
                        "このカードが破壊されたとき、ランダムなコスト2のクリーチャーを1体場に出す。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouCemetery,
                            new EffectWhen(new EffectTiming(
                                Destroy: new(
                                    OrCardCondition: new[]
                                    {
                                        new CardCondition(CardCondition.ContextConditionValue.This)
                                    }))))),
                        new[]
                        {
                            new EffectAction(AddCard: new(
                                new Choice(new ChoiceSource(
                                    OrCardDefConditions: new[]
                                    {
                                        new CardDefCondition(
                                            OutZoneCondition: new OutZoneCondition(
                                                new[]{ OutZonePrettyName.CardPool }),
                                            CostCondition: new NumCompare(
                                                2,
                                                NumCompare.CompareValue.Equality),
                                            TypeCondition: new(new[]{ CardType.Creature })
                                        )
                                    }),
                                    how: Choice.HowValue.Random,
                                    numPicks: new NumValue(1)),
                                new ZoneValue(new[]{ ZonePrettyName.YouField })
                                ))
                        }
                    )
                });

        public static CardDef MadScientist
            => SampleCards.Creature(4, "マッドサイエンティスト",
                3, 3,
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、自分の場か、相手の場にあるクリーチャーカード1枚を選択して、それを破壊する。" +
                            "その後、破壊したカードのコピーをもとの場に出す。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]{
                            // 効果を付与したカードを破壊する
                            new EffectAction(DestroyCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ContextCondition: CardCondition.ContextConditionValue.Others,
                                                ZoneCondition: new(new(new[]{
                                                    ZonePrettyName.YouField, ZonePrettyName.OpponentField })),
                                                TypeCondition: new(new[]{ CardType.Creature })
                                            )
                                        }),
                                    Choice.HowValue.Choose,
                                    new NumValue(1)),
                                name: "delete"
                                )),
                            new EffectAction(AddCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ActionContext: new(DestroyCard: new(
                                                    "delete",
                                                    ActionContextCardsOfDestroyCard.TypeValue.DestroyedCards))
                                            )
                                        })),
                                new ZoneValue(new[]{ ZonePrettyName.OwnerField })
                                )),
                        }
                    )
                });

        public static CardDef BraveGoblin
            => SampleCards.Creature(4, "ゴブリンの勇者",
                2, 2,
                effects: new[]
                {
                    // 自分が受けるダメージを2軽減する
                    new CardEffect(
                        "自分が受けるダメージを2軽減する。",
                        new EffectConditionWrap(
                            ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new(new(
                                DamageBefore: new(
                                    TakeCardCondition: new CardCondition(
                                        ContextCondition: CardCondition.ContextConditionValue.This
                                    )))
                            )
                        )),
                        new[]
                        {
                            new EffectAction(
                                ModifyDamage: new EffectActionModifyDamage(
                                    new NumValueModifier(
                                        NumValueModifier.OperatorValue.Sub,
                                        new NumValue(2)
                                    )
                                ))
                        }
                    ),
                    // 自分の他のゴブリンクリーチャーが戦闘で与えるダメージを1増加する
                    new CardEffect(
                        "自分の場の他のゴブリンクリーチャーが戦闘で与えるダメージを1増加する。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                DamageBefore: new(
                                    EffectTimingDamageBeforeEvent.TypeValue.Battle,
                                    SourceCardCondition: new CardCondition(
                                        NameCondition: new(
                                            new TextValue("ゴブリン"),
                                            TextCompare.CompareValue.Contains),
                                        ZoneCondition: new(new(new[]{ ZonePrettyName.YouField })),
                                        TypeCondition: new CardTypeCondition(new[]{ CardType.Creature }),
                                        ContextCondition: CardCondition.ContextConditionValue.Others
                                    )))
                            )
                        )),
                        new[]
                        {
                            new EffectAction(
                                ModifyDamage: new EffectActionModifyDamage(
                                    new NumValueModifier(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(1)
                                    )
                                )
                            )
                        }
                    )
                });

        public static CardDef Faceless
            => SampleCards.Creature(4, "顔なし",
                7, 7,
                effects: new[]
                {
                    new CardEffect(
                        "あなたの次のターン開始時、あなたのMPは2減少する。",
                        new EffectConditionWrap(ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                ReserveEffect: new(new[]
                                {
                                    new CardEffect(
                                        "",
                                        new EffectConditionWrap(
                                            Reserve: new(
                                                When: new(new EffectTiming(
                                                    StartTurn: new(
                                                        OrPlayerCondition: new[]
                                                        {
                                                            new PlayerCondition(PlayerCondition.ContextValue.You),
                                                        }))),
                                                While: new(new(
                                                    StartTurn: new(
                                                        OrPlayerCondition: new[]
                                                        {
                                                            new PlayerCondition(PlayerCondition.ContextValue.You),
                                                        })),
                                                    0, 1)
                                                )),
                                        new[]
                                        {
                                            new EffectAction(
                                                ModifyPlayer: new(
                                                    new Choice(new ChoiceSource(
                                                        orPlayerConditions: new[]
                                                        {
                                                            new PlayerCondition(PlayerCondition.ContextValue.You)
                                                        })),
                                                    new PlayerModifier(
                                                        Mp: new NumValueModifier(
                                                            NumValueModifier.OperatorValue.Sub,
                                                            new NumValue(2))
                                                        ))
                                            )
                                        })
                                }))
                        }
                    )
                });

        public static CardDef Prophet
            => SampleCards.Creature(5, "預言者",
                0, 7,
                effects: new[]
                {
                    new CardEffect(
                        "あなたのターンの開始時にこのカードが場にあるとき、このカードの攻撃力を7にする",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                StartTurn: new(
                                    OrPlayerCondition: new[]
                                    {
                                        new PlayerCondition(PlayerCondition.ContextValue.You)
                                    }))))),
                        new[]{
                            new EffectAction(
                                ModifyCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ContextCondition: CardCondition.ContextConditionValue.This)
                                            })),
                                    Power: new(
                                        NumValueModifier.OperatorValue.Replace,
                                        new NumValue(7))))
                        }),
                });

        public static CardDef MagicDragon
            => SampleCards.Creature(5, "マジックドラゴン",
                4, 4,
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、あなたはカードを1枚ドローする。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]{
                            new EffectAction(
                                DrawCard: new(
                                    new NumValue(1),
                                    new PlayerCondition(PlayerCondition.ContextValue.You)))
                        }),
                    new CardEffect(
                        "このカードが場にある限り、あなたがプレイした魔法カードによるダメージを+1する。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                DamageBefore: new(
                                    EffectTimingDamageBeforeEvent.TypeValue.NonBattle,
                                    SourceCardCondition: new(
                                        TypeCondition: new(new[]{ CardType.Sorcery }),
                                        OwnerCondition: CardCondition.OwnerConditionValue.You
                                    )))))),
                        new[]{
                            new EffectAction(
                                ModifyDamage: new(
                                    new NumValueModifier(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(1))
                                    ))
                        }),
                });

        public static CardDef GiantGoblin
            => SampleCards.Creature(5, "ゴブリンの巨人",
                3, 7, abilities: new[] { CreatureAbility.Cover },
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、自分の場にある他のクリチャーカードに3ダメージを与える。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                Damage: new EffectActionDamage(
                                    new NumValue(3),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new(new[]{ ZonePrettyName.YouField })),
                                                    TypeCondition: new CardTypeCondition(new[]{ CardType.Creature }),
                                                    ContextCondition: CardCondition.ContextConditionValue.Others
                                                )
                                            }))
                                ))
                        }
                    )
                });

        public static CardDef LeaderGoblin
            => SampleCards.Creature(5, "ゴブリンリーダー",
                1, 5,
                effects: new[]
                {
                    // プレイ時：自分のクリーチャーすべてを+1/+0 する。
                    // 自分のターン開始時：自分のクリーチャーすべてを+1/+0 する。
                    new CardEffect(
                        "このカードをプレイしたとき、または自分のターン開始時に自分の場にあるほかのゴブリンクリーチャーのパワーを1増加する。",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new(new(
                                Play: new (
                                    OrCardConditions : new[] {
                                        new CardCondition(CardCondition.ContextConditionValue.This)
                                    }),
                                StartTurn: new (
                                    OrPlayerCondition : new[] {
                                        new PlayerCondition(PlayerCondition.ContextValue.You)
                                    })
                                )))),
                        new[]
                        {
                            new EffectAction(
                                ModifyCard: new (
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    NameCondition: new(
                                                        new TextValue("ゴブリン"),
                                                        TextCompare.CompareValue.Contains),
                                                    ZoneCondition: new(new(new[]{ ZonePrettyName.YouField })),
                                                    TypeCondition: new (new[]{ CardType.Creature }),
                                                    ContextCondition: CardCondition.ContextConditionValue.Others
                                                )
                                            })),
                                    Power: new NumValueModifier(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(1))
                                ))
                        }
                    )
                });

        public static CardDef DoubleShield
            => SampleCards.Creature(5, "二重の盾持ちゴブリン",
                3, 4,
                abilities: new[] { CreatureAbility.Cover },
                effects: new[]
                {
                    new CardEffect(
                        "このクリーチャーがダメージを受けるとき、1度だけそのダメージを0にする。",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                DamageBefore: new(
                                    TakeCardCondition: new(
                                        ContextCondition: CardCondition.ContextConditionValue.This)
                                    ))),
                            While: new(new EffectTiming(
                                DamageBefore: new(
                                    TakeCardCondition: new(
                                        ContextCondition: CardCondition.ContextConditionValue.This)
                                    )),
                                0, 1))),
                        new[]{
                            new EffectAction(
                                ModifyDamage: new(
                                    new NumValueModifier(
                                        NumValueModifier.OperatorValue.Replace,
                                        new NumValue(0)))
                                )
                        })
                });

        public static CardDef Nightmare
            => SampleCards.Creature(6, "悪夢",
                2, 8,
                effects: new[]
                {
                    new CardEffect(
                        "あなたのターンの開始時にこのカードが場にあるとき、このカードの攻撃力を2倍にする",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                StartTurn: new(
                                    OrPlayerCondition: new[]
                                    {
                                        new PlayerCondition(PlayerCondition.ContextValue.You)
                                    }))))),
                        new[]{
                            new EffectAction(
                                ModifyCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ContextCondition: CardCondition.ContextConditionValue.This)
                                            })),
                                    Power: new(
                                        NumValueModifier.OperatorValue.Multi,
                                        new NumValue(2))))
                        }),
                });

        public static CardDef RiderGoblin
            => SampleCards.Creature(6, "騎兵ゴブリン",
                3, 5,
                effects: new[]
                {
                    new CardEffect(
                        "あなたが魔法カードをプレイしたとき、あなたの場に「騎兵ゴブリン」1枚を追加する。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                Play: new(
                                    OrCardConditions: new[]
                                    {
                                        new CardCondition(
                                            ContextCondition: CardCondition.ContextConditionValue.Others,
                                            OwnerCondition: CardCondition.OwnerConditionValue.You,
                                            TypeCondition: new CardTypeCondition(
                                                new[]{ CardType.Sorcery })
                                            )
                                    }))))),
                        new[]
                        {
                            new EffectAction(
                                AddCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            OrCardDefConditions: new[]
                                            {
                                                new CardDefCondition(
                                                    OutZoneCondition: new(
                                                        new[]{ OutZonePrettyName.CardPool }),
                                                    NameCondition: new(
                                                        new TextValue(WarGoblin.Name),
                                                        TextCompare.CompareValue.Equality))
                                            }
                                    )),
                                    new ZoneValue(
                                        new []{ ZonePrettyName.YouField })
                                    ))
                        }
                    )
                });

        public static CardDef WarGoblin
            => SampleCards.Creature(5, "戦闘ゴブリン", 3, 5, isToken: true);

        public static CardDef TyrantGoblin
            => SampleCards.Creature(6, "暴君ゴブリン",
                6, 6,
                effects: new[]
                {
                    // 手札をすべて捨てて、捨てた枚数パワーアップ
                    new CardEffect(
                        "このカードをプレイしたとき、あなたの手札をすべて捨てる。このカードのパワーとタフネスをX増加する。Xは捨てた手札の枚数である。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]{
                            new EffectAction(SetVariable: new(
                                "x",
                                new NumValue(NumValueCalculator: new(
                                    ForCard: new(
                                        NumValueCalculatorForCard.TypeValue.Count,
                                        new Choice(
                                            new ChoiceSource(
                                                orCardConditions: new[]
                                                {
                                                    new CardCondition(
                                                        ZoneCondition: new(new(new[]{ ZonePrettyName.YouHand }))
                                                    )
                                                })))
                                    )))),
                            new EffectAction(MoveCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ZoneCondition: new(new(new[]{ ZonePrettyName.YouHand }))
                                            )
                                        })),
                                ZonePrettyName.YouCemetery)),
                            new EffectAction(ModifyCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ContextCondition: CardCondition.ContextConditionValue.This
                                            )
                                        })),
                                Power: new NumValueModifier(
                                    NumValueModifier.OperatorValue.Add,
                                    new NumValue(NumVariable: new("x"))),
                                Toughness: new NumValueModifier(
                                    NumValueModifier.OperatorValue.Add,
                                    new NumValue(NumVariable: new("x")))
                                ))
                        })
                });

        public static CardDef Bomb
            => SampleCards.Creature(1, "ボム",
                1, 1, isToken: true,
                effects: new[]
                {
                    new CardEffect(
                        "このカードが破壊されたとき、ランダムな敵クリーチャー1体か敵プレイヤーにランダムに1~4ダメージを与える。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouCemetery,
                            new EffectWhen(new EffectTiming(
                                Destroy: new(
                                    OrCardCondition: new[]
                                    {
                                        new CardCondition(CardCondition.ContextConditionValue.This)
                                    }))))),
                        new[]{
                            new EffectAction(
                                Damage: new(
                                    new NumValue(
                                            NumValueCalculator: new(Random: new(1, 4))),
                                    new Choice(
                                        new ChoiceSource(
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition(PlayerCondition.ContextValue.Opponent)
                                            },
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(
                                                        new ZoneValue(new[]{ ZonePrettyName.OpponentField })),
                                                    TypeCondition: new(new[]{ CardType.Creature })
                                                )
                                            }),
                                        how: Choice.HowValue.Random,
                                        numPicks: new NumValue(1)
                                        )))
                        })
                });

        public static CardDef Bomber
            => SampleCards.Creature(7, "ボマー",
                7, 7,
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、「ボム」2枚をあなたの場に追加する。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]{
                            new EffectAction(
                                AddCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            OrCardDefConditions: new[]
                                            {
                                                new CardDefCondition(
                                                    OutZoneCondition: new(new[]{ OutZonePrettyName.CardPool }),
                                                    CardSetCondition: new(CardSetCondition.TypeValue.This),
                                                    NameCondition: new(
                                                        new TextValue(Bomb.Name),
                                                        TextCompare.CompareValue.Equality)
                                                )
                                            })),
                                    new ZoneValue(new[]{ ZonePrettyName.YouField }),
                                    NumOfAddCards: 2
                                    ))
                        })
                });

        public static CardDef Disaster
            => SampleCards.Creature(7, "災い",
                6, 6,
                effects: new[]
                {
                    new CardEffect(
                        "このクリーチャーがダメージを受けるたび、「ノール」1体をあなたの場に追加する。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                DamageAfter: new(
                                    TakeCardCondition: new(
                                        ContextCondition: CardCondition.ContextConditionValue.This)
                                    ))))),
                        new[]{
                            new EffectAction(
                                AddCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            OrCardDefConditions: new[]
                                            {
                                                new CardDefCondition(
                                                    OutZoneCondition: new(new[]{ OutZonePrettyName.CardPool }),
                                                    CardSetCondition: new(CardSetCondition.TypeValue.This),
                                                    NameCondition: new(
                                                        new TextValue(Gnoll.Name),
                                                        TextCompare.CompareValue.Equality)
                                                )
                                            })),
                                    new ZoneValue(new[]{ ZonePrettyName.YouField }),
                                    NumOfAddCards: 1
                                    ))
                        })
                });

        public static CardDef Gnoll
            => SampleCards.Creature(7, "ノール", 7, 7, isToken: true,
                abilities: new[] { CreatureAbility.Cover });

        public static CardDef Firelord
            => SampleCards.Creature(8, "炎の王",
                8, 8, abilities: new[] { CreatureAbility.CantAttack },
                effects: new[]
                {
                    new CardEffect(
                        "このカードが場にあるとき、あなたのターン終了時に、ランダムな敵クリーチャー1体か敵プレイヤーに8ダメージを与える。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                EndTurn: new(
                                    OrPlayerCondition: new[]
                                    {
                                        new PlayerCondition(PlayerCondition.ContextValue.You)
                                    }))))),
                        new[]
                        {
                            new EffectAction(
                                Damage: new(
                                    new NumValue(8),
                                    new Choice(
                                        new ChoiceSource(
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition(PlayerCondition.ContextValue.Opponent)
                                            },
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(
                                                        new ZoneValue(new[]{ ZonePrettyName.OpponentField })),
                                                    TypeCondition: new(new[]{ CardType.Creature })
                                                )
                                            }),
                                        how: Choice.HowValue.Random,
                                        numPicks: new NumValue(1))))
                        })
                });

        //TODO 未実装
        // コストが0から元に戻る処理が無理
        // 別の効果でベースのコストから変更されているとそれが消えてしまう
        //public static CardDef KingGoblin
        //    => SampleCards.Creature(10, "ゴブリンの王", 8, 8,
        //        "あなたの場にカードが4枚以上あるとき、このカードのコストは0になる。" +
        //            "このカードのプレイ時、あなたの場にカードが4枚以上あるなら、あなたの場のカードをすべて破壊する。",
        //        effects: new[]
        //        {
        //        });

        public static CardDef Death
            => SampleCards.Creature(10, "死",
                12, 12,
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、このカードを除く全てのクリーチャーを破壊する。" +
                        "その後、X枚のランダムな自分の手札を墓地へ移動する。X=この効果で破壊したクリーチャーの数",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]{
                            new EffectAction(
                                DestroyCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ContextCondition: CardCondition.ContextConditionValue.Others,
                                                    TypeCondition: new CardTypeCondition(
                                                        new[]{ CardType.Creature }),
                                                    ZoneCondition: new(
                                                        new ZoneValue(new[]
                                                        {
                                                            ZonePrettyName.OpponentField,
                                                            ZonePrettyName.YouField
                                                        })))
                                            })),
                                    name: "destroy"),
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(
                                                        new ZoneValue(new[]{ ZonePrettyName.YouHand })))
                                            }),
                                        Choice.HowValue.Random,
                                        new NumValue(
                                            NumValueCalculator: new(
                                                ForCard: new(
                                                    NumValueCalculatorForCard.TypeValue.Count,
                                                    new Choice(
                                                        new ChoiceSource(
                                                            orCardConditions: new[]{
                                                                new CardCondition(
                                                                    ActionContext: new(
                                                                        DestroyCard: new(
                                                                            "destroy",
                                                                            ActionContextCardsOfDestroyCard.TypeValue.DestroyedCards)
                                                                    ))
                                                            })))))),
                                    ZonePrettyName.YouCemetery))
                        }),
                });

        public static CardDef TempRamp
            => SampleCards.Sorcery(0, "一時的なランプ",
                effects: new[]
                {
                    new CardEffect(
                        "あなたの最大MPを1増加する。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                ModifyPlayer: new(
                                    new Choice(new ChoiceSource(
                                        orPlayerConditions: new[]
                                        {
                                            new PlayerCondition(PlayerCondition.ContextValue.You)
                                        })),
                                    new PlayerModifier(
                                        MaxMp: new NumValueModifier(
                                            NumValueModifier.OperatorValue.Add,
                                            new NumValue(1))
                                        ))
                            )
                        }
                    ),
                    // ターン終了時あなたの最大MPを1減少する。
                    new CardEffect(
                        "ターン終了時あなたの最大MPを1減少する。",
                        new EffectConditionWrap(ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(ReserveEffect: new(
                                new[]
                                {
                                    new CardEffect(
                                        "",
                                        new EffectConditionWrap(Reserve: new(
                                            new EffectWhen(new EffectTiming(
                                                EndTurn: new(
                                                    OrPlayerCondition: new[]
                                                    {
                                                        new PlayerCondition(PlayerCondition.ContextValue.You)
                                                    }))),
                                            While: new(new EffectTiming(EndTurn: new(
                                                OrPlayerCondition: new[]
                                                {
                                                    new PlayerCondition(PlayerCondition.ContextValue.You)
                                                }))
                                                , 0, 1)
                                            )),
                                        new[]
                                        {
                                            new EffectAction(ModifyPlayer: new(
                                                new Choice(new ChoiceSource(
                                                    orPlayerConditions: new[]
                                                    {
                                                        new PlayerCondition(PlayerCondition.ContextValue.You)
                                                    })),
                                                new PlayerModifier(
                                                    MaxMp: new NumValueModifier(
                                                        NumValueModifier.OperatorValue.Sub,
                                                        new NumValue(1))
                                                    ))
                                            )
                                        })
                                }))
                        }
                    )
                });

        public static CardDef SelectDamage
            => SampleCards.Sorcery(1, "ファイア",
                effects: new[]
                {
                    new CardEffect(
                        "プレイヤーか、場のクリーチャー1体を選択する。それに1ダメージを与える。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                Damage: new(
                                    new NumValue(1),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new ZoneValue(new[]{
                                                        ZonePrettyName.YouField,
                                                        ZonePrettyName.OpponentField
                                                    })),
                                                    TypeCondition: new(new[]{ CardType.Creature })
                                                )
                                            },
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition(
                                                    PlayerCondition.ContextValue.Any
                                                )
                                            }),
                                        Choice.HowValue.Choose,
                                        new NumValue(1)))
                            )
                        }
                    )
                });

        public static CardDef RandomDamage
            => SampleCards.Sorcery(1, "稲妻",
                effects: new[]
                {
                    new CardEffect(
                        "ランダムな場のクリーチャー1体に2ダメージを与える。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                Damage: new(
                                    new NumValue(2),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new ZoneValue(new[]{
                                                        ZonePrettyName.YouField,
                                                        ZonePrettyName.OpponentField
                                                    })),
                                                    TypeCondition: new(new[]{ CardType.Creature })
                                                )
                                            }),
                                        Choice.HowValue.Random,
                                        new NumValue(1)))
                            )
                        }
                    )
                });

        public static CardDef SelectDeathDamage
            => SampleCards.Sorcery(3, "破壊光線",
                effects: new[]
                {
                    new CardEffect(
                        "場のクリーチャー1体を選択する。それにXのダメージを与える。X=そのクリーチャーの元々のタフネス",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                Damage: new(
                                    new NumValue(NumValueCalculator: new(ForCard: new(
                                        NumValueCalculatorForCard.TypeValue.CardBaseToughness,
                                        new Choice(
                                            new ChoiceSource(
                                                orCardConditions: new[]
                                                {
                                                    new CardCondition(CardCondition.ContextConditionValue.ActionTarget)
                                                }
                                            )
                                            )
                                        ))),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new ZoneValue(new[]{
                                                        ZonePrettyName.YouField,
                                                        ZonePrettyName.OpponentField
                                                    })),
                                                    TypeCondition: new(new[]{ CardType.Creature })
                                                )
                                            }
                                            ),
                                        Choice.HowValue.Choose,
                                        new NumValue(1)
                                        )
                                    )
                            )
                        }
                    )
                });

        public static CardDef Salvage
            => SampleCards.Sorcery(1, "サルベージ",
                effects: new[]
                {
                    new CardEffect(
                        "墓地のカードを1枚選択する。それをあなたの手札に加える。そのカードがクリーチャーならタフネスを元々のタフネスと等しい値にする。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ZoneCondition: new(new ZoneValue(
                                                    new[]{ ZonePrettyName.YouCemetery, ZonePrettyName.OpponentCemetery }))
                                            )
                                        }),
                                        Choice.HowValue.Choose,
                                        new NumValue(1)),
                                    ZonePrettyName.YouHand,
                                    Name: "move"
                                    )),
                            new EffectAction(
                                ModifyCard: new(
                                    new Choice(
                                        new ChoiceSource(orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ActionContext: new(
                                                    MoveCard: new(
                                                        "move",
                                                        ActionContextCardsOfMoveCard.TypeValue.Moved))
                                            )
                                        })),
                                    Toughness: new(
                                        NumValueModifier.OperatorValue.Replace,
                                        new NumValue(NumValueCalculator: new(
                                            ForCard: new(
                                                NumValueCalculatorForCard.TypeValue.CardBaseToughness,
                                                new Choice(
                                                    new ChoiceSource(orCardConditions: new[]
                                                    {
                                                        new CardCondition(
                                                            ActionContext: new(
                                                                MoveCard: new(
                                                                    "move",
                                                                    ActionContextCardsOfMoveCard.TypeValue.Moved))
                                                        )
                                                    }))))))))
                        }
                    )
                });

        public static CardDef Recycle
            => SampleCards.Sorcery(1, "リサイクル",
                effects: new[]
                {
                    new CardEffect(
                        "墓地のカードを1枚選択する。それのコピーをあなたの手札に加える。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                AddCard: new(
                                    new Choice(
                                        new ChoiceSource(orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ZoneCondition: new(new ZoneValue(
                                                    new[]{ ZonePrettyName.YouCemetery, ZonePrettyName.OpponentCemetery }))
                                            )
                                        }),
                                        Choice.HowValue.Choose,
                                        new NumValue(1)),
                                    new ZoneValue(new[]{ ZonePrettyName.YouHand })
                                    )),
                        }
                    )
                });

        public static CardDef SimpleReborn
            => SampleCards.Sorcery(1, "簡易蘇生",
                effects: new[]
                {
                    new CardEffect(
                        "墓地のクリーチャーをランダムに1体選択する。それをあなたの場に出す。それのタフネスを1にする。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ZoneCondition: new(new ZoneValue(
                                                    new[]{ZonePrettyName.YouCemetery, ZonePrettyName.OpponentCemetery })),
                                                TypeCondition: new(new[]{ CardType.Creature })
                                            )
                                        }),
                                        Choice.HowValue.Random,
                                        new NumValue(1)),
                                    ZonePrettyName.YouField,
                                    Name: "move"
                                    )),
                            new EffectAction(
                                ModifyCard: new(
                                    new Choice(
                                        new ChoiceSource(orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ActionContext: new(
                                                    MoveCard: new(
                                                        "move",
                                                        ActionContextCardsOfMoveCard.TypeValue.Moved))
                                            )
                                        })),
                                    Toughness: new(
                                        NumValueModifier.OperatorValue.Replace,
                                        new NumValue(1))))
                        }
                    )
                });

        public static CardDef Sword
            => SampleCards.Sorcery(1, "剣",
                effects: new[]
                {
                    new CardEffect(
                        "あなたの場にあるクリチャー1体を選択する。それは+1/+0 の修整を受ける。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                ModifyCard: new EffectActionModifyCard(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new(new[]{ ZonePrettyName.YouField })),
                                                    TypeCondition: new CardTypeCondition(new[]{ CardType.Creature })
                                                )
                                            }),
                                        Choice.HowValue.Choose,
                                        new NumValue(1)),
                                    Power: new NumValueModifier(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(1))
                                )
                            )
                        }
                    )
                });

        public static CardDef Shield
            => SampleCards.Sorcery(1, "盾",
                effects: new[]
                {
                    new CardEffect(
                        "あなたの場にあるクリチャー1体を選択する。それは+0/+1 の修整を受ける。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                ModifyCard: new EffectActionModifyCard(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new(new[]{ ZonePrettyName.YouField })),
                                                    TypeCondition: new CardTypeCondition(new[]{ CardType.Creature })
                                                )
                                            }),
                                        Choice.HowValue.Choose,
                                        new NumValue(1)),
                                    Toughness: new NumValueModifier(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(1))
                                )
                            )
                        }
                    )
                });

        public static CardDef Hit
            => SampleCards.Sorcery(1, "ヒット",
                isToken: true,
                effects: new[]
                {
                    new CardEffect(
                        "相手プレイヤーに1ダメージを与える。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(Damage: new(
                                new NumValue(1),
                                new Choice(new ChoiceSource(
                                    orPlayerConditions: new[]
                                    {
                                        new PlayerCondition(PlayerCondition.ContextValue.Opponent)
                                    }))))
                        })
                });

        public static CardDef Heal
            => SampleCards.Sorcery(1, "ヒール",
                isToken: true,
                effects: new[]
                {
                    new CardEffect(
                        "あなたのライフを1回復する。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(ModifyPlayer: new(
                                new Choice(new ChoiceSource(
                                    orPlayerConditions: new[]
                                    {
                                        new PlayerCondition(PlayerCondition.ContextValue.You)
                                    })),
                                new PlayerModifier(Hp: new NumValueModifier(
                                    NumValueModifier.OperatorValue.Add,
                                    new NumValue(1)))))
                        })
                });

        public static CardDef HitOrHeal
            => SampleCards.Sorcery(1, "ヒットorヒール",
                effects: new[]
                {
                    new CardEffect(
                        "「ヒット」か「ヒール」のうち1枚を選択し、それを自分の手札に加える。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(AddCard: new(
                                new Choice(new ChoiceSource(
                                    OrCardDefConditions: new[]
                                    {
                                        new CardDefCondition(
                                            OutZoneCondition: new(new[]{ OutZonePrettyName.CardPool }),
                                            CardSetCondition: new(CardSetCondition.TypeValue.This),
                                            NameCondition: new(
                                                new TextValue(Hit.Name),
                                                TextCompare.CompareValue.Equality)
                                        ),
                                        new CardDefCondition(
                                            OutZoneCondition: new(new[]{ OutZonePrettyName.CardPool }),
                                            CardSetCondition: new(CardSetCondition.TypeValue.This),
                                            NameCondition: new(
                                                new TextValue(Heal.Name),
                                                TextCompare.CompareValue.Equality)
                                        ),
                                    }),
                                    how: Choice.HowValue.Choose,
                                    numPicks: new NumValue(1)
                                    ),
                                new ZoneValue(new[]{ ZonePrettyName.YouHand })
                                ))
                        })
                });

        public static CardDef Sealed
            => SampleCards.Sorcery(0, "封印",
                effects: new[]
                {
                    new CardEffect(
                        "選択したクリーチャー1体を「封印」状態にする",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                ModifyCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new ZoneValue(new[]{
                                                        ZonePrettyName.YouField,
                                                        ZonePrettyName.OpponentField,
                                                    })),
                                                    TypeCondition: new(
                                                        new[]{ CardType.Creature })
                                                ),
                                            }),
                                        how: Choice.HowValue.Choose,
                                        numPicks: new NumValue(1)
                                        ),
                                    Ability: new(
                                        CreatureAbilityModifier.OperatorValue.Add,
                                        CreatureAbility.Sealed)
                                    ))
                        })
                });

        public static CardDef BreakCover
            => SampleCards.Sorcery(1, "盾砕き",
                effects: new[]
                {
                    new CardEffect(
                        "相手の場の「カバー」アビリティを持つすべてのクリーチャーから、「カバー」アビリティを削除する。" +
                            "その後それらのクリーチャーに1ダメージを与える。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                ModifyCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new ZoneValue(new[]
                                                    {
                                                        ZonePrettyName.OpponentField,
                                                    })),
                                                    AbilityCondition: new[]{ CreatureAbility.Cover }
                                                ),
                                            })
                                        ),
                                    Ability: new(
                                        CreatureAbilityModifier.OperatorValue.Remove,
                                        CreatureAbility.Cover),
                                    Name: "modify"
                                    )),
                            new EffectAction(
                                Damage: new(
                                    new NumValue(1),
                                    new Choice(
                                        new ChoiceSource(orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ActionContext: new(
                                                    ModifyCard:new(
                                                        "modify",
                                                        ActionContextCardsOfModifyCard.TypeValue.Modified
                                                    )))
                                        }))))
                        })
                });

        public static CardDef Exclude
            => SampleCards.Sorcery(5, "除外",
                effects: new[]
                {
                    new CardEffect(
                        "場にあるカード一つを選択する。それを除外する。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                ExcludeCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new ZoneValue(new[]{
                                                        ZonePrettyName.YouField,
                                                        ZonePrettyName.OpponentField,
                                                    }))
                                                ),
                                            }),
                                        how: Choice.HowValue.Choose,
                                        numPicks: new NumValue(1)
                                        )))
                        })
                });

        public static CardDef DDObserver
            => SampleCards.Creature(1, "異次元の観測者",
                1, 1,
                effects: new[]
                {
                    new CardEffect(
                        "このカードが場にあるとき、場のほかのカードが除外されるたびに、このカードを+1/+0する。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                ExcludeCard: new(new[]
                                {
                                    new CardCondition(CardCondition.ContextConditionValue.Others)
                                }))))),
                        new[]
                        {
                            new EffectAction(
                                ModifyCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(CardCondition.ContextConditionValue.This)
                                            })),
                                    Power: new(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(1))))
                        }),
                });

        public static CardDef DDVisitor
            => SampleCards.Creature(1, "異次元からの来訪者",
                1, 1,
                effects: new[]
                {
                    new CardEffect(
                        "このカードを場に出したとき、このカードを+X/+0 する。X=すでに除外されている自分のカードの数",
                        new EffectConditionWrap(ByNotPlay: new (
                            Zone: ZonePrettyName.YouField,
                            When: new(new EffectTiming(MoveCard: new(
                                new[]{
                                    new CardCondition(CardCondition.ContextConditionValue.This)
                                },
                                To: ZonePrettyName.YouField
                                ))))),
                        new[]
                        {
                            new EffectAction(
                                ModifyCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(CardCondition.ContextConditionValue.This)
                                            })),
                                    Power: new(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(
                                            NumValueCalculator: new(
                                                ForCard: new(
                                                    NumValueCalculatorForCard.TypeValue.Count,
                                                    new Choice(
                                                        new ChoiceSource(
                                                            OrCardDefConditions: new[]
                                                            {
                                                                new CardDefCondition(
                                                                    OutZoneCondition: new(new[]
                                                                    {
                                                                        OutZonePrettyName.YouExcluded
                                                                    }))
                                                            }))))))
                                    ))
                        }),
                });

        public static CardDef ReturnFromDD
            => SampleCards.Sorcery(1, "異次元からの脱出",
                effects: new[]
                {
                    new CardEffect(
                        "自分の除外済みのクリーチャーカードをランダムで一つを選択する。そのカードを場に追加する。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                AddCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            OrCardDefConditions: new[]
                                            {
                                                new CardDefCondition(
                                                    TypeCondition: new(new[]
                                                    {
                                                        CardType.Creature
                                                    }),
                                                    OutZoneCondition: new(new[]
                                                    {
                                                        OutZonePrettyName.YouExcluded
                                                    }))
                                            }),
                                        Choice.HowValue.Random,
                                        new NumValue(1)),
                                    new ZoneValue(new[]
                                    {
                                        ZonePrettyName.YouField
                                    })))
                        }),
                });

        public static CardDef DDTransaction
            => SampleCards.Sorcery(1, "異次元との取引",
                effects: new[]
                {
                    new CardEffect(
                        "自分の手札をランダムに1枚除外する。その後、場のカードX枚をランダムに破壊する。X=除外したカードのコスト",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                ExcludeCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ContextCondition: CardCondition.ContextConditionValue.Others,
                                                    ZoneCondition: new(
                                                        new ZoneValue(new[]
                                                        {
                                                            ZonePrettyName.YouHand
                                                        })))
                                            }),
                                        Choice.HowValue.Random,
                                        new NumValue(1)),
                                    "exclude")),
                            new EffectAction(
                                DestroyCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    CardCondition.ContextConditionValue.Others,
                                                    ZoneCondition: new(
                                                        new ZoneValue(new[]
                                                        {
                                                            ZonePrettyName.YouField,
                                                            ZonePrettyName.OpponentField
                                                        })))
                                            }),
                                        Choice.HowValue.Random,
                                        new NumValue(
                                            NumValueCalculator: new(
                                                ForCard: new(
                                                    NumValueCalculatorForCard.TypeValue.CardCost,
                                                    new Choice(
                                                        new ChoiceSource(
                                                            orCardConditions: new[]
                                                            {
                                                                new CardCondition(
                                                                    ActionContext: new(
                                                                        ExcludeCard: new(
                                                                            "exclude",
                                                                            ActionContextCardsOfExcludeCard.TypeValue.Excluded)))
                                                            }))))))
                                    ))
                        }),
                });

        public static CardDef DDDraw
            => SampleCards.Sorcery(1, "異次元ドロー",
                effects: new[]
                {
                    new CardEffect(
                        "あなたのデッキからランダムに3枚除外する。その後、あなたは1枚ドローする。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                ExcludeCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(
                                                        new ZoneValue(new[]
                                                        {
                                                            ZonePrettyName.YouDeck
                                                        })))
                                            }),
                                        Choice.HowValue.Random,
                                        new NumValue(3)),
                                    "exclude")),
                            new EffectAction(
                                DrawCard: new(
                                    new NumValue(1),
                                    new PlayerCondition(PlayerCondition.ContextValue.You)
                                    ))
                        }),
                });

        public static CardDef ExcludHand
            => SampleCards.Sorcery(1, "異次元への追放",
                effects: new[]
                {
                    new CardEffect(
                        "あなたは残りHPの半分のダメージを受ける。その後、相手の手札をランダムに1枚選択し、それを除外する。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                Damage: new(
                                    new NumValue(
                                        NumValueCalculator: new(ForPlayer: new(
                                            NumValueCalculatorForPlayer.TypeValue.PlayerCurrentHp,
                                            new Choice(
                                                new ChoiceSource(
                                                    orPlayerConditions: new[]
                                                    {
                                                        new PlayerCondition(PlayerCondition.ContextValue.You)
                                                    })))),
                                        NumValueModifier: new(
                                            NumValueModifier.OperatorValue.Div,
                                            new NumValue(2))),
                                    new Choice(
                                        new ChoiceSource(
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition(PlayerCondition.ContextValue.You),
                                            })))
                                ),
                            new EffectAction(
                                ExcludeCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(
                                                        new ZoneValue(new[]
                                                        {
                                                            ZonePrettyName.OpponentHand
                                                        })))
                                            }),
                                        Choice.HowValue.Random,
                                        new NumValue(1)),
                                    "exclude")),
                        }),
                });

        public static CardDef DDFighter
            => SampleCards.Creature(2, "異次元の戦士",
                1, 1, abilities: new[] { CreatureAbility.Cover },
                effects: new[] {
                    new CardEffect(
                        "このカードの戦闘時に、このカードと戦闘相手を除外する。",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                AttackBefore: new(
                                    AttackCardCondition: new(
                                        CardCondition.ContextConditionValue.This),
                                    GuardCardCondition: new(
                                        CardCondition.ContextConditionValue.Others)))))),
                        new[]{
                            new EffectAction(
                                ExcludeCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    BattleEventContextCondition: CardCondition.BattleEventContextConditionValue.Guard),
                                                new CardCondition(
                                                    CardCondition.ContextConditionValue.This)
                                            })))),
                        }),
                    new CardEffect(
                        "",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                AttackBefore: new(
                                    AttackCardCondition: new(
                                        CardCondition.ContextConditionValue.Others),
                                    GuardCardCondition: new(
                                        CardCondition.ContextConditionValue.This)))))),
                        new[]{
                            new EffectAction(
                                ExcludeCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    BattleEventContextCondition: CardCondition.BattleEventContextConditionValue.Attack),
                                                new CardCondition(
                                                    CardCondition.ContextConditionValue.This)
                                            })))),
                        })
                });

        public static CardDef Parasite
            => SampleCards.Creature(1, "寄生虫",
                1, 1, isToken: true,
                effects: new[]
                {
                    new CardEffect(
                        "このカードがデッキから手札に移動したとき、このカードをあなたの場に出す。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouHand,
                            new EffectWhen(new EffectTiming(MoveCard: new(
                                OrCardConditions: new[]
                                {
                                    new CardCondition(CardCondition.ContextConditionValue.This)
                                },
                                ZonePrettyName.YouDeck, ZonePrettyName.YouHand))))),
                        new[]
                        {
                            new EffectAction(MoveCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ContextCondition: CardCondition.ContextConditionValue.This
                                            )
                                        })),
                                ZonePrettyName.YouField)),
                        }),
                    new CardEffect(
                        "このカードがあなたの場にある限り、あなたのターン終了時に、あなたは1ダメージを受ける。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(EndTurn: new(
                                OrPlayerCondition: new[]
                                {
                                    new PlayerCondition(PlayerCondition.ContextValue.You)
                                }))))),
                        new[]
                        {
                            new EffectAction(Damage: new(
                                new NumValue(1),
                                new Choice(
                                    new ChoiceSource(
                                        orPlayerConditions: new[]
                                        {
                                            new PlayerCondition(
                                                PlayerCondition.ContextValue.You)
                                        }))))
                        }),
                });

        public static CardDef Insector
            => SampleCards.Creature(1, "虫つかい",
                1, 1,
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、相手のデッキの1番上に「寄生虫」を1枚追加する。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(AddCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        OrCardDefConditions: new[]
                                        {
                                            new CardDefCondition(
                                                CardSetCondition: new(CardSetCondition.TypeValue.This),
                                                NameCondition: new(
                                                    new TextValue(Parasite.Name),
                                                    TextCompare.CompareValue.Equality),
                                                OutZoneCondition: new(new[]{ OutZonePrettyName.CardPool })
                                            )
                                        })),
                                new ZoneValue(new[]{ ZonePrettyName.OpponentDeck }),
                                InsertCardPosition: new(InsertCardPosition.PositionTypeValue.Top, 1))),
                        }),
                });

        public static CardDef EmergencyFood
            => SampleCards.Sorcery(1, "非常食",
                effects: new[] {
                    new CardEffect(
                        "あなたはランダムに手札を1枚捨てる。あなたはXのライフを得る。X=捨てたカードのコスト",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]{
                            new EffectAction(MoveCard: new(
                                new Choice(new ChoiceSource(
                                    orCardConditions: new[]
                                    {
                                        new CardCondition(
                                            ZoneCondition: new(new(new[]{ ZonePrettyName.YouHand }))
                                        )
                                    }),
                                    Choice.HowValue.Random,
                                    new NumValue(1)),
                                ZonePrettyName.YouCemetery,
                                Name: "moveCard"
                                )),
                            new EffectAction(ModifyPlayer: new(
                                new Choice(new ChoiceSource(
                                    orPlayerConditions: new[]
                                    {
                                        new PlayerCondition(PlayerCondition.ContextValue.You)
                                    })),
                                new PlayerModifier(
                                    Hp: new(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(NumValueCalculator: new(
                                            ForCard: new(
                                                NumValueCalculatorForCard.TypeValue.CardCost,
                                                new Choice(new ChoiceSource(
                                                    orCardConditions: new[]
                                                    {
                                                        new CardCondition(
                                                            ActionContext: new(MoveCard: new(
                                                                "moveCard",
                                                                ActionContextCardsOfMoveCard.TypeValue.Moved
                                                                ))
                                                        )
                                                    })))))))))
                        })
                });

        public static CardDef Gather
            => SampleCards.Sorcery(1, "集合",
                effects: new[] {
                    new CardEffect(
                        "あなたの手札に「ゴブリン」を3枚加える。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                AddCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            OrCardDefConditions: new[]
                                            {
                                                new CardDefCondition(
                                                    OutZoneCondition: new(new[]{ OutZonePrettyName.CardPool }),
                                                    CardSetCondition: new(CardSetCondition.TypeValue.This),
                                                    NameCondition: new(
                                                        new TextValue(Goblin.Name),
                                                        TextCompare.CompareValue.Equality
                                                    )
                                                )
                                            })),
                                    new ZoneValue(new[]{ ZonePrettyName.YouHand }),
                                    NumOfAddCards: 3)
                            )
                        })
                });

        public static CardDef Copy
            => SampleCards.Sorcery(2, "複製",
                effects: new[]
                {
                    new CardEffect(
                        "相手の場のカードを一枚選択する。そのカードと同名のカードを一枚あなたの手札に加える。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                AddCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new ZoneValue(new[]{ ZonePrettyName.OpponentField }))
                                                )
                                            }),
                                        Choice.HowValue.Choose,
                                        new NumValue(1)),
                                    new ZoneValue(new[]{ ZonePrettyName.YouHand })
                                    )
                            )
                        }
                    )
                });

        public static CardDef FirstAttack
            => SampleCards.Sorcery(2, "ゴブリンの一撃",
                effects: new[]
                {
                    new CardEffect(
                        "相手プレイヤーか、あなたの場か、相手の場にあるクリーチャー1体を選択する。それに1ダメージを与える。" +
                        "あなたの手札に「ゴブリンの二撃」1枚を加える。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                Damage: new(
                                    new NumValue(1),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new ZoneValue(new[]{ ZonePrettyName.YouField, ZonePrettyName.OpponentField })),
                                                    TypeCondition: new(new[]{ CardType.Creature })
                                                )
                                            },
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition(
                                                    PlayerCondition.ContextValue.Opponent
                                                )
                                            }),
                                        Choice.HowValue.Choose,
                                        new NumValue(1))),
                                AddCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            OrCardDefConditions: new[]
                                            {
                                                new CardDefCondition(
                                                    OutZoneCondition: new(new[]{ OutZonePrettyName.CardPool }),
                                                    CardSetCondition: new(CardSetCondition.TypeValue.This),
                                                    NameCondition: new(
                                                        new TextValue(SecondAttack.Name),
                                                        TextCompare.CompareValue.Equality)
                                                )
                                            }),
                                        Choice.HowValue.All,
                                        new NumValue(1)),
                                    new ZoneValue(new[]{ ZonePrettyName.YouHand })
                                    )
                            )
                        }
                    )
                });

        public static CardDef SecondAttack
            => SampleCards.Sorcery(2, "ゴブリンの二撃",
                isToken: true,
                effects: new[]
                {
                    new CardEffect(
                        "相手プレイヤーか、あなたの場か、相手の場にあるクリーチャー1体を選択する。それに2ダメージを与える。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                Damage: new(
                                    new NumValue(2),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new ZoneValue(new[]{ ZonePrettyName.YouField, ZonePrettyName.OpponentField })),
                                                    TypeCondition: new(new[]{ CardType.Creature })
                                                )
                                            },
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition(
                                                    PlayerCondition.ContextValue.Opponent
                                                )
                                            }),
                                        Choice.HowValue.Choose,
                                        new NumValue(1)))
                            )
                        }
                    )
                });

        public static CardDef HolyShield
            => SampleCards.Sorcery(2, "聖なる盾",
                effects: new[]
                {
                    new CardEffect(
                        "あなたの場にある他のカードに次の効果を付与する。「ターン終了時まで、受けるダメージは0になる。」",
                        new EffectConditionWrap(ByPlay: new ()),
                        new[]{
                            new EffectAction(
                                AddEffect: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new (new(new[]{ ZonePrettyName.YouField })),
                                                    TypeCondition: new(new[]{ CardType.Creature }),
                                                    ContextCondition: CardCondition.ContextConditionValue.Others
                                                )
                                            })),
                                    new[]
                                    {
                                        new CardEffect(
                                            "ターン終了時まで、受けるダメージは0になる。",
                                            new EffectConditionWrap(ByNotPlay: new(
                                                ZonePrettyName.YouField,
                                                new EffectWhen(new EffectTiming(DamageBefore: new(
                                                    TakeCardCondition: new(
                                                        ContextCondition: CardCondition.ContextConditionValue.This
                                                    )))),
                                                While: new(new EffectTiming(EndTurn: new(
                                                    OrPlayerCondition: new[]
                                                    {
                                                        new PlayerCondition(PlayerCondition.ContextValue.You)
                                                    })),
                                                    0, 1)
                                                )),
                                            new[]
                                            {
                                                new EffectAction(
                                                    ModifyDamage: new(
                                                        new NumValueModifier(
                                                            NumValueModifier.OperatorValue.Replace,
                                                            new NumValue(0))
                                                    )
                                                )
                                            })
                                    })
                                )
                        }
                    )
                });

        public static CardDef ChangeHands => SampleCards.Sorcery(2, "手札入れ替え",
            effects: new[]
            {
                new CardEffect(
                    "あなたは手札をすべて捨てる。あなたは捨てた枚数カードをドローする。",
                    new EffectConditionWrap(
                        ByPlay: new EffectConditionByPlaying()),
                    new[]
                    {
                        // 手札をすべて捨てる
                        new EffectAction(
                            MoveCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ZoneCondition: new(new(new[]{ ZonePrettyName.YouHand }))
                                            )
                                        })),
                                ZonePrettyName.YouCemetery,
                                Name: "discard")
                            ),

                        // 捨てたカードと同じ枚数引く
                        new EffectAction(
                            DrawCard: new(
                                new NumValue(
                                    NumValueCalculator: new(ForCard: new(
                                        NumValueCalculatorForCard.TypeValue.Count,
                                        new Choice(new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ActionContext: new(
                                                        MoveCard: new(
                                                            "discard",
                                                            ActionContextCardsOfMoveCard.TypeValue.Moved
                                                            )))
                                            }))))),
                                new PlayerCondition(PlayerCondition.ContextValue.You)
                                )),
                    }
                ),
            });

        public static CardDef Slap
            => SampleCards.Sorcery(2, "袋叩き",
                effects: new[]
                {
                    // 使用時、対象の相手クリーチャー一体にxダメージ。x="自分の場のクリーチャーの数"
                    new CardEffect(
                        "相手の場にあるクリーチャー1体を選択する。それにXダメージを与える。X=あなたの場にあるクリーチャーの数",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                Damage: new EffectActionDamage(
                                    new NumValue(
                                        NumValueCalculator: new(
                                            ForCard: new(
                                                NumValueCalculatorForCard.TypeValue.Count,
                                                new Choice(
                                                    new ChoiceSource(
                                                        orCardConditions: new[]
                                                        {
                                                            new CardCondition(
                                                                TypeCondition: new(new[]{CardType.Creature}),
                                                                ZoneCondition: new(
                                                                    new ZoneValue(new[]{ZonePrettyName.YouField}))
                                                            )
                                                        }))))),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new(new[]{ ZonePrettyName.OpponentField })),
                                                    TypeCondition: new CardTypeCondition(new []{CardType.Creature, })
                                                )
                                            }),
                                        Choice.HowValue.Choose,
                                        new NumValue(1))
                                )
                            )
                        }
                    )
                });

        public static CardDef Ramp
            => SampleCards.Sorcery(2, "ランプ",
                effects: new[]
                {
                    new CardEffect(
                        "あなたのMPの最大値を1増加し、利用可能なMPを1減少する。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                ModifyPlayer: new(
                                    new Choice(new ChoiceSource(
                                        orPlayerConditions: new[]
                                        {
                                            new PlayerCondition(PlayerCondition.ContextValue.You)
                                        })),
                                    new PlayerModifier(
                                        MaxMp: new NumValueModifier(
                                            NumValueModifier.OperatorValue.Add,
                                            new NumValue(1)),
                                        Mp: new NumValueModifier(
                                            NumValueModifier.OperatorValue.Sub,
                                            new NumValue(1))
                                        ))
                            )
                        }
                    )
                });

        public static CardDef BounceHand
            => SampleCards.Sorcery(2, "手札へ戻す",
                effects: new[] {
                    new CardEffect(
                        "場のカード1枚を選択する。選択したカードを持ち主の手札に移動する。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new ZoneValue(
                                                        new[]{ ZonePrettyName.YouField, ZonePrettyName.OpponentField }))
                                                )
                                            }),
                                        Choice.HowValue.Choose,
                                        new NumValue(1)),
                                    ZonePrettyName.OwnerHand))
                        })
                });

        public static CardDef BounceDeck
            => SampleCards.Sorcery(2, "デッキへ戻す",
                effects: new[] {
                    new CardEffect(
                        "場のカード1枚を選択する。選択したカードを持ち主のデッキのランダムな位置に移動する。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new ZoneValue(
                                                        new[]{ ZonePrettyName.YouField, ZonePrettyName.OpponentField }))
                                                )
                                            }),
                                        Choice.HowValue.Choose,
                                        new NumValue(1)),
                                    ZonePrettyName.OwnerDeck,
                                    new InsertCardPosition(InsertCardPosition.PositionTypeValue.Random)))
                        })
                });

        public static CardDef DoubleCopy
            => SampleCards.Sorcery(3, "二重複製",
                effects: new[]
                {
                    new CardEffect(
                        "相手の場のカードを一枚選択する。そのカードと同名のカードを二枚あなたの手札に加える。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                AddCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new ZoneValue(new[]{ ZonePrettyName.OpponentField }))
                                                )
                                            }),
                                        Choice.HowValue.Choose
                                        ),
                                    new ZoneValue(new[]{ ZonePrettyName.YouHand }),
                                    NumOfAddCards: 2)
                            )
                        }
                    )
                });

        public static CardDef FullAttack
            => SampleCards.Sorcery(3, "一斉射撃",
                effects: new[]
                {
                    new CardEffect(
                        "相手プレイヤーか、相手の場にあるクリーチャーすべてに、1ダメージを与える。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                Damage: new EffectActionDamage(
                                    new NumValue(1),
                                    new Choice(
                                        new ChoiceSource(
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition(PlayerCondition.ContextValue.Opponent)
                                            },
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new(new[]{ ZonePrettyName.OpponentField })),
                                                    TypeCondition: new CardTypeCondition(new[]{ CardType.Creature })
                                                )
                                            })))
                            )
                        }
                    )
                });

        public static CardDef Search
            => SampleCards.Sorcery(3, "探索",
                effects: new[]
                {
                    new CardEffect(
                        "あなたのデッキからランダムなカード1枚を、あなたの手札に加える。そのカードのコストを半分にする。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new ZoneValue(new[]{ ZonePrettyName.YouDeck }))
                                                )
                                            }),
                                        Choice.HowValue.Random,
                                        numPicks: new NumValue(1)),
                                    ZonePrettyName.YouHand,
                                    Name: "search_card")
                            ),
                            new EffectAction(
                                ModifyCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ActionContext: new(MoveCard: new(
                                                        "search_card",
                                                        ActionContextCardsOfMoveCard.TypeValue.Moved
                                                        ))
                                                )
                                            }
                                        )),
                                    Cost: new(
                                        NumValueModifier.OperatorValue.Div,
                                        new NumValue(2)
                                        ))
                            )
                        }
                    )
                });

        public static CardDef GoblinCaptureJar
            => SampleCards.Sorcery(4, "ゴブリン封印の壺",
                effects: new[]
                {
                    new CardEffect(
                        "あなたの場か、相手の場にあるクリーチャーのうち、名前に「ゴブリン」を含むカードすべてのパワーを1にし、「封印」状態にする。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                ModifyCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    NameCondition: new(
                                                        new TextValue("ゴブリン"),
                                                        TextCompare.CompareValue.Contains),
                                                    TypeCondition: new(new[]{ CardType.Creature }),
                                                    ZoneCondition: new(new(new[]{ ZonePrettyName.YouField, ZonePrettyName.OpponentField }))
                                                )
                                            })),
                                    Power: new(NumValueModifier.OperatorValue.Replace,
                                        new NumValue(1)),
                                    Ability: new(CreatureAbilityModifier.OperatorValue.Add,
                                        CreatureAbility.Sealed)
                                    )
                            )
                        }
                    )
                });

        public static CardDef Virus
            => SampleCards.Sorcery(5, "ウイルス",
                effects: new[]
                {
                    new CardEffect(
                        "相手の場と相手の手札にあるパワー4以上のクリーチャーをすべて墓地に移動する。" +
                        "2回後の相手ターン終了時まで、相手のドローしたパワー4以上のクリーチャーを墓地へ移動する。",
                        new EffectConditionWrap(ByPlay: new()),
                        new[]
                        {
                            new EffectAction(DestroyCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                PowerCondition: new NumCompare(
                                                    4, NumCompare.CompareValue.GreaterThan),
                                                ZoneCondition: new(
                                                    new ZoneValue(new[]{
                                                        ZonePrettyName.OpponentField,
                                                        ZonePrettyName.OpponentHand
                                                        })))
                                        })))
                            ),
                            new EffectAction(ReserveEffect: new(
                                new[]
                                {
                                    new CardEffect(
                                        "",
                                        new EffectConditionWrap(Reserve: new(
                                            new EffectWhen(new EffectTiming(
                                                MoveCard: new(
                                                    OrCardConditions: new[]
                                                    {
                                                        new CardCondition(
                                                            PowerCondition: new NumCompare(
                                                                4, NumCompare.CompareValue.GreaterThan))
                                                    },
                                                    ZonePrettyName.OpponentDeck,
                                                    ZonePrettyName.OpponentHand))),
                                            While: new(new EffectTiming(EndTurn: new(
                                                OrPlayerCondition: new[]
                                                {
                                                    new PlayerCondition(PlayerCondition.ContextValue.Opponent)
                                                })),
                                                0, 2))),
                                        new[]
                                        {
                                            new EffectAction(MoveCard: new(
                                                new Choice(
                                                    new ChoiceSource(
                                                        orCardConditions: new[]
                                                        {
                                                            new CardCondition(
                                                                CardCondition.ContextConditionValue.EventSource)
                                                        })),
                                                ZonePrettyName.YouCemetery
                                                )
                                            )
                                        }
                                    )
                                }))
                        }
                    ),
                });

        public static CardDef OldShield
            => SampleCards.Artifact(1, "ぼろの盾",
                effects: new[]
                {
                    new CardEffect(
                        "あなたの場にあるクリーチャーがダメージを受けるとき、そのダメージを1軽減する。その後、このカードを破壊する。",
                        new EffectConditionWrap(
                            ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new(new(
                                DamageBefore: new(
                                    TakeCardCondition: new CardCondition(
                                        TypeCondition: new(new[]{ CardType.Creature }),
                                        ZoneCondition: new(new(new[]{ ZonePrettyName.YouField })),
                                        ContextCondition: CardCondition.ContextConditionValue.Others
                                    )))))),
                        new[]
                        {
                            new EffectAction(
                                ModifyDamage: new EffectActionModifyDamage(
                                    new NumValueModifier(
                                        NumValueModifier.OperatorValue.Sub,
                                        new NumValue(1)
                                    )
                                )
                            ),
                            new EffectAction(
                                DestroyCard: new EffectActionDestroyCard(
                                    new Choice(new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ContextCondition: CardCondition.ContextConditionValue.This
                                            )
                                        }))
                                )
                            )
                        }
                    )
                });

        public static CardDef OldWall
            => SampleCards.Artifact(1, "ぼろの壁",
                effects: new[]
                {
                    new CardEffect(
                        "あなたか、あなたの場にあるクリーチャーがダメージを受けるとき、そのダメージを1軽減する。",
                        new EffectConditionWrap(
                            ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new(new(
                                DamageBefore: new(
                                    TakePlayerCondition: new PlayerCondition(
                                        PlayerCondition.ContextValue.You
                                    )
                                    )
                                ))
                        )),
                        new[]
                        {
                            new EffectAction(
                                ModifyDamage: new EffectActionModifyDamage(
                                    new NumValueModifier(
                                        NumValueModifier.OperatorValue.Sub,
                                        new NumValue(1)
                                    )
                                )
                            ),
                            new EffectAction(
                                DestroyCard: new EffectActionDestroyCard(
                                    new Choice(new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ContextCondition: CardCondition.ContextConditionValue.This
                                            )
                                        }))
                                )
                            )
                        }
                    ),
                    new CardEffect(
                        "その後、このカードを破壊する。",
                        new EffectConditionWrap(
                            ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new(new(
                                DamageBefore: new(
                                    TakeCardCondition: new CardCondition(
                                        TypeCondition: new CardTypeCondition(new[]{ CardType.Creature }),
                                        ZoneCondition: new(new(new[]{ ZonePrettyName.YouField })),
                                        ContextCondition: CardCondition.ContextConditionValue.Others
                                    ))
                                ))
                        )),
                        new[]
                        {
                            new EffectAction(
                                ModifyDamage: new EffectActionModifyDamage(
                                    new NumValueModifier(
                                        NumValueModifier.OperatorValue.Sub,
                                        new NumValue(1)
                                    )
                                )
                            ),
                            new EffectAction(
                                DestroyCard: new EffectActionDestroyCard(
                                    new Choice(new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ContextCondition: CardCondition.ContextConditionValue.This
                                            )
                                        }))
                                )
                            )
                        }
                    )
                });

        public static CardDef GoblinStatue
            => SampleCards.Artifact(4, "呪いのゴブリン像",
                effects: new[]
                {
                    new CardEffect(
                        "あなたのターン終了時、もしあなたの墓地が30枚以上なら、相手プレイヤー、相手の場にあるクリーチャーすべてに6ダメージを与える。",
                        new EffectConditionWrap(
                            ByNotPlay: new(ZonePrettyName.YouField,
                                new(new(EndTurn: new(
                                    OrPlayerCondition: new[]
                                    {
                                        new PlayerCondition(PlayerCondition.ContextValue.You)
                                    }))),
                            If: new(new ConditionWrap(NumCondition: new(
                                new NumValue(NumValueCalculator: new(
                                    ForCard: new(
                                        NumValueCalculatorForCard.TypeValue.Count,
                                        new Choice(new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new(new[]{ ZonePrettyName.YouCemetery }))
                                                )
                                            }))))),
                                new NumCompare(
                                    30, NumCompare.CompareValue.GreaterThan)
                                    ))))),
                        new[]{
                            new EffectAction(Damage: new(
                                new NumValue(6),
                                new Choice(new ChoiceSource(
                                    orPlayerConditions: new[]
                                    {
                                        new PlayerCondition(PlayerCondition.ContextValue.Opponent)
                                    },
                                    orCardConditions: new[]
                                    {
                                        new CardCondition(
                                            ZoneCondition: new(new(new[]{ ZonePrettyName.OpponentField })),
                                            TypeCondition: new(new[]{ CardType.Creature })
                                        )
                                    }))))
                        }
                    )
                });

        public static CardDef HolyStatue
            => SampleCards.Artifact(4, "癒やしの像",
                effects: new[]
                {
                    // 使用時、すべての自分クリーチャーを+0/+1
                    new CardEffect(
                        "あなたの場にあるクリーチャーすべては+0/+1 の修整を受ける。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                ModifyCard: new EffectActionModifyCard(
                                    new Choice(new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ZoneCondition: new(new(new[]{ ZonePrettyName.YouField })),
                                                TypeCondition: new CardTypeCondition(new[]{ CardType.Creature })
                                            )
                                        })),
                                    Toughness: new NumValueModifier(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(1))
                                )
                            )
                        }),

                    // 自分クリーチャーのプレイ時+0/+1
                    new CardEffect(
                        "あなたが場にクリーチャーを出したとき、それは+0/+1 の修整を受ける。",
                        new EffectConditionWrap(
                            ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new(new(MoveCard: new(
                                OrCardConditions: new[]
                                {
                                    new CardCondition(CardCondition.ContextConditionValue.Others)
                                },
                                ZonePrettyName.YouHand,
                                ZonePrettyName.YouField)))
                        )),
                        new[]
                        {
                            new EffectAction(
                                ModifyCard: new EffectActionModifyCard(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new(new[]{ ZonePrettyName.YouField })),
                                                    TypeCondition: new CardTypeCondition(new[]{ CardType.Creature }),
                                                    ContextCondition: CardCondition.ContextConditionValue.EventSource
                                                )
                                            })),
                                    Toughness: new NumValueModifier(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(1))
                                )
                            )
                        })
                });

        public static CardDef VictoryRoad
            => SampleCards.Artifact(1, "勝利への道",
                effects: new[]
                {
                    new CardEffect(
                        "あなたのターン開始時に、場にあるこのカードを墓地に移動する。" ,
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(StartTurn: new(
                                OrPlayerCondition: new[]
                                {
                                    new PlayerCondition(PlayerCondition.ContextValue.You)
                                }))))),
                        new[]
                        {
                            new EffectAction(
                                DestroyCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ContextCondition: CardCondition.ContextConditionValue.This
                                                )
                                            }))))
                        }),
                    new CardEffect(
                        "場にあるこのカードが墓地に移動されたとき、「勝利の像」1枚をあなたの場に追加する。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouCemetery,
                            new EffectWhen(new EffectTiming(
                                MoveCard: new(
                                    OrCardConditions: new[]
                                    {
                                        new CardCondition(CardCondition.ContextConditionValue.This)
                                    },
                                    ZonePrettyName.YouField,
                                    ZonePrettyName.YouCemetery))))),
                        new[]
                        {
                            new EffectAction(
                                AddCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            OrCardDefConditions: new[]
                                            {
                                                new CardDefCondition(
                                                    NameCondition: new(
                                                        new TextValue(VictoryStatue.Name),
                                                        TextCompare.CompareValue.Equality),
                                                    OutZoneCondition: new(
                                                        new[]
                                                        {
                                                            OutZonePrettyName.CardPool
                                                        })
                                                )
                                            })),
                                    new ZoneValue(new[]
                                    {
                                        ZonePrettyName.YouField
                                    })))
                        })
                });

        public static CardDef VictoryStatue
            => SampleCards.Artifact(10, "勝利の像",
                isToken: true,
                effects: new[]
                {
                    new CardEffect(
                        "あなたのターン開始時に、このカードがあなたの場にあるとき、あなたはゲームに勝利する。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(StartTurn: new(
                                OrPlayerCondition: new[]
                                {
                                    new PlayerCondition(PlayerCondition.ContextValue.You)
                                }))))),
                        new[]
                        {
                            new EffectAction(
                                Win: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition(
                                                    PlayerCondition.ContextValue.You)
                                            }))))
                        }),
                });

        public static CardDef MagicObject
            => SampleCards.Creature(1, "魔力吸収体",
                1, 1, annotations: new[] { ":魔導" },
                effects: new[]
                {
                    new CardEffect(
                        "いずれかのプレイヤーが魔法をプレイするたびに、場のこのカードに「魔導」カウンターを1つ置く。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(Play: new(
                                OrCardConditions: new[]
                                {
                                    new CardCondition(
                                        CardCondition.ContextConditionValue.Others,
                                        TypeCondition: new(new[]{ CardType.Sorcery })
                                        )
                                }))))),
                        new[]
                        {
                            new EffectAction(
                                ModifyCounter: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(CardCondition.ContextConditionValue.This)
                                            })),
                                    "魔導",
                                    new NumValueModifier(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(1))))
                        }),

                    new CardEffect(
                        "このカードに「魔導」カウンターが置かれるたびに、このカードを+1/+0する。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                ModifyCounter: new(
                                    OrCardConditions: new[]
                                    {
                                        new CardCondition(CardCondition.ContextConditionValue.This)
                                    },
                                    "魔導",
                                    EffectTimingModifyCounterOnCardEvent.OperatorValue.Add))))),
                        new[]
                        {
                            new EffectAction(
                                ModifyCard: new(
                                    new Choice(new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(CardCondition.ContextConditionValue.This)
                                        })),
                                    Power: new(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(1))))
                        }),

                    new CardEffect(
                        "このカードから「魔導」カウンターが取り除かれるたびに、このカードを-1/+0する。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                ModifyCounter: new(
                                    OrCardConditions: new[]
                                    {
                                        new CardCondition(CardCondition.ContextConditionValue.This)
                                    },
                                    "魔導",
                                    EffectTimingModifyCounterOnCardEvent.OperatorValue.Remove))))),
                        new[]
                        {
                            new EffectAction(
                                ModifyCard: new(
                                    new Choice(new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(CardCondition.ContextConditionValue.This)
                                        })),
                                    Power: new(
                                        NumValueModifier.OperatorValue.Sub,
                                        new NumValue(1))))
                        })
                });

        public static CardDef MagicMonster
            => SampleCards.Creature(5, "魔法生物",
                3, 4, annotations: new[] { ":魔導" },
                effects: new[]
                {
                    new CardEffect(
                        "あなたが魔法をプレイするたびに、このカードに「魔導」カウンターを1つ置く。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouHand,
                            new EffectWhen(new EffectTiming(
                                Play: new(
                                    OrCardConditions: new[]
                                    {
                                        new CardCondition(
                                            ContextCondition: CardCondition.ContextConditionValue.Others,
                                            OwnerCondition: CardCondition.OwnerConditionValue.You,
                                            TypeCondition: new(new[]{ CardType.Sorcery })
                                            )
                                    }))))),
                        new[]
                        {
                            new EffectAction(
                                ModifyCounter: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(CardCondition.ContextConditionValue.This)
                                            })),
                                    "魔導",
                                    new NumValueModifier(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(1))))
                        }),

                    new CardEffect(
                        "このカードに「魔導」カウンターが1つ置かれるたびに、このカードのコスト-1。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouHand,
                            new EffectWhen(new EffectTiming(
                                ModifyCounter: new(
                                    OrCardConditions: new[]
                                    {
                                        new CardCondition(CardCondition.ContextConditionValue.This)
                                    },
                                    "魔導",
                                    EffectTimingModifyCounterOnCardEvent.OperatorValue.Add))))),
                        new[]
                        {
                            new EffectAction(
                                ModifyCard: new(
                                    new Choice(new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(CardCondition.ContextConditionValue.This)
                                        })),
                                    Cost: new(
                                        NumValueModifier.OperatorValue.Sub,
                                        new NumValue(1))))
                        })
                });

        public static CardDef BeginnerSorcerer
            => SampleCards.Creature(2, "初級魔導士",
                1, 2,
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、あなたの場の「:魔導」を持つカード1枚を選択する。そのカードに「魔導」カウンターを2つ置く。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                ModifyCounter: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    CardCondition.ContextConditionValue.Others,
                                                    ZoneCondition: new(new ZoneValue(new[]
                                                    {
                                                        ZonePrettyName.YouField
                                                    })),
                                                    AnnotationCondition: new(":魔導")
                                                    ),
                                            }),
                                        Choice.HowValue.Choose,
                                        new NumValue(1)),
                                    "魔導",
                                    new NumValueModifier(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(2))))
                        }),
                });

        public static CardDef GreatSorcerer
            => SampleCards.Creature(9, "偉大な魔導士",
                5, 5, annotations: new[] { ":魔導" },
                effects: new[]
                {
                    // 魔法をプレイするたび、カウンターを置く。
                    new CardEffect(
                        "あなたが魔法をプレイするたびに、手札のこのカードに「魔導」カウンターを1つ置く。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouHand,
                            new EffectWhen(new EffectTiming(
                                Play: new(
                                    OrCardConditions: new[]
                                    {
                                        new CardCondition(
                                            CardCondition.ContextConditionValue.Others,
                                            OwnerCondition: CardCondition.OwnerConditionValue.You,
                                            TypeCondition: new(new[]{ CardType.Sorcery })
                                            )
                                    }))))),
                        new[]
                        {
                            new EffectAction(
                                ModifyCounter: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(CardCondition.ContextConditionValue.This)
                                            })),
                                    "魔導",
                                    new NumValueModifier(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(1))))
                        }),

                    // カウンターが置かれるたび、コスト-1
                    new CardEffect(
                        "このカードに「魔導」カウンターが1つ置かれるたびに、このカードのコスト-1。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouHand,
                            new EffectWhen(new EffectTiming(
                                ModifyCounter: new(
                                    OrCardConditions: new[]
                                    {
                                        new CardCondition(CardCondition.ContextConditionValue.This)
                                    },
                                    "魔導",
                                    EffectTimingModifyCounterOnCardEvent.OperatorValue.Add))))),
                        new[]
                        {
                            new EffectAction(
                                ModifyCard: new(
                                    new Choice(new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(CardCondition.ContextConditionValue.This)
                                        })),
                                    Cost: new(
                                        NumValueModifier.OperatorValue.Sub,
                                        new NumValue(1))))
                        }),

                    // カードをプレイしたとき
                    new CardEffect(
                        "このカードをプレイしたとき、あなたの手札をすべて除外する。" +
                        "その後、あなたは手札を5枚引く。" +
                        "その後、あなたの手札にある「:魔導」カードすべてに「魔導」カウンターを5つ置く。",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                ExcludeCard: new(
                                    new Choice(new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ZoneCondition: new(new ZoneValue(new[]
                                                {
                                                    ZonePrettyName.YouHand
                                                })))
                                        })))),
                            new EffectAction(
                                DrawCard: new(
                                    new NumValue(5),
                                    new PlayerCondition(PlayerCondition.ContextValue.You))),
                            new EffectAction(
                                ModifyCounter: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new ZoneValue(new[]
                                                    {
                                                        ZonePrettyName.YouHand
                                                    })),
                                                    AnnotationCondition: new(":魔導")
                                                    )
                                            })),
                                    "魔導",
                                    new NumValueModifier(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(5))))
                        })
                });

        public static CardDef UltraMagic
            => SampleCards.Sorcery(3, "大魔法",
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、あなたの場からすべての「魔導」カウンターを取り除く。" +
                            "その後、相手にXのダメージを与える。X=取り除いた「魔導」カウンターの数+1",
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                ModifyCounter: new(
                                    new Choice(new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ZoneCondition: new(new ZoneValue(new[]
                                                {
                                                    ZonePrettyName.YouField
                                                })))
                                        })),
                                    "魔導",
                                    new NumValueModifier(
                                        NumValueModifier.OperatorValue.Replace,
                                        new NumValue(0)
                                    ),
                                    "remove_counters")),
                            new EffectAction(
                                Damage: new(
                                    new NumValue(
                                        NumValueCalculator: new(
                                            ForCounter: new(
                                                ActionContextCounters: new(
                                                    OfModifyCounter: new(
                                                        "remove_counters",
                                                        ActionContextCountersOfModifyCounter.TypeValue.Modified)))),
                                        NumValueModifier: new(
                                            NumValueModifier.OperatorValue.Add,
                                            new NumValue(1))),
                                    new Choice(new ChoiceSource(
                                        orPlayerConditions: new[]
                                        {
                                            new PlayerCondition(PlayerCondition.ContextValue.Opponent)
                                        }))))
                        }),
                });

        public static CardDef MagicFighter
            => SampleCards.Creature(3, "魔導の戦士",
                1, 2, abilities: new[] { CreatureAbility.Cover },
                effects: new[] {
                    new CardEffect(
                        "このカードが攻撃されたとき、攻撃したカードを持ち主の手札に移動する。",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                AttackAfter: new(
                                    GuardCardCondition: new(
                                        ContextCondition: CardCondition.ContextConditionValue.This
                                    )))))),
                        new[]
                        {
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    BattleEventContextCondition: CardCondition.BattleEventContextConditionValue.Attack,
                                                    ZoneCondition: new(
                                                        new ZoneValue(new[]{
                                                            ZonePrettyName.OpponentField,
                                                            ZonePrettyName.YouField,
                                                        }))
                                                )
                                            })),
                                    ZonePrettyName.OwnerHand))
                        })
                });

        public static CardDef SuperMagicFighter
            => SampleCards.Creature(4, "魔導の戦士長",
                1, 2, abilities: new[] { CreatureAbility.Cover },
                effects: new[] {
                    new CardEffect(
                        "このカードが攻撃されたとき、攻撃したカードを相手のデッキの一番上に移動する。",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                AttackAfter: new(
                                    GuardCardCondition: new(
                                        ContextCondition: CardCondition.ContextConditionValue.This
                                    )))))),
                        new[]
                        {
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    BattleEventContextCondition: CardCondition.BattleEventContextConditionValue.Attack,
                                                    ZoneCondition: new(
                                                        new ZoneValue(new[]{
                                                            ZonePrettyName.OpponentField,
                                                            ZonePrettyName.YouField,
                                                        }))
                                                )
                                            })),
                                    ZonePrettyName.OpponentDeck,
                                    new InsertCardPosition(
                                        InsertCardPosition.PositionTypeValue.Top,
                                        1)))
                        })
                });

        public static CardDef DashGoblin
            => SampleCards.Creature(2, "特攻ゴブリン", 1, 1,
                effects: new[]
                {
                    new CardEffect(
                        "このカードが場に出たとき、相手プレイヤーに1ダメージを与える。",
                        new EffectConditionWrap(ByNotPlay: new (
                            Zone: ZonePrettyName.YouField,
                            When: new(new EffectTiming(MoveCard: new(
                                new[]{
                                    new CardCondition(CardCondition.ContextConditionValue.This)
                                },
                                To: ZonePrettyName.YouField
                                ))))),
                        new[]
                        {
                            new EffectAction(
                                Damage: new(
                                    new NumValue(1),
                                    new Choice(new ChoiceSource(
                                        orPlayerConditions: new[]
                                        {
                                            new PlayerCondition(PlayerCondition.ContextValue.Opponent)
                                        })))),
                        }),
                });

        public static CardDef Investment
            => Sorcery(1, "投資",
                effects: new[]
                {
                    new CardEffect(
                        "1ターン後のターン終了時に、あなたはカードを2枚ドローする。",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouCemetery,
                            When: new(new EffectTiming(EndTurn:new(
                                new[]
                                {
                                    new PlayerCondition(PlayerCondition.ContextValue.You)
                                }))),
                            While: new(new EffectTiming(EndTurn: new(
                                new[]{
                                    new PlayerCondition(PlayerCondition.ContextValue.You)
                                })),
                                1, 1
                                ))),
                        new[]
                        {
                            new EffectAction(DrawCard: new(
                                new NumValue(2),
                                new PlayerCondition(PlayerCondition.ContextValue.You)))
                        })
                });

        public static CardDef GoblinLover
            => Creature(1, "ゴブリン好き", 1, 1,
                effects: new[]
                {
                    new CardEffect(
                        "このカードのプレイ時、あなたの場にほかの「ゴブリン」と名の付くカードがあるとき、このカードを+1/+1する。",
                        new EffectConditionWrap(
                            ByPlay: new(
                                If: new(new ConditionWrap(NumCondition: new(
                                        new NumValue(NumValueCalculator: new(ForCard: new(
                                            NumValueCalculatorForCard.TypeValue.Count,
                                            new Choice(new ChoiceSource(
                                                orCardConditions: new[]{
                                                    new CardCondition(
                                                        CardCondition.ContextConditionValue.OtherDefs,
                                                        NameCondition: new(
                                                            new TextValue("ゴブリン"),
                                                            TextCompare.CompareValue.Contains),
                                                        ZoneCondition: new(new ZoneValue(new[]{ ZonePrettyName.YouField }))
                                                        )
                                                }))
                                            ))),
                                        new NumCompare(
                                            1,
                                            NumCompare.CompareValue.GreaterThan)
                                        )))
                                )),
                        new[]
                        {
                            new EffectAction(ModifyCard: new(
                                new Choice(new ChoiceSource(
                                    orCardConditions: new[]
                                    {
                                        new CardCondition(CardCondition.ContextConditionValue.This)
                                    })),
                                Power: new(
                                    NumValueModifier.OperatorValue.Add,
                                    new NumValue(1)),
                                Toughness: new(
                                    NumValueModifier.OperatorValue.Add,
                                    new NumValue(1))
                                ))
                        })
                });

        public static CardDef Key1
            => Sorcery(1, "封印の鍵1",
                effects: Array.Empty<CardEffect>()
                );

        public static CardDef Key2
            => Sorcery(1, "封印の鍵2",
                effects: Array.Empty<CardEffect>()
                );

        public static CardDef SealedGoblin
            => Creature(1, "封印されしゴブリン", 1, 1,
                effects: new[]
                {
                    new CardEffect(
                        "このカードと「封印の鍵1」「封印の鍵2」があなたの手札にそろったとき、あなたはゲームに勝利する。",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouHand,
                            If: new(new ConditionWrap(ConditionAnd: new(new[]
                            {
                                new ConditionWrap(NumCondition: new(
                                        new NumValue(NumValueCalculator: new(ForCard: new(
                                            NumValueCalculatorForCard.TypeValue.Count,
                                            new Choice(new ChoiceSource(
                                                orCardConditions: new[]{
                                                    new CardCondition(
                                                        NameCondition: new(
                                                            new TextValue("封印の鍵1"),
                                                            TextCompare.CompareValue.Equality),
                                                        ZoneCondition: new(new ZoneValue(new[]{ ZonePrettyName.YouHand }))
                                                        )
                                                }))
                                            ))),
                                        new NumCompare(
                                            1,
                                            NumCompare.CompareValue.GreaterThan)
                                        )),
                                new ConditionWrap(NumCondition: new(
                                        new NumValue(NumValueCalculator: new(ForCard: new(
                                            NumValueCalculatorForCard.TypeValue.Count,
                                            new Choice(new ChoiceSource(
                                                orCardConditions: new[]{
                                                    new CardCondition(
                                                        NameCondition: new(
                                                            new TextValue("封印の鍵2"),
                                                            TextCompare.CompareValue.Equality),
                                                        ZoneCondition: new(new ZoneValue(new[]{ ZonePrettyName.YouHand }))
                                                        )
                                                }))
                                            ))),
                                        new NumCompare(
                                            1,
                                            NumCompare.CompareValue.GreaterThan)
                                        ))
                            })))
                            )),
                        new[]
                        {
                            new EffectAction(Win: new(
                                new Choice(new ChoiceSource(
                                    orPlayerConditions: new[]
                                    {
                                        new PlayerCondition(PlayerCondition.ContextValue.You)
                                    }))))
                        })
                });

        public static CardDef Emergency
            => Sorcery(3, "緊急出動",
                effects: new[]
                {
                    new CardEffect(
                        "相手の場にクリーチャーが1枚以上あり、あなたの場にクリーチャーが0枚のとき、あなたの場に「盾持ちゴブリン」2枚を追加する。",
                        new EffectConditionWrap(
                            ByPlay: new(
                                If: new(new ConditionWrap(ConditionAnd: new(new[]
                                {
                                    new ConditionWrap(NumCondition: new(
                                            new NumValue(NumValueCalculator: new(ForCard: new(
                                                NumValueCalculatorForCard.TypeValue.Count,
                                                new Choice(new ChoiceSource(
                                                    orCardConditions: new[]{
                                                        new CardCondition(
                                                            ZoneCondition: new(new ZoneValue(new[]{ ZonePrettyName.YouField })),
                                                            TypeCondition: new(new[]{ CardType.Creature })
                                                            )
                                                    }))
                                                ))),
                                            new NumCompare(
                                                0,
                                                NumCompare.CompareValue.Equality)
                                            )),
                                    new ConditionWrap(NumCondition: new(
                                            new NumValue(NumValueCalculator: new(ForCard: new(
                                                NumValueCalculatorForCard.TypeValue.Count,
                                                new Choice(new ChoiceSource(
                                                    orCardConditions: new[]{
                                                        new CardCondition(
                                                            ZoneCondition: new(new ZoneValue(new[]{ ZonePrettyName.OpponentField })),
                                                            TypeCondition: new(new[]{ CardType.Creature })
                                                            )
                                                    }))
                                                ))),
                                            new NumCompare(
                                                1,
                                                NumCompare.CompareValue.GreaterThan)
                                            )),
                                })))
                                )),
                        new[]
                        {
                            new EffectAction(AddCard: new(
                                new Choice(new ChoiceSource(
                                    OrCardDefConditions: new[]
                                    {
                                        new CardDefCondition(
                                            new(new[]{ OutZonePrettyName.CardPool }),
                                            NameCondition: new(
                                                new TextValue(ShieldGoblin.Name),
                                                TextCompare.CompareValue.Equality))
                                    })),
                                new ZoneValue(new[]{ ZonePrettyName.YouField }),
                                NumOfAddCards: 2
                                ))
                        })
                });

        public static CardDef HealOrDamage
            => Sorcery(3, "回復かダメージ",
                effects: new[]
                {
                    new CardEffect(
                        "あなたのHPが5以下なら、あなたのHPを+5する。",
                        new EffectConditionWrap(
                            ByPlay: new(
                                If: new(new ConditionWrap(NumCondition: new(
                                    new NumValue(NumValueCalculator: new(ForPlayer: new(
                                        NumValueCalculatorForPlayer.TypeValue.PlayerCurrentHp,
                                        new Choice(new ChoiceSource(
                                            orPlayerConditions: new[]{
                                                new PlayerCondition(PlayerCondition.ContextValue.You)
                                            }))))),
                                    new NumCompare(
                                        5,
                                        NumCompare.CompareValue.LessThan)
                                    ))))),
                        new[]
                        {
                            new EffectAction(ModifyPlayer: new(
                                new Choice(
                                    new ChoiceSource(
                                        orPlayerConditions: new[]
                                        {
                                            new PlayerCondition(PlayerCondition.ContextValue.You)
                                        })),
                                new PlayerModifier(
                                    Hp: new NumValueModifier(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(5)))
                                ))
                        }),
                    new CardEffect(
                        "そうでないなら、相手プレイヤーに3のダメージを与える。",
                        new EffectConditionWrap(
                            ByPlay: new(
                                If: new(new ConditionWrap(ConditionNot: new(
                                    new ConditionWrap(NumCondition: new(
                                    new NumValue(NumValueCalculator: new(ForPlayer: new(
                                        NumValueCalculatorForPlayer.TypeValue.PlayerCurrentHp,
                                        new Choice(new ChoiceSource(
                                            orPlayerConditions: new[]{
                                                new PlayerCondition(PlayerCondition.ContextValue.You)
                                            }))))),
                                    new NumCompare(
                                        5,
                                        NumCompare.CompareValue.LessThan)
                                    ))))))),
                        new[]
                        {
                            new EffectAction(Damage: new(
                                new NumValue(3),
                                new Choice(new ChoiceSource(
                                    orPlayerConditions: new[]
                                    {
                                        new PlayerCondition(PlayerCondition.ContextValue.Opponent)
                                    }))
                                ))
                        })
                });

        public static CardDef RevengeGoblin
            => SampleCards.Creature(1, "仕返しゴブリン", 1, 1,
                effects: new[]
                {
                    new CardEffect(
                        "このカードが破壊されたターンの終了時に、相手に1ダメージを与える。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouCemetery,
                            When: new(new EffectTiming(Destroy: new(
                                new[]
                                {
                                    new CardCondition(CardCondition.ContextConditionValue.This)
                                }))))),
                        new[]
                        {
                            new EffectAction(ReserveEffect: new(
                                new[]
                                {
                                    new CardEffect(
                                        "",
                                        new EffectConditionWrap(Reserve: new(
                                            new EffectWhen(new EffectTiming(EndTurn: new(
                                                new[]
                                                {
                                                    new PlayerCondition(PlayerCondition.ContextValue.Active)
                                                }))),
                                            While:new(new EffectTiming(EndTurn: new(
                                                new[]
                                                {
                                                    new PlayerCondition(PlayerCondition.ContextValue.Active)
                                                })),
                                                0, 1))),
                                        new[]
                                        {
                                            new EffectAction(Damage: new(
                                                new NumValue(1),
                                                new Choice(
                                                    new ChoiceSource(orPlayerConditions: new[]
                                                    {
                                                        new PlayerCondition(PlayerCondition.ContextValue.Opponent)
                                                    }))))
                                        })
                                }))
                        }),
                });

        public static CardDef Impact
            => SampleCards.Sorcery(5, "衝撃",
                effects: new[]
                {
                    new CardEffect(
                        "場のクリーチャーをすべて破壊する",
                        new EffectConditionWrap(ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(DestroyCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ZoneCondition: new(new ZoneValue(new[]
                                                {
                                                    ZonePrettyName.OpponentField,
                                                    ZonePrettyName.YouField
                                                })),
                                                TypeCondition: new(new[]
                                                {
                                                    CardType.Creature
                                                }))
                                        }))))
                        }),
                });

        public static CardDef SevereEarthquake
            => SampleCards.Sorcery(5, "激震",
                effects: new[]
                {
                    new CardEffect(
                        "場のアーティファクトをすべて破壊する",
                        new EffectConditionWrap(ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(DestroyCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ZoneCondition: new(new ZoneValue(new[]
                                                {
                                                    ZonePrettyName.OpponentField,
                                                    ZonePrettyName.YouField
                                                })),
                                                TypeCondition: new(new[]
                                                {
                                                    CardType.Artifact
                                                }))
                                        }))))
                        }),
                });

        public static CardDef Prodigy
            => SampleCards.Sorcery(7, "驚異",
                effects: new[]
                {
                    new CardEffect(
                        "場のカードをすべて破壊する",
                        new EffectConditionWrap(ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(DestroyCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ZoneCondition: new(new ZoneValue(new[]
                                                {
                                                    ZonePrettyName.OpponentField,
                                                    ZonePrettyName.YouField
                                                })))
                                        }))))
                        }),
                });

        public static CardDef ZombieToken
            => SampleCards.Creature(1, "ゾンビトークン", 1, 1, annotations: new[] { ":ゾンビ" }, isToken: true);

        public static CardDef Zombie
            => SampleCards.Creature(1, "ゾンビ", 1, 1,
                annotations: new[] { ":ゾンビ" },
                effects: new[]
                {
                    new CardEffect(
                        "このクリーチャーが破壊されたターンの終了時に、あなたの場にゾンビトークンを1つ追加する。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouCemetery,
                            When: new(new EffectTiming(Destroy: new(
                                new[]
                                {
                                    new CardCondition(CardCondition.ContextConditionValue.This)
                                }))))),
                        new[]
                        {
                            new EffectAction(ReserveEffect: new(new[]
                            {
                                new CardEffect(
                                    "",
                                    new EffectConditionWrap(Reserve: new(
                                        new EffectWhen(new EffectTiming(EndTurn: new())),
                                        While: new(
                                            new EffectTiming(EndTurn: new()),
                                            0, 1))),
                                    new[]
                                    {
                                        new EffectAction(AddCard: new(
                                            new Choice(
                                                new ChoiceSource(OrCardDefConditions: new[]
                                                {
                                                    new CardDefCondition(
                                                        new OutZoneCondition(new[]
                                                        {
                                                            OutZonePrettyName.CardPool
                                                        }),
                                                        NameCondition: new(
                                                            new TextValue(ZombieToken.Name),
                                                            TextCompare.CompareValue.Equality)
                                                        )
                                                }),
                                                Choice.HowValue.All,
                                                new NumValue(1)
                                                ),
                                            new ZoneValue(new[]
                                            {
                                                ZonePrettyName.YouField
                                            })))
                                    })
                            }))
                        }),
                });

        public static CardDef ZombieDog
            => SampleCards.Creature(2, "ゾンビ犬", 1, 1, annotations: new[] { ":ゾンビ" }, numTurnsToCanAttack: 0);

        public static CardDef ZombieKiller
            => SampleCards.Creature(2, "ゾンビキラー", 2, 2,
                annotations: new[] { ":ゾンビ" },
                effects: new[]
                {
                    new CardEffect(
                        "このクリーチャーが:ゾンビから受ける戦闘ダメージを-5する。",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            When: new(new EffectTiming(DamageBefore: new(
                                EffectTimingDamageBeforeEvent.TypeValue.Battle,
                                SourceCardCondition: new(
                                    AnnotationCondition: new(":ゾンビ")
                                    ),
                                TakeCardCondition: new(CardCondition.ContextConditionValue.This)
                                )))
                            )),
                        new[]
                        {
                            new EffectAction(ModifyDamage: new(
                                new NumValueModifier(
                                    NumValueModifier.OperatorValue.Sub,
                                    new NumValue(5))))
                        }),
                    new CardEffect(
                        "このクリーチャーが:ゾンビへ与える戦闘ダメージを+5する。",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            When: new(new EffectTiming(DamageBefore: new(
                                EffectTimingDamageBeforeEvent.TypeValue.Battle,
                                SourceCardCondition: new(CardCondition.ContextConditionValue.This),
                                TakeCardCondition: new(
                                    AnnotationCondition: new(":ゾンビ")
                                    )
                                )))
                            )),
                        new[]
                        {
                            new EffectAction(ModifyDamage: new(
                                new NumValueModifier(
                                    NumValueModifier.OperatorValue.Add,
                                    new NumValue(5))))
                        })
                });

        public static CardDef PrinceZombie
            => SampleCards.Creature(2, "ゾンビプリンス", 1, 1,
                annotations: new[] { ":ゾンビ" },
                effects: new[]
                {
                    new CardEffect(
                        "このカード以外のあなたの場のすべてのクリーチャーに:ゾンビを付与して、次の効果を追加する。" +
                            "「このカードが破壊されたとき、あなたの場にゾンビトークンを1つ追加する。」",
                        new EffectConditionWrap(ByPlay: new()),
                        new[]
                        {
                            new EffectAction(ModifyCard:new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                CardCondition.ContextConditionValue.Others,
                                                ZoneCondition: new(new ZoneValue(new[]
                                                {
                                                    ZonePrettyName.YouField
                                                })),
                                                TypeCondition: new(new[]
                                                {
                                                    CardType.Creature
                                                }))
                                        })),
                                Annotations: new(new[]{":ゾンビ"}, AnnotationsModifier.OperatorValue.Add),
                                Name: "modify"
                                )),
                            new EffectAction(AddEffect:new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ActionContext: new(ModifyCard: new(
                                                    "modify",
                                                    ActionContextCardsOfModifyCard.TypeValue.Modified
                                                    ))
                                                )
                                        })),
                                new[]
                                {
                                    new CardEffect(
                                        "このカードが破壊されたとき、あなたの場にゾンビトークンを1つ追加する。",
                                        new EffectConditionWrap(ByNotPlay: new (
                                            ZonePrettyName.YouCemetery,
                                            When: new(new EffectTiming(Destroy: new(
                                                new[]
                                                {
                                                    new CardCondition(CardCondition.ContextConditionValue.This)
                                                }))))),
                                        new[]
                                        {
                                            new EffectAction(AddCard: new(
                                                new Choice(
                                                    new ChoiceSource(OrCardDefConditions: new[]
                                                    {
                                                        new CardDefCondition(
                                                            new OutZoneCondition(new[]
                                                            {
                                                                OutZonePrettyName.CardPool
                                                            }),
                                                            NameCondition: new(
                                                                new TextValue(ZombieToken.Name),
                                                                TextCompare.CompareValue.Equality)
                                                            )
                                                    }),
                                                    Choice.HowValue.All,
                                                    new NumValue(1)
                                                    ),
                                                new ZoneValue(new[]
                                                {
                                                    ZonePrettyName.YouField
                                                })))
                                        }),
                                }
                                )),
                        }),
                });

        public static CardDef ZombieMaster
            => SampleCards.Creature(4, "ゾンビ使い", 1, 1,
                annotations: new[] { ":ゾンビ" },
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、墓地にある:ゾンビクリーチャーを1体選択し、場に移動する。",
                        new EffectConditionWrap(ByPlay: new()),
                        new[]
                        {
                            new EffectAction(ModifyCard:new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ZoneCondition: new(new ZoneValue(new[]
                                                {
                                                    ZonePrettyName.YouCemetery,
                                                    ZonePrettyName.OpponentCemetery,
                                                })),
                                                TypeCondition: new(new[]
                                                {
                                                    CardType.Creature
                                                }),
                                                AnnotationCondition:new(":ゾンビ")
                                                )
                                        }),
                                    Choice.HowValue.Choose,
                                    new NumValue(1)
                                    ),
                                Toughness: new(
                                    NumValueModifier.OperatorValue.Replace,
                                    new NumValue(NumValueCalculator: new(ForCard: new(
                                        NumValueCalculatorForCard.TypeValue.CardBaseToughness,
                                        new Choice(
                                            new ChoiceSource(
                                                orCardConditions: new[]
                                                {
                                                    new CardCondition(
                                                        CardCondition.ContextConditionValue.ActionTarget
                                                        )
                                                }
                                                )
                                            )
                                        )))
                                    ),
                                Name: "modify"
                                )),
                            new EffectAction(MoveCard:new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ActionContext: new(ModifyCard: new(
                                                    "modify",
                                                    ActionContextCardsOfModifyCard.TypeValue.Modified))
                                                )
                                        })
                                    ),
                                ZonePrettyName.YouField
                                )),
                        }),
                });

        public static CardDef GluttonZombie
            => SampleCards.Creature(3, "大食らいのゾンビ", 0, 1,
                annotations: new[] { ":ゾンビ" },
                abilities: new[] { CreatureAbility.Cover },
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、あなたの墓地から:ゾンビを任意の枚数だけ除外する。" +
                            "その後、このクリーチャーを+X/+Xする。" +
                            "X=この効果で除外したカードの枚数",
                        new EffectConditionWrap(ByPlay: new()),
                        new[]
                        {
                            new EffectAction(ExcludeCard:new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ZoneCondition: new(new ZoneValue(new[]
                                                {
                                                    ZonePrettyName.YouCemetery
                                                })),
                                                AnnotationCondition: new(":ゾンビ")
                                                ),
                                        }),
                                    Choice.HowValue.Choose,
                                    new NumValue(NumValueCalculator: new(ForCard: new(
                                        NumValueCalculatorForCard.TypeValue.Count,
                                        new Choice(
                                            new ChoiceSource(
                                                orCardConditions: new[]{
                                                    new CardCondition(
                                                        ZoneCondition: new(new ZoneValue(new[]
                                                        {
                                                            ZonePrettyName.YouCemetery
                                                        })),
                                                        AnnotationCondition: new(":ゾンビ")
                                                        ),
                                                }
                                                )
                                            )
                                    )))
                                    ),
                                "exclude"
                                )),
                            new EffectAction(ModifyCard:new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                CardCondition.ContextConditionValue.This),
                                        })),
                                Power: new(
                                    NumValueModifier.OperatorValue.Add,
                                    new NumValue(NumValueCalculator: new(ForCard: new(
                                        NumValueCalculatorForCard.TypeValue.Count,
                                        new Choice(
                                            new ChoiceSource(
                                                orCardConditions: new[]
                                                {
                                                    new CardCondition(
                                                        ActionContext: new(ExcludeCard: new(
                                                            "exclude",
                                                            ActionContextCardsOfExcludeCard.TypeValue.Excluded
                                                        ))
                                                        )
                                                })))))),
                                Toughness: new(
                                    NumValueModifier.OperatorValue.Add,
                                    new NumValue(NumValueCalculator: new(ForCard: new(
                                        NumValueCalculatorForCard.TypeValue.Count,
                                        new Choice(
                                            new ChoiceSource(
                                                orCardConditions: new[]
                                                {
                                                    new CardCondition(
                                                        ActionContext: new(ExcludeCard: new(
                                                            "exclude",
                                                            ActionContextCardsOfExcludeCard.TypeValue.Excluded
                                                        ))
                                                        )
                                                }))))))
                                )),
                        }),
                });

        public static CardDef KingZombie
            => SampleCards.Creature(3, "ゾンビキング", 0, 1,
                annotations: new[] { ":ゾンビ" },
                effects: new[]
                {
                    new CardEffect(
                        "このカードが場に出たとき、このカードを+x/+yする。" +
                            "X=あなたの墓地の:ゾンビの枚数" +
                            "Y=相手の墓地の:ゾンビの枚数",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            When: new(new EffectTiming(MoveCard: new(
                                OrCardConditions: new[]
                                {
                                    new CardCondition(CardCondition.ContextConditionValue.This)
                                },
                                To: ZonePrettyName.YouField)))
                            )),
                        new[]
                        {
                            new EffectAction(ModifyCard:new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                CardCondition.ContextConditionValue.This),
                                        })),
                                Power: new(
                                    NumValueModifier.OperatorValue.Add,
                                    new NumValue(NumValueCalculator: new(ForCard: new(
                                        NumValueCalculatorForCard.TypeValue.Count,
                                        new Choice(
                                            new ChoiceSource(
                                                orCardConditions: new[]
                                                {
                                                    new CardCondition(
                                                        ZoneCondition: new(new ZoneValue(new[]
                                                        {
                                                            ZonePrettyName.YouCemetery
                                                        })),
                                                        AnnotationCondition: new(":ゾンビ")
                                                        )
                                                })))))),
                                Toughness: new(
                                    NumValueModifier.OperatorValue.Add,
                                    new NumValue(NumValueCalculator: new(ForCard: new(
                                        NumValueCalculatorForCard.TypeValue.Count,
                                        new Choice(
                                            new ChoiceSource(
                                                orCardConditions: new[]
                                                {
                                                    new CardCondition(
                                                        ZoneCondition: new(new ZoneValue(new[]
                                                        {
                                                            ZonePrettyName.OpponentCemetery
                                                        })),
                                                        AnnotationCondition: new(":ゾンビ")
                                                        )
                                                }))))))
                                )),
                        }),
                    new CardEffect(
                        "このカードが破壊されたとき、「ゾンビキング」以外のあなたの墓地にある:ゾンビを2つまで選択する。" +
                        "そのカードをあなたの場に移動する。その後、それらのカードのタフネスを1にする。",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouCemetery,
                            When: new(new EffectTiming(Destroy: new(
                                OrCardCondition: new[]
                                {
                                    new CardCondition(CardCondition.ContextConditionValue.This)
                                })))
                            )),
                        new[]
                        {
                            new EffectAction(MoveCard:new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                CardCondition.ContextConditionValue.OtherDefs,
                                                ZoneCondition: new(new ZoneValue(new[]
                                                {
                                                    ZonePrettyName.YouCemetery,
                                                })),
                                                TypeCondition: new(new[]
                                                {
                                                    CardType.Creature
                                                }),
                                                AnnotationCondition:new(":ゾンビ")
                                                )
                                        }),
                                    Choice.HowValue.Choose,
                                    new NumValue(2)
                                    ),
                                ZonePrettyName.YouField,
                                Name: "move"
                                )),
                                new EffectAction(ModifyCard:new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ActionContext: new(MoveCard: new(
                                                        "move",
                                                        ActionContextCardsOfMoveCard.TypeValue.Moved
                                                    ))),
                                            })),
                                    Toughness: new(
                                        NumValueModifier.OperatorValue.Replace,
                                        new NumValue(1)
                                    )))
                        }
                        )
                });

        // 「このターンに破壊された」を検索できない
        //public static CardDef ZombiesTreasure
        //    => SampleCards.Artifact(4, "ゾンビの秘宝",
        //        annotations: new[] { ":ゾンビ" },
        //        "あなたのターン終了時に、場のこのカードにターンカウンターを1つ置く。" +
        //            "場のこのカードのターンカウンターが3以上になったとき、このカードを破壊する。" +
        //            "ターン終了時に発動する。" +
        //            "このターンに破壊されたあなたの墓地にある:ゾンビクリーチャーをあなたの場に移動する。",
        //        effects: new[]
        //        {
        //            new CardEffect(
        //                new EffectConditionWrap(ByNotPlay: new(
        //                    ZonePrettyName.YouField,
        //                    When: new(new EffectTiming(EndTurn: new(OrPlayerCondition: new[]
        //                    {
        //                        new PlayerCondition(PlayerCondition.ContextValue.You)
        //                    })))
        //                    )),
        //                new[]
        //                {
        //                    new EffectAction(ModifyCounter: new(
        //                        new Choice(
        //                            new ChoiceSource(
        //                                orCardConditions: new[]
        //                                {
        //                                    new CardCondition(CardCondition.ContextConditionValue.This)
        //                                }),
        //                            Choice.HowValue.Choose,
        //                            new NumValue(1)
        //                            ),
        //                        "ターン",
        //                        new NumValueModifier(
        //                            NumValueModifier.OperatorValue.Add,
        //                            new NumValue(1))
        //                        ))
        //                }),
        //            new CardEffect(
        //                new EffectConditionWrap(ByNotPlay: new(
        //                    ZonePrettyName.YouField,
        //                    If: new(new ConditionWrap(NumCondition: new(
        //                        new NumValue(NumValueCalculator: new(ForCounter: new(
        //                            "ターン",
        //                            new Choice(
        //                                new ChoiceSource(
        //                                    orCardConditions: new[]
        //                                    {
        //                                        new CardCondition(CardCondition.ContextConditionValue.This)
        //                                    })
        //                                )
        //                            ))),
        //                        new NumCompare(3, NumCompare.CompareValue.GreaterThan)
        //                        )))
        //                    )),
        //                new[]
        //                {
        //                    new EffectAction(DestroyCard: new(
        //                        new Choice(
        //                            new ChoiceSource(
        //                                orCardConditions: new[]
        //                                {
        //                                    new CardCondition(CardCondition.ContextConditionValue.This)
        //                                })
        //                            )
        //                        ))
        //                }),
        //            new CardEffect(
        //                new EffectConditionWrap(ByNotPlay: new(
        //                    ZonePrettyName.YouField,
        //                    When: new(new EffectTiming(EndTurn: new()))
        //                    )),
        //                new[]
        //                {
        //                    new EffectAction(MoveCard: new(
        //                        new Choice(
        //                            new ChoiceSource(
        //                                orCardConditions: new[]
        //                                {
        //                                    new CardCondition(
        //                                        ZoneCondition: new(new ZoneValue(new[]
        //                                        {
        //                                            ZonePrettyName.YouCemetery
        //                                        })),
        //                                        TypeCondition: new(new[]
        //                                        {
        //                                            CardType.Creature
        //                                        }),
        //                                        AnnotationCondition: new(":ゾンビ"),
        //                                        NumTurnsFromDestroy: new NumCompare(
        //                                            0,
        //                                            NumCompare.CompareValue.Equality)
        //                                        ),
        //                                }),
        //                            Choice.HowValue.Choose,
        //                            new NumValue(1)
        //                            ),
        //                        ZonePrettyName.YouField
        //                        ))
        //                }),
        //        });

        public static CardDef ZombieVirus
            => SampleCards.Sorcery(2, "ゾンビウイルス",
                annotations: new[] { ":ゾンビ" },
                effects: new[]
                {
                    new CardEffect(
                        "お互いの手札、場のクリーチャーすべてに:ゾンビを付与する。",
                        new EffectConditionWrap(ByPlay: new()),
                        new[]
                        {
                            new EffectAction(ModifyCard:new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ZoneCondition: new(new ZoneValue(new[]
                                                {
                                                    ZonePrettyName.YouField,
                                                    ZonePrettyName.OpponentField,
                                                    ZonePrettyName.YouHand,
                                                    ZonePrettyName.OpponentHand,
                                                })),
                                                TypeCondition: new(new[]
                                                {
                                                    CardType.Creature
                                                }))
                                        })),
                                Annotations: new(new[]{":ゾンビ"}, AnnotationsModifier.OperatorValue.Add)))
                        }),
                });

        public static CardDef Sunlight
            => SampleCards.Sorcery(2, "日の光",
                annotations: new[] { ":ゾンビ" },
                effects: new[]
                {
                    new CardEffect(
                        "場の:ゾンビをすべて除外する",
                        new EffectConditionWrap(ByPlay: new()),
                        new[]
                        {
                            new EffectAction(ExcludeCard:new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ZoneCondition: new(new ZoneValue(new[]
                                                {
                                                    ZonePrettyName.YouField,
                                                    ZonePrettyName.OpponentField,
                                                })),
                                                TypeCondition: new(new[]
                                                {
                                                    CardType.Creature
                                                }),
                                                AnnotationCondition: new(":ゾンビ")
                                                )
                                        }))
                                )),
                        }),
                });

        public static CardDef ZombieSearch
            => SampleCards.Sorcery(1, "ゾンビの呼び声",
                annotations: new[] { ":ゾンビ" },
                effects: new[]
                {
                    new CardEffect(
                        "あなたのデッキから、「ゾンビの呼び声」以外の:ゾンビ1枚を選択する。" +
                            "そのカードをあなたの墓地に移動する。",
                        new EffectConditionWrap(ByPlay: new()),
                        new[]
                        {
                            new EffectAction(MoveCard:new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                CardCondition.ContextConditionValue.OtherDefs,
                                                ZoneCondition: new(new ZoneValue(new[]
                                                {
                                                    ZonePrettyName.YouDeck,
                                                })),
                                                AnnotationCondition: new(":ゾンビ")
                                                ),
                                        }),
                                    Choice.HowValue.Choose,
                                    new NumValue(1)
                                    ),
                                ZonePrettyName.YouCemetery
                                )),
                        }),
                });

        public static CardDef LivingDead
            => SampleCards.Sorcery(1, "リビングデッド",
                annotations: new[] { ":ゾンビ" },
                effects: new[]
                {
                    new CardEffect(
                        "あなたの墓地にある:ゾンビクリーチャーを1つ選択する。そのカードをあなたの場に移動する。それのタフネスを1にする。",
                        new EffectConditionWrap(ByPlay: new()),
                        new[]
                        {
                            new EffectAction(ModifyCard:new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                AnnotationCondition: new(":ゾンビ"),
                                                TypeCondition: new(new[]{ CardType.Creature }),
                                                ZoneCondition: new ZoneCondition(new ZoneValue(new[]
                                                {
                                                    ZonePrettyName.YouCemetery
                                                }))
                                                ),
                                        }),
                                    Choice.HowValue.Choose,
                                    new NumValue(1)
                                    ),
                                Toughness: new(
                                    NumValueModifier.OperatorValue.Replace,
                                    new NumValue(1)
                                ),
                                Name: "modify"
                                )),
                            new EffectAction(MoveCard:new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ActionContext: new(ModifyCard: new(
                                                    "modify",
                                                    ActionContextCardsOfModifyCard.TypeValue.Modified
                                                    ))
                                                )
                                        })
                                    ),
                                ZonePrettyName.YouField
                                )),
                        }),
                });

        public static CardDef ZombiesCurse
            => SampleCards.Sorcery(4, "ゾンビの呪い",
                annotations: new[] { ":ゾンビ" },
                effects: new[]
                {
                    new CardEffect(
                        "あなたの墓地にある:ゾンビをすべて除外する。場にあるカードをX枚ランダムに破壊する。" +
                            "X=この効果であなたの墓地から除外した枚数",
                        new EffectConditionWrap(ByPlay: new()),
                        new[]
                        {
                            new EffectAction(ExcludeCard:new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ZoneCondition: new(new ZoneValue(new[]
                                                {
                                                    ZonePrettyName.YouCemetery,
                                                })),
                                                AnnotationCondition: new(":ゾンビ")
                                                ),
                                        })),
                                Name: "exclude"
                                )),
                            new EffectAction(DestroyCard:new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ZoneCondition: new(new ZoneValue(new[]
                                                {
                                                    ZonePrettyName.YouField,
                                                    ZonePrettyName.OpponentField,
                                                }))
                                                ),
                                        }),
                                    Choice.HowValue.Random,
                                    new NumValue(NumValueCalculator: new(ForCard: new(
                                        NumValueCalculatorForCard.TypeValue.Count,
                                        new Choice(
                                            new ChoiceSource(
                                                orCardConditions: new[]
                                                {
                                                    new CardCondition(
                                                        ActionContext: new(ExcludeCard: new(
                                                            "exclude",
                                                            ActionContextCardsOfExcludeCard.TypeValue.Excluded)))
                                                })))))
                                    )
                                )),
                        }),
                });

        public static CardDef ZombiesStatue
            => SampleCards.Artifact(1, "ゾンビの像",
                annotations: new[] { ":ゾンビ" },
                effects: new[]
                {
                    new CardEffect(
                        "このカードが場にある限り、あなたの場から墓地に:ゾンビが移動したとき、そのカードのコピーをあなたの墓地に追加する。",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            When: new(new EffectTiming(MoveCard: new(
                                OrCardConditions: new[]
                                {
                                    new CardCondition(
                                        AnnotationCondition: new(":ゾンビ")
                                        )
                                },
                                From: ZonePrettyName.YouField,
                                To: ZonePrettyName.YouCemetery
                                )))
                            )),
                        new[]
                        {
                            new EffectAction(AddCard:new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                CardCondition.ContextConditionValue.EventSource
                                                )
                                        })
                                    ),
                                new ZoneValue(new[]
                                {
                                    ZonePrettyName.YouCemetery
                                })
                                )),
                        }),
                });
    }
}
