using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;

namespace Cauldron.Core.Entities
{
    public class SampleCards
    {
        public static readonly string CardsetName = "Sample";

        public static readonly CardDef KarakuriGoblin
            = MessageObjectExtensions.Creature(1, "からくりゴブリン", "トークン", 1, 1, isToken: true);

        public static readonly CardDef Goblin
            = MessageObjectExtensions.Creature(1, "ゴブリン", "ただのゴブリン", 1, 2);

        public static readonly CardDef QuickGoblin
            = MessageObjectExtensions.Creature(1, "素早いゴブリン", "早い", 1, 1, numTurnsToCanAttack: 0);

        public static readonly CardDef ShieldGoblin
            = MessageObjectExtensions.Creature(2, "盾持ちゴブリン", "盾になる", 1, 2,
                abilities: new[] { CreatureAbility.Cover });

        public static readonly CardDef DeadlyGoblin
            = MessageObjectExtensions.Creature(3, "暗殺ゴブリン", "暗殺者", 1, 1,
                abilities: new[] { CreatureAbility.Stealth, CreatureAbility.Deadly });

        public static readonly CardDef MechanicGoblin
            = MessageObjectExtensions.Creature(1, "ゴブリンの技師", "からくりを作り出す", 1, 1,
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
                                new Choice()
                                {
                                    CardCondition = new ()
                                    {
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.CardPool })),
                                        NameCondition = new(
                                            new TextValue($"{CardsetName}.{KarakuriGoblin.Name}"),
                                            TextCondition.ConditionCompare.Equality
                                        )
                                    },
                                    How = Choice.ChoiceHow.All,
                                    NumPicks=1,
                                }
                            )
                        }
                    }
                )
            });

        public static readonly CardDef NinjaGoblin
            = MessageObjectExtensions.Creature(3, "分身ゴブリン", "分身する", 1, 2,
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
                                    new Choice()
                                    {
                                        How = Choice.ChoiceHow.All,
                                        CardCondition = new()
                                        {
                                            ZoneCondition = new(new(new[]{ ZonePrettyName.CardPool })),
                                            NameCondition = new(
                                                new TextValue($"{CardsetName}.分身ゴブリン"),
                                                TextCondition.ConditionCompare.Equality)
                                        }
                                    }
                                    ))
                        })
                });

        public static readonly CardDef GoblinsGreed
            = MessageObjectExtensions.Sorcery(2, "ゴブリンの強欲", "ドローするぞ",
                effects: new[]
                {
                    // カードを2枚引く
                    new CardEffect(
                        MessageObjectExtensions.Spell,
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

        public static readonly CardDef ShamanGoblin
            = MessageObjectExtensions.Creature(3, "呪術師ゴブリン", "相手を呪い殺すぞ", 1, 1,
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
                                    new Choice(){
                                        How = Choice.ChoiceHow.Random,
                                        NumPicks = 1,
                                        CardCondition = new()
                                        {
                                            TypeCondition = new(new[]{CardType.Creature}),
                                            ZoneCondition = new(new ZoneValue(new[]{ZonePrettyName.OpponentField}))
                                        }
                                    })
                            }
                        }
                    )
                });

        public static readonly CardDef HealGoblin
            = MessageObjectExtensions.Creature(3, "優しいゴブリン", "回復してくれる", 1, 2,
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
                                    new Choice()
                                    {
                                        PlayerCondition = new(Type: PlayerCondition.PlayerConditionType.You),
                                        NumPicks = 1
                                    },
                                    new PlayerModifier(
                                        Hp: new NumValueModifier(
                                            NumValueModifier.ValueModifierOperator.Add,
                                            new NumValue(2)))
                                )
                                )
                        }
                    )
                });

        public static readonly CardDef FireGoblin
            = MessageObjectExtensions.Creature(4, "火炎のゴブリン", "火を飛ばす", 4, 2,
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
                                    new Choice()
                                    {
                                        How = Choice.ChoiceHow.Choose,
                                        NumPicks = 1,
                                        PlayerCondition = new PlayerCondition()
                                        {
                                            Type = PlayerCondition.PlayerConditionType.Opponent
                                        },
                                        CardCondition = new CardCondition()
                                        {
                                            Context = CardCondition.CardConditionContext.Others,
                                            TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                            ZoneCondition = new(new(new[]{ ZonePrettyName.OpponentField })),
                                        }
                                    }
                                )
                            }
                        }
                    )
                });

        public static readonly CardDef MadScientist
            = MessageObjectExtensions.Creature(4, "マッドサイエンティスト", "どこかおかしい", 3, 3,
                // 自分か相手のクリーチャーを一体破壊して、再生する
                effects: new[]
                {
                    // 破壊時にコピーを場に出す効果を追加する
                    new CardEffect(
                        MessageObjectExtensions.Spell,
                        new[]{
                            new EffectAction(AddEffect: new(
                                new Choice()
                                {
                                    How = Choice.ChoiceHow.Choose,
                                    NumPicks = 1,
                                    CardCondition = new()
                                    {
                                        Context = CardCondition.CardConditionContext.Others,
                                        ZoneCondition = new(new(new[]{
                                            ZonePrettyName.YouField, ZonePrettyName.OpponentField })),
                                        TypeCondition = new(new[]{ CardType.Creature })
                                    }
                                },
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
                                                new Choice()
                                                {
                                                    How = Choice.ChoiceHow.All,
                                                    CardCondition = new()
                                                    {
                                                        NameCondition = new TextCondition(
                                                            new TextValue(TextValueCalculator: new(
                                                                TextValueCalculator.ValueType.CardName,
                                                                new Choice(){
                                                                    How = Choice.ChoiceHow.All,
                                                                    CardCondition = new()
                                                                    {
                                                                        Context = CardCondition.CardConditionContext.This,
                                                                    }
                                                                })),
                                                            TextCondition.ConditionCompare.Equality),
                                                        ZoneCondition = new ZoneCondition(
                                                            new ZoneValue(new[]{ ZonePrettyName.CardPool })),
                                                    }
                                                }))
                                        })
                                }, "addEffect")),
                            // 効果を付与したカードを破壊する
                            new EffectAction(DestroyCard: new(
                                new Choice()
                                {
                                    How = Choice.ChoiceHow.All,
                                    CardCondition = new()
                                    {
                                        ActionContext = new(ActionContextCardsOfAddEffect: new(
                                            "addEffect",
                                            ActionContextCardsOfAddEffect.ValueType.TargetCards)),
                                    }
                                })),
                        }
                    )
                });

        public static readonly CardDef BraveGoblin
            = MessageObjectExtensions.Creature(4, "ゴブリンの勇者", "勇者！", 2, 2,
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
                                new Choice()
                                {
                                    CardCondition = new CardCondition()
                                    {
                                        Context = CardCondition.CardConditionContext.EventSource,
                                    }
                                }
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
                                new Choice()
                                {
                                    CardCondition = new CardCondition()
                                    {
                                        Context = CardCondition.CardConditionContext.EventSource,
                                    }
                                }
                            )
                        }
                    }
                )
            });

        public static readonly CardDef GiantGoblin
            = MessageObjectExtensions.Creature(5, "ゴブリンの巨人", "超でかい", 3, 7,
                abilities: new[] { CreatureAbility.Cover },
                effects: new[]
                {
                    new CardEffect(
                        MessageObjectExtensions.Spell,
                        new[]
                        {
                            new EffectAction()
                            {
                                Damage = new EffectActionDamage(
                                    new NumValue(3),
                                    new Choice()
                                    {
                                        How = Choice.ChoiceHow.All,
                                        CardCondition = new CardCondition()
                                        {
                                            ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                            TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                            Context = CardCondition.CardConditionContext.Others,
                                        }
                                    }
                                )
                            }
                        }
                    )
                });

        public static readonly CardDef LeaderGoblin
            = MessageObjectExtensions.Creature(5, "ゴブリンリーダー", "", 1, 5,
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
                                    new Choice()
                                    {
                                        How = Choice.ChoiceHow.All,
                                        CardCondition = new CardCondition()
                                        {
                                            ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                            TypeCondition = new (new[]{ CardType.Creature }),
                                            Context = CardCondition.CardConditionContext.Others,
                                        }
                                    },
                                    Power: new NumValueModifier(
                                        NumValueModifier.ValueModifierOperator.Add,
                                        new NumValue(1))
                                )
                            }
                        }
                    )
                });

        public static readonly CardDef TyrantGoblin
            = MessageObjectExtensions.Creature(6, "暴君ゴブリン", "界隈一のあばれもの", 6, 6,
                effects: new[]
                {
                    // 手札をすべて捨てて、捨てた枚数パワーアップ
                    new CardEffect(
                        MessageObjectExtensions.Spell,
                        new[]{
                            new EffectAction(EffectActionSetVariable: new(
                                "x",
                                new NumValue(NumValueCalculator: new(
                                    NumValueCalculator.ValueType.Count,
                                    new Choice()
                                    {
                                        How = Choice.ChoiceHow.All,
                                        CardCondition = new()
                                        {
                                            ZoneCondition = new(new(new[]{ ZonePrettyName.YouHand }))
                                        }
                                    }
                                    )))),
                            new EffectAction(MoveCard: new(
                                new Choice(){
                                    How = Choice.ChoiceHow.All,
                                    CardCondition = new()
                                    {
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.YouHand }))
                                    }
                                },
                                ZonePrettyName.YouCemetery)),
                            new EffectAction(ModifyCard: new(
                                new Choice()
                                {
                                    How = Choice.ChoiceHow.All,
                                    CardCondition = new()
                                    {
                                        Context = CardCondition.CardConditionContext.This
                                    }
                                },
                                Power: new NumValueModifier(
                                    NumValueModifier.ValueModifierOperator.Add,
                                    new NumValue(NumValueVariableCalculator: new("x"))),
                                Toughness: new NumValueModifier(
                                    NumValueModifier.ValueModifierOperator.Add,
                                    new NumValue(NumValueVariableCalculator: new("x")))
                                ))
                        })
                });

        public static readonly CardDef KingGoblin
            = MessageObjectExtensions.Creature(10, "ゴブリンの王", "偉大な存在", 8, 8,
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
                                        new Choice(){
                                            How = Choice.ChoiceHow.All,
                                            CardCondition = new()
                                            {
                                                ZoneCondition = new(new ZoneValue(new[]{ZonePrettyName.YouField})),
                                            }
                                        })))),
                        new[]{
                            new EffectAction(ModifyCard:new(
                                new Choice(){
                                    CardCondition = new()
                                    {
                                        Context = CardCondition.CardConditionContext.This
                                    }
                                },
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
                                        new Choice(){
                                            How = Choice.ChoiceHow.All,
                                            CardCondition = new()
                                            {
                                                Context = CardCondition.CardConditionContext.Others,
                                                ZoneCondition = new(new ZoneValue(new[]{ZonePrettyName.YouField})),
                                            }
                                        })))),
                        new[]{
                            new EffectAction(DestroyCard:new(
                                new Choice()
                                {
                                    CardCondition = new()
                                    {
                                        Context = CardCondition.CardConditionContext.Others,
                                        ZoneCondition = new(new(new[]{ZonePrettyName.YouField}))
                                    }
                                }))
                        })
                });

        public static readonly CardDef Sword
            = MessageObjectExtensions.Sorcery(1, "剣", "パワーアップ",
            effects: new[]
            {
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            ModifyCard = new EffectActionModifyCard(
                                new Choice()
                                {
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature })
                                    },
                                    How = Choice.ChoiceHow.Choose,
                                    NumPicks = 1,
                                },
                                Power: new NumValueModifier(
                                    NumValueModifier.ValueModifierOperator.Add,
                                    new NumValue(1))
                            )
                        }
                    }
                )
            });

        public static readonly CardDef Shield
            = MessageObjectExtensions.Sorcery(1, "盾", "タフネスアップ",
            effects: new[]
            {
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            ModifyCard = new EffectActionModifyCard(
                                new Choice()
                                {
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature })
                                    },
                                    How = Choice.ChoiceHow.Choose,
                                    NumPicks = 1,
                                },
                                Toughness: new NumValueModifier(
                                    NumValueModifier.ValueModifierOperator.Add,
                                    new NumValue(1))
                            )
                        }
                    }
                )
            });

        public static readonly CardDef HolyShield
            = MessageObjectExtensions.Sorcery(2, "聖なる盾", "",
                effects: new[]
                {
                    // 自軍クリーチャーに次の効果を付与する。
                    // 「ターン終了時まで、受けるダメージは0になる。」
                    new CardEffect(
                        MessageObjectExtensions.Spell,
                        new[]{
                            new EffectAction(
                                AddEffect: new(
                                    new Choice()
                                    {
                                        How = Choice.ChoiceHow.All,
                                        CardCondition = new()
                                        {
                                            ZoneCondition = new (new(new[]{ ZonePrettyName.YouField })),
                                            Context = CardCondition.CardConditionContext.Others,
                                        }
                                    },
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
                                                        new Choice()
                                                        {
                                                            CardCondition = new()
                                                            {
                                                                Context = CardCondition.CardConditionContext.This,
                                                            }
                                                        }
                                                    )
                                                )
                                            })
                                    })
                                )
                        }
                    )
                });

        public static readonly CardDef ChangeHands = MessageObjectExtensions.Sorcery(2, "手札入れ替え", "",
            effects: new[]
            {
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]
                    {
                        // 手札の枚数をとっとく
                        new EffectAction(EffectActionSetVariable: new(
                            "x",
                            new NumValue(NumValueCalculator: new(
                                NumValueCalculator.ValueType.Count,
                                new Choice()
                                {
                                    How = Choice.ChoiceHow.All,
                                    CardCondition = new()
                                    {
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.YouHand }))
                                    }
                                }
                                )))),

                        // 手札をすべて捨てる
                        new EffectAction(
                            MoveCard: new(
                                new Choice()
                                {
                                    How = Choice.ChoiceHow.All,
                                    CardCondition = new()
                                    {
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.YouHand }))
                                    }
                                },
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

        public static readonly CardDef Slap
            = MessageObjectExtensions.Sorcery(2, "袋叩き", "",
                effects: new[]
                {
                    // 使用時、対象の相手クリーチャー一体にxダメージ。x="自分の場のクリーチャーの数"
                    new CardEffect(
                        MessageObjectExtensions.Spell,
                        new[]
                        {
                            new EffectAction()
                            {
                                Damage = new EffectActionDamage(
                                    new NumValue(
                                        NumValueCalculator: new(
                                            NumValueCalculator.ValueType.Count,
                                            new Choice()
                                            {
                                                How = Choice.ChoiceHow.All,
                                                CardCondition = new()
                                                {
                                                    TypeCondition = new(new[]{CardType.Creature}),
                                                    ZoneCondition = new(
                                                        new ZoneValue(new[]{ZonePrettyName.YouField}))
                                                }
                                            })),
                                    new Choice()
                                    {
                                        How = Choice.ChoiceHow.Choose,
                                        NumPicks=1,
                                        CardCondition = new CardCondition()
                                        {
                                            ZoneCondition= new(new(new[]{ ZonePrettyName.OpponentField })),
                                            TypeCondition = new CardTypeCondition(new []{CardType.Creature, }),
                                        }
                                    }
                                )
                            }
                        }
                    )
                });

        public static readonly CardDef FullAttack
            = MessageObjectExtensions.Sorcery(3, "一斉射撃", "撃てー！",
            effects: new[]
            {
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            Damage = new EffectActionDamage(
                                new NumValue(1),
                                new Choice()
                                {
                                    PlayerCondition = new PlayerCondition(
                                        Type: PlayerCondition.PlayerConditionType.Opponent),
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.OpponentField })),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature })
                                    },
                                    How = Choice.ChoiceHow.All,
                                }
                            )
                        }
                    }
                )
            });

        public static readonly CardDef OldShield
            = MessageObjectExtensions.Artifact(1, "ぼろの盾", "いまにも壊れそう",
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
                                    new Choice()
                                    {
                                        CardCondition = new CardCondition()
                                        {
                                            Context = CardCondition.CardConditionContext.EventSource,
                                        }
                                    }
                                )
                            },
                            new EffectAction()
                            {
                                DestroyCard = new EffectActionDestroyCard(
                                    new Choice()
                                    {
                                        CardCondition = new CardCondition()
                                        {
                                            Context = CardCondition.CardConditionContext.This
                                        }
                                    }
                                )
                            }
                        }
                    )
                });

        public static readonly CardDef OldWall
            = MessageObjectExtensions.Artifact(1, "ぼろの壁", "いまにも壊れそう",
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
                                    new Choice()
                                    {
                                        PlayerCondition = new PlayerCondition()
                                        {
                                            Context = PlayerCondition.PlayerConditionContext.EventSource,
                                        },
                                        CardCondition = new CardCondition()
                                        {
                                            Context = CardCondition.CardConditionContext.EventSource,
                                        }
                                    }
                                )
                            },
                            new EffectAction()
                            {
                                DestroyCard = new EffectActionDestroyCard(
                                    new Choice()
                                    {
                                        CardCondition = new CardCondition()
                                        {
                                            Context = CardCondition.CardConditionContext.This
                                        }
                                    }
                                )
                            }
                        }
                    )
                });

        public static readonly CardDef GoblinStatue
            = MessageObjectExtensions.Artifact(4, "呪いのゴブリン像", "縁起は良くない",
                effects: new[]
                {
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(EndTurn: new(EffectTimingEndTurnEvent.EventSource.You))),
                            If: new(
                                new NumCondition(30, NumCondition.ConditionCompare.GreaterThan),
                                new NumValue(NumValueCalculator: new(
                                    NumValueCalculator.ValueType.Count,
                                    new Choice()
                                    {
                                        How = Choice.ChoiceHow.All,
                                        CardCondition = new()
                                        {
                                            ZoneCondition = new(new(new[]{ ZonePrettyName.YouCemetery }))
                                        }
                                    }
                                    ))
                                )),
                        new[]{
                            new EffectAction(Damage: new(
                                new NumValue(6),
                                new Choice()
                                {
                                    How = Choice.ChoiceHow.All,
                                    PlayerCondition = new(Type: PlayerCondition.PlayerConditionType.Opponent),
                                    CardCondition = new()
                                    {
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.OpponentField })),
                                        TypeCondition = new(new[]{ CardType.Creature })
                                    }
                                }))
                        }
                    )
                });

        public static readonly CardDef HolyStatue
            = MessageObjectExtensions.Artifact(4, "癒やしの像", "みんなを癒やすぞ",
            effects: new[]
            {
                // 使用時、すべての自分クリーチャーを+0/+1
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]
                    {
                        new EffectAction(){
                            ModifyCard = new EffectActionModifyCard(
                                new Choice()
                                {
                                    How = Choice.ChoiceHow.All,
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                    }
                                },
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
                                new Choice()
                                {
                                    How = Choice.ChoiceHow.All,
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                        Context = CardCondition.CardConditionContext.EventSource,
                                    }
                                },
                                Toughness: new NumValueModifier(
                                    NumValueModifier.ValueModifierOperator.Add,
                                    new NumValue(1))
                            )
                        }
                    })
            });
    }
}
