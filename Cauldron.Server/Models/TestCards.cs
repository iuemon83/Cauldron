using Cauldron.Server.Models.Effect;

namespace Cauldron.Server.Models
{
    public class TestCards
    {
        public static readonly string CardsetName = "Sample";

        public static readonly CardDef angelSnipe = CardDef.Sorcery(1, $"{CardsetName}.エンジェルスナイプ", "エンジェルスナイプ", "",
            effects: new[]
            {
                new CardEffect(
                    EffectCondition.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            Damage = new EffectActionDamage()
                            {
                                Value = 1,
                                Choice = new Choice()
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
                                        ZoneCondition = new(new[]{ ZonePrettyName.OpponentField }),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature })
                                    }
                                }
                            }
                        }
                    }
                )
            });

        public static readonly CardDef bellAngel = CardDef.Creature(2, $"{CardsetName}.ベルエンジェル", "ベルエンジェル", "", 0, 2, 1,
            abilities: new[] { CreatureAbility.Cover },
            effects: new[]{
                new CardEffect(
                    new(ZonePrettyName.YouCemetery,
                        new(new(Destroy: new EffectTimingDestroyEvent(EffectTimingDestroyEvent.EventSource.This)))),
                    new[]
                    {
                        new EffectAction(
                            DrawCard: new EffectActionDrawCard(
                                1,
                                new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You)))
                    }
                )
            });

        public static readonly CardDef mino = CardDef.Creature(2, $"{CardsetName}.ミノタウロス", "ミノタウロス", "", 2, 1, 1,
            abilities: new[] { CreatureAbility.Cover });

        public static readonly CardDef kenma = CardDef.Sorcery(2, $"{CardsetName}.研磨の魔法", "研磨の魔法", "",
            effects: new[]
            {
                new CardEffect(
                    EffectCondition.Spell,
                    new[]
                    {
                        new EffectAction(
                            ModifyCard: new EffectActionModifyCard(){
                                Power = 2,
                                Choice = new Choice()
                                {
                                    How = Choice.ChoiceHow.Choose,
                                    NumPicks=1,
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new[]{ ZonePrettyName.YouField }),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                    }
                                }
                            }
                        )
                    }
               )
            });

        public static readonly CardDef hikari = CardDef.Sorcery(2, $"{CardsetName}.光の道筋", "光の道筋", "",
            effects: new[]
            {
                // カードを1枚引く
                new CardEffect(
                    EffectCondition.Spell,
                    new[]
                    {
                        new EffectAction(
                            DrawCard: new EffectActionDrawCard(1, new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You))
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
                            DrawCard: new EffectActionDrawCard(1, new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You))
                        )
                    }
                )
            });

        // X=手札の数。X枚捨てて、X枚ドローする
        // C=指定したカード。X=Cのコスト。Cを捨てる。クリーチャー一体を+X/+X する

        //TODO 新たなる運命
        public static readonly CardDef unmei = CardDef.Sorcery(2, $"{CardsetName}.新たなる運命", "新たなる運命", "",
            effects: new[]
            {
                new CardEffect(
                    EffectCondition.Spell,
                    new[]
                    {
                        // 手札をすべて捨てる
                        new EffectAction(
                            MoveCard: new(
                                new Choice()
                                {
                                    How = Choice.ChoiceHow.All,
                                    CardCondition = new()
                                    {
                                        ZoneCondition = new(new[]{ ZonePrettyName.YouHand })
                                    }
                                },
                                ZonePrettyName.YouCemetery)
                            ),
                        // 捨てたカードと同じ枚数引く
                        new EffectAction(
                            DrawCard: new(
                                0,
                                new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You)
                                )),
                    }
                ),
            });

        public static readonly CardDef shieldAngel = CardDef.Creature(3, $"{CardsetName}.シールドエンジェル", "シールドエンジェル", "", 1, 4, 1,
            abilities: new[] { CreatureAbility.Cover });

        public static readonly CardDef healingAngel = CardDef.Creature(3, $"{CardsetName}.ヒーリングエンジェル", "ヒーリングエンジェル", "", 2, 3, 1,
            effects: new[]
            {
                new CardEffect(
                    EffectCondition.Spell,
                    new[]{
                        new EffectAction(
                            ModifyPlayer: new(
                                new Choice()
                                {
                                    PlayerCondition = new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You),
                                    NumPicks = 1
                                },
                                new PlayerModifier(Hp: new ValueModifier(ValueModifier.ValueModifierOperator.Add, 2))
                            )
                        )
                    }
                )
            });

        public static readonly CardDef lizardman = CardDef.Creature(3, $"{CardsetName}.リザードマン", "リザードマン", "", 3, 2, 1,
            effects: new[]
            {
                new CardEffect(
                    EffectCondition.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            AddCard = new EffectActionAddCard()
                            {
                                ZoneToAddCard = ZonePrettyName.YouField,
                                Choice = new Choice()
                                {
                                    CardCondition = new CardCondition()
                                    {
                                        NameCondition = new(
                                            $"{CardsetName}.リザードマン",
                                            TextCondition.ConditionCompare.Equality
                                        ),
                                        ZoneCondition = new(new[]{ ZonePrettyName.CardPool })
                                    },
                                    How = Choice.ChoiceHow.All,
                                    NumPicks = 1,
                                }
                            }
                        }
                    }
                )
            });

        public static readonly CardDef angelBarrage = CardDef.Sorcery(3, $"{CardsetName}.エンジェルバレッジ", "エンジェルバレッジ", "",
            effects: new[]
            {
                new CardEffect(
                    EffectCondition.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            Damage = new EffectActionDamage()
                            {
                                Value = 1,
                                Choice = new Choice()
                                {
                                    PlayerCondition = new PlayerCondition(
                                        Type: PlayerCondition.PlayerConditionType.Opponent),
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new[]{ ZonePrettyName.OpponentField }),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature })
                                    },
                                    How = Choice.ChoiceHow.All,
                                }
                            }
                        }
                    }
                )
            });

        public static readonly CardDef gakkyoku = CardDef.Sorcery(3, $"{CardsetName}.天上の楽曲", "天上の楽曲", "",
            effects: new[]
            {
                new CardEffect(
                    EffectCondition.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            ModifyCard = new EffectActionModifyCard()
                            {
                                Power = 0,
                                Toughness = 3,
                                Choice = new Choice()
                                {
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new[]{ ZonePrettyName.YouField }),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature })
                                    },
                                    How = Choice.ChoiceHow.Choose,
                                    NumPicks = 1,
                                }
                            }
                        }
                    }
                )
            });

        //TODO ウルズ
        public static readonly CardDef ulz = CardDef.Creature(4, $"{CardsetName}.ウルズ", "ウルズ", "", 1, 1,
            effects: new[]
            {
                new CardEffect(
                    EffectCondition.Spell,
                    new[]{ new EffectAction() }
                )
            });

        public static readonly CardDef demonStraike = CardDef.Sorcery(4, $"{CardsetName}.デモンストライク", "デモンストライク", "",
            effects: new[]
            {
                new CardEffect(
                    EffectCondition.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            Damage = new EffectActionDamage()
                            {
                                Value = 3,
                                Choice = new Choice()
                                {
                                    PlayerCondition = new PlayerCondition(
                                        Type: PlayerCondition.PlayerConditionType.Opponent),
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new[]{ ZonePrettyName.OpponentField }),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                    },
                                    How = Choice.ChoiceHow.Choose,
                                    NumPicks = 1,
                                }
                            }
                        }
                    }
                )
            });

        //TODO 冥府への道
        // 墓地の数を参照
        // if の実装
        public static readonly CardDef meifu = CardDef.Artifact(4, $"{CardsetName}.冥府への道", "冥府への道", "",
            effects: new[]
            {
                new CardEffect(
                    new(ZonePrettyName.YouField,
                        new(new(EndTurn: new EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.EventSource.You)))),
                    new[]{ new EffectAction() }
                )
            });

        public static readonly CardDef goblinDemon = CardDef.Creature(5, $"{CardsetName}.ゴブリンマウントデーモン", "ゴブリンマウントデーモン", "", 3, 7, 1,
            abilities: new[] { CreatureAbility.Cover },
            effects: new[]
            {
                new CardEffect(
                    EffectCondition.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            Damage = new EffectActionDamage()
                            {
                                Value = 3,
                                Choice = new Choice()
                                {
                                    How = Choice.ChoiceHow.All,
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new[]{ ZonePrettyName.YouField }),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                        Context = CardCondition.CardConditionContext.Others,
                                    }
                                }
                            }
                        }
                    }
                )
            });

        public static readonly CardDef fujin = CardDef.Creature(5, $"{CardsetName}.風神", "風神", "", 1, 5, 1,
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
                            ModifyCard = new EffectActionModifyCard()
                            {
                                Power = 1,
                                Toughness = 0,
                                Choice = new Choice()
                                {
                                    How = Choice.ChoiceHow.All,
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new[]{ ZonePrettyName.YouField }),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                        Context = CardCondition.CardConditionContext.Others,
                                    }
                                }
                            }
                        }
                    }
                )
            });

        public static readonly CardDef excution = CardDef.Sorcery(5, $"{CardsetName}.エクスキューション", "エクスキューション", "",
            effects: new[]
            {
                new CardEffect(
                    EffectCondition.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            DestroyCard = new EffectActionDestroyCard()
                            {
                                Choice = new Choice()
                                {
                                    How = Choice.ChoiceHow.Choose,
                                    NumPicks = 1,
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new[]{ ZonePrettyName.OpponentField }),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature, CardType.Artifact })
                                    }
                                }
                            }
                        }
                    }
                )
            });

        //TODO アテナ
        public static readonly CardDef atena = CardDef.Creature(6, $"{CardsetName}.アテナ", "アテナ", "", 5, 4,
            effects: new[]
            {
                // 自軍クリーチャーに次の効果を付与する。
                // 「ターン終了時まで、受けるダメージは0になる。」
                new CardEffect(
                    EffectCondition.Spell,
                    new[]{
                        new EffectAction(
                            AddEffect: new(
                                new Choice()
                                {
                                    How = Choice.ChoiceHow.All,
                                    CardCondition =new CardCondition()
                                    {
                                        ZoneCondition = new ZoneCondition(new[]{ ZonePrettyName.YouField }),
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
                                                ModifyDamage: new()
                                                {
                                                    Value = new ValueModifier(ValueModifier.ValueModifierOperator.Replace, 0),
                                                    Choice = new Choice()
                                                    {
                                                        CardCondition = new()
                                                        {
                                                            Context = CardCondition.CardConditionContext.This,
                                                        }
                                                    }
                                                }
                                                )
                                        })
                                })
                            )
                    }
                )
            });

        //TODO 天翼を食う者
        public static readonly CardDef tenyoku = CardDef.Creature(6, $"{CardsetName}.天翼を食う者", "天翼を食う者", "", 6, 6, 1,
            effects: new[]
            {
                new CardEffect(
                    EffectCondition.Spell,
                    new[]{ new EffectAction() }
                    )
            });

        public static readonly CardDef gabriel = CardDef.Creature(7, $"{CardsetName}.ガブリエル", "ガブリエル", "", 3, 4, 1,
            effects: new[]
            {
                new CardEffect(
                    EffectCondition.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            ModifyCard = new EffectActionModifyCard()
                            {
                                Power = 4,
                                Toughness = 3,
                                Choice = new Choice()
                                {
                                    How = Choice.ChoiceHow.Choose,
                                    NumPicks = 1,
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new[]{ ZonePrettyName.YouField }),
                                        Context = CardCondition.CardConditionContext.Others,
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature })
                                    }
                                }
                            }
                        }
                    }
                )
            });

        public static readonly CardDef gilgamesh = CardDef.Creature(7, $"{CardsetName}.ギルガメッシュ", "ギルガメッシュ", "", 5, 4, 0);

        public static readonly CardDef lucifer = CardDef.Creature(8, $"{CardsetName}.ルシフェル", "ルシフェル", "", 6, 7, 1,
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
                            new PlayerModifier(Hp: new ValueModifier(ValueModifier.ValueModifierOperator.Add,4))
                        )
                        )}
                    )
            });

        //TODO サタン
        public static readonly CardDef satan = CardDef.Creature(10, $"{CardsetName}.サタン", "サタン", "", 6, 6, 1,
            effects: new[] {
                new CardEffect(
                    EffectCondition.Spell,
                    new[]{ new EffectAction() }
                    )
            });

        public static readonly CardDef fairy = CardDef.Creature(1, $"{CardsetName}.フェアリー", "フェアリー", "", 1, 1, 1, isToken: true);

        public static readonly CardDef goblin = CardDef.Creature(1, $"{CardsetName}.ゴブリン", "ゴブリン", "", 1, 2, 1);

        public static readonly CardDef mouse = CardDef.Creature(1, $"{CardsetName}.ネズミ", "ネズミ", "", 1, 1, 1,
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
                            Damage = new EffectActionDamage()
                            {
                                Value = 1,
                                Choice = new Choice()
                                {
                                    PlayerCondition = new PlayerCondition()
                                    {
                                        Type = PlayerCondition.PlayerConditionType.Opponent,
                                    },
                                }
                            }
                        }
                    }
                )
            });

        public static readonly CardDef ninja = CardDef.Creature(1, $"{CardsetName}.忍者", "忍者", "テストクリーチャー", 1, 1, 1,
            abilities: new[] { CreatureAbility.Stealth });

        public static readonly CardDef waterFairy = CardDef.Creature(1, $"{CardsetName}.ウォーターフェアリー", "ウォーターフェアリー", "テストクリーチャー", 1, 1, 1,
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
                            AddCard = new EffectActionAddCard()
                            {
                                ZoneToAddCard = ZonePrettyName.YouHand,
                                Choice = new Choice()
                                {
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new[]{ ZonePrettyName.CardPool }),
                                        NameCondition = new(
                                            fairy.FullName,
                                            TextCondition.ConditionCompare.Equality
                                        )
                                    },
                                    How = Choice.ChoiceHow.All,
                                    NumPicks=1,
                                }
                            }
                        }
                    }
                )
            });

        public static readonly CardDef slime = CardDef.Creature(2, $"{CardsetName}.スライム", "スライム", "テストクリーチャー", 1, 1, 1,
            effects: new[]
            {
                // 召喚時、スライムを一体召喚
                new CardEffect(
                    EffectCondition.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            AddCard = new EffectActionAddCard()
                            {
                                ZoneToAddCard = ZonePrettyName.YouField,
                                Choice = new Choice()
                                {
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new ZoneCondition(new[]{ ZonePrettyName.CardPool }),
                                        NameCondition = new(
                                            $"{CardsetName}.スライム",
                                            TextCondition.ConditionCompare.Equality
                                        )
                                    },
                                    How = Choice.ChoiceHow.All,
                                    NumPicks = 1,
                                }
                            }
                        }
                    }
                )
            });

        public static readonly CardDef knight
            = CardDef.Creature(2, $"{CardsetName}.ナイト", "ナイト", "テストクリーチャー", 1, 2, 1, abilities: new[] { CreatureAbility.Cover });
        public static readonly CardDef ninjaKnight
            = CardDef.Creature(3, $"{CardsetName}.忍者ナイト", "忍者ナイト", "テストクリーチャー", 1, 2, 1,
                abilities: new[] { CreatureAbility.Cover, CreatureAbility.Stealth });

        public static readonly CardDef whiteGeneral = CardDef.Creature(4, $"{CardsetName}.ホワイトジェネラル", "ホワイトジェネラル", "テストクリーチャー", 2, 2, 1,
            effects: new[]
            {
                // 召喚時、自分のクリーチャーをランダムで一体を+2/+0
                new CardEffect(
                    EffectCondition.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            ModifyCard = new EffectActionModifyCard()
                            {
                                Choice = new Choice()
                                {
                                    CardCondition = new CardCondition()
                                    {
                                        Context = CardCondition.CardConditionContext.Others,
                                        ZoneCondition = new(new[]{ ZonePrettyName.YouField }),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                    },
                                    NumPicks = 1,
                                    How = Choice.ChoiceHow.Random
                                },
                                Power = 2
                            }
                        }
                    }
                )
            });

        public static readonly CardDef commander = CardDef.Creature(6, $"{CardsetName}.セージコマンダー", "セージコマンダー", "テストクリーチャー", 3, 3, 1,
            effects: new[]
            {
                // 召喚時、自分のクリーチャーすべてを+1/+2
                new CardEffect(
                    EffectCondition.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            ModifyCard = new EffectActionModifyCard()
                            {
                                Choice = new Choice()
                                {
                                    CardCondition = new CardCondition()
                                    {
                                        Context = CardCondition.CardConditionContext.Others,
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                        ZoneCondition = new ZoneCondition(new[]{ ZonePrettyName.YouField })
                                    },
                                    How = Choice.ChoiceHow.All,
                                },
                                Power=1,
                                Toughness=2
                            }
                        }
                    }
                )
            });

        public static readonly CardDef angel = CardDef.Artifact(2, $"{CardsetName}.天使の像", "天使の像", "テストアーティファクト",
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
                            Damage = new EffectActionDamage()
                            {
                                Choice = new Choice()
                                {
                                    PlayerCondition = new PlayerCondition()
                                    {
                                        Type = PlayerCondition.PlayerConditionType.Active,
                                    }
                                },
                                Value = 1
                            }
                        }
                    }
                )
            });

        public static readonly CardDef devil = CardDef.Artifact(1, $"{CardsetName}.悪魔の像", "悪魔の像", "テストアーティファクト",
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
                            Damage = new EffectActionDamage()
                            {
                                Value=1,
                                Choice = new Choice()
                                {
                                    How = Choice.ChoiceHow.Random,
                                    CardCondition= new CardCondition()
                                    {
                                        ZoneCondition = new(new[]{ ZonePrettyName.OpponentField }),
                                        TypeCondition=new CardTypeCondition(new[]{CardType.Creature}),
                                    },
                                    NumPicks=1
                                }
                            }
                        },
                        new EffectAction()
                        {
                            DestroyCard = new EffectActionDestroyCard()
                            {
                                Choice = new Choice()
                                {
                                    How = Choice.ChoiceHow.All,
                                    CardCondition = new CardCondition()
                                    {
                                        Context = CardCondition.CardConditionContext.This,
                                    }
                                }
                            }
                        }
                    }
                )
            });

        public static readonly CardDef fortuneSpring = CardDef.Artifact(2, $"{CardsetName}.運命の泉", "運命の泉", "テストアーティファクト",
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
                            ModifyCard = new EffectActionModifyCard()
                            {
                                Choice = new Choice()
                                {
                                    CardCondition=  new CardCondition()
                                    {
                                        ZoneCondition= new(new[]{ ZonePrettyName.YouField }),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature}),
                                    },
                                    NumPicks=1,
                                    How= Choice.ChoiceHow.Random
                                },
                                Power=1,
                            }
                        }
                    }
                )
            });

        public static readonly CardDef flag = CardDef.Artifact(4, $"{CardsetName}.王家の御旗", "王家の御旗", "",
            effects: new[]
            {
                // 使用時、すべての自分クリーチャーを+1/+0
                new CardEffect(
                    EffectCondition.Spell,
                    new[]
                    {
                        new EffectAction(){
                            ModifyCard = new EffectActionModifyCard()
                            {
                                Power = 1,
                                Toughness = 0,
                                Choice = new Choice()
                                {
                                    How = Choice.ChoiceHow.All,
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new[]{ ZonePrettyName.YouField }),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                    }
                                }
                            }
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
                            ModifyCard = new EffectActionModifyCard()
                            {
                                Power = 1,
                                Toughness = 0,
                                Choice = new Choice()
                                {
                                    How = Choice.ChoiceHow.All,
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition = new(new[]{ ZonePrettyName.YouField }),
                                        TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                        Context = CardCondition.CardConditionContext.EventSource,
                                    }
                                }
                            }
                        }
                    })
            });

        public static readonly CardDef shock = CardDef.Sorcery(1, $"{CardsetName}.ショック", "ショック", "テストソーサリー",
            effects: new[]
            {
                // 使用時、相手かランダムな相手クリーチャー一体に2ダメージ
                new CardEffect(
                    EffectCondition.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            Damage=  new EffectActionDamage()
                            {
                                Choice = new Choice()
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
                                        ZoneCondition = new(new[]{ ZonePrettyName.OpponentField }),
                                    }
                                },
                                Value=2
                            }
                        }
                    }
                )
            });

        public static readonly CardDef buf = CardDef.Sorcery(3, $"{CardsetName}.武装強化", "武装強化", "テストソーサリー",
            effects: new[]
            {
            // 使用時、対象の自分クリーチャーを+2/+2
            new CardEffect(
                EffectCondition.Spell,
                new[]
                {
                    new EffectAction(){
                        ModifyCard=new EffectActionModifyCard()
                        {
                            Power=2,
                            Toughness=2,
                            Choice =new Choice()
                            {
                                How= Choice.ChoiceHow.Choose,
                                NumPicks=1,
                                CardCondition=new CardCondition()
                                {
                                    ZoneCondition = new(new[]{ ZonePrettyName.YouField }),
                                    TypeCondition = new CardTypeCondition(new[]{CardType.Creature, }),
                                }
                            }
                        }
                    }
                }
            )
            });

        public static readonly CardDef shield = CardDef.Artifact(1, $"{CardsetName}.盾", "盾", "盾",
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
                                    ZoneCondition = new(new[]{ ZonePrettyName.YouField }),
                                    Context = CardCondition.CardConditionContext.Others
                                })))),
                    new[]
                    {
                        new EffectAction()
                        {
                            ModifyDamage = new EffectActionModifyDamage()
                            {
                                Choice = new Choice()
                                {
                                    CardCondition = new CardCondition()
                                    {
                                        Context = CardCondition.CardConditionContext.EventSource,
                                    }
                                },
                                Value = new ValueModifier(
                                    ValueModifier.ValueModifierOperator.Sub,
                                    1
                                )
                            }
                        },
                        new EffectAction()
                        {
                            DestroyCard = new EffectActionDestroyCard()
                            {
                                Choice = new Choice()
                                {
                                    CardCondition = new CardCondition()
                                    {
                                        Context = CardCondition.CardConditionContext.This
                                    }
                                }
                            }
                        }
                    }
                )
            });

        public static readonly CardDef wall = CardDef.Artifact(1, $"{CardsetName}.壁", "壁", "壁",
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
                                ZoneCondition = new(new[]{ ZonePrettyName.YouField }),
                                Context = CardCondition.CardConditionContext.Others
                            }))
                    )
                ),
                new[]
                {
                    new EffectAction()
                    {
                        ModifyDamage = new EffectActionModifyDamage()
                        {
                            Choice = new Choice()
                            {
                                PlayerCondition = new PlayerCondition()
                                {
                                    Context = PlayerCondition.PlayerConditionContext.EventSource,
                                },
                                CardCondition = new CardCondition()
                                {
                                    Context = CardCondition.CardConditionContext.EventSource,
                                }
                            },
                            Value = new ValueModifier(
                                ValueModifier.ValueModifierOperator.Sub,
                                1
                            )
                        }
                    },
                    new EffectAction()
                    {
                        DestroyCard = new EffectActionDestroyCard()
                        {
                            Choice = new Choice()
                            {
                                CardCondition = new CardCondition()
                                {
                                    Context = CardCondition.CardConditionContext.This
                                }
                            }
                        }
                    }
                }
            )
            });

        public static readonly CardDef holyKnight = CardDef.Creature(2, $"{CardsetName}.聖騎士", "聖騎士", "聖騎士", 1, 1, 1,
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
                            ModifyDamage = new EffectActionModifyDamage()
                            {
                                Choice = new Choice()
                                {
                                    CardCondition = new CardCondition()
                                    {
                                        Context = CardCondition.CardConditionContext.EventSource,
                                    }
                                },
                                Value = new ValueModifier(
                                    ValueModifier.ValueModifierOperator.Sub,
                                    2
                                )
                            }
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
                                    ZoneCondition = new(new[]{ ZonePrettyName.YouField }),
                                    TypeCondition = new CardTypeCondition(new[]{ CardType.Creature }),
                                    Context = CardCondition.CardConditionContext.Others,
                                }))
                        )
                    ),
                    new[]
                    {
                        new EffectAction()
                        {
                            ModifyDamage = new EffectActionModifyDamage()
                            {
                                Choice = new Choice()
                                {
                                    CardCondition = new CardCondition()
                                    {
                                        Context = CardCondition.CardConditionContext.EventSource,
                                    }
                                },
                                Value = new ValueModifier(
                                    ValueModifier.ValueModifierOperator.Add,
                                    1
                                )
                            }
                        }
                    }
                )
            });

        public static readonly CardDef shippu = CardDef.Sorcery(2, $"{CardsetName}.疾風怒濤", "疾風怒濤", "テストソーサリー",
            effects: new[]
            {
                // 使用時、対象の相手クリーチャー一体にxダメージ。x="自分の場のクリーチャーの数"
                new CardEffect(
                    EffectCondition.Spell,
                    new[]
                    {
                        new EffectAction()
                        {
                            Damage = new EffectActionDamage()
                            {
                                Choice = new Choice()
                                {
                                    How=  Choice.ChoiceHow.Choose,
                                    NumPicks=1,
                                    CardCondition = new CardCondition()
                                    {
                                        ZoneCondition= new(new[]{ ZonePrettyName.OpponentField }),
                                        TypeCondition = new CardTypeCondition(new []{CardType.Creature, }),
                                    }
                                },
                                Value=1
                            }
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
