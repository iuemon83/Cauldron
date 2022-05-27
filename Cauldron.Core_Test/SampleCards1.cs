using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Core_Test
{
    public class SampleCards1
    {
        public static readonly string CardsetName = "Sample1";

        public static CardDef Creature(int cost, string name, int power, int toughness,
            IEnumerable<string> annotations = null,
            string flavorText = "",
            int? numTurnsToCanAttack = null, int? numAttacksInTurn = null, bool isToken = false,
            int? limitNumInDeck = null,
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
            t.LimitNumCardsInDeck = isToken ? 0 : limitNumInDeck;
            t.Abilities = abilities?.ToArray() ?? Array.Empty<CreatureAbility>();
            t.Effects = effects?.ToArray() ?? Array.Empty<CardEffect>();

            t.Type = CardType.Creature;

            return t;
        }

        public static CardDef Artifact(int cost, string name, bool isToken = false,
            int? limitNumInDeck = null,
            IEnumerable<string> annotations = null,
            string flavorText = "",
            IEnumerable<CardEffect> effects = null)
        {
            var t = CardDef.Empty;
            t.Cost = cost;
            t.IsToken = isToken;
            t.Type = CardType.Artifact;
            t.Name = name;
            t.LimitNumCardsInDeck = isToken ? 0 : limitNumInDeck;
            t.Annotations = annotations?.ToArray() ?? Array.Empty<string>();
            t.FlavorText = flavorText;
            t.Effects = effects?.ToArray() ?? Array.Empty<CardEffect>();

            return t;
        }

        public static CardDef Sorcery(int cost, string name, bool isToken = false,
            int? limitNumInDeck = null,
            IEnumerable<string> annotations = null,
            string flavorText = "",
            IEnumerable<CardEffect> effects = null)
        {
            var t = CardDef.Empty;
            t.Cost = cost;
            t.IsToken = isToken;
            t.Type = CardType.Sorcery;
            t.Name = name;
            t.LimitNumCardsInDeck = isToken ? 0 : limitNumInDeck;
            t.Annotations = annotations?.ToArray() ?? Array.Empty<string>();
            t.FlavorText = flavorText;
            t.Effects = effects?.ToArray() ?? Array.Empty<CardEffect>();

            return t;
        }

        public static CardDef Vanilla
            => SampleCards1.Creature(1, "バニラ", 1, 2, flavorText: "なんの能力もない");

        public static CardDef Rare
            => SampleCards1.Creature(1, "レア", 1, 2, flavorText: "このカードはデッキに1枚しか入れられない",
                limitNumInDeck: 1);

        public static CardDef Quick
            => SampleCards1.Creature(1, "速攻", 1, 1,
                flavorText: "すぐに攻撃できる",
                numTurnsToCanAttack: 0);

        public static CardDef SlowStarter
            => SampleCards1.Creature(4, "スロースターター", 7, 4,
                flavorText: "攻撃できるまでが長い",
                numTurnsToCanAttack: 2);

        public static CardDef Twin
            => SampleCards1.Creature(2, "双子", 1, 1,
                flavorText: "二回攻撃できる",
                numAttacksInTurn: 2);

        public static CardDef Shielder
            => SampleCards1.Creature(2, "盾持ち", 1, 2, flavorText: "味方の盾になる",
                abilities: new[] { CreatureAbility.Cover });

        public static CardDef BigShielder
            => SampleCards1.Creature(3, "大盾持ち", 1, 4, flavorText: "攻撃できない",
                abilities: new[] { CreatureAbility.Cover },
                numAttacksInTurn: 0
                );

        public static CardDef Assassin
            => SampleCards1.Creature(3, "暗殺者", 1, 1,
                flavorText: "一撃で相手を倒す",
                abilities: new[] { CreatureAbility.Stealth, CreatureAbility.Deadly });

        public static CardDef Destroy
            => SampleCards1.Sorcery(5, "破壊",
                effects: new[]
                {
                    new CardEffect(
                        "場にあるカード一つを選択する。それを破壊する。",
                        new EffectConditionWrap(ByPlay: new()),
                        new[]
                        {
                            new EffectAction(
                                DestroyCard: new(
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

        public static CardDef Exclude
            => SampleCards1.Sorcery(6, "除外",
                effects: new[]
                {
                    new CardEffect(
                        "場にあるカード一つを選択する。それを除外する。",
                        new EffectConditionWrap(ByPlay: new()),
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

        public static CardDef BounceHand
            => SampleCards1.Sorcery(2, "手札へ戻す",
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
            => SampleCards1.Sorcery(3, "デッキへ戻す",
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

        public static CardDef Impact
            => SampleCards1.Sorcery(8, "衝撃",
                effects: new[]
                {
                    new CardEffect(
                        "場のクリーチャーをすべて破壊する",
                        new EffectConditionWrap(ByPlay: new()),
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
            => SampleCards1.Sorcery(8, "激震",
                effects: new[]
                {
                    new CardEffect(
                        "場のアーティファクトをすべて破壊する",
                        new EffectConditionWrap(ByPlay: new()),
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
            => SampleCards1.Sorcery(10, "驚異",
                effects: new[]
                {
                    new CardEffect(
                        "場のカードをすべて破壊する",
                        new EffectConditionWrap(ByPlay: new()),
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

        public static CardDef Sword
            => SampleCards1.Sorcery(1, "剣",
                effects: new[]
                {
                    new CardEffect(
                        "あなたの場にあるクリチャー1体を選択する。それは+1/+0 の修整を受ける。",
                        new EffectConditionWrap(ByPlay: new()),
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
                                                    TypeCondition: new(new[]{ CardType.Creature })
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
            => SampleCards1.Sorcery(1, "盾",
                effects: new[]
                {
                    new CardEffect(
                        "あなたの場にあるクリチャー1体を選択する。それは+0/+1 の修整を受ける。",
                        new EffectConditionWrap(ByPlay: new()),
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
                                                    TypeCondition: new(new[]{ CardType.Creature })
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

        public static CardDef Deadly
            => SampleCards1.Sorcery(0, "必殺",
                effects: new[]
                {
                    new CardEffect(
                        "選択したクリーチャー1体に「必殺」を付与する",
                        new EffectConditionWrap(ByPlay: new()),
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
                                        CreatureAbility.Deadly
                                        )
                                    ))
                        })
                });

        public static CardDef SearchRight
            => SampleCards1.Sorcery(1, "サーチライト",
                effects: new[]
                {
                    new CardEffect(
                        "相手の場の「ステルス」を持つすべてのクリーチャーから、「ステルス」を削除する。",
                        new EffectConditionWrap(ByPlay: new()),
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
                                                    AbilityCondition: new[]{ CreatureAbility.Stealth }
                                                ),
                                            })
                                        ),
                                    Ability: new(
                                        CreatureAbilityModifier.OperatorValue.Remove,
                                        CreatureAbility.Stealth
                                        )
                                    ))
                        })
                });

        public static CardDef SelectDamage
            => SampleCards1.Sorcery(1, "ダメージ",
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
            => SampleCards1.Sorcery(1, "稲妻",
                effects: new[]
                {
                    new CardEffect(
                        "ランダムな場のクリーチャー1体に2ダメージを与える。",
                        new EffectConditionWrap(ByPlay: new()),
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
            => SampleCards1.Sorcery(3, "破壊光線",
                effects: new[]
                {
                    new CardEffect(
                        "場のクリーチャー1体を選択する。それにXのダメージを与える。X=そのクリーチャーの元々のタフネス",
                        new EffectConditionWrap(ByPlay: new()),
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

        public static CardDef FullAttack
            => SampleCards1.Sorcery(3, "一斉射撃",
                effects: new[]
                {
                    new CardEffect(
                        "相手プレイヤーと相手の場のクリーチャーすべてに、1ダメージを与える。",
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

        public static CardDef Salvage
            => SampleCards1.Sorcery(1, "サルベージ",
                effects: new[]
                {
                    new CardEffect(
                        "墓地のカード1枚を選択する。それをあなたの手札に加える。",
                        new EffectConditionWrap(ByPlay: new()),
                        new[]
                        {
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ZoneCondition: new(new ZoneValue(
                                                    new[]{
                                                        ZonePrettyName.YouCemetery,
                                                        ZonePrettyName.OpponentCemetery
                                                    }))
                                            )
                                        }),
                                        Choice.HowValue.Choose,
                                        new NumValue(1)),
                                    ZonePrettyName.YouHand
                                    )),
                        }
                    )
                });

        public static CardDef Search
            => SampleCards1.Sorcery(3, "探索",
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

        public static CardDef Reborn
            => SampleCards1.Sorcery(5, "蘇生",
                effects: new[]
                {
                    new CardEffect(
                        "墓地のクリーチャー1体を選択する。それをあなたの場に出す。",
                        new EffectConditionWrap(ByPlay: new()),
                        new[]
                        {
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ZoneCondition: new(new ZoneValue(new[]{
                                                    ZonePrettyName.YouCemetery,
                                                    ZonePrettyName.OpponentCemetery
                                                })),
                                                TypeCondition: new(new[]{ CardType.Creature })
                                            )
                                        }),
                                        Choice.HowValue.Choose,
                                        new NumValue(1)),
                                    ZonePrettyName.YouField
                                    )),
                        }
                    )
                });

        public static CardDef Draw
            => SampleCards1.Sorcery(1, "ドロー",
                effects: new[]
                {
                    new CardEffect(
                        "あなたはカードを1枚ドローする。",
                        new EffectConditionWrap(ByPlay: new()),
                        new[]
                        {
                            new EffectAction(
                                DrawCard: new(new NumValue(1),
                                    new PlayerCondition(PlayerCondition.ContextValue.You)
                                    )
                                )
                        }
                    ),
                });

        public static CardDef Greed
            => SampleCards1.Sorcery(2, "強欲",
                effects: new[]
                {
                    // カードを2枚引く
                    new CardEffect(
                        "あなたはカードを2枚ドローする。",
                        new EffectConditionWrap(ByPlay: new()),
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
            => SampleCards1.Sorcery(1, "希望",
                effects: new[]
                {
                    new CardEffect(
                        "あなたはカードをX枚ドローする。X=相手のHPとあなたのHPの差",
                        new EffectConditionWrap(ByPlay: new()),
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

        public static CardDef Investment
            => SampleCards1.Sorcery(1, "投資",
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

        public static CardDef ChangeHands
            => SampleCards1.Sorcery(2, "手札入れ替え",
                effects: new[]
                {
                    new CardEffect(
                        "あなたは手札をすべて捨てる。あなたは捨てた枚数カードをドローする。",
                        new EffectConditionWrap(ByPlay: new()),
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
                                            },
                                            how: ChoiceSource.HowValue.All,
                                            numPicks: new NumValue(0)
                                            ),
                                        how: Choice.HowValue.All,
                                        numPicks: new NumValue(0)
                                        ),
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

        public static CardDef Copy
            => SampleCards1.Sorcery(2, "複製",
                effects: new[]
                {
                    new CardEffect(
                        "相手の場のカードを1枚選択する。そのカードと同名のカードを1枚あなたの手札に加える。",
                        new EffectConditionWrap(ByPlay: new()),
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

        public static CardDef TempRamp
            => SampleCards1.Sorcery(0, "一時的なランプ",
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
                                                    OrPlayerConditions: new[]
                                                    {
                                                        new PlayerCondition(PlayerCondition.ContextValue.You)
                                                    }))),
                                            While: new(new EffectTiming(EndTurn: new(
                                                OrPlayerConditions: new[]
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

        public static CardDef Ramp
            => SampleCards1.Sorcery(2, "ランプ",
                effects: new[]
                {
                    new CardEffect(
                        "あなたのMPの最大値を1増加し、利用可能なMPを1減少する。",
                        new EffectConditionWrap(ByPlay: new()),
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

        public static CardDef Ninja
            => SampleCards1.Creature(3, "忍者",
                1, 1,
                effects: new[] {
                    new CardEffect(
                        "このカードをプレイしたとき、あなたの場に「忍者」2体を追加する。",
                        new EffectConditionWrap(ByPlay: new()),
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

        public static CardDef Shaman
            => SampleCards1.Creature(3, "呪術師",
                1, 1,
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、相手の場にあるランダムなクリーチャーカード1枚を破壊する。",
                        new EffectConditionWrap(ByPlay: new()),
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

        public static CardDef SuperShaman
            => SampleCards1.Creature(4, "高等呪術師",
                1, 1,
                effects: new[]
                {
                    new CardEffect(
                        "このカードが場に出たとき、相手の場にあるランダムなクリーチャーカード1枚を破壊する。",
                        new EffectConditionWrap(ByNotPlay:new(
                            ZonePrettyName.YouField,
                            When: new(new EffectTiming(MoveCard: new(
                                OrCardConditions: new[]
                                {
                                    new CardCondition(CardCondition.ContextConditionValue.This)
                                },
                                To: ZonePrettyName.YouField
                                )))
                            )),
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

        public static CardDef Healer
            => SampleCards1.Creature(3, "衛生兵",
                1, 2,
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、あなたのHPを2回復する。",
                        new EffectConditionWrap(ByPlay: new()),
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

        public static CardDef Emergency
            => SampleCards1.Sorcery(3, "緊急出動",
                effects: new[]
                {
                    new CardEffect(
                        $"相手の場にクリーチャーが1枚以上あり、あなたの場にクリーチャーが0枚のとき、あなたの場に「{Shielder.Name}」2枚を追加する。",
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
                                                new NumValue(0),
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
                                                new NumValue(1),
                                                NumCompare.CompareValue.GreaterThan)
                                            )),
                                })))
                                )),
                        new[]
                        {
                            new EffectAction(AddCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        OrCardDefConditions: new[]
                                        {
                                            new CardDefCondition(
                                                new(new[]{ OutZonePrettyName.CardPool }),
                                                NameCondition: new(
                                                    new TextValue(Shielder.Name),
                                                    TextCompare.CompareValue.Equality))
                                        },
                                        how: ChoiceSource.HowValue.All,
                                        numPicks: new NumValue(0)
                                        ),
                                    how: Choice.HowValue.All,
                                    numPicks: new NumValue(0)
                                    ),
                                new ZoneValue(new[]{ ZonePrettyName.YouField }),
                                NumOfAddCards: 2
                                ))
                        })
                });

        public static CardDef BeginnerSummoner
            => SampleCards1.Creature(4, "初心者召喚士",
                3, 3,
                effects: new[]
                {
                    new CardEffect(
                        "このカードが破壊されたとき、ランダムなコスト2のクリーチャーを1体場に出す。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouCemetery,
                            new EffectWhen(new EffectTiming(
                                Destroy: new(
                                    OrCardConditions: new[]
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
                                                new NumValue(2),
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
            => SampleCards1.Creature(4, "マッドサイエンティスト",
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

        public static CardDef Faceless
            => SampleCards1.Creature(4, "顔なし",
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
                                                        OrPlayerConditions: new[]
                                                        {
                                                            new PlayerCondition(PlayerCondition.ContextValue.You),
                                                        }))),
                                                While: new(new(
                                                    StartTurn: new(
                                                        OrPlayerConditions: new[]
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
            => SampleCards1.Creature(5, "預言者",
                0, 7,
                effects: new[]
                {
                    new CardEffect(
                        "あなたのターンの開始時にこのカードが場にあるとき、このカードの攻撃力を7にする",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                StartTurn: new(
                                    OrPlayerConditions: new[]
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
            => SampleCards1.Creature(5, "マジックドラゴン",
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

        public static CardDef DoubleShield
            => SampleCards1.Creature(5, "二重の盾",
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
            => SampleCards1.Creature(6, "悪夢",
                2, 8,
                effects: new[]
                {
                    new CardEffect(
                        "あなたのターンの開始時にこのカードが場にあるとき、このカードの攻撃力を2倍にする",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                StartTurn: new(
                                    OrPlayerConditions: new[]
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

        public static CardDef Disaster
            => SampleCards1.Creature(7, "災い",
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
            => SampleCards1.Creature(7, "ノール", 7, 7, isToken: true,
                abilities: new[] { CreatureAbility.Cover });

        public static CardDef Firelord
            => SampleCards1.Creature(8, "炎の王",
                8, 8,
                numAttacksInTurn: 0,
                effects: new[]
                {
                    new CardEffect(
                        "このカードが場にあるとき、あなたのターン終了時に、ランダムな敵クリーチャー1体か敵プレイヤーに8ダメージを与える。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                EndTurn: new(
                                    OrPlayerConditions: new[]
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

        public static CardDef Reaper
            => SampleCards1.Creature(10, "死神",
                12, 12,
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、このカードを除く全てのクリーチャーを破壊する。" +
                        "その後、X枚のランダムな自分の手札を墓地へ移動する。X=この効果で破壊したクリーチャーの数",
                        new EffectConditionWrap(ByPlay: new()),
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

        public static CardDef Hit
            => SampleCards1.Sorcery(1, "ヒット",
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
            => SampleCards1.Sorcery(1, "ヒール",
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
            => SampleCards1.Sorcery(1, "ヒットorヒール",
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

        public static CardDef EmergencyFood
            => SampleCards1.Sorcery(1, "非常食",
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


        public static CardDef Slap
            => SampleCards1.Sorcery(2, "袋叩き",
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

        public static CardDef Virus
            => SampleCards1.Sorcery(5, "ウイルス",
                effects: new[]
                {
                    new CardEffect(
                        "相手の場と相手の手札にあるパワー4以上のクリーチャーをすべて墓地に移動する。" +
                        "2回後の相手ターン終了時まで、相手のドローしたパワー4以上のクリーチャーを墓地へ移動する。",
                        new EffectConditionWrap(ByPlay: new()),
                        new[]
                        {
                            new EffectAction(MoveCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                PowerCondition: new NumCompare(
                                                    new NumValue(4),
                                                    NumCompare.CompareValue.GreaterThan),
                                                ZoneCondition: new(
                                                    new ZoneValue(new[]{
                                                        ZonePrettyName.OpponentField,
                                                        ZonePrettyName.OpponentHand
                                                        })))
                                        })
                                    ),
                                ZonePrettyName.OwnerCemetery
                                )),
                            new EffectAction(ReserveEffect: new(
                                new[]
                                {
                                    new CardEffect(
                                        "相手のドローしたパワー4以上のクリーチャーを墓地へ移動する。",
                                        new EffectConditionWrap(Reserve: new(
                                            new EffectWhen(new EffectTiming(
                                                MoveCard: new(
                                                    OrCardConditions: new[]
                                                    {
                                                        new CardCondition(
                                                            PowerCondition: new NumCompare(
                                                                new NumValue(4),
                                                                NumCompare.CompareValue.GreaterThan))
                                                    },
                                                    ZonePrettyName.OpponentDeck,
                                                    ZonePrettyName.OpponentHand))),
                                            While: new(new EffectTiming(EndTurn: new(
                                                OrPlayerConditions: new[]
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
                                                ZonePrettyName.OwnerCemetery
                                                )
                                            )
                                        }
                                    )
                                }))
                        }
                    ),
                });

        public static CardDef OldShield
            => SampleCards1.Artifact(1, "ぼろの盾",
                effects: new[]
                {
                    new CardEffect(
                        "あなたの場にあるクリーチャーがダメージを受けるとき、そのダメージを1軽減する。その後、このカードを破壊する。",
                        new EffectConditionWrap(ByNotPlay: new(
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
            => SampleCards1.Artifact(1, "ぼろの壁",
                effects: new[]
                {
                    new CardEffect(
                        "あなたか、あなたの場にあるクリーチャーがダメージを受けるとき、そのダメージを1軽減する。その後、このカードを破壊する。",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new(new(
                                DamageBefore: new(
                                    TakePlayerCondition: new PlayerCondition(
                                        PlayerCondition.ContextValue.You
                                    ),
                                    TakeCardCondition: new CardCondition(
                                        TypeCondition: new CardTypeCondition(new[]{ CardType.Creature }),
                                        ZoneCondition: new(new(new[]{ ZonePrettyName.YouField })),
                                        ContextCondition: CardCondition.ContextConditionValue.Others
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
                });

        public static CardDef StatueOfCurse
            => SampleCards1.Artifact(4, "呪いの像",
                effects: new[]
                {
                    new CardEffect(
                        "あなたのターン終了時、あなたの墓地が30枚以上なら発動する。相手プレイヤー、相手の場にあるすべてのクリーチャーに6ダメージを与える。",
                        new EffectConditionWrap(
                            ByNotPlay: new(ZonePrettyName.YouField,
                                new(new(EndTurn: new(
                                    OrPlayerConditions: new[]
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
                                    new NumValue(30),
                                    NumCompare.CompareValue.GreaterThan)
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

        public static CardDef StatueOfHoly
            => SampleCards1.Artifact(4, "癒やしの像",
                effects: new[]
                {
                    // 使用時、すべての自分クリーチャーを+0/+1
                    new CardEffect(
                        "このカードをプレイしたとき、あなたの場にあるすべてのクリーチャーは+0/+1 の修整を受ける。",
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
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new(new(MoveCard: new(
                                OrCardConditions: new[]
                                {
                                    new CardCondition(
                                        TypeCondition: new(new[]
                                        {
                                            CardType.Creature
                                        }),
                                        ZoneCondition: new(new ZoneValue(new[]
                                        {
                                            ZonePrettyName.YouField
                                        }))
                                        ),
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
            => SampleCards1.Artifact(1, "勝利への道",
                effects: new[]
                {
                    new CardEffect(
                        "あなたのターン開始時に、場にあるこのカードを墓地に移動する。" ,
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(StartTurn: new(
                                OrPlayerConditions: new[]
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
            => SampleCards1.Artifact(10, "勝利の像",
                isToken: true,
                effects: new[]
                {
                    new CardEffect(
                        "あなたのターン開始時に、このカードがあなたの場にあるとき、あなたはゲームに勝利する。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(StartTurn: new(
                                OrPlayerConditions: new[]
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
                                            new NumValue(1),
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
                                            new NumValue(1),
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
                                        new NumValue(5),
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
                                        new NumValue(5),
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

        public static CardDef KarakuriDoll
            => SampleCards1.Creature(1, "からくり人形", 1, 1, flavorText: "トークン", isToken: true);

        public static CardDef Mechanic
            => SampleCards1.Creature(1, "からくり技師",
                1, 1,
                effects: new[]
                {
                    // 破壊時、からくりゴブリン１枚を手札に加える
                    new CardEffect(
                        "このカードが破壊されたとき、手札に「からくり人形」1枚を加える。",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouCemetery,
                            new(new(Destroy: new (
                                OrCardConditions: new[]
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
                                                        new TextValue(KarakuriDoll.Name),
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
            => SampleCards1.Creature(1, "魔法の本",
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

        public static CardDef MindController
            => SampleCards1.Creature(3, "催眠術師",
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
                                        new NumValue(4),
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

        public static CardDef Parasite
            => SampleCards1.Creature(1, "寄生虫",
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
                                OrPlayerConditions: new[]
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
            => SampleCards1.Creature(1, "虫つかい",
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

        public static CardDef Mutation
            => SampleCards1.Sorcery(1, "変異",
                effects: new[]
                {
                    new CardEffect(
                        "あなたの場のクリーチャーを1体選択する。" +
                        "そのクリーチャーを除外する。" +
                        "その後、そのクリーチャーよりもコストが1高いランダムなクリーチャーをあなたの場に1体追加する。",
                        new EffectConditionWrap(ByPlay: new()),
                        new[]
                        {
                            new EffectAction(ExcludeCard:new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                TypeCondition: new(new[]
                                                {
                                                    CardType.Creature
                                                }),
                                                ZoneCondition: new(new ZoneValue(new[]
                                                {
                                                    ZonePrettyName.YouField
                                                }))
                                                )
                                        }),
                                    Choice.HowValue.Choose,
                                    new NumValue(1)
                                    ),
                                Name: "exclude"
                                )),
                            new EffectAction(AddCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        OrCardDefConditions: new[]
                                        {
                                            new CardDefCondition(
                                                new OutZoneCondition(new[]
                                                {
                                                    OutZonePrettyName.CardPool
                                                }),
                                                TypeCondition: new(new[]
                                                {
                                                    CardType.Creature
                                                }),
                                                CostCondition: new(
                                                    new NumValue(
                                                        NumValueCalculator: new(ForCard: new(
                                                            NumValueCalculatorForCard.TypeValue.CardCost,
                                                            new Choice(
                                                                new ChoiceSource(
                                                                    orCardConditions: new[]
                                                                    {
                                                                        new CardCondition(
                                                                            ActionContext: new(ExcludeCard:new(
                                                                                "exclude",
                                                                                ActionContextCardsOfExcludeCard.TypeValue.Excluded
                                                                                ))
                                                                            )
                                                                    }
                                                                    )
                                                                )
                                                            )),
                                                        NumValueModifier: new(
                                                            NumValueModifier.OperatorValue.Add,
                                                            new NumValue(1)
                                                            )
                                                        ),
                                                    NumCompare.CompareValue.Equality
                                                    )
                                                )
                                        }
                                        ),
                                    Choice.HowValue.Random,
                                    new NumValue(1)
                                    ),
                                new ZoneValue(new[]
                                {
                                    ZonePrettyName.YouField
                                })
                                ))
                        }),
                });

        public static CardDef Bomb
            => SampleCards1.Creature(1, "ボム",
                1, 1, isToken: true,
                effects: new[]
                {
                    new CardEffect(
                        "このカードが破壊されたとき、ランダムな敵クリーチャー1体か敵プレイヤーにランダムに1~4ダメージを与える。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouCemetery,
                            new EffectWhen(new EffectTiming(
                                Destroy: new(
                                    OrCardConditions: new[]
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
            => SampleCards1.Creature(7, "ボマー",
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

        public static CardDef UnluckyStatue
            => SampleCards1.Artifact(1, "追撃の像",
                effects: new[]
                {
                    new CardEffect(
                        "相手がダメージを受けたときに発動する。相手に1ダメージを与える。",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            When: new(new EffectTiming(DamageAfter: new(
                                TakePlayerCondition: new PlayerCondition(
                                    PlayerCondition.ContextValue.Opponent
                                    )
                                )))
                            )),
                        new[]
                        {
                            new EffectAction(
                                Damage: new(
                                    new NumValue(1),
                                    new Choice(
                                        new ChoiceSource(
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition(PlayerCondition.ContextValue.Opponent)
                                            })
                                        )
                                    )
                            )
                        }
                    )
                });

        public static CardDef Disturber
            => SampleCards1.Artifact(1, "邪魔者",
                effects: new[]
                {
                    new CardEffect(
                        "このカードがあなたの場に出たときに発動する。このカードを相手の場に移動する。",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            When: new(new EffectTiming(MoveCard: new(
                                OrCardConditions: new[]
                                {
                                    new CardCondition(CardCondition.ContextConditionValue.This)
                                }
                                )))
                            )),
                        new[]
                        {
                            new EffectAction(MoveCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(CardCondition.ContextConditionValue.This)
                                        }
                                        )
                                    ),
                                ZonePrettyName.OpponentField
                                ))
                        }
                    )
                });
    }
}
