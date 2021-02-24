using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;

namespace Cauldron.Core.Entities
{
    public class TestCards
    {
        public static readonly string CardsetName = "Sample";

        public static readonly CardDef angelSnipe = MessageObjectExtensions.Sorcery(1, "エンジェルスナイプ", "",
            effects: new[]
            {
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            Damage = new EffectActionDamage(
                                1,
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
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.OpponentField })),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature })
                                    }
                                }
                            )
                        }
                    }
                )
            });

        public static readonly CardDef bellAngel = MessageObjectExtensions.Creature(2, "ベルエンジェル", "", 0, 2, 1,
            abilities: new[] { CreatureAbility.Cover },
            effects: new[]{
                new CardEffect(
                    new(ZonePrettyName.YouCemetery,
                        new(new(Destroy: new EffectTimingDestroyEvent(EffectTimingDestroyEvent.EventSource.This)))),
                    new[]
                    {
                        new EffectAction(
                            DrawCard: new EffectActionDrawCard(
                                new NumValue(1),
                                new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You)))
                    }
                )
            });

        public static readonly CardDef mino = MessageObjectExtensions.Creature(2, "ミノタウロス", "", 2, 1, 1,
            abilities: new[] { CreatureAbility.Cover });

        public static readonly CardDef kenma = MessageObjectExtensions.Sorcery(2, "研磨の魔法", "",
            effects: new[]
            {
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]
                    {
                        new EffectAction(
                            ModifyCard: new EffectActionModifyCard(
                                new NumValue(2),
                                new NumValue(0),
                                new Choice()
                                {
                                    How = Choice.ChoiceHow.Choose,
                                    NumPicks=1,
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                    }
                                }
                            )
                        )
                    }
               )
            });

        public static readonly CardDef hikari = MessageObjectExtensions.Sorcery(2, "光の道筋", "",
            effects: new[]
            {
                // カードを1枚引く
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]
                    {
                        new EffectAction(
                            DrawCard: new(new NumValue(1), new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You))
                        )
                    }
                ),
                // このカードが手札から捨てられたなら、1枚引く
                new CardEffect(
                    new(ZonePrettyName.YouCemetery,
                        new(new(MoveCard: new(EffectTimingMoveCardEvent.EventSource.This, ZonePrettyName.YouHand, ZonePrettyName.YouCemetery)))),
                    new[]
                    {
                        new EffectAction(
                            DrawCard: new(new NumValue(1), new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You))
                        )
                    }
                )
            });

        public static readonly CardDef unmei = MessageObjectExtensions.Sorcery(2, "新たなる運命", "",
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

        public static readonly CardDef shieldAngel = MessageObjectExtensions.Creature(3, "シールドエンジェル", "", 1, 4, 1,
            abilities: new[] { CreatureAbility.Cover });

        public static readonly CardDef healingAngel = MessageObjectExtensions.Creature(3, "ヒーリングエンジェル", "", 2, 3, 1,
            effects: new[]
            {
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]{
                        new EffectAction(
                            ModifyPlayer: new(
                                new Choice()
                                {
                                    PlayerCondition = new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You),
                                    NumPicks = 1
                                },
                                new PlayerModifier(Hp: new NumValueModifier(NumValueModifier.ValueModifierOperator.Add, new NumValue(2)))
                            )
                        )
                    }
                )
            });

        public static readonly CardDef lizardman = MessageObjectExtensions.Creature(3, "リザードマン", "", 3, 2, 1,
            effects: new[]
            {
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            AddCard = new(
                                new ZoneValue(new[]{ ZonePrettyName.YouField }),
                                new Choice()
                                {
                                    CardCondition = new CardCondition()
                                    {
                                        NameCondition = new(
                                            new TextValue($"{CardsetName}.リザードマン"),
                                            TextCondition.ConditionCompare.Equality
                                        ),
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.CardPool }))
                                    },
                                    How = Choice.ChoiceHow.All,
                                    NumPicks = 1,
                                }
                            )
                        }
                    }
                )
            });

        public static readonly CardDef angelBarrage = MessageObjectExtensions.Sorcery(3, "エンジェルバレッジ", "",
            effects: new[]
            {
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            Damage = new EffectActionDamage(
                                1,
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

        public static readonly CardDef gakkyoku = MessageObjectExtensions.Sorcery(3, "天上の楽曲", "",
            effects: new[]
            {
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            ModifyCard = new EffectActionModifyCard(
                                new NumValue(0),
                                new NumValue(3),
                                new Choice()
                                {
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature })
                                    },
                                    How = Choice.ChoiceHow.Choose,
                                    NumPicks = 1,
                                }
                            )
                        }
                    }
                )
            });

        public static readonly CardDef ulz = MessageObjectExtensions.Creature(4, "ウルズ", "", 3, 3,
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
                                    ZoneCondition = new(new(new[]{ ZonePrettyName.YouField, ZonePrettyName.OpponentField })),
                                    TypeCondition = new(new[]{ CardType.Creature })
                                }
                            },
                            new[]
                            {
                                new CardEffect(
                                    new EffectCondition(ZonePrettyName.YouCemetery,
                                        new EffectWhen(new EffectTiming(Destroy: new(EffectTimingDestroyEvent.EventSource.This)))),
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
                                                    ZoneCondition = new ZoneCondition(new ZoneValue(new[]{ ZonePrettyName.CardPool })),
                                                }
                                            }))
                                    })
                            }, "addEffect")),
                        // 自分か相手のクリーチャーを一体破壊する
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

        public static readonly CardDef demonStraike = MessageObjectExtensions.Sorcery(4, "デモンストライク", "",
            effects: new[]
            {
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            Damage = new EffectActionDamage(
                                3,
                                new Choice()
                                {
                                    PlayerCondition = new PlayerCondition(
                                        Type: PlayerCondition.PlayerConditionType.Opponent),
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.OpponentField })),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                    },
                                    How = Choice.ChoiceHow.Choose,
                                    NumPicks = 1,
                                }
                            )
                        }
                    }
                )
            });

        public static readonly CardDef meifu = MessageObjectExtensions.Artifact(4, "冥府への道", "",
            effects: new[]
            {
                new CardEffect(
                    new(ZonePrettyName.YouField,
                        new(new(EndTurn: new EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.EventSource.You))),
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
                            6,
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

        public static readonly CardDef goblinDemon = MessageObjectExtensions.Creature(5, "ゴブリンマウントデーモン", "", 3, 7, 1,
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
                                3,
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

        public static readonly CardDef fujin = MessageObjectExtensions.Creature(5, "風神", "", 1, 5, 1,
            effects: new[]
            {
                // プレイ時：自分のクリーチャーすべてを+1/+0 する。
                // 自分のターン開始時：自分のクリーチャーすべてを+1/+0 する。
                new CardEffect(
                    new(
                        ZonePrettyName.YouField,
                        new(new(
                            Play: new EffectTimingPlayEvent(EffectTimingPlayEvent.EventSource.This),
                            StartTurn: new EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.EventSource.You)
                            ))),
                    new[]
                    {
                        new EffectAction()
                        {
                            ModifyCard = new EffectActionModifyCard(
                                new NumValue(1),
                                new NumValue(0),
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

        public static readonly CardDef excution = MessageObjectExtensions.Sorcery(5, "エクスキューション", "",
            effects: new[]
            {
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            DestroyCard = new EffectActionDestroyCard(
                                new Choice()
                                {
                                    How = Choice.ChoiceHow.Choose,
                                    NumPicks = 1,
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.OpponentField })),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature, CardType.Artifact })
                                    }
                                }
                            )
                        }
                    }
                )
            });

        public static readonly CardDef atena = MessageObjectExtensions.Creature(6, "アテナ", "", 5, 4,
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
                                    CardCondition =new CardCondition()
                                    {
                                        ZoneCondition = new ZoneCondition(new(new[]{ ZonePrettyName.YouField })),
                                        Context = CardCondition.CardConditionContext.Others,
                                    }
                                },
                                new[]
                                {
                                    new CardEffect(
                                        new(ZonePrettyName.YouField,
                                            new(new(
                                                DamageBefore: new(EffectTimingDamageBeforeEvent.EventSource.Guard,
                                                    CardCondition: new(){
                                                        Context = CardCondition.CardConditionContext.This,
                                                    }))),
                                            While: new(new(EndTurn: new(EffectTimingEndTurnEvent.EventSource.You)), 0, 1)
                                            ),
                                        new[]
                                        {
                                            new EffectAction(
                                                ModifyDamage: new(
                                                    new NumValueModifier(NumValueModifier.ValueModifierOperator.Replace, new NumValue(0)),
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

        public static readonly CardDef tenyoku = MessageObjectExtensions.Creature(6, "天翼を食う者", "", 6, 6,
            effects: new[]
            {
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
                            new NumValue(NumValueVariableCalculator: new("x")),
                            new NumValue(NumValueVariableCalculator: new("x")),
                            new Choice()
                            {
                                How = Choice.ChoiceHow.All,
                                CardCondition = new()
                                {
                                    Context = CardCondition.CardConditionContext.This
                                }
                            }))
                    })
            });

        public static readonly CardDef gabriel = MessageObjectExtensions.Creature(7, "ガブリエル", "", 3, 4, 1,
            effects: new[]
            {
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            ModifyCard = new EffectActionModifyCard(
                                new NumValue(4),
                                new NumValue(3),
                                new Choice()
                                {
                                    How = Choice.ChoiceHow.Choose,
                                    NumPicks = 1,
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                        Context = CardCondition.CardConditionContext.Others,
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature })
                                    }
                                }
                            )
                        }
                    }
                )
            });

        public static readonly CardDef gilgamesh = MessageObjectExtensions.Creature(7, "ギルガメッシュ", "", 5, 4, 0);

        public static readonly CardDef lucifer = MessageObjectExtensions.Creature(8, "ルシフェル", "", 6, 7, 1,
            effects: new[] {
                new CardEffect(
                    new(ZonePrettyName.YouField,
                        new(new(EndTurn: new EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.EventSource.You)))),
                    new[]{ new EffectAction(
                        ModifyPlayer: new(
                            new Choice()
                            {
                                PlayerCondition = new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You),
                                NumPicks = 1
                            },
                            new PlayerModifier(Hp: new NumValueModifier(NumValueModifier.ValueModifierOperator.Add, new NumValue(4)))
                        )
                        )}
                    )
            });

        //TODO サタン
        public static readonly CardDef satan = MessageObjectExtensions.Creature(10, "サタン", "", 6, 6, 1,
            effects: new[] {
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]{ new EffectAction() }
                    )
            });

        public static readonly CardDef fairy = MessageObjectExtensions.Creature(1, "フェアリー", "", 1, 1, 1, isToken: true);

        public static readonly CardDef goblin = MessageObjectExtensions.Creature(1, "ゴブリン", "", 1, 2, 1);

        public static readonly CardDef mouse = MessageObjectExtensions.Creature(1, "ネズミ", "", 1, 1, 1,
            effects: new[]
            {
                // 死亡時、相手に1ダメージ
                new CardEffect(
                    new(
                        ZonePrettyName.YouCemetery,
                        new(new(Destroy: new EffectTimingDestroyEvent(EffectTimingDestroyEvent.EventSource.This)))
                    ),
                    new []
                    {
                        new EffectAction()
                        {
                            Damage = new EffectActionDamage(
                                1,
                                new Choice()
                                {
                                    PlayerCondition = new PlayerCondition()
                                    {
                                        Type = PlayerCondition.PlayerConditionType.Opponent,
                                    },
                                }
                            )
                        }
                    }
                )
            });

        public static readonly CardDef ninja = MessageObjectExtensions.Creature(1, "忍者", "テストクリーチャー", 1, 1, 1,
            abilities: new[] { CreatureAbility.Stealth });

        public static readonly CardDef waterFairy = MessageObjectExtensions.Creature(1, "ウォーターフェアリー", "テストクリーチャー", 1, 1, 1,
            effects: new[]
            {
                // 破壊時、フェアリー１枚を手札に加える
                new CardEffect(
                    new(
                        ZonePrettyName.YouCemetery,
                        new(new(Destroy: new EffectTimingDestroyEvent(EffectTimingDestroyEvent.EventSource.This)))
                    ),
                    new[]
                    {
                        new EffectAction()
                        {
                            AddCard = new EffectActionAddCard(
                                new ZoneValue(new[]{ ZonePrettyName.YouHand }),
                                new Choice()
                                {
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.CardPool })),
                                        NameCondition = new(
                                            new TextValue($"{CardsetName}.{fairy.Name}"),
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

        public static readonly CardDef slime = MessageObjectExtensions.Creature(2, "スライム", "テストクリーチャー", 1, 1, 1,
            effects: new[]
            {
                // 召喚時、スライムを一体召喚
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            AddCard = new EffectActionAddCard(
                                new ZoneValue(new[]{ ZonePrettyName.YouField }),
                                new Choice()
                                {
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new ZoneCondition(new(new[]{ ZonePrettyName.CardPool })),
                                        NameCondition = new(
                                            new TextValue($"{CardsetName}.スライム"),
                                            TextCondition.ConditionCompare.Equality
                                        )
                                    },
                                    How = Choice.ChoiceHow.All,
                                    NumPicks = 1,
                                }
                            )
                        }
                    }
                )
            });

        public static readonly CardDef knight
            = MessageObjectExtensions.Creature(2, "ナイト", "テストクリーチャー", 1, 2, 1, abilities: new[] { CreatureAbility.Cover });
        public static readonly CardDef ninjaKnight
            = MessageObjectExtensions.Creature(3, "忍者ナイト", "テストクリーチャー", 1, 2, 1,
                abilities: new[] { CreatureAbility.Cover, CreatureAbility.Stealth });

        public static readonly CardDef whiteGeneral = MessageObjectExtensions.Creature(4, "ホワイトジェネラル", "テストクリーチャー", 2, 2, 1,
            effects: new[]
            {
                // 召喚時、自分のクリーチャーをランダムで一体を+2/+0
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            ModifyCard = new EffectActionModifyCard(
                                new NumValue(2),
                                new NumValue(0),
                                new Choice()
                                {
                                    CardCondition = new CardCondition()
                                    {
                                        Context = CardCondition.CardConditionContext.Others,
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                    },
                                    NumPicks = 1,
                                    How = Choice.ChoiceHow.Random
                                }
                            )
                        }
                    }
                )
            });

        public static readonly CardDef commander = MessageObjectExtensions.Creature(6, "セージコマンダー", "テストクリーチャー", 3, 3, 1,
            effects: new[]
            {
                // 召喚時、自分のクリーチャーすべてを+1/+2
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            ModifyCard = new EffectActionModifyCard(
                                new NumValue(1),
                                new NumValue(2),
                                new Choice()
                                {
                                    CardCondition = new CardCondition()
                                    {
                                        Context = CardCondition.CardConditionContext.Others,
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                        ZoneCondition = new ZoneCondition(new(new[]{ ZonePrettyName.YouField }))
                                    },
                                    How = Choice.ChoiceHow.All,
                                }
                            )
                        }
                    }
                )
            });

        public static readonly CardDef angel = MessageObjectExtensions.Artifact(2, "天使の像", "テストアーティファクト",
            effects: new[]
            {
                // ターン開始時、カレントプレイヤーに1ダメージ
                new CardEffect(
                    new(
                        ZonePrettyName.YouField,
                        new(new(StartTurn: new EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.EventSource.Both)))
                    ),
                    new []{
                        new EffectAction()
                        {
                            Damage = new EffectActionDamage(
                                1,
                                new Choice()
                                {
                                    PlayerCondition = new PlayerCondition()
                                    {
                                        Type = PlayerCondition.PlayerConditionType.Active,
                                    }
                                }
                            )
                        }
                    }
                )
            });

        public static readonly CardDef devil = MessageObjectExtensions.Artifact(1, "悪魔の像", "テストアーティファクト",
            effects: new[]
            {
                // ターン終了時、ランダムな相手クリーチャー一体に1ダメージ。その後このカードを破壊
                new CardEffect(
                    new(
                        ZonePrettyName.YouField,
                        new(new(EndTurn: new EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.EventSource.Both)))
                    ),
                    new[]
                    {
                        new EffectAction()
                        {
                            Damage = new EffectActionDamage(
                                1,
                                new Choice()
                                {
                                    How = Choice.ChoiceHow.Random,
                                    CardCondition= new CardCondition()
                                    {
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.OpponentField })),
                                        TypeCondition=new CardTypeCondition(new[]{CardType.Creature}),
                                    },
                                    NumPicks=1
                                }
                            )
                        },
                        new EffectAction()
                        {
                            DestroyCard = new EffectActionDestroyCard(
                                new Choice()
                                {
                                    How = Choice.ChoiceHow.All,
                                    CardCondition = new CardCondition()
                                    {
                                        Context = CardCondition.CardConditionContext.This,
                                    }
                                }
                            )
                        }
                    }
                )
            });

        public static readonly CardDef fortuneSpring = MessageObjectExtensions.Artifact(2, "運命の泉", "テストアーティファクト",
            effects: new[]
            {
                // ターン終了時、ランダムな自分のクリーチャー一体を+1/+0
                new CardEffect(
                    new(
                        ZonePrettyName.YouField,
                        new(new(EndTurn: new EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.EventSource.You)))
                    ),
                    new[]
                    {
                        new EffectAction()
                        {
                            ModifyCard = new EffectActionModifyCard(
                                new NumValue(1),
                                new NumValue(0),
                                new Choice()
                                {
                                    CardCondition=  new CardCondition()
                                    {
                                        ZoneCondition= new(new(new[]{ ZonePrettyName.YouField })),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature}),
                                    },
                                    NumPicks=1,
                                    How= Choice.ChoiceHow.Random
                                }
                            )
                        }
                    }
                )
            });

        public static readonly CardDef flag = MessageObjectExtensions.Artifact(4, "王家の御旗", "",
            effects: new[]
            {
                // 使用時、すべての自分クリーチャーを+1/+0
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]
                    {
                        new EffectAction(){
                            ModifyCard = new EffectActionModifyCard(
                                new NumValue(1),
                                new NumValue(0),
                                new Choice()
                                {
                                    How = Choice.ChoiceHow.All,
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                    }
                                }
                            )
                        }
                    }),
                // 自分クリーチャーのプレイ時+1/+0
                new CardEffect(
                    new(
                        ZonePrettyName.YouField,
                        new(new(MoveCard: new(EffectTimingMoveCardEvent.EventSource.Other, ZonePrettyName.YouHand, ZonePrettyName.YouField)))
                    ),
                    new[]
                    {
                        new EffectAction(){
                            ModifyCard = new EffectActionModifyCard(
                                new NumValue(1),
                                new NumValue(0),
                                new Choice()
                                {
                                    How = Choice.ChoiceHow.All,
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                        Context = CardCondition.CardConditionContext.EventSource,
                                    }
                                }
                            )
                        }
                    })
            });

        public static readonly CardDef shock = MessageObjectExtensions.Sorcery(1, "ショック", "テストソーサリー",
            effects: new[]
            {
                // 使用時、相手かランダムな相手クリーチャー一体に2ダメージ
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            Damage=  new EffectActionDamage(
                                2,
                                new Choice()
                                {
                                    PlayerCondition = new PlayerCondition()
                                    {
                                        Type = PlayerCondition.PlayerConditionType.Opponent
                                    },
                                    NumPicks= 1,
                                    How= Choice.ChoiceHow.Random,
                                    CardCondition = new CardCondition()
                                    {
                                        TypeCondition = new CardTypeCondition(new []{CardType.Creature}),
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.OpponentField })),
                                    }
                                }
                            )
                        }
                    }
                )
            });

        public static readonly CardDef buf = MessageObjectExtensions.Sorcery(3, "武装強化", "テストソーサリー",
            effects: new[]
            {
            // 使用時、対象の自分クリーチャーを+2/+2
            new CardEffect(
                MessageObjectExtensions.Spell,
                new[]
                {
                    new EffectAction(){
                        ModifyCard=new EffectActionModifyCard(
                            new NumValue(2),
                            new NumValue(2),
                            new Choice()
                            {
                                How= Choice.ChoiceHow.Choose,
                                NumPicks=1,
                                CardCondition=new CardCondition()
                                {
                                    ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                    TypeCondition = new CardTypeCondition(new[]{CardType.Creature, }),
                                }
                            }
                        )
                    }
                }
            )
            });

        public static readonly CardDef shield = MessageObjectExtensions.Artifact(1, "盾", "盾",
            effects: new[]
            {
                // 自分のクリーチャーが受けるダメージを1軽減する.その後このカードを破壊する
                new CardEffect(
                    new(
                        ZonePrettyName.YouField,
                        new(new(
                            DamageBefore: new(
                                EffectTimingDamageBeforeEvent.EventSource.All,
                                CardCondition: new CardCondition()
                                {
                                    TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
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

        public static readonly CardDef wall = MessageObjectExtensions.Artifact(1, "壁", "壁",
            effects: new[]
            {
            // 自分のプレイヤーまたはクリーチャーが受けるダメージを1軽減する.その後このカードを破壊する
            new CardEffect(
                new(
                    ZonePrettyName.YouField,
                    new(new(
                        DamageBefore: new(
                            EffectTimingDamageBeforeEvent.EventSource.Guard,
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

        public static readonly CardDef holyKnight = MessageObjectExtensions.Creature(2, "聖騎士", "聖騎士", 1, 1, 1,
            effects: new[]
            {
                // 自分が受けるダメージを2軽減する
                new CardEffect(
                    new(
                        ZonePrettyName.YouField,
                        new(new(
                            DamageBefore: new(
                                EffectTimingDamageBeforeEvent.EventSource.Guard,
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
                    new(
                        ZonePrettyName.YouField,
                        new(new(
                            BattleBefore: new(
                                EffectTimingBattleBeforeEvent.EventSource.Attack,
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

        public static readonly CardDef shippu = MessageObjectExtensions.Sorcery(2, "疾風怒濤", "テストソーサリー",
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
                                1,
                                new Choice()
                                {
                                    How=  Choice.ChoiceHow.Choose,
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
            }
            //effects: new[]
            //{
            //    // 使用時、対象の相手クリーチャー一体にxダメージ。x="自分の場のクリーチャーの数"
            //    new CardEffect(CardEffectType.OnPlay, (gameMaster, ownerCard) =>
            //    {
            //        var owner = gameMaster.PlayersById[ownerCard.OwnerId];
            //        var damage = owner.Field.AllCards
            //            .Count(c => c.Type == CardType.Creature || c.Type == CardType.Token);

            //        Card targetCreature = gameMaster.AskCard(owner.Id, TargetCardType.OpponentCreature);
            //        if(targetCreature == null)
            //        {
            //            // 対象が存在しない場合はなにもしない
            //            return;
            //        }

            //        gameMaster.HitCreature(targetCreature.Id, damage);
            //    }),
            //}
            );
    }
}
