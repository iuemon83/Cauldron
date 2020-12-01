using Cauldron.Server.Models.Effect;
using System.Linq;

namespace Cauldron.Server.Models
{
    public class TestCards
    {
        public static readonly string CardsetName = "sample";

        public static readonly CardDef fairy = CardDef.CreatureCard(1, $"{CardsetName}.フェアリー", "フェアリー", "テストクリーチャー", 1, 1, isToken: true);

        public static readonly CardDef goblin = CardDef.CreatureCard(1, $"{CardsetName}.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);

        public static readonly CardDef mouse = CardDef.CreatureCard(1, $"{CardsetName}.ネズミ", "ネズミ", "テストクリーチャー", 1, 1,
            effects: new[]
            {
                    // 死亡時、相手に1ダメージ
                    new CardEffect(){
                        Timing = new EffectTiming()
                        {
                            Destroy = new EffectTimingDestroyEvent(){
                                Source = EffectTimingDestroyEvent.EventSource.This
                            }
                        },
                        Actions =new []
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
                                            Type = PlayerCondition.PlayerConditionType.NotOwner,
                                        },
                                    }
                                }
                            }
                        }
                    }
            }
            );

        public static readonly CardDef ninja = CardDef.CreatureCard(1, $"{CardsetName}.忍者", "忍者", "テストクリーチャー", 1, 1, abilities: new[] { CreatureAbility.Stealth });

        public static readonly CardDef waterFairy = CardDef.CreatureCard(1, $"{CardsetName}.ウォーターフェアリー", "ウォーターフェアリー", "テストクリーチャー", 1, 1,
            effects: new[]
            {
                    // 破壊時、フェアリー１枚を手札に加える
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            Destroy = new EffectTimingDestroyEvent()
                            {
                                Source = EffectTimingDestroyEvent.EventSource.This
                            }
                        },
                        Actions = new[]
                        {
                            new EffectAction()
                            {
                                AddCard = new EffectActionAddCard()
                                {
                                    ZoneToAddCard = ZoneType.YouHand,
                                    Choice = new Choice()
                                    {
                                        NewCardCondition = new CardCondition()
                                        {
                                            NameCondition = new TextCondition()
                                            {
                                                Value = fairy.FullName,
                                                Compare = TextCondition.ConditionCompare.Equality
                                            }
                                        },
                                        How = Choice.ChoiceHow.All,
                                        NumPicks=1,
                                    }
                                }
                            }
                        }
                    }
            }
            );

        public static readonly CardDef slime = CardDef.CreatureCard(2, $"{CardsetName}.スライム", "スライム", "テストクリーチャー", 1, 1,
            effects: new[]
            {
                    // 召喚時、スライムを一体召喚
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            Play = new EffectTimingPlayEvent()
                            {
                                Source = EffectTimingPlayEvent.EventSource.This
                            }
                        },
                        Actions = new[]
                        {
                            new EffectAction()
                            {
                                AddCard = new EffectActionAddCard()
                                {
                                    ZoneToAddCard = ZoneType.YouField,
                                    Choice = new Choice()
                                    {
                                        NewCardCondition = new CardCondition()
                                        {
                                            NameCondition = new TextCondition()
                                            {
                                                Value = $"{CardsetName}.スライム",
                                                Compare = TextCondition.ConditionCompare.Equality
                                            }
                                        },
                                        How = Choice.ChoiceHow.All,
                                        NumPicks = 1,
                                    }
                                }
                            }
                        }
                    }
            }
            );

        public static readonly CardDef knight = CardDef.CreatureCard(2, $"{CardsetName}.ナイト", "ナイト", "テストクリーチャー", 1, 2, abilities: new[] { CreatureAbility.Cover });
        public static readonly CardDef ninjaKnight = CardDef.CreatureCard(3, $"{CardsetName}.忍者ナイト", "忍者ナイト", "テストクリーチャー", 1, 2, abilities: new[] { CreatureAbility.Cover, CreatureAbility.Stealth });

        public static readonly CardDef whiteGeneral = CardDef.CreatureCard(4, $"{CardsetName}.ホワイトジェネラル", "ホワイトジェネラル", "テストクリーチャー", 2, 2,
            effects: new[]
            {
                    // 召喚時、自分のクリーチャーをランダムで一体を+2/+0
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            Play = new EffectTimingPlayEvent()
                            {
                                Source = EffectTimingPlayEvent.EventSource.This
                            }
                        },
                        Actions = new[]
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
                                            ZoneCondition = ZoneType.YouField,
                                            TypeCondition = new CardTypeCondition()
                                            {
                                                Value = new[]{ CardType.Creature },
                                            },
                                        },
                                        NumPicks = 1,
                                        How = Choice.ChoiceHow.Random
                                    },
                                    Power = 2
                                }
                            }
                        }
                    }
            }
            );

        public static readonly CardDef commander = CardDef.CreatureCard(6, $"{CardsetName}.セージコマンダー", "セージコマンダー", "テストクリーチャー", 3, 3,
            effects: new[]
            {
                    // 召喚時、自分のクリーチャーすべてを+1/+1
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            Play = new EffectTimingPlayEvent()
                            {
                                Source = EffectTimingPlayEvent.EventSource.This
                            }
                        },
                        Actions = new[]
                        {
                            new EffectAction()
                            {
                                ModifyCard = new EffectActionModifyCard()
                                {
                                    Choice = new Choice()
                                    {
                                        CardCondition = new CardCondition()
                                        {
                                            TypeCondition = new CardTypeCondition()
                                            {
                                                Value =new[]{ CardType.Creature },
                                            }
                                        },
                                        How = Choice.ChoiceHow.All,
                                    },
                                    Power=1,
                                    Toughness=1
                                }
                            }
                        }
                    }
            }
            );

        public static readonly CardDef angel = CardDef.ArtifactCard(2, $"{CardsetName}.天使の像", "天使の像", "テストアーティファクト",
            effects: new[]
            {
                    // ターン開始時、カレントプレイヤーに1ダメージ
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            StartTurn = new EffectTimingStartTurnEvent()
                            {
                                Source = EffectTimingStartTurnEvent.EventSource.Both,
                            }
                        },
                        Actions = new []{
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
                    }
            }
            );

        public static readonly CardDef devil = CardDef.ArtifactCard(1, $"{CardsetName}.悪魔の像", "悪魔の像", "テストアーティファクト",
            effects: new[]
            {
                    // ターン終了時、ランダムな相手クリーチャー一体に1ダメージ。その後このカードを破壊
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            EndTurn = new EffectTimingEndTurnEvent()
                            {
                                Source = EffectTimingEndTurnEvent.EventSource.Both,
                            }
                        },
                        Actions = new[]
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
                                            ZoneCondition = ZoneType.OpponentField,
                                            TypeCondition=new CardTypeCondition()
                                            {
                                                Value = new[]{CardType.Creature}
                                            }
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
                    }
            }
            );

        public static readonly CardDef fortuneSpring = CardDef.ArtifactCard(2, $"{CardsetName}.運命の泉", "運命の泉", "テストアーティファクト",
            effects: new[]
            {
                    // ターン終了時、ランダムな自分のクリーチャー一体を+1/+0
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            EndTurn = new EffectTimingEndTurnEvent()
                            {
                                Source = EffectTimingEndTurnEvent.EventSource.Owner
                            }
                        },
                        Actions = new[]
                        {
                            new EffectAction()
                            {
                                ModifyCard = new EffectActionModifyCard()
                                {
                                    Choice = new Choice()
                                    {
                                        CardCondition=  new CardCondition()
                                        {
                                            ZoneCondition= ZoneType.YouField,
                                            TypeCondition = new CardTypeCondition()
                                            {
                                                Value = new[]{ CardType.Creature}
                                            }
                                        },
                                        NumPicks=1,
                                        How= Choice.ChoiceHow.Random
                                    },
                                    Power=1,
                                }
                            }
                        }
                    }
            }
            );

        public static readonly CardDef flag = CardDef.ArtifactCard(4, $"{CardsetName}.王家の御旗", "王家の御旗", "テストソーサリー",
            effects: new[]
            {
                    // 使用時、すべての自分クリーチャーを+1/+0
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            Play = new EffectTimingPlayEvent()
                            {
                                Source = EffectTimingPlayEvent.EventSource.This,
                            }
                        },
                        Actions = new[]
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
                                            ZoneCondition = ZoneType.YouField,
                                            TypeCondition = new CardTypeCondition()
                                            {
                                                Value = new[]{ CardType.Creature }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // 自分クリーチャーのプレイ時+1/+0
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            Play = new EffectTimingPlayEvent()
                            {
                                Source = EffectTimingPlayEvent.EventSource.Other,
                            }
                        },
                        Actions = new[]
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
                                            ZoneCondition = ZoneType.YouField,
                                            TypeCondition = new CardTypeCondition()
                                            {
                                                Value = new[]{ CardType.Creature }
                                            },
                                            Context = CardCondition.CardConditionContext.EventSource,
                                        }
                                    }
                                }
                            }
                        }
                    }
            });

        public static readonly CardDef shock = CardDef.SorceryCard(1, $"{CardsetName}.ショック", "ショック", "テストソーサリー",
            effects: new[]
            {
                    // 使用時、相手かランダムな相手クリーチャー一体に2ダメージ
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            Play = new EffectTimingPlayEvent()
                            {
                                Source = EffectTimingPlayEvent.EventSource.This
                            }
                        },
                        Actions = new[]
                        {
                            new EffectAction()
                            {
                                Damage=  new EffectActionDamage()
                                {
                                    Choice = new Choice()
                                    {
                                        PlayerCondition = new PlayerCondition()
                                        {
                                            Type = PlayerCondition.PlayerConditionType.NotOwner
                                        },
                                        NumPicks= 1,
                                        How= Choice.ChoiceHow.Random,
                                        CardCondition = new CardCondition()
                                        {
                                            TypeCondition = new CardTypeCondition()
                                            {
                                                Value = new []{CardType.Creature}
                                            },
                                            ZoneCondition = ZoneType.OpponentField,
                                        }
                                    },
                                    Value=2
                                }
                            }
                        }
                    },
            });

        public static readonly CardDef buf = CardDef.SorceryCard(3, $"{CardsetName}.武装強化", "武装強化", "テストソーサリー",
            require: new CardRequireToPlay(environment =>
            {
                return environment.You.Field.AllCards
                    .Any(c => c.Type == CardType.Creature);
            }),
            effects: new[]
            {
                    // 使用時、対象の自分クリーチャーを+2/+2
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            Play = new EffectTimingPlayEvent()
                            {
                                Source= EffectTimingPlayEvent.EventSource.This
                            }
                        },
                        Actions= new[]
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
                                            ZoneCondition= ZoneType.YouField,
                                            TypeCondition =new CardTypeCondition()
                                            {
                                                Value= new[]{CardType.Creature, }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
            }
            );

        public static readonly CardDef shield = CardDef.ArtifactCard(1, $"{CardsetName}.盾", "盾", "盾",
            effects: new[]
            {
                    // 自分のクリーチャーが受けるダメージを1軽減する.その後このカードを破壊する
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            DamageBefore = new EffectTimingDamageBeforeEvent()
                            {
                                Source = EffectTimingDamageBeforeEvent.EventSource.All,
                                CardCondition = new CardCondition()
                                {
                                    TypeCondition = new CardTypeCondition()
                                    {
                                        Value = new[]{ CardType.Creature },
                                    },
                                    ZoneCondition = ZoneType.YouField,
                                    Context = CardCondition.CardConditionContext.Others
                                }
                            }
                        },
                        Actions= new[]
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
                                    Value = new ValueModifier()
                                    {
                                        Operator = ValueModifier.ValueModifierOperator.Sub,
                                        Value = 1
                                    }
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
                    }
            }
            );

        public static readonly CardDef wall = CardDef.ArtifactCard(1, $"{CardsetName}.壁", "壁", "壁",
            effects: new[]
            {
                    // 自分のプレイヤーまたはクリーチャーが受けるダメージを1軽減する.その後このカードを破壊する
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            DamageBefore = new EffectTimingDamageBeforeEvent()
                            {
                                Source = EffectTimingDamageBeforeEvent.EventSource.Guard,
                                PlayerCondition = new PlayerCondition()
                                {
                                    Type = PlayerCondition.PlayerConditionType.Owner,
                                },
                                CardCondition = new CardCondition()
                                {
                                    TypeCondition = new CardTypeCondition()
                                    {
                                        Value = new[]{ CardType.Creature },
                                    },
                                    ZoneCondition = ZoneType.YouField,
                                    Context = CardCondition.CardConditionContext.Others
                                }
                            }
                        },
                        Actions= new[]
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
                                    Value = new ValueModifier()
                                    {
                                        Operator = ValueModifier.ValueModifierOperator.Sub,
                                        Value = 1
                                    }
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
                    }
            }
            );

        public static readonly CardDef holyKnight = CardDef.CreatureCard(2, $"{CardsetName}.聖騎士", "聖騎士", "聖騎士", 1, 1,
            effects: new[]
            {
                    // 自分が受けるダメージを2軽減する
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            DamageBefore = new EffectTimingDamageBeforeEvent()
                            {
                                Source = EffectTimingDamageBeforeEvent.EventSource.Guard,
                                CardCondition = new CardCondition()
                                {
                                    Context = CardCondition.CardConditionContext.This
                                }
                            }
                        },
                        Actions= new[]
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
                                    Value = new ValueModifier()
                                    {
                                        Operator = ValueModifier.ValueModifierOperator.Sub,
                                        Value = 2
                                    }
                                }
                            }
                        }
                    },
                    // 自分の他のクリーチャーが戦闘で与えるダメージを1増加する
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            BattleBefore = new EffectTimingBattleBeforeEvent()
                            {
                                Source = EffectTimingBattleBeforeEvent.EventSource.Attack,
                                CardCondition = new CardCondition()
                                {
                                    ZoneCondition = ZoneType.YouField,
                                    TypeCondition = new CardTypeCondition()
                                    {
                                        Value = new[]{ CardType.Creature },
                                    },
                                    Context = CardCondition.CardConditionContext.Others,
                                }
                            },
                        },
                        Actions= new[]
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
                                    Value = new ValueModifier()
                                    {
                                        Operator = ValueModifier.ValueModifierOperator.Add,
                                        Value = 1
                                    }
                                }
                            }
                        }
                    }
            }
            );

        public static readonly CardDef shippu = CardDef.SorceryCard(2, $"{CardsetName}.疾風怒濤", "疾風怒濤", "テストソーサリー",
            require: new CardRequireToPlay(environment =>
            {
                return environment.Opponent.Field.AllCards
                    .Any(c => c.Type == CardType.Creature);
            }),
            effects: new[]
            {
                    // 使用時、対象の相手クリーチャー一体にxダメージ。x="自分の場のクリーチャーの数"
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            Play = new EffectTimingPlayEvent()
                            {
                                Source = EffectTimingPlayEvent.EventSource.This
                            }
                        },
                        Actions = new[]
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
                                            ZoneCondition= ZoneType.OpponentField,
                                            TypeCondition = new CardTypeCondition()
                                            {
                                                Value = new []{CardType.Creature, }
                                            }
                                        }
                                    },
                                    Value=1
                                }
                            }
                        }
                    }
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
