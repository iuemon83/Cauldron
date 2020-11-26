using Cauldron.Server.Models.Effect;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cauldron.Server.Models
{
    public class CardPool
    {
        public CardDef[] Load()
        {
            var jsonFilePath = "cardset.json";
            var cardSet = new CardSet()
            {
                Name = "sample",
                Cards = new CardDefJson[0]
            };
            //var cardSet = this.LoadFromFile(jsonFilePath);

            var fairy = CardDef.TokenCard(1, $"{cardSet.Name}.フェアリー", "フェアリー", "テストクリーチャー", 1, 1);

            var goblin = CardDef.CreatureCard(1, $"{cardSet.Name}.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);

            var mouse = CardDef.CreatureCard(1, $"{cardSet.Name}.ネズミ", "ネズミ", "テストクリーチャー", 1, 1,
                effects: new[]
                {
                    // 死亡時、相手に1ダメージ
                    new CardEffect(){
                        Timing = new EffectTiming()
                        {
                            Destroy = new EffectTimingDestroyEvent(){
                                Owner = EffectTimingDestroyEvent.EventOwner.This
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
                                        Candidates =new[]{ Choice.ChoiceCandidateType.OtherOwnerPlayer },
                                        NumPicks=1
                                    }
                                }
                            }
                        }
                    }
                }
                );

            var ninja = CardDef.CreatureCard(1, $"{cardSet.Name}.忍者", "忍者", "テストクリーチャー", 1, 1, new[] { CreatureAbility.Stealth });

            var waterFairy = CardDef.CreatureCard(1, $"{cardSet.Name}.ウォーターフェアリー", "ウォーターフェアリー", "テストクリーチャー", 1, 1,
                effects: new[]
                {
                    // 破壊時、フェアリー１枚を手札に加える
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            Destroy = new EffectTimingDestroyEvent()
                            {
                                Owner = EffectTimingDestroyEvent.EventOwner.This
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
                                        Candidates =new[]{ Choice.ChoiceCandidateType.Card },
                                        CardCondition = new CardCondition()
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

            var slime = CardDef.CreatureCard(2, $"{cardSet.Name}.スライム", "スライム", "テストクリーチャー", 1, 1,
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
                                        Candidates =new[]{ Choice.ChoiceCandidateType.Card },
                                        CardCondition = new CardCondition()
                                        {
                                            NameCondition = new TextCondition()
                                            {
                                                Value = $"{cardSet.Name}.スライム",
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

            var knight = CardDef.CreatureCard(2, $"{cardSet.Name}.ナイト", "ナイト", "テストクリーチャー", 1, 2, new[] { CreatureAbility.Cover });
            var ninjaKnight = CardDef.CreatureCard(3, $"{cardSet.Name}.忍者ナイト", "忍者ナイト", "テストクリーチャー", 1, 2, new[] { CreatureAbility.Cover, CreatureAbility.Stealth });

            var whiteGeneral = CardDef.CreatureCard(4, $"{cardSet.Name}.ホワイトジェネラル", "ホワイトジェネラル", "テストクリーチャー", 2, 2,
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
                                        Candidates=new[]{ Choice.ChoiceCandidateType.Card },
                                        CardCondition = new CardCondition()
                                        {
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

            var commander = CardDef.CreatureCard(6, $"{cardSet.Name}.セージコマンダー", "セージコマンダー", "テストクリーチャー", 3, 3,
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
                                        Candidates =new[]{ Choice.ChoiceCandidateType.Card },
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

            var angel = CardDef.ArtifactCard(2, $"{cardSet.Name}.天使の像", "天使の像", "テストアーティファクト",
                new[]
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
                                        Candidates =new[]{ Choice.ChoiceCandidateType.TurnPlayer }
                                    },
                                    Value = 1
                                }
                            }
                        }
                    }
                }
                );

            var devil = CardDef.ArtifactCard(1, $"{cardSet.Name}.悪魔の像", "悪魔の像", "テストアーティファクト",
                new[]
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
                                        Candidates = new[]{Choice.ChoiceCandidateType.Card},
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
                                        Candidates = new[]{ Choice.ChoiceCandidateType.Card },
                                        How = Choice.ChoiceHow.All,
                                        CardCondition = new CardCondition()
                                        {
                                            Context = CardCondition.CardConditionContext.Me,
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                );

            var fortuneSpring = CardDef.ArtifactCard(2, $"{cardSet.Name}.運命の泉", "運命の泉", "テストアーティファクト",
                new[]
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
                                        Candidates = new[]{ Choice.ChoiceCandidateType.Card},
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

            var flag = CardDef.ArtifactCard(4, $"{cardSet.Name}.王家の御旗", "王家の御旗", "テストソーサリー",
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
                                        Candidates = new[]{ Choice.ChoiceCandidateType.Card },
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
                                        Candidates = new[]{ Choice.ChoiceCandidateType.Card },
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

            var shock = CardDef.SorceryCard(1, $"{cardSet.Name}.ショック", "ショック", "テストソーサリー",
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
                                        Candidates = new []{Choice.ChoiceCandidateType.Card, Choice.ChoiceCandidateType.OtherOwnerPlayer},
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

            var buf = CardDef.SorceryCard(3, $"{cardSet.Name}.武装強化", "武装強化", "テストソーサリー",
                require: new CardRequireToPlay(environment =>
                {
                    return environment.You.Field.AllCards
                        .Any(c => c.Type == CardType.Creature || c.Type == CardType.Token);
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
                                        Candidates =new []{Choice.ChoiceCandidateType.Card },
                                        CardCondition=new CardCondition()
                                        {
                                            ZoneCondition= ZoneType.YouField,
                                            TypeCondition =new CardTypeCondition()
                                            {
                                                Value= new[]{CardType.Creature, CardType.Token }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                );

            var shippu = CardDef.SorceryCard(2, $"{cardSet.Name}.疾風怒濤", "疾風怒濤", "テストソーサリー",
                require: new CardRequireToPlay(environment =>
                {
                    return environment.Opponent.Field.AllCards
                        .Any(c => c.Type == CardType.Creature || c.Type == CardType.Token);
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
                                        Candidates = new[]{Choice.ChoiceCandidateType.Card },
                                        CardCondition = new CardCondition()
                                        {
                                            ZoneCondition= ZoneType.OpponentField,
                                            TypeCondition = new CardTypeCondition()
                                            {
                                                Value = new []{CardType.Creature, CardType.Token}
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

            var manual = new[] {
                fairy,
                goblin,
                slime,
                mouse,
                waterFairy,
                ninja, knight, ninjaKnight, whiteGeneral,
                angel, devil, fortuneSpring,
                flag,
                shock,
                buf,
                //shippu
            };

            //return cardSet.AsCardDefs().ToArray();

            return cardSet.AsCardDefs()
                .Concat(manual)
                .ToArray();
        }

        private CardSet LoadFromFile(string jsonFilePath)
        {
            var jsonString = File.ReadAllText(jsonFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            var cardSet = JsonSerializer.Deserialize<CardSet>(jsonString, options);
            return cardSet;
        }
    }
}
