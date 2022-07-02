using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;

namespace Cauldron.Core_Test
{
    public class SampleCards2
    {
        public static readonly string CardsetName = "Sample2";

        public static CardDef Goblin
            => SampleCards1.Creature(1, "ゴブリン", 1, 2, flavorText: "ただのゴブリン");

        public static CardDef DashGoblin
            => SampleCards1.Creature(2, "特攻ゴブリン", 1, 1,
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

        public static CardDef GoblinLover
            => SampleCards1.Creature(1, "ゴブリン好き", 1, 1,
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
                                            new NumValue(1),
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

        public static CardDef RevengeGoblin
            => SampleCards1.Creature(1, "仕返しゴブリン", 1, 1,
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

        public static CardDef DoubleStrikeGoblin
            => SampleCards1.Creature(3, "二刀流のゴブリン",
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

        public static CardDef GoblinFollower
            => SampleCards1.Creature(1, "ゴブリンフォロワー",
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
            => SampleCards1.Creature(2, "ゴブリンのペット",
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

        public static CardDef FireGoblin
            => SampleCards1.Creature(4, "火炎のゴブリン",
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

        public static CardDef BraveGoblin
            => SampleCards1.Creature(4, "ゴブリンの勇者",
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

        public static CardDef GiantGoblin
            => SampleCards1.Creature(5, "ゴブリンの巨人",
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
            => SampleCards1.Creature(5, "ゴブリンリーダー",
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
                                    OrPlayerConditions : new[] {
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

        public static CardDef TyrantGoblin
            => SampleCards1.Creature(6, "暴君ゴブリン",
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

        public static CardDef Gather
            => SampleCards1.Sorcery(1, "集合",
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

        public static CardDef DDObserver
            => SampleCards1.Creature(1, "異次元の観測者",
                1, 1,
                effects: new[]
                {
                    new CardEffect(
                        "このカードが場にあるとき、場のほかのカードが除外されるたびに、このカードを+1/+0する。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                ExcludeCard: new(
                                    new[]
                                    {
                                        new CardCondition(
                                            CardCondition.ContextConditionValue.Others
                                            )
                                    },
                                    FromZoneCondition: new(new ZoneValue(new[]
                                    {
                                        ZonePrettyName.YouField,
                                        ZonePrettyName.OpponentField,
                                    }))
                                )
                                ))
                            )),
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
            => SampleCards1.Creature(1, "異次元からの来訪者",
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
            => SampleCards1.Sorcery(1, "異次元からの脱出",
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
            => SampleCards1.Sorcery(1, "異次元との取引",
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
            => SampleCards1.Sorcery(1, "異次元ドロー",
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
            => SampleCards1.Sorcery(1, "異次元からの手",
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
            => SampleCards1.Creature(2, "異次元の戦士",
                1, 1, abilities: new[] { CreatureAbility.Cover },
                effects: new[] {
                    new CardEffect(
                        "このカードの戦闘前に、このカードと戦闘相手を除外する。",
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


        public static CardDef MagicObject
            => SampleCards1.Creature(1, "魔力吸収体",
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

        public static CardDef RunawayMagic
            => SampleCards1.Sorcery(2, "暴走する魔力",
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

        public static CardDef MagicMonster
            => SampleCards1.Creature(5, "魔法生物",
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
            => SampleCards1.Creature(2, "初級魔導士",
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
            => SampleCards1.Creature(9, "偉大な魔導士",
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
            => SampleCards1.Sorcery(3, "大魔法",
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
            => SampleCards1.Creature(3, "魔導の戦士",
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
            => SampleCards1.Creature(4, "魔導の戦士長",
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

        public static CardDef MagicBarrier
            => SampleCards1.Artifact(2, "防御魔法",
                annotations: new[] { ":魔導" },
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、このカードに魔導カウンターを5つ置く。",
                        new EffectConditionWrap(ByPlay: new()),
                        new[]
                        {
                            new EffectAction(ModifyCounter: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                CardCondition.ContextConditionValue.This
                                                )
                                        })
                                    ),
                                "魔導",
                                new NumValueModifier(
                                    NumValueModifier.OperatorValue.Add,
                                    new NumValue(5)
                                    )
                                )),
                        }),
                    new CardEffect(
                        "場にあるこのカードの魔導カウンターが0になったとき、このカードを破壊する。",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            When: new(new EffectTiming(ModifyCounter: new(
                                OrCardConditions: new[]
                                {
                                    new CardCondition(CardCondition.ContextConditionValue.This)
                                },
                                "魔導",
                                EffectTimingModifyCounterOnCardEvent.OperatorValue.Remove
                                ))),
                            If: new(new ConditionWrap(NumCondition: new(
                                new NumValue(NumValueCalculator: new(ForCounter: new(
                                    "魔導",
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(CardCondition.ContextConditionValue.This)
                                            })
                                        )
                                    ))),
                                new NumCompare(
                                    new NumValue(0),
                                    NumCompare.CompareValue.Equality
                                    )
                                )))
                            )),
                        new[]
                        {
                            new EffectAction(DestroyCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(CardCondition.ContextConditionValue.This)
                                        })
                                    )
                                ))
                        }),
                    new CardEffect(
                        "あなたがダメージを受ける前に、このカードの魔導カウンターをX個取り除く。X=受けるダメージ" +
                        "あなたが受けるダメージをY減少する。Y=取り除いた魔導カウンターの数",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            When: new(new EffectTiming(DamageBefore: new(
                                EffectTimingDamageBeforeEvent.TypeValue.Any,
                                TakePlayerCondition: new PlayerCondition(PlayerCondition.ContextValue.You)
                                )))
                            )),
                        new[]
                        {
                            new EffectAction(ModifyCounter: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(CardCondition.ContextConditionValue.This)
                                        })
                                    ),
                                "魔導",
                                new NumValueModifier(
                                    NumValueModifier.OperatorValue.Sub,
                                    new NumValue(NumValueCalculator: new(NumValueCalculator.EventContextValue.DamageValue))
                                    ),
                                "modifycounter"
                                )),
                            new EffectAction(ModifyDamage: new(
                                new NumValueModifier(
                                    NumValueModifier.OperatorValue.Sub,
                                    new NumValue(NumValueCalculator: new(ForCounter: new(
                                        CounterName: "魔導",
                                        ActionContextCounters: new(new ActionContextCountersOfModifyCounter(
                                            "modifycounter",
                                            ActionContextCountersOfModifyCounter.TypeValue.Modified))
                                        )))
                                    )
                                )),
                        }),
                });

        public static CardDef MagicRider
            => SampleCards1.Creature(6, "魔導騎兵",
                3, 5,
                effects: new[]
                {
                    new CardEffect(
                        $"あなたが魔法カードをプレイしたとき、あなたの場に「{MagicSoldier.Name}」1枚を追加する。",
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
                                                        new TextValue(MagicSoldier.Name),
                                                        TextCompare.CompareValue.Equality))
                                            }
                                    )),
                                    new ZoneValue(
                                        new []{ ZonePrettyName.YouField })
                                    ))
                        }
                    )
                });

        public static CardDef FirstSpell
            => SampleCards1.Sorcery(1, "第1魔法",
                effects: new[]
                {
                    new CardEffect(
                        "場にあるクリーチャー1体を選択する。それに1ダメージを与える。" +
                        $"あなたの手札に「{SecondSpell.Name}」1枚を加える。",
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
                                                        new TextValue(SecondSpell.Name),
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

        public static CardDef SecondSpell
            => SampleCards1.Sorcery(1, "第2魔法",
                isToken: true,
                effects: new[]
                {
                    new CardEffect(
                        "場にあるクリーチャー1体を選択する。それに1ダメージを与える。",
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
                                                    PlayerCondition.ContextValue.Opponent
                                                )
                                            }),
                                        Choice.HowValue.Choose,
                                        new NumValue(1)
                                        )
                                    )
                                )
                        }
                    )
                });

        public static CardDef MagicSoldier
            => SampleCards1.Creature(5, "魔導兵", 3, 5, isToken: true);

        public static CardDef ZombieToken
            => SampleCards1.Creature(1, "ゾンビトークン", 1, 1, annotations: new[] { ":ゾンビ" }, isToken: true);

        public static CardDef Zombie
            => SampleCards1.Creature(1, "ゾンビ", 1, 1,
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

        public static CardDef ThinZombie
            => SampleCards1.Creature(1, "ガリガリゾンビ", 1, 1,
                annotations: new[] { ":ゾンビ" },
                effects: new[]
                {
                    new CardEffect(
                        "このカードが墓地に移動したとき、あなたの墓地にあるすべての:ゾンビクリーチャーを+1/+0する。",
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouCemetery,
                            When: new(new EffectTiming(MoveCard: new(
                                OrCardConditions: new[]
                                {
                                    new CardCondition(CardCondition.ContextConditionValue.This)
                                },
                                To: ZonePrettyName.YouCemetery
                                )))
                            )),
                        new[]
                        {
                            new EffectAction(ModifyCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition(
                                                ZoneCondition: new(new ZoneValue(new[]
                                                {
                                                    ZonePrettyName.YouCemetery
                                                })),
                                                TypeCondition: new(new[]
                                                {
                                                    CardType.Creature
                                                }),
                                                AnnotationCondition: new(":ゾンビ")
                                                ),
                                        })
                                    ),
                                Power: new(
                                    NumValueModifier.OperatorValue.Add,
                                    new NumValue(1)
                                    )
                                ))
                        }),
                });

        public static CardDef ZombieDog
            => SampleCards1.Creature(2, "ゾンビ犬", 1, 1, annotations: new[] { ":ゾンビ" },
                numTurnsToCanAttackToCreature: 0,
                numTurnsToCanAttackToPlayer: 0
                );

        public static CardDef ZombieKiller
            => SampleCards1.Creature(2, "ゾンビキラー", 2, 2,
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
            => SampleCards1.Creature(2, "ゾンビプリンス", 1, 1,
                annotations: new[] { ":ゾンビ" },
                effects: new[]
                {
                    new CardEffect(
                        "このカードが場にある限り、あなたの場のほかのカードが場から墓地に移動するたび、あなたの場にゾンビトークンを追加する。",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            When: new(new EffectTiming(MoveCard: new(
                                OrCardConditions: new[]
                                {
                                    new CardCondition(CardCondition.ContextConditionValue.Others)
                                },
                                From: ZonePrettyName.YouField,
                                To: ZonePrettyName.YouCemetery
                                )))
                            )),
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
                });

        public static CardDef ZombieMaster
            => SampleCards1.Creature(4, "ゾンビ使い", 1, 1,
                annotations: new[] { ":ゾンビ" },
                effects: new[]
                {
                    new CardEffect(
                        "このカードをプレイしたとき、墓地にある:ゾンビクリーチャーを1体選択し、場に移動する。",
                        new EffectConditionWrap(ByPlay: new()),
                        new[]
                        {
                            new EffectAction(MoveCard:new(
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
                                ZonePrettyName.YouField
                                )),
                        }),
                });

        public static CardDef GluttonZombie
            => SampleCards1.Creature(3, "大食らいのゾンビ", 0, 1,
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
            => SampleCards1.Creature(3, "ゾンビキング", 0, 1,
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
                                OrCardConditions: new[]
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
            => SampleCards1.Sorcery(2, "ゾンビウイルス",
                annotations: new[] { ":ゾンビ" },
                effects: new[]
                {
                    new CardEffect(
                        "お互いの手札、墓地のクリーチャーすべてに:ゾンビを付与する。",
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
                                                    ZonePrettyName.YouHand,
                                                    ZonePrettyName.OpponentHand,
                                                    ZonePrettyName.YouCemetery,
                                                    ZonePrettyName.OpponentCemetery,
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
            => SampleCards1.Sorcery(2, "日の光",
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
            => SampleCards1.Sorcery(1, "ゾンビの呼び声",
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
            => SampleCards1.Sorcery(1, "リビングデッド",
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

        public static CardDef ZombiesStatue
            => SampleCards1.Artifact(1, "ゾンビの像",
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

        public static CardDef GoblinsElixir => SampleCards1.Sorcery(
            0,
            "ゴブリンの秘薬",
            effects: new[]
            {
                new CardEffect(
                    "選択したクリーチャーを-1/+0する。選択したクリーチャーが「ゴブリン」なら代わりに+1/+0する。",
                    new EffectConditionWrap(ByPlay: new()),
                    new[]
                    {
                        new EffectAction(
                            ModifyCard: new(
                                new Choice(
                                    new ChoiceSource(orCardConditions: new[]
                                    {
                                        new CardCondition(
                                            ZoneCondition: new(new ZoneValue(new[]
                                            {
                                                ZonePrettyName.YouField,
                                                ZonePrettyName.OpponentField
                                            })),
                                            TypeCondition: new(new[]
                                            {
                                                CardType.Creature
                                            }))
                                    }),
                                    Choice.HowValue.Choose,
                                    new NumValue(1)
                                    ),
                                Power: new(
                                    NumValueModifier.OperatorValue.Add,
                                    new NumValue(
                                        1,
                                        NumValueModifier: new(
                                            NumValueModifier.OperatorValue.Sub,
                                            new NumValue(
                                                2,
                                                NumValueModifier: new(
                                                    NumValueModifier.OperatorValue.Multi,
                                                    new NumValue(
                                                        NumValueCalculator: new(
                                                            ForCard: new(
                                                                NumValueCalculatorForCard.TypeValue.Count,
                                                                new Choice(new ChoiceSource(
                                                                    orCardConditions: new[]{
                                                                        new CardCondition(
                                                                            CardCondition.ContextConditionValue.ActionTarget,
                                                                            NameCondition: new(
                                                                                new TextValue("ゴブリン"),
                                                                                TextCompare.CompareValue.Contains,
                                                                                Not: true
                                                                            ))
                                                                    }))
                                                                )
                                                            )
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                    })
            });
    }
}
