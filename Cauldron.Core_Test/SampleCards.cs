using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Core.Entities
{
    public class SampleCards
    {
        public static readonly string CardsetName = "Sample";

        public static CardDef Creature(int cost, string name, string flavorText, int power, int toughness,
            int? numTurnsToCanAttack = null, int? numAttacksInTurn = null, bool isToken = false,
            IEnumerable<CreatureAbility> abilities = null, IEnumerable<CardEffect> effects = null)
        {
            return new CardDef()
            {
                Cost = cost,
                Type = CardType.Creature,
                Name = name,
                FlavorText = flavorText,
                Power = power,
                Toughness = toughness,
                NumTurnsToCanAttack = numTurnsToCanAttack,
                NumAttacksLimitInTurn = numAttacksInTurn,
                IsToken = isToken,
                Abilities = abilities?.ToList() ?? new List<CreatureAbility>(),
                Effects = effects?.ToArray() ?? Array.Empty<CardEffect>()
            };
        }

        public static CardDef Artifact(int cost, string name, string flavorText, bool isToken = false, IEnumerable<CardEffect> effects = null)
        {
            return new CardDef()
            {
                Cost = cost,
                IsToken = isToken,
                Type = CardType.Artifact,
                Name = name,
                FlavorText = flavorText,
                Effects = effects?.ToArray() ?? Array.Empty<CardEffect>()
            };
        }

        public static CardDef Sorcery(int cost, string name, string flavorText, bool isToken = false, IEnumerable<CardEffect> effects = null)
        {
            return new CardDef()
            {
                Cost = cost,
                IsToken = isToken,
                Type = CardType.Sorcery,
                Name = name,
                FlavorText = flavorText,
                Effects = effects?.ToArray() ?? Array.Empty<CardEffect>()
            };
        }

        public static readonly EffectCondition Spell
            = new(ZonePrettyName.YouField, new(new(Play: new(EffectTimingPlayEvent.EventSource.This))));


        public static CardDef KarakuriGoblin
            => SampleCards.Creature(1, "からくりゴブリン", "トークン", 1, 1, isToken: true);

        public static CardDef Goblin
            => SampleCards.Creature(1, "ゴブリン", "ただのゴブリン", 1, 2);

        public static CardDef QuickGoblin
            => SampleCards.Creature(1, "素早いゴブリン", "早い", 1, 1, numTurnsToCanAttack: 0);

        public static CardDef ShieldGoblin
            => SampleCards.Creature(2, "盾持ちゴブリン", "盾になる", 1, 2,
                abilities: new[] { CreatureAbility.Cover });

        public static CardDef DeadlyGoblin
            => SampleCards.Creature(3, "暗殺ゴブリン", "暗殺者", 1, 1,
                abilities: new[] { CreatureAbility.Stealth, CreatureAbility.Deadly });

        public static CardDef MechanicGoblin
            => SampleCards.Creature(1, "ゴブリンの技師", "このカードが破壊されたとき、手札に「からくりゴブリン」1枚を加える。", 1, 1,
            effects: new[]
            {
                // 破壊時、からくりゴブリン１枚を手札に加える
                new CardEffect(
                    new EffectCondition(
                        ZonePrettyName.YouCemetery,
                        new(new(Destroy: new (EffectTimingDestroyEvent.EventSource.This)))
                    ),
                    new[]
                    {
                        new EffectAction()
                        {
                            AddCard = new(
                                new ZoneValue(new[]{ ZonePrettyName.YouHand }),
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                ZoneCondition = new(new(new[]{ ZonePrettyName.CardPool })),
                                                NameCondition = new(
                                                    new TextValue(KarakuriGoblin.Name),
                                                    TextCondition.ConditionCompare.Equality
                                                )
                                            },
                                        }))
                            )
                        }
                    }
                )
            });

        public static CardDef NinjaGoblin
            => SampleCards.Creature(3, "分身ゴブリン", "このカードが場に出たとき、「分身ゴブリン」一体を場に出す。", 1, 2,
                effects: new[] {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                Play: new(EffectTimingPlayEvent.EventSource.This)))),
                        new[]
                        {
                            new EffectAction(
                                AddCard:new(
                                    new ZoneValue(new[]{ZonePrettyName.YouField }),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    Context = CardCondition.CardConditionContext.This
                                                }
                                            }))
                                    ))
                        })
                });

        public static CardDef SuperNinjaGoblin
            => SampleCards.Creature(3, "多重分身ゴブリン", "このカードが場に出たとき、「多重分身ゴブリン」2体を場に出す。", 1, 1,
                effects: new[] {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                Play: new(EffectTimingPlayEvent.EventSource.This)))),
                        new[]
                        {
                            new EffectAction(
                                AddCard: new(
                                    new ZoneValue(new[]{ZonePrettyName.YouField }),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    Context = CardCondition.CardConditionContext.This,
                                                }
                                            })),
                                    NumOfAddCards: 2
                                    ))
                        })
                });

        public static CardDef GoblinsGreed
            => SampleCards.Sorcery(2, "ゴブリンの強欲", "ドローするぞ",
                effects: new[]
                {
                    // カードを2枚引く
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction(
                                DrawCard: new(new NumValue(2),
                                    new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You))
                            )
                        }
                    ),
                    // このカードが手札から捨てられたなら、1枚引く
                    new CardEffect(
                        new(ZonePrettyName.YouCemetery,
                            new(new(MoveCard: new(EffectTimingMoveCardEvent.EventSource.This,
                                ZonePrettyName.YouHand, ZonePrettyName.YouCemetery)))),
                        new[]
                        {
                            new EffectAction(
                                DrawCard: new(new NumValue(1),
                                    new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You))
                            )
                        }
                    )
                });

        public static CardDef ShamanGoblin
            => SampleCards.Creature(3, "呪術師ゴブリン", "相手を呪い殺すぞ", 1, 1,
                effects: new[]
                {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                Play: new(EffectTimingPlayEvent.EventSource.This)))),
                        new[]
                        {
                            new EffectAction()
                            {
                                DestroyCard = new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    TypeCondition = new(new[]{CardType.Creature}),
                                                    ZoneCondition = new(new ZoneValue(new[]{ZonePrettyName.OpponentField}))
                                                }
                                            }),
                                        Choice.ChoiceHow.Random,
                                        1)
                                    )
                            }
                        }
                    )
                });

        public static CardDef HealGoblin
            => SampleCards.Creature(3, "優しいゴブリン", "回復してくれる", 1, 2,
                effects: new[]
                {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                Play: new(EffectTimingPlayEvent.EventSource.This)))),
                        new[]
                        {
                            new EffectAction(
                                ModifyPlayer: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You)
                                            })),
                                    new PlayerModifier(
                                        Hp: new NumValueModifier(
                                            NumValueModifier.ValueModifierOperator.Add,
                                            new NumValue(2)))
                                )
                                )
                        }
                    )
                });

        public static CardDef FireGoblin
            => SampleCards.Creature(4, "火炎のゴブリン", "火を飛ばす", 4, 2,
                effects: new[]
                {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                Play: new(EffectTimingPlayEvent.EventSource.This)))),
                        new[]
                        {
                            new EffectAction()
                            {
                                Damage = new EffectActionDamage(
                                    new NumValue(2),
                                    new Choice(
                                        new ChoiceSource(
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition()
                                                {
                                                    Type = PlayerCondition.PlayerConditionType.Opponent
                                                },
                                            },
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    Context = CardCondition.CardConditionContext.Others,
                                                    TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                                    ZoneCondition = new(new(new[]{ ZonePrettyName.OpponentField })),
                                                }
                                            }),
                                        Choice.ChoiceHow.Choose,
                                        1)
                                )
                            }
                        }
                    )
                });

        public static CardDef BeginnerSummoner
            => SampleCards.Creature(4, "初心者召喚士", "このカードが破壊されたとき、ランダムなコスト2のクリーチャーを1体場に出す。", 3, 3,
                effects: new[]
                {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouCemetery,
                            new EffectWhen(new EffectTiming(
                                Destroy: new(EffectTimingDestroyEvent.EventSource.This)))),
                        new[]
                        {
                            new EffectAction(AddCard: new(
                                new ZoneValue(new[]{ ZonePrettyName.YouField }),
                                new Choice(new ChoiceSource(
                                    orCardConditions: new[]
                                    {
                                        new CardCondition()
                                        {
                                            ZoneCondition = new ZoneCondition(new ZoneValue(
                                                new[]{ ZonePrettyName.CardPool })),
                                            CostCondition = new NumCondition(2,
                                                NumCondition.ConditionCompare.Equality)
                                        }
                                    }),
                                    how: Choice.ChoiceHow.Random,
                                    numPicks: 1)))
                        }
                    )
                });

        public static CardDef MadScientist
            => SampleCards.Creature(4, "マッドサイエンティスト", "どこかおかしい", 3, 3,
                // 自分か相手のクリーチャーを一体破壊して、再生する
                effects: new[]
                {
                    // 破壊時にコピーを場に出す効果を追加する
                    new CardEffect(
                        SampleCards.Spell,
                        new[]{
                            new EffectAction(AddEffect: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                Context = CardCondition.CardConditionContext.Others,
                                                ZoneCondition = new(new(new[]{
                                                    ZonePrettyName.YouField, ZonePrettyName.OpponentField })),
                                                TypeCondition = new(new[]{ CardType.Creature })
                                            }
                                        }),
                                    Choice.ChoiceHow.Choose,
                                    1),
                                new[]
                                {
                                    new CardEffect(
                                        new EffectCondition(ZonePrettyName.YouCemetery,
                                            new EffectWhen(new EffectTiming(
                                                Destroy: new(EffectTimingDestroyEvent.EventSource.This)))),
                                        new[]
                                        {
                                            new EffectAction(AddCard: new(
                                                new ZoneValue(new[]{ ZonePrettyName.YouField }),
                                                new Choice(
                                                    new ChoiceSource(
                                                        orCardConditions: new[]
                                                        {
                                                            new CardCondition()
                                                            {
                                                                Context = CardCondition.CardConditionContext.This,
                                                            }
                                                        }))
                                                ))
                                        })
                                }, "addEffect")),
                            // 効果を付与したカードを破壊する
                            new EffectAction(DestroyCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                ActionContext = new(ActionContextCardsOfAddEffect: new(
                                                    "addEffect",
                                                    ActionContextCardsOfAddEffect.ValueType.TargetCards)),
                                            }
                                        }))
                                )),
                        }
                    )
                });

        public static CardDef BraveGoblin
            => SampleCards.Creature(4, "ゴブリンの勇者", "勇者！", 2, 2,
            effects: new[]
            {
                // 自分が受けるダメージを2軽減する
                new CardEffect(
                    new(
                        ZonePrettyName.YouField,
                        new(new(
                            DamageBefore: new(
                                Source: EffectTimingDamageBeforeEvent.EventSource.Take,
                                CardCondition: new CardCondition()
                                {
                                    Context = CardCondition.CardConditionContext.This
                                }))
                        )
                    ),
                    new[]
                    {
                        new EffectAction()
                        {
                            ModifyDamage = new EffectActionModifyDamage(
                                new NumValueModifier(
                                    NumValueModifier.ValueModifierOperator.Sub,
                                    new NumValue(2)
                                ),
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                Context = CardCondition.CardConditionContext.EventSource,
                                            }
                                        }))
                            )
                        }
                    }
                ),
                // 自分の他のクリーチャーが戦闘で与えるダメージを1増加する
                new CardEffect(
                    new EffectCondition(
                        ZonePrettyName.YouField,
                        new EffectWhen(new EffectTiming(
                            DamageBefore: new(
                                Type: EffectTimingDamageBeforeEvent.DamageType.Battle,
                                Source: EffectTimingDamageBeforeEvent.EventSource.DamageSource,
                                CardCondition: new CardCondition()
                                {
                                    ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                    TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                    Context = CardCondition.CardConditionContext.Others,
                                }))
                        )
                    ),
                    new[]
                    {
                        new EffectAction()
                        {
                            ModifyDamage = new EffectActionModifyDamage(
                                new NumValueModifier(
                                    NumValueModifier.ValueModifierOperator.Add,
                                    new NumValue(1)
                                ),
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                Context = CardCondition.CardConditionContext.EventSource,
                                            }
                                        }))
                            )
                        }
                    }
                )
            });

        public static CardDef GiantGoblin
            => SampleCards.Creature(5, "ゴブリンの巨人", "超でかい", 3, 7,
                abilities: new[] { CreatureAbility.Cover },
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction()
                            {
                                Damage = new EffectActionDamage(
                                    new NumValue(3),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                                    TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                                    Context = CardCondition.CardConditionContext.Others,
                                                }
                                            }))
                                )
                            }
                        }
                    )
                });

        public static CardDef LeaderGoblin
            => SampleCards.Creature(5, "ゴブリンリーダー", "", 1, 5,
                effects: new[]
                {
                    // プレイ時：自分のクリーチャーすべてを+1/+0 する。
                    // 自分のターン開始時：自分のクリーチャーすべてを+1/+0 する。
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(
                                Play: new (EffectTimingPlayEvent.EventSource.This),
                                StartTurn: new (EffectTimingStartTurnEvent.EventSource.You)
                                ))),
                        new[]
                        {
                            new EffectAction()
                            {
                                ModifyCard = new (
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                                    TypeCondition = new (new[]{ CardType.Creature }),
                                                    Context = CardCondition.CardConditionContext.Others,
                                                }
                                            })),
                                    Power: new NumValueModifier(
                                        NumValueModifier.ValueModifierOperator.Add,
                                        new NumValue(1))
                                )
                            }
                        }
                    )
                });

        public static CardDef TyrantGoblin
            => SampleCards.Creature(6, "暴君ゴブリン", "界隈一のあばれもの", 6, 6,
                effects: new[]
                {
                    // 手札をすべて捨てて、捨てた枚数パワーアップ
                    new CardEffect(
                        SampleCards.Spell,
                        new[]{
                            new EffectAction(EffectActionSetVariable: new(
                                "x",
                                new NumValue(NumValueCalculator: new(
                                    NumValueCalculator.ValueType.Count,
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ZoneCondition = new(new(new[]{ ZonePrettyName.YouHand }))
                                                }
                                            }))
                                    )))),
                            new EffectAction(MoveCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                ZoneCondition = new(new(new[]{ ZonePrettyName.YouHand }))
                                            }
                                        })),
                                ZonePrettyName.YouCemetery)),
                            new EffectAction(ModifyCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                Context = CardCondition.CardConditionContext.This
                                            }
                                        })),
                                Power: new NumValueModifier(
                                    NumValueModifier.ValueModifierOperator.Add,
                                    new NumValue(NumValueVariableCalculator: new("x"))),
                                Toughness: new NumValueModifier(
                                    NumValueModifier.ValueModifierOperator.Add,
                                    new NumValue(NumValueVariableCalculator: new("x")))
                                ))
                        })
                });

        public static CardDef KingGoblin
            => SampleCards.Creature(10, "ゴブリンの王", "偉大な存在", 8, 8,
                effects: new[]
                {
                    //TODO 未実装
                    // 場に4枚以上いるならコストがゼロになる
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouHand,
                            new EffectWhen(new EffectTiming()),
                            If: new(new NumCondition(4, NumCondition.ConditionCompare.GreaterThan),
                                    new NumValue(NumValueCalculator: new(
                                        NumValueCalculator.ValueType.Count,
                                        new Choice(
                                            new ChoiceSource(
                                                orCardConditions: new[]
                                                {
                                                    new CardCondition()
                                                    {
                                                        ZoneCondition = new(new ZoneValue(new[]{ZonePrettyName.YouField})),
                                                    }
                                                }))
                                            )
                                        ))),
                        new[]{
                            new EffectAction(ModifyCard:new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                Context = CardCondition.CardConditionContext.This
                                            }
                                        })),
                                Cost: new NumValueModifier(
                                    NumValueModifier.ValueModifierOperator.Replace,
                                    new NumValue(0))
                                ))
                        }),
                    // 場に4枚以上いるなら、プレイ時に場のカードを全て破壊する
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(Play: new(
                                EffectTimingPlayEvent.EventSource.This))),
                            If: new(new NumCondition(4, NumCondition.ConditionCompare.GreaterThan),
                                    new NumValue(NumValueCalculator: new(
                                        NumValueCalculator.ValueType.Count,
                                        new Choice(
                                            new ChoiceSource(
                                                orCardConditions: new[]
                                                {
                                                    new CardCondition()
                                                    {
                                                        Context = CardCondition.CardConditionContext.Others,
                                                        ZoneCondition = new(new ZoneValue(new[]{ZonePrettyName.YouField})),
                                                    }
                                                })))))),
                        new[]{
                            new EffectAction(DestroyCard:new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                Context = CardCondition.CardConditionContext.Others,
                                                ZoneCondition = new(new(new[]{ZonePrettyName.YouField}))
                                            }
                                        }))))
                        })
                });

        public static CardDef TempRamp
            => SampleCards.Sorcery(0, "一時的なランプ", "あなたの最大MPを1増加する。ターン終了時あなたの最大MPを1減少する。",
                effects: new[]
                {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(Play: new(EffectTimingPlayEvent.EventSource.This)))
                            ),
                        new[]
                        {
                            new EffectAction()
                            {
                                ModifyPlayer = new(
                                    new Choice(new ChoiceSource(
                                        orPlayerConditions: new[]
                                        {
                                            new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You)
                                        })),
                                    new PlayerModifier(
                                        MaxMp: new NumValueModifier(
                                            NumValueModifier.ValueModifierOperator.Add,
                                            new NumValue(1))
                                        ))
                            }
                        }
                    ),
                    // ターン終了時あなたの最大MPを1減少する。
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.None,
                            new EffectWhen(new EffectTiming(EndTurn: new(EffectTimingEndTurnEvent.EventSource.You))),
                            While: new(new(EndTurn: new(EffectTimingEndTurnEvent.EventSource.You)), 0, 1)
                            ),
                        new[]
                        {
                            new EffectAction()
                            {
                                ModifyPlayer = new(
                                    new Choice(new ChoiceSource(
                                        orPlayerConditions: new[]
                                        {
                                            new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You)
                                        })),
                                    new PlayerModifier(
                                        MaxMp: new NumValueModifier(
                                            NumValueModifier.ValueModifierOperator.Sub,
                                            new NumValue(1))
                                        ))
                            }
                        }
                    )
                });

        public static CardDef Sword
            => SampleCards.Sorcery(1, "剣", "パワーアップ",
            effects: new[]
            {
                new CardEffect(
                    SampleCards.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            ModifyCard = new EffectActionModifyCard(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                                TypeCondition = new CardTypeCondition(new[]{ CardType.Creature })
                                            },
                                        }),
                                    Choice.ChoiceHow.Choose,
                                    1),
                                Power: new NumValueModifier(
                                    NumValueModifier.ValueModifierOperator.Add,
                                    new NumValue(1))
                            )
                        }
                    }
                )
            });

        public static CardDef Shield
            => SampleCards.Sorcery(1, "盾", "タフネスアップ",
            effects: new[]
            {
                new CardEffect(
                    SampleCards.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            ModifyCard = new EffectActionModifyCard(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                                TypeCondition = new CardTypeCondition(new[]{ CardType.Creature })
                                            },
                                        }),
                                    Choice.ChoiceHow.Choose,
                                    1),
                                Toughness: new NumValueModifier(
                                    NumValueModifier.ValueModifierOperator.Add,
                                    new NumValue(1))
                            )
                        }
                    }
                )
            });

        public static CardDef Hit
            => SampleCards.Sorcery(1, "ヒット", "相手プレイヤーに1ダメージを与える。", isToken: true,
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction(Damage: new(
                                new NumValue(1),
                                new Choice(new ChoiceSource(
                                    orPlayerConditions: new[]
                                    {
                                        new PlayerCondition(Type: PlayerCondition.PlayerConditionType.Opponent)
                                    }))))
                        })
                });

        public static CardDef Heal
            => SampleCards.Sorcery(1, "ヒール", "あなたのライフを1回復する。", isToken: true,
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction(ModifyPlayer: new(
                                new Choice(new ChoiceSource(
                                    orPlayerConditions: new[]
                                    {
                                        new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You)
                                    })),
                                new PlayerModifier(Hp: new NumValueModifier(
                                    NumValueModifier.ValueModifierOperator.Add,
                                    new NumValue(1)))))
                        })
                });

        public static CardDef HitOrHeal
            => SampleCards.Sorcery(1, "ヒットorヒール", "「ヒット」か「ヒール」のうち1枚を選択し、それを自分の手札に加える。",
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction(AddCard: new(
                                new ZoneValue(new[]{ ZonePrettyName.YouHand }),
                                new Choice(new ChoiceSource(
                                    orCardConditions: new[]
                                    {
                                        new CardCondition()
                                        {
                                            ZoneCondition = new(new ZoneValue(new[]{ ZonePrettyName.CardPool })),
                                            CardSetCondition = new(CardSetCondition.ConditionType.This),
                                            NameCondition = new TextCondition(
                                                new TextValue(Hit.Name),
                                                TextCondition.ConditionCompare.Equality)
                                        },
                                        new CardCondition()
                                        {
                                            ZoneCondition = new(new ZoneValue(new[]{ ZonePrettyName.CardPool })),
                                            CardSetCondition = new(CardSetCondition.ConditionType.This),
                                            NameCondition = new TextCondition(
                                                new TextValue(Heal.Name),
                                                TextCondition.ConditionCompare.Equality)
                                        },
                                    }),
                                    how: Choice.ChoiceHow.Choose,
                                    numPicks: 1
                                    )))
                        })
                });

        public static CardDef Copy
            => SampleCards.Sorcery(2, "複製", "相手の場のカードを一枚選択する。そのカードと同名のカードを一枚あなたの手札に加える。",
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction()
                            {
                                AddCard = new(
                                    new ZoneValue(new[]{ ZonePrettyName.YouHand }),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ZoneCondition = new(new ZoneValue(new[]{ ZonePrettyName.OpponentField })),
                                                }
                                            }),
                                        Choice.ChoiceHow.Choose,
                                        1))
                            }
                        }
                    )
                });

        public static CardDef DoubleCopy
            => SampleCards.Sorcery(3, "二重複製", "相手の場のカードを一枚選択する。そのカードと同名のカードを二枚あなたの手札に加える。",
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction()
                            {
                                AddCard = new(
                                    new ZoneValue(new[]{ ZonePrettyName.YouHand }),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ZoneCondition = new(new ZoneValue(new[]{ ZonePrettyName.OpponentField })),
                                                }
                                            }),
                                        Choice.ChoiceHow.Choose
                                        ),
                                    NumOfAddCards: 2)
                            }
                        }
                    )
                });

        public static CardDef FirstAttack
            => SampleCards.Sorcery(2, "ゴブリンの一撃", "一撃目",
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction()
                            {
                                Damage = new(
                                    new NumValue(1),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ZoneCondition = new(new ZoneValue(new[]{ ZonePrettyName.YouField, ZonePrettyName.OpponentField })),
                                                    TypeCondition = new(new[]{ CardType.Creature })
                                                },
                                            },
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition()
                                                {
                                                    Type = PlayerCondition.PlayerConditionType.Opponent
                                                }
                                            }),
                                        Choice.ChoiceHow.Choose,
                                        1)),
                                AddCard = new(
                                    new ZoneValue(new[]{ ZonePrettyName.YouHand }),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ZoneCondition = new(new ZoneValue(new[]{ ZonePrettyName.CardPool })),
                                                    CardSetCondition = new(CardSetCondition.ConditionType.This),
                                                    NameCondition = new(
                                                        new TextValue(SecondAttack.Name),
                                                        TextCondition.ConditionCompare.Equality)
                                                }
                                            }),
                                        Choice.ChoiceHow.All,
                                        1))
                            }
                        }
                    )
                });

        public static CardDef SecondAttack
            => SampleCards.Sorcery(2, "ゴブリンの二撃", "二撃目", isToken: true,
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction()
                            {
                                Damage = new(
                                    new NumValue(2),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ZoneCondition = new(new ZoneValue(new[]{ ZonePrettyName.YouField, ZonePrettyName.OpponentField })),
                                                    TypeCondition = new(new[]{ CardType.Creature })
                                                },
                                            },
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition()
                                                {
                                                    Type = PlayerCondition.PlayerConditionType.Opponent
                                                }
                                            }),
                                        Choice.ChoiceHow.Choose,
                                        1))
                            }
                        }
                    )
                });

        public static CardDef HolyShield
            => SampleCards.Sorcery(2, "聖なる盾", "",
                effects: new[]
                {
                    // 自軍クリーチャーに次の効果を付与する。
                    // 「ターン終了時まで、受けるダメージは0になる。」
                    new CardEffect(
                        SampleCards.Spell,
                        new[]{
                            new EffectAction(
                                AddEffect: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ZoneCondition = new (new(new[]{ ZonePrettyName.YouField })),
                                                    Context = CardCondition.CardConditionContext.Others,
                                                }
                                            })),
                                    new[]
                                    {
                                        new CardEffect(
                                            new EffectCondition(ZonePrettyName.YouField,
                                                new EffectWhen(new EffectTiming(DamageBefore: new(
                                                    Source: EffectTimingDamageBeforeEvent.EventSource.Take,
                                                    CardCondition: new(){
                                                        Context = CardCondition.CardConditionContext.This,
                                                    }))),
                                                While: new(new EffectTiming(EndTurn: new(
                                                    EffectTimingEndTurnEvent.EventSource.You)),
                                                    0, 0)
                                                ),
                                            new[]
                                            {
                                                new EffectAction(
                                                    ModifyDamage: new(
                                                        new NumValueModifier(
                                                            NumValueModifier.ValueModifierOperator.Replace,
                                                            new NumValue(0)),
                                                        new Choice(
                                                            new ChoiceSource(
                                                                orCardConditions: new[]
                                                                {
                                                                    new CardCondition()
                                                                    {
                                                                        Context = CardCondition.CardConditionContext.This,
                                                                    }
                                                                }))
                                                    )
                                                )
                                            })
                                    })
                                )
                        }
                    )
                });

        public static CardDef ChangeHands => SampleCards.Sorcery(2, "手札入れ替え", "",
            effects: new[]
            {
                new CardEffect(
                    SampleCards.Spell,
                    new[]
                    {
                        // 手札の枚数をとっとく
                        new EffectAction(EffectActionSetVariable: new(
                            "x",
                            new NumValue(NumValueCalculator: new(
                                NumValueCalculator.ValueType.Count,
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                ZoneCondition = new(new(new[]{ ZonePrettyName.YouHand }))
                                            }
                                        })))))),
                        // 手札をすべて捨てる
                        new EffectAction(
                            MoveCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                ZoneCondition = new(new(new[]{ ZonePrettyName.YouHand }))
                                            }
                                        })),
                                ZonePrettyName.YouCemetery)
                            ),

                        // 捨てたカードと同じ枚数引く
                        new EffectAction(
                            DrawCard: new(
                                new NumValue(NumValueVariableCalculator: new("x")),
                                new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You)
                                )),
                    }
                ),
            });

        public static CardDef Slap
            => SampleCards.Sorcery(2, "袋叩き", "",
                effects: new[]
                {
                    // 使用時、対象の相手クリーチャー一体にxダメージ。x="自分の場のクリーチャーの数"
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction()
                            {
                                Damage = new EffectActionDamage(
                                    new NumValue(
                                        NumValueCalculator: new(
                                            NumValueCalculator.ValueType.Count,
                                            new Choice(
                                                new ChoiceSource(
                                                    orCardConditions: new[]
                                                    {
                                                        new CardCondition()
                                                        {
                                                            TypeCondition = new(new[]{CardType.Creature}),
                                                            ZoneCondition = new(
                                                                new ZoneValue(new[]{ZonePrettyName.YouField}))
                                                        }
                                                    })))),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ZoneCondition= new(new(new[]{ ZonePrettyName.OpponentField })),
                                                    TypeCondition = new CardTypeCondition(new []{CardType.Creature, }),
                                                }
                                            }),
                                        Choice.ChoiceHow.Choose,
                                        1)
                                )
                            }
                        }
                    )
                });

        public static CardDef Ramp
            => SampleCards.Sorcery(2, "ランプ", "あなたのMPの最大値を1増加し、利用可能なMPを1減少する。",
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction()
                            {
                                ModifyPlayer = new(
                                    new Choice(new ChoiceSource(
                                        orPlayerConditions: new[]
                                        {
                                            new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You)
                                        })),
                                    new PlayerModifier(
                                        MaxMp: new NumValueModifier(
                                            NumValueModifier.ValueModifierOperator.Add,
                                            new NumValue(1)),
                                        Mp: new NumValueModifier(
                                            NumValueModifier.ValueModifierOperator.Sub,
                                            new NumValue(1))
                                        ))
                            }
                        }
                    )
                });

        public static CardDef FullAttack
            => SampleCards.Sorcery(3, "一斉射撃", "撃てー！",
            effects: new[]
            {
                new CardEffect(
                    SampleCards.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            Damage = new EffectActionDamage(
                                new NumValue(1),
                                new Choice(
                                    new ChoiceSource(
                                        orPlayerConditions: new[]
                                        {
                                            new PlayerCondition(Type: PlayerCondition.PlayerConditionType.Opponent)
                                        },
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                ZoneCondition = new(new(new[]{ ZonePrettyName.OpponentField })),
                                                TypeCondition = new CardTypeCondition(new[]{ CardType.Creature })
                                            },
                                        })))
                        }
                    }
                )
            });

        public static CardDef GoblinCaptureJar
            => SampleCards.Sorcery(4, "ゴブリン封印の壺", "ゴブリンを無力化する不思議な壺",
                effects: new[]
                {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                Play: new(EffectTimingPlayEvent.EventSource.This)))),
                        new[]
                        {
                            new EffectAction()
                            {
                                ModifyCard = new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    NameCondition = new(new TextValue("ゴブリン"),
                                                        TextCondition.ConditionCompare.Like),
                                                    TypeCondition = new(new[]{ CardType.Creature }),
                                                    ZoneCondition = new(new(new[]{ ZonePrettyName.YouField, ZonePrettyName.OpponentField }))
                                                }
                                            })),
                                    Power: new(NumValueModifier.ValueModifierOperator.Replace,
                                        new NumValue(1)),
                                    Ability: new(CreatureAbilityModifier.OperatorValue.Add,
                                        CreatureAbility.Sealed)
                                    )
                            }
                        }
                    )
                });

        public static CardDef OldShield
            => SampleCards.Artifact(1, "ぼろの盾", "いまにも壊れそう",
                effects: new[]
                {
                    // 自分のクリーチャーが受けるダメージを1軽減する.その後このカードを破壊する
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(
                                DamageBefore: new(
                                    Source: EffectTimingDamageBeforeEvent.EventSource.Take,
                                    CardCondition: new CardCondition()
                                    {
                                        TypeCondition = new(new[]{ CardType.Creature }),
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                        Context = CardCondition.CardConditionContext.Others
                                    })))),
                        new[]
                        {
                            new EffectAction()
                            {
                                ModifyDamage = new EffectActionModifyDamage(
                                    new NumValueModifier(
                                        NumValueModifier.ValueModifierOperator.Sub,
                                        new NumValue(1)
                                    ),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    Context = CardCondition.CardConditionContext.EventSource,
                                                }
                                            }))
                                )
                            },
                            new EffectAction()
                            {
                                DestroyCard = new EffectActionDestroyCard(
                                    new Choice(new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                Context = CardCondition.CardConditionContext.This
                                            }
                                        }))
                                )
                            }
                        }
                    )
                });

        public static CardDef OldWall
            => SampleCards.Artifact(1, "ぼろの壁", "いまにも壊れそう",
                effects: new[]
                {
                    // 自分のプレイヤーまたはクリーチャーが受けるダメージを1軽減する.その後このカードを破壊する
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(
                                DamageBefore: new(
                                    Source: EffectTimingDamageBeforeEvent.EventSource.Take,
                                    PlayerCondition: new PlayerCondition()
                                    {
                                        Type = PlayerCondition.PlayerConditionType.You,
                                    },
                                    CardCondition: new CardCondition()
                                    {
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                        Context = CardCondition.CardConditionContext.Others
                                    }))
                            )
                        ),
                        new[]
                        {
                            new EffectAction()
                            {
                                ModifyDamage = new EffectActionModifyDamage(
                                    new NumValueModifier(
                                        NumValueModifier.ValueModifierOperator.Sub,
                                        new NumValue(1)
                                    ),
                                    new Choice(new ChoiceSource(
                                        orPlayerConditions: new[]
                                        {
                                            new PlayerCondition()
                                            {
                                                Context = PlayerCondition.PlayerConditionContext.EventSource,
                                            },
                                        },
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                Context = CardCondition.CardConditionContext.EventSource,
                                            }
                                        }))
                                )
                            },
                            new EffectAction()
                            {
                                DestroyCard = new EffectActionDestroyCard(
                                    new Choice(new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                Context = CardCondition.CardConditionContext.This
                                            }
                                        }))
                                )
                            }
                        }
                    )
                });

        public static CardDef EmergencyFood
            => SampleCards.Sorcery(1, "非常食", "ランダムに手札を1枚捨てる。あなたはそのカードのコスト分のライフを得る。",
                effects: new[] {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]{
                            new EffectAction(MoveCard: new(
                                new Choice(new ChoiceSource(
                                    orCardConditions: new[]
                                    {
                                        new CardCondition()
                                        {
                                            ZoneCondition = new(new(new[]{ ZonePrettyName.YouHand }))
                                        }
                                    }),
                                    Choice.ChoiceHow.Random,
                                    1),
                                ZonePrettyName.YouCemetery,
                                "moveCard"
                                )),
                            new EffectAction(ModifyPlayer: new(
                                new Choice(new ChoiceSource(
                                    orPlayerConditions: new[]
                                    {
                                        new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You)
                                    })),
                                new PlayerModifier(
                                    Hp: new(
                                        NumValueModifier.ValueModifierOperator.Add,
                                        new NumValue(NumValueCalculator: new(
                                            NumValueCalculator.ValueType.CardCost,
                                            new Choice(new ChoiceSource(
                                                orCardConditions: new[]
                                                {
                                                    new CardCondition()
                                                    {
                                                        ActionContext = new(ActionContextCardsOfMoveCard: new(
                                                            "moveCard",
                                                            ActionContextCardsOfMoveCard.ValueType.Moved
                                                            ))
                                                    }
                                                }))))))))
                        })
                });

        public static CardDef GoblinStatue
            => SampleCards.Artifact(4, "呪いのゴブリン像", "縁起は良くない",
                effects: new[]
                {
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(EndTurn: new(EffectTimingEndTurnEvent.EventSource.You))),
                            If: new(
                                new NumCondition(30, NumCondition.ConditionCompare.GreaterThan),
                                new NumValue(NumValueCalculator: new(
                                    NumValueCalculator.ValueType.Count,
                                    new Choice(new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                ZoneCondition = new(new(new[]{ ZonePrettyName.YouCemetery }))
                                            }
                                        })))))),
                        new[]{
                            new EffectAction(Damage: new(
                                new NumValue(6),
                                new Choice(new ChoiceSource(
                                    orPlayerConditions: new[]
                                    {
                                        new PlayerCondition(Type: PlayerCondition.PlayerConditionType.Opponent)
                                    },
                                    orCardConditions: new[]
                                    {
                                        new CardCondition()
                                        {
                                            ZoneCondition = new(new(new[]{ ZonePrettyName.OpponentField })),
                                            TypeCondition = new(new[]{ CardType.Creature })
                                        }
                                    }))))
                        }
                    )
                });

        public static CardDef HolyStatue
            => SampleCards.Artifact(4, "癒やしの像", "みんなを癒やすぞ",
            effects: new[]
            {
                // 使用時、すべての自分クリーチャーを+0/+1
                new CardEffect(
                    SampleCards.Spell,
                    new[]
                    {
                        new EffectAction(){
                            ModifyCard = new EffectActionModifyCard(
                                new Choice(new ChoiceSource(
                                    orCardConditions: new[]
                                    {
                                        new CardCondition()
                                        {
                                            ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                            TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                        }
                                    })),
                                Toughness: new NumValueModifier(
                                    NumValueModifier.ValueModifierOperator.Add,
                                    new NumValue(1))
                            )
                        }
                    }),

                // 自分クリーチャーのプレイ時+0/+1
                new CardEffect(
                    new(
                        ZonePrettyName.YouField,
                        new(new(MoveCard: new(
                            EffectTimingMoveCardEvent.EventSource.Other,
                            ZonePrettyName.YouHand,
                            ZonePrettyName.YouField)))
                    ),
                    new[]
                    {
                        new EffectAction(){
                            ModifyCard = new EffectActionModifyCard(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                                TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                                Context = CardCondition.CardConditionContext.EventSource,
                                            }
                                        })),
                                Toughness: new NumValueModifier(
                                    NumValueModifier.ValueModifierOperator.Add,
                                    new NumValue(1))
                            )
                        }
                    })
            });
    }
}
