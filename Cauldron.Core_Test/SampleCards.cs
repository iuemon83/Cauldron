﻿using Cauldron.Shared;
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
            string effectText = "",
            IEnumerable<CardEffect> effects = null)
        {
            return new CardDef()
            {
                Cost = cost,
                Type = CardType.Creature,
                Name = name,
                FlavorText = flavorText,
                Power = power,
                Toughness = toughness,
                Annotations = annotations?.ToArray() ?? Array.Empty<string>(),
                NumTurnsToCanAttack = numTurnsToCanAttack,
                NumAttacksLimitInTurn = numAttacksInTurn,
                IsToken = isToken,
                Abilities = abilities?.ToArray() ?? Array.Empty<CreatureAbility>(),
                EffectText = effectText,
                Effects = effects?.ToArray() ?? Array.Empty<CardEffect>()
            };
        }

        public static CardDef Artifact(int cost, string name, string effectText, bool isToken = false,
            string flavorText = "",
            IEnumerable<CardEffect> effects = null)
        {
            return new CardDef()
            {
                Cost = cost,
                IsToken = isToken,
                Type = CardType.Artifact,
                Name = name,
                FlavorText = flavorText,
                EffectText = effectText,
                Effects = effects?.ToArray() ?? Array.Empty<CardEffect>()
            };
        }

        public static CardDef Sorcery(int cost, string name, string effectText, bool isToken = false,
            string flavorText = "",
            IEnumerable<CardEffect> effects = null)
        {
            return new CardDef()
            {
                Cost = cost,
                IsToken = isToken,
                Type = CardType.Sorcery,
                Name = name,
                FlavorText = flavorText,
                EffectText = effectText,
                Effects = effects?.ToArray() ?? Array.Empty<CardEffect>()
            };
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
                effectText: "このカードの戦闘時、戦闘開始前に相手クリーチャーに、Xのダメージを与える。X=このカードの攻撃力",
                effects: new[]
                {
                    new CardEffect(
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
                                                    CardCondition.ContextConditionValue.Guard)
                                            }))))
                        }),
                    new CardEffect(
                        new EffectConditionWrap(
                            ByNotPlay: new(
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
                                                    CardCondition.ContextConditionValue.Attack)
                                            }))))
                        })
                });

        public static CardDef MagicShieldGoblin
            => SampleCards.Creature(2, "魔法の盾持ちゴブリン",
                1, 2, abilities: new[] { CreatureAbility.Cover },
                effectText: "このカードが攻撃されたとき、攻撃したカードを持ち主の手札に移動する。",
                effects: new[] {
                    new CardEffect(
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
                                                    ContextCondition: CardCondition.ContextConditionValue.Attack,
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

        public static CardDef SuperMagicShieldGoblin
            => SampleCards.Creature(2, "強魔法の盾持ちゴブリン",
                1, 2, abilities: new[] { CreatureAbility.Cover },
                effectText: "このカードが攻撃されたとき、攻撃したカードを相手のデッキの一番上に移動する。",
                effects: new[] {
                    new CardEffect(
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
                                                    ContextCondition: CardCondition.ContextConditionValue.Attack,
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

        public static CardDef DDShieldGoblin
            => SampleCards.Creature(2, "異次元の盾持ちゴブリン",
                1, 2, abilities: new[] { CreatureAbility.Cover },
                effectText: "このカードの戦闘時に、このカードと戦闘相手を除外する。",
                effects: new[] {
                    new CardEffect(
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
                                                    CardCondition.ContextConditionValue.Guard),
                                                new CardCondition(
                                                    CardCondition.ContextConditionValue.This)
                                            })))),
                        }),
                    new CardEffect(
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
                                                    CardCondition.ContextConditionValue.Attack),
                                                new CardCondition(
                                                    CardCondition.ContextConditionValue.This)
                                            })))),
                        })
                });

        public static CardDef DeadlyGoblin
            => SampleCards.Creature(3, "暗殺ゴブリン", 1, 1, flavorText: "暗殺者",
                abilities: new[] { CreatureAbility.Stealth, CreatureAbility.Deadly });

        public static CardDef MechanicGoblin
            => SampleCards.Creature(1, "ゴブリンの技師",
                1, 1,
                effectText: "このカードが破壊されたとき、手札に「からくりゴブリン」1枚を加える。",
                effects: new[]
                {
                    // 破壊時、からくりゴブリン１枚を手札に加える
                    new CardEffect(
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
                effectText: "このカードが場に出たとき、ランダムな魔法カード1枚をあなたの手札に追加する。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "あなたが「ゴブリン」と名のつくクリーチャーカードをプレイしたとき、このカードをデッキから場に出す。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "このカードが場に出たとき、相手の手札からランダムなクリーチャー1枚を相手の場に出す。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "このカードが場に出たとき、敵のフィールドにクリーチャーが4体以上いるなら、ランダムに1体を選択し、それをあなたの場に移動する。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "このカードが場に出たとき、「分身ゴブリン」一体を場に出す。",
                effects: new[] {
                    new CardEffect(
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
                effectText: "このカードが場に出たとき、「多重分身ゴブリン」2体を場に出す。",
                effects: new[] {
                    new CardEffect(
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

        public static CardDef GoblinsGreed
            => SampleCards.Sorcery(2, "ゴブリンの強欲",
                effectText: "あなたはカードを2枚ドローする。このカードが手札から捨てられたとき、あなたはカードを1枚ドローする。",
                effects: new[]
                {
                    // カードを2枚引く
                    new CardEffect(
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

        public static CardDef ShamanGoblin
            => SampleCards.Creature(3, "呪術師ゴブリン",
                1, 1,
                effectText: "このカードが場に出たとき、相手の場にあるランダムなクリーチャーカード1枚を破壊する。",
                effects: new[]
                {
                    new CardEffect(
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

        public static CardDef HealGoblin
            => SampleCards.Creature(3, "優しいゴブリン",
                1, 2,
                effectText: "このカードが場に出たとき、あなたのHPを2回復する。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "このカードをプレイしたとき、相手プレイヤーか、相手の場にいるクリーチャーカード1枚に2ダメージを与える。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "このカードが破壊されたとき、ランダムなコスト2のクリーチャーを1体場に出す。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "このカードが場に出たとき、自分の場か、相手の場にあるクリーチャーカード1枚を選択して、それを破壊する。" +
                "その後、破壊したカードのコピーをもとの場に出す。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "自分が受けるダメージを2軽減する。自分の場の他のクリーチャーカードが戦闘で与えるダメージを1増加する。",
                effects: new[]
                {
                    // 自分が受けるダメージを2軽減する
                    new CardEffect(
                        new EffectConditionWrap(
                            ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new(new(
                                DamageBefore: new(
                                    Source: EffectTimingDamageBeforeEvent.SourceValue.Take,
                                    CardCondition: new CardCondition(
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
                    // 自分の他のクリーチャーが戦闘で与えるダメージを1増加する
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                DamageBefore: new(
                                    EffectTimingDamageBeforeEvent.TypeValue.Battle,
                                    Source: EffectTimingDamageBeforeEvent.SourceValue.DamageSource,
                                    CardCondition: new CardCondition(
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
                effectText: "あなたの次のターン開始時、あなたのMPは2減少する。",
                effects: new[]
                {
                    new CardEffect(
                        new EffectConditionWrap(ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                ReserveEffect: new(new[]
                                {
                                    new CardEffect(
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
                effectText: "あなたのターンの開始時にこのカードが場にあるとき、このカードの攻撃力を7にする",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "このカードが場に出たとき、あなたはカードを1枚ドローする。このカードが場にある限り、あなたがプレイした魔法カードによるダメージを+1する。",
                effects: new[]
                {
                    new CardEffect(
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]{
                            new EffectAction(
                                DrawCard: new(
                                    new NumValue(1),
                                    new PlayerCondition(PlayerCondition.ContextValue.You)))
                        }),
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                DamageBefore: new(
                                    EffectTimingDamageBeforeEvent.TypeValue.NonBattle,
                                    CardCondition: new(
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
                effectText: "このカードが場に出たとき、自分の場にある他のクリチャーカードに3ダメージを与える。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "このカードが場に出たとき、または自分のターン開始時に自分の場にあるほかのクリーチャーのパワーを1増加する。",
                effects: new[]
                {
                    // プレイ時：自分のクリーチャーすべてを+1/+0 する。
                    // 自分のターン開始時：自分のクリーチャーすべてを+1/+0 する。
                    new CardEffect(
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
                effectText: "このクリーチャーがダメージを受けるとき、1度だけそのダメージを0にする。",
                effects: new[]
                {
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                DamageBefore: new(
                                    Source: EffectTimingDamageAfterEvent.SourceValue.Take,
                                    CardCondition: new(
                                        ContextCondition: CardCondition.ContextConditionValue.This)
                                    ))),
                            While: new(new EffectTiming(
                                DamageBefore: new(
                                    Source: EffectTimingDamageAfterEvent.SourceValue.Take,
                                    CardCondition: new(
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
                effectText: "あなたのターンの開始時にこのカードが場にあるとき、このカードの攻撃力を2倍にする",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "あなたが魔法カードをプレイしたとき、あなたの場に「騎兵ゴブリン」1枚を追加する。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "このカードが場に出たとき、あなたの手札をすべて捨てる。このカードのパワーとタフネスをX増加する。Xは捨てた手札の枚数である。",
                effects: new[]
                {
                    // 手札をすべて捨てて、捨てた枚数パワーアップ
                    new CardEffect(
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

        public static CardDef DoctorBomb
            => SampleCards.Creature(1, "ドクターボム",
                1, 1, isToken: true,
                effectText: "このカードが破壊されたとき、ランダムな敵クリーチャー1体か敵プレイヤーにランダムに1~4ダメージを与える。",
                effects: new[]
                {
                    new CardEffect(
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

        public static CardDef Doctor
            => SampleCards.Creature(7, "ドクター",
                7, 7,
                effectText: "このカードが場に出たとき、「ドクターボム」2枚をあなたの場に追加する。",
                effects: new[]
                {
                    new CardEffect(
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
                                                        new TextValue(DoctorBomb.Name),
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
                effectText: "このクリーチャーがダメージを受けるたび、「ノール」1体をあなたの場に追加する。",
                effects: new[]
                {
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                DamageAfter: new(
                                    Source: EffectTimingDamageAfterEvent.SourceValue.Take,
                                    CardCondition: new(
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
                effectText: "このカードが場にあるとき、あなたのターン終了時に、ランダムな敵クリーチャー1体か敵プレイヤーに8ダメージを与える。",
                effects: new[]
                {
                    new CardEffect(
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
        //        effectText: "あなたの場にカードが4枚以上あるとき、このカードのコストは0になる。" +
        //            "このカードのプレイ時、あなたの場にカードが4枚以上あるなら、あなたの場のカードをすべて破壊する。",
        //        effects: new[]
        //        {
        //        });

        public static CardDef Death
            => SampleCards.Creature(10, "死",
                12, 12,
                effectText: "このカードをプレイしたとき、このカードを除く全てのクリーチャーを破壊する。" +
                "その後、X枚のランダムな自分の手札を墓地へ移動する。X=この効果で破壊したクリーチャーの数",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "あなたの最大MPを1増加する。ターン終了時あなたの最大MPを1減少する。",
                effects: new[]
                {
                    new CardEffect(
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
                        new EffectConditionWrap(ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(ReserveEffect: new(
                                new[]
                                {
                                    new CardEffect(
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
                effectText: "プレイヤーか、場のクリーチャー1体を選択する。それに1ダメージを与える。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "ランダムな場のクリーチャー1体に2ダメージを与える。",
                effects: new[]
                {
                    new CardEffect(
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

        public static CardDef Salvage
            => SampleCards.Sorcery(1, "サルベージ",
                effectText: "墓地のカードを1枚選択する。それをあなたの手札に加える。そのカードがクリーチャーならタフネスを元々のタフネスと等しい値にする。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "墓地のカードを1枚選択する。それのコピーをあなたの手札に加える。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "墓地のクリーチャーをランダムに1体選択する。それをあなたの場に出す。それのタフネスを1にする。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "あなたの場にあるクリチャー1体を選択する。それは+1/+0 の修整を受ける。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "あなたの場にあるクリチャー1体を選択する。それは+0/+1 の修整を受ける。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "相手プレイヤーに1ダメージを与える。", isToken: true,
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "あなたのライフを1回復する。", isToken: true,
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "「ヒット」か「ヒール」のうち1枚を選択し、それを自分の手札に加える。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "選択したクリーチャー1体を「封印」状態にする",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "相手の場の「カバー」アビリティを持つすべてのクリーチャーから、「カバー」アビリティを削除する。" +
                "その後それらのクリーチャーに1ダメージを与える。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "場にあるカード一つを選択する。それを「除外」する。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "このカードが場にあるとき、場のほかのカードが除外されるたびに、このカードを+1/+0する。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "このカードが場に出たとき、このカードを+X/+0 する。X=すでに除外されている自分のカードの数",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "自分の除外済みのクリーチャーカードをランダムで一つを選択する。そのカードを場に追加する。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "自分の手札をランダムに1枚除外する。その後、場のカードX枚をランダムに破壊する。X=除外したカードのコスト",
                effects: new[]
                {
                    new CardEffect(
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

        public static CardDef Parasite
            => SampleCards.Creature(1, "寄生虫",
                1, 1, isToken: true,
                effectText: "このカードがデッキから手札に移動したとき、このカードをあなたの場に出す。" +
                "このカードがあなたの場にある限り、あなたのターン終了時に、あなたは1ダメージを受ける。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "このカードをプレイしたとき、相手のデッキの1番上に「寄生虫」を1枚追加する。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "あなたはランダムに手札を1枚捨てる。あなたはXのライフを得る。X=捨てたカードのコスト",
                effects: new[] {
                    new CardEffect(
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
                effectText: "あなたの手札に「ゴブリン」を3枚加える。",
                effects: new[] {
                    new CardEffect(
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
                effectText: "相手の場のカードを一枚選択する。そのカードと同名のカードを一枚あなたの手札に加える。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "相手プレイヤーか、あなたの場か、相手の場にあるクリーチャー1体を選択する。それに1ダメージを与える。" +
                "あなたの手札に「ゴブリンの二撃」1枚を加える。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "相手プレイヤーか、あなたの場か、相手の場にあるクリーチャー1体を選択する。それに2ダメージを与える。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "あなたの場にある他のカードに次の効果を付与する。「ターン終了時まで、受けるダメージは0になる。」",
                effects: new[]
                {
                    new CardEffect(
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
                                            new EffectConditionWrap(ByNotPlay: new(
                                                ZonePrettyName.YouField,
                                                new EffectWhen(new EffectTiming(DamageBefore: new(
                                                    Source: EffectTimingDamageBeforeEvent.SourceValue.Take,
                                                    CardCondition: new(
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
            effectText: "あなたは手札をすべて捨てる。あなたは捨てた枚数カードをドローする。",
            effects: new[]
            {
                new CardEffect(
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
                effectText: "相手の場にあるクリーチャー1体を選択する。それにXダメージを与える。X=あなたの場にあるクリーチャーの数",
                effects: new[]
                {
                    // 使用時、対象の相手クリーチャー一体にxダメージ。x="自分の場のクリーチャーの数"
                    new CardEffect(
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
                effectText: "あなたのMPの最大値を1増加し、利用可能なMPを1減少する。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "場のカード1枚を選択する。選択したカードを持ち主の手札に移動する。",
                effects: new[] {
                    new CardEffect(
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
                effectText: "場のカード1枚を選択する。選択したカードを持ち主のデッキのランダムな位置に移動する。",
                effects: new[] {
                    new CardEffect(
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
                effectText: "相手の場のカードを一枚選択する。そのカードと同名のカードを二枚あなたの手札に加える。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "相手プレイヤーか、相手の場にあるクリーチャーすべてに、1ダメージを与える。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "あなたのデッキからランダムなカード1枚を、あなたの手札に加える。そのカードのコストを半分にする。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "あなたの場か、相手の場にあるクリーチャーのうち、名前に「ゴブリン」を含むカードすべてのパワーを1にし、「封印」状態にする。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "相手の場と相手の手札にあるパワー4以上のクリーチャーをすべて墓地に移動する。" +
                "2回後の相手ターン終了時まで、相手のドローしたパワー4以上のクリーチャーを墓地へ移動する。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "あなたの場にあるクリーチャーがダメージを受けるとき、そのダメージを1軽減する。その後、このカードを破壊する。",
                effects: new[]
                {
                    new CardEffect(
                        new EffectConditionWrap(
                            ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new(new(
                                DamageBefore: new(
                                    Source: EffectTimingDamageBeforeEvent.SourceValue.Take,
                                    CardCondition: new CardCondition(
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
                effectText: "あなたか、あなたの場にあるクリーチャーがダメージを受けるとき、そのダメージを1軽減する。その後、このカードを破壊する。",
                effects: new[]
                {
                    new CardEffect(
                        new EffectConditionWrap(
                            ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new(new(
                                DamageBefore: new(
                                    Source: EffectTimingDamageBeforeEvent.SourceValue.Take,
                                    PlayerCondition: new PlayerCondition(
                                        PlayerCondition.ContextValue.You
                                    ),
                                    CardCondition: new CardCondition(
                                        TypeCondition: new CardTypeCondition(new[]{ CardType.Creature }),
                                        ZoneCondition: new(new(new[]{ ZonePrettyName.YouField })),
                                        ContextCondition: CardCondition.ContextConditionValue.Others
                                    )))
                            )
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
                effectText: "あなたのターン終了時、もしあなたの墓地が30枚以上なら、相手プレイヤー、相手の場にあるクリーチャーすべてに6ダメージを与える。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "あなたの場にあるクリーチャーすべては+0/+1 の修整を受ける。" +
                "あなたが場にクリーチャーを出したとき、それは+0/+1 の修整を受ける。",
                effects: new[]
                {
                    // 使用時、すべての自分クリーチャーを+0/+1
                    new CardEffect(
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
                effectText: "あなたのターン開始時に、場にあるこのカードを墓地に移動する。" +
                "場にあるこのカードが墓地に移動されたとき、「勝利の像」1枚をあなたの場に追加する。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "あなたのターン開始時に、このカードがあなたの場にあるとき、あなたはゲームに勝利する。",
                isToken: true,
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "いずれかのプレイヤーが魔法をプレイするたびに、場のこのカードに「魔導」カウンターを1つ置く。" +
                "このカードに「魔導」カウンターが置かれるたびに、このカードを+1/+0する。" +
                "このカードから「魔導」カウンターが取り除かれるたびに、このカードを-1/+0する。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "あなたが魔法をプレイするたびに、このカードに「魔導」カウンターを1つ置く。" +
                "このカードに「魔導」カウンターが1つ置かれるたびに、このカードのコスト-1。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "このカードが場に出たとき、あなたの場の「:魔導」を持つカード1枚を選択する。そのカードに「魔導」カウンターを2つ置く。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "あなたが魔法をプレイするたびに、このカードに「魔導」カウンターを1つ置く。" +
                "このカードに「魔導」カウンターが1つ置かれるたびに、このカードのコスト-1。" +
                "このカードをプレイしたとき、あなたの手札をすべて除外する。" +
                "その後、あなたは手札を5枚引く。" +
                "その後、あなたの手札「:魔導」カードすべてに「魔導」カウンターを5つ置く。",
                effects: new[]
                {
                    // 魔法をプレイするたび、カウンターを置く。
                    new CardEffect(
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

                    // カードが場に出たとき
                    new CardEffect(
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
                effectText: "このカードが場に出たとき、あなたの場からすべての「魔導」カウンターを取り除く。" +
                "その後、相手にXのダメージを与える。X=取り除いた「魔導」カウンターの数+1",
                effects: new[]
                {
                    new CardEffect(
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

        public static CardDef DashGoblin
            => SampleCards.Creature(2, "特攻ゴブリン", 1, 1,
                effectText: "このカードが場に出たとき、相手プレイヤーに1ダメージを与える。",
                effects: new[]
                {
                    new CardEffect(
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
                "1ターン後のターン終了時に、あなたはカードを2枚ドローする。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "このカードのプレイ時、あなたの場にほかの「ゴブリン」と名の付くカードがあるとき、このカードを+1/+1する。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "",
                effects: Array.Empty<CardEffect>()
                );

        public static CardDef Key2
            => Sorcery(1, "封印の鍵2",
                effectText: "",
                effects: Array.Empty<CardEffect>()
                );

        public static CardDef SealedGoblin
            => Creature(1, "封印されしゴブリン", 1, 1,
                effectText: "このカードと「封印の鍵1」「封印の鍵2」があなたの手札にそろったとき、あなたはゲームに勝利する。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "相手の場にクリーチャーが1枚以上あり、あなたの場にクリーチャーが0枚のとき、あなたの場に「盾持ちゴブリン」2枚を追加する。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "あなたのHPが5以下なら、あなたのHPを+5する。そうでないなら、相手プレイヤーに3のダメージを与える。",
                effects: new[]
                {
                    new CardEffect(
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
                effectText: "このカードが破壊されたターンの終了時に、相手に1ダメージを与える。",
                effects: new[]
                {
                    new CardEffect(
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
    }
}
