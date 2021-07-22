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
            = new(ZonePrettyName.YouField, new(new(Play: new(EffectTimingPlayEvent.SourceValue.This))));


        public static CardDef KarakuriGoblin
            => SampleCards.Creature(1, "からくりゴブリン", "トークン", 1, 1, isToken: true);

        public static CardDef Goblin
            => SampleCards.Creature(1, "ゴブリン", "ただのゴブリン", 1, 2);

        public static CardDef QuickGoblin
            => SampleCards.Creature(1, "素早いゴブリン", "このカードは場に出したターンに攻撃できる。", 1, 1, numTurnsToCanAttack: 0);

        public static CardDef ShieldGoblin
            => SampleCards.Creature(2, "盾持ちゴブリン", "盾になる", 1, 2,
                abilities: new[] { CreatureAbility.Cover });

        public static CardDef MagicShieldGoblin
            => SampleCards.Creature(2, "魔法の盾持ちゴブリン", "このカードが攻撃されたとき、攻撃したカードを相手の手札に移動する。",
                1, 2, abilities: new[] { CreatureAbility.Cover },
                effects: new[] {
                    new CardEffect(
                        new EffectCondition(
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                DamageAfter: new(
                                    EffectTimingDamageBeforeEvent.DamageType.Battle,
                                    EffectTimingDamageBeforeEvent.EventSource.Take,
                                    CardCondition: new(){
                                        ContextCondition = CardCondition.ContextConditionValue.This
                                    })))),
                        new[]
                        {
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ContextCondition = CardCondition.ContextConditionValue.Attack
                                                }
                                            })),
                                    ZonePrettyName.OpponentHand))
                        })
                });

        public static CardDef SuperMagicShieldGoblin
            => SampleCards.Creature(2, "強魔法の盾持ちゴブリン", "このカードが攻撃されたとき、攻撃したカードを相手のデッキの一番上に移動する。",
                1, 2, abilities: new[] { CreatureAbility.Cover },
                effects: new[] {
                    new CardEffect(
                        new EffectCondition(
                            ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                DamageAfter: new(
                                    EffectTimingDamageBeforeEvent.DamageType.Battle,
                                    EffectTimingDamageBeforeEvent.EventSource.Take,
                                    CardCondition: new(){
                                        ContextCondition = CardCondition.ContextConditionValue.This
                                    })))),
                        new[]
                        {
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ContextCondition = CardCondition.ContextConditionValue.Attack
                                                }
                                            })),
                                    ZonePrettyName.OpponentDeck,
                                    new InsertCardPosition(
                                        InsertCardPosition.PositionTypeValue.Top,
                                        1)))
                        })
                });

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
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                ZoneCondition = new(new(new[]{ ZonePrettyName.CardPool })),
                                                CardSetCondition = new(CardSetCondition.ConditionType.This),
                                                NameCondition = new(
                                                    new TextValue(KarakuriGoblin.Name),
                                                    TextCondition.CompareValue.Equality
                                                )
                                            },
                                        })),
                                new ZoneValue(new[]{ ZonePrettyName.YouHand })
                            )
                        }
                    }
                )
            });

        public static CardDef MagicBook
            => SampleCards.Creature(1, "魔法の本",
                "このカードが場に出たとき、ランダムな魔法カード1枚をあなたの手札に追加する。",
                1, 1,
                effects: new[]
                {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                Play: new(EffectTimingPlayEvent.SourceValue.This)))),
                        new[]{
                            new EffectAction(
                                AddCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ZoneCondition = new(new ZoneValue(new[]{
                                                        ZonePrettyName.CardPool
                                                    })),
                                                    TypeCondition = new(new[]{ CardType.Sorcery })
                                                }
                                            }),
                                        how: Choice.ChoiceHow.Random,
                                        1),
                                    new ZoneValue(new[]{ ZonePrettyName.YouHand })))
                        })
                });

        public static CardDef GoblinFollower
            => SampleCards.Creature(1, "ゴブリンフォロワー",
                "あなたが「ゴブリン」と名のつくクリーチャーカードをプレイしたとき、このカードをデッキから場に出す。",
                1, 1,
                effects: new[]
                {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouDeck,
                            new EffectWhen(new EffectTiming(
                                Play: new(
                                    EffectTimingPlayEvent.SourceValue.Other,
                                    new CardCondition() {
                                        NameCondition = new(
                                            new TextValue("ゴブリン"),
                                            TextCondition.CompareValue.Contains),
                                        ZoneCondition = new(new ZoneValue(new[]{ ZonePrettyName.YouField })),
                                        TypeCondition = new(new[]{ CardType.Creature })
                                    })))),
                        new[]{
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ContextCondition = CardCondition.ContextConditionValue.This
                                                }
                                            })),
                                    ZonePrettyName.YouField))
                        })
                });

        public static CardDef GoblinsPet
            => SampleCards.Creature(2, "ゴブリンのペット",
                "このカードが場に出たとき、相手の手札からランダムなクリーチャー1枚を相手の場に出す。",
                2, 6, abilities: new[] { CreatureAbility.Cover },
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]{
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]{
                                                new CardCondition()
                                                {
                                                    ZoneCondition = new(new ZoneValue(new[]
                                                    {
                                                        ZonePrettyName.OpponentHand
                                                    })),
                                                    TypeCondition = new(new[]{ CardType.Creature }),
                                                }
                                            }),
                                        how: Choice.ChoiceHow.Random,
                                        1),
                                    ZonePrettyName.OpponentField))
                        })
                });

        public static CardDef MindController
            => SampleCards.Creature(3, "催眠術師",
                "このカードが場に出たとき、敵のフィールドにクリーチャーが4体以上いるなら、ランダムに1体を選択し、それをあなたの場に移動する。",
                3, 3,
                effects: new[]
                {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                Play: new(EffectTimingPlayEvent.SourceValue.This))),
                            If: new(
                                new NumCondition(4, NumCondition.ConditionCompare.GreaterThan),
                                new NumValue(NumValueCalculator: new(
                                    NumValueCalculator.ValueType.Count,
                                    new Choice(
                                        new ChoiceSource(orCardConditions: new[]{
                                            new CardCondition()
                                            {
                                                ZoneCondition = new(new ZoneValue(new[]
                                                {
                                                    ZonePrettyName.OpponentField
                                                })),
                                                TypeCondition = new(new[]
                                                {
                                                    CardType.Creature
                                                })
                                            }
                                        })))))),
                        new[]{
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]{
                                                new CardCondition()
                                                {
                                                    ZoneCondition = new(new ZoneValue(new[]
                                                    {
                                                        ZonePrettyName.OpponentField
                                                    })),
                                                    TypeCondition = new(new[]{ CardType.Creature }),
                                                }
                                            }),
                                        how: Choice.ChoiceHow.Random,
                                        1),
                                    ZonePrettyName.YouField))
                        })
                });

        public static CardDef NinjaGoblin
            => SampleCards.Creature(3, "分身ゴブリン", "このカードが場に出たとき、「分身ゴブリン」一体を場に出す。", 1, 2,
                effects: new[] {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                Play: new(EffectTimingPlayEvent.SourceValue.This)))),
                        new[]
                        {
                            new EffectAction(
                                AddCard:new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ContextCondition = CardCondition.ContextConditionValue.This
                                                }
                                            })),
                                    new ZoneValue(new[]{ZonePrettyName.YouField })
                                    ))
                        })
                });

        public static CardDef SuperNinjaGoblin
            => SampleCards.Creature(3, "多重分身ゴブリン", "このカードが場に出たとき、「多重分身ゴブリン」2体を場に出す。", 1, 1,
                effects: new[] {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                Play: new(EffectTimingPlayEvent.SourceValue.This)))),
                        new[]
                        {
                            new EffectAction(
                                AddCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ContextCondition = CardCondition.ContextConditionValue.This,
                                                }
                                            })),
                                    new ZoneValue(new[]{ZonePrettyName.YouField }),
                                    NumOfAddCards: 2
                                    ))
                        })
                });

        public static CardDef GoblinsGreed
            => SampleCards.Sorcery(2, "ゴブリンの強欲", "あなたはカードを2枚ドローする。このカードが手札から捨てられたとき、あなたはカードを1枚ドローする。",
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
            => SampleCards.Creature(3, "呪術師ゴブリン", "このカードが場に出たとき、相手の場にあるランダムなクリーチャーカード1枚を破壊する。", 1, 1,
                effects: new[]
                {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                Play: new(EffectTimingPlayEvent.SourceValue.This)))),
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
            => SampleCards.Creature(3, "優しいゴブリン", "このカードが場に出たとき、あなたのHPを2回復する。", 1, 2,
                effects: new[]
                {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                Play: new(EffectTimingPlayEvent.SourceValue.This)))),
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
            => SampleCards.Creature(4, "火炎のゴブリン", "このカードが場に出たとき、相手プレイヤーか、相手の場にいるクリーチャーカード1枚に2ダメージを与える。", 4, 2,
                effects: new[]
                {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                Play: new(EffectTimingPlayEvent.SourceValue.This)))),
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
                                                    ContextCondition = CardCondition.ContextConditionValue.Others,
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
                                new Choice(new ChoiceSource(
                                    orCardConditions: new[]
                                    {
                                        new CardCondition()
                                        {
                                            ZoneCondition = new ZoneCondition(new ZoneValue(
                                                new[]{ ZonePrettyName.CardPool })),
                                            CostCondition = new NumCondition(2,
                                                NumCondition.ConditionCompare.Equality),
                                            TypeCondition = new(new[]{ CardType.Creature }),
                                        }
                                    }),
                                    how: Choice.ChoiceHow.Random,
                                    numPicks: 1),
                                new ZoneValue(new[]{ ZonePrettyName.YouField })
                                ))
                        }
                    )
                });

        public static CardDef MadScientist
            => SampleCards.Creature(4, "マッドサイエンティスト", "このカードが場に出たとき、自分の場か、相手の場にあるクリーチャーカード1枚を選択して、それを破壊する。その後、破壊したカードのコピーをもとの場に出す。", 3, 3,
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]{
                            // 効果を付与したカードを破壊する
                            new EffectAction(DestroyCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                ContextCondition = CardCondition.ContextConditionValue.Others,
                                                ZoneCondition = new(new(new[]{
                                                    ZonePrettyName.YouField, ZonePrettyName.OpponentField })),
                                                TypeCondition = new(new[]{ CardType.Creature })
                                            }
                                        }),
                                    Choice.ChoiceHow.Choose,
                                    1),
                                name: "delete"
                                )),
                            new EffectAction(AddCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                ActionContext = new(ActionContextCardsOfDestroyCard: new(
                                                    "delete",
                                                    ActionContextCardsOfDestroyCard.ValueType.Destroyed))
                                            }
                                        })),
                                new ZoneValue(new[]{ ZonePrettyName.OwnerField })
                                )),
                        }
                    )
                });

        public static CardDef BraveGoblin
            => SampleCards.Creature(4, "ゴブリンの勇者", "自分が受けるダメージを2軽減する。自分の場の他のクリーチャーカードが戦闘で与えるダメージを1増加する。",
                2, 2,
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
                                        ContextCondition = CardCondition.ContextConditionValue.This
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
                                                    ContextCondition = CardCondition.ContextConditionValue.EventSource,
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
                                        ContextCondition = CardCondition.ContextConditionValue.Others,
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
                                                    ContextCondition = CardCondition.ContextConditionValue.EventSource,
                                                }
                                            }))
                                )
                            }
                        }
                    )
                });

        public static CardDef MagicDragon
            => SampleCards.Creature(5, "マジックドラゴン",
                "このカードが場に出たとき、あなたはカードを1枚ドローする。このカードが場にある限り、あなたがプレイした魔法カードによるダメージを+1する。",
                4, 4,
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]{
                            new EffectAction(
                                DrawCard: new(
                                    new NumValue(1),
                                    new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You)))
                        }),
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                DamageBefore: new(
                                    Type: EffectTimingDamageBeforeEvent.DamageType.NonBattle,
                                    CardCondition: new()
                                    {
                                        TypeCondition = new(new[]{ CardType.Sorcery }),
                                        OwnerCondition = CardCondition.OwnerConditionValue.You,
                                    })))),
                        new[]{
                            new EffectAction(
                                ModifyDamage: new(
                                    new NumValueModifier(
                                        NumValueModifier.ValueModifierOperator.Add,
                                        new NumValue(1)),
                                    default))
                        }),
                });

        public static CardDef GiantGoblin
            => SampleCards.Creature(5, "ゴブリンの巨人", "このカードが場に出たとき、自分の場にある他のクリチャーカードに3ダメージを与える。", 3, 7,
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
                                                    ContextCondition = CardCondition.ContextConditionValue.Others,
                                                }
                                            }))
                                )
                            }
                        }
                    )
                });

        public static CardDef LeaderGoblin
            => SampleCards.Creature(5, "ゴブリンリーダー", "このカードが場に出たとき、または自分のターン開始時に自分の場にあるほかのクリーチャーのパワーを1増加する。", 1, 5,
                effects: new[]
                {
                    // プレイ時：自分のクリーチャーすべてを+1/+0 する。
                    // 自分のターン開始時：自分のクリーチャーすべてを+1/+0 する。
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(
                                Play: new (EffectTimingPlayEvent.SourceValue.This),
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
                                                    ContextCondition = CardCondition.ContextConditionValue.Others,
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
            => SampleCards.Creature(6, "暴君ゴブリン", "このカードが場に出たとき、あなたの手札をすべて捨てる。このカードのパワーとタフネスをX増加する。Xは捨てた手札の枚数である。", 6, 6,
                effects: new[]
                {
                    // 手札をすべて捨てて、捨てた枚数パワーアップ
                    new CardEffect(
                        SampleCards.Spell,
                        new[]{
                            new EffectAction(SetVariable: new(
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
                                                ContextCondition = CardCondition.ContextConditionValue.This
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

        public static CardDef DoctorBomb
            => SampleCards.Creature(1, "ドクターボム",
                "このカードが破壊されたとき、ランダムな敵クリーチャー1体か敵プレイヤーに4ダメージを与える。",
                1, 1, isToken: true,
                effects: new[]
                {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouCemetery,
                            new EffectWhen(new EffectTiming(
                                Destroy: new(EffectTimingDestroyEvent.EventSource.This)))),
                        new[]{
                            new EffectAction(
                                Damage: new(
                                    new NumValue(4),
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
                                                    ZoneCondition = new(
                                                        new ZoneValue(new[]{ ZonePrettyName.OpponentField })),
                                                    TypeCondition = new(new[]{ CardType.Creature }),
                                                }
                                            }),
                                        how: Choice.ChoiceHow.Random,
                                        numPicks: 1)))
                        })
                });

        public static CardDef Doctor
            => SampleCards.Creature(7, "ドクター",
                "このカードが場に出たとき、「ドクターボム」2枚をあなたの場に追加する。", 7, 7,
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]{
                            new EffectAction(
                                AddCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ZoneCondition = new(new(new[]{ ZonePrettyName.CardPool })),
                                                    CardSetCondition = new(CardSetCondition.ConditionType.This),
                                                    NameCondition = new(
                                                        new TextValue(DoctorBomb.Name),
                                                        TextCondition.CompareValue.Equality)
                                                }
                                            })),
                                    new ZoneValue(new[]{ ZonePrettyName.YouField }),
                                    NumOfAddCards: 2
                                    ))
                        })
                });

        public static CardDef Firelord
            => SampleCards.Creature(8, "炎の王", "このカードが場にあるとき、あなたのターン終了時に、ランダムな敵クリーチャー1体か敵プレイヤーに8ダメージを与える。",
                8, 8, abilities: new[] { CreatureAbility.CantAttack },
                effects: new[]
                {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                EndTurn: new(EffectTimingEndTurnEvent.EventSource.You)))),
                        new[]
                        {
                            new EffectAction(
                                Damage: new(
                                    new NumValue(8),
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
                                                    ZoneCondition = new(
                                                        new ZoneValue(new[]{ ZonePrettyName.OpponentField })),
                                                    TypeCondition = new(new[]{ CardType.Creature }),
                                                }
                                            }),
                                        how: Choice.ChoiceHow.Random,
                                        numPicks: 1)))
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
                                                ContextCondition = CardCondition.ContextConditionValue.This
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
                                EffectTimingPlayEvent.SourceValue.This))),
                            If: new(new NumCondition(4, NumCondition.ConditionCompare.GreaterThan),
                                    new NumValue(NumValueCalculator: new(
                                        NumValueCalculator.ValueType.Count,
                                        new Choice(
                                            new ChoiceSource(
                                                orCardConditions: new[]
                                                {
                                                    new CardCondition()
                                                    {
                                                        ContextCondition = CardCondition.ContextConditionValue.Others,
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
                                                ContextCondition = CardCondition.ContextConditionValue.Others,
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
                            new EffectWhen(new EffectTiming(Play: new(EffectTimingPlayEvent.SourceValue.This)))
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

        public static CardDef SelectDamage
            => SampleCards.Sorcery(1, "ファイア", "プレイヤーか、場のクリーチャー1体を選択する。それに1ダメージを与える。",
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
                                                    ZoneCondition = new(new ZoneValue(new[]{
                                                        ZonePrettyName.YouField,
                                                        ZonePrettyName.OpponentField
                                                    })),
                                                    TypeCondition = new(new[]{ CardType.Creature })
                                                },
                                            },
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition()
                                                {
                                                    Type = PlayerCondition.PlayerConditionType.Any
                                                }
                                            }),
                                        Choice.ChoiceHow.Choose,
                                        1)),
                            }
                        }
                    )
                });

        public static CardDef RandomDamage
            => SampleCards.Sorcery(1, "稲妻", "ランダムな場のクリーチャー1体に2ダメージを与える。",
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
                                                    ZoneCondition = new(new ZoneValue(new[]{
                                                        ZonePrettyName.YouField,
                                                        ZonePrettyName.OpponentField
                                                    })),
                                                    TypeCondition = new(new[]{ CardType.Creature })
                                                },
                                            }),
                                        Choice.ChoiceHow.Random,
                                        1)),
                            }
                        }
                    )
                });

        public static CardDef Salvage
            => SampleCards.Sorcery(1, "サルベージ", "墓地のカードを1枚選択する。それをあなたの手札に加える。そのカードがクリーチャーならタフネスを元々のタフネスと等しい値にする。",
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                ZoneCondition = new(new ZoneValue(
                                                    new[]{ ZonePrettyName.YouCemetery, ZonePrettyName.OpponentCemetery })),
                                            }
                                        }),
                                        Choice.ChoiceHow.Choose,
                                        1),
                                    ZonePrettyName.YouHand,
                                    Name: "move"
                                    )),
                            new EffectAction(
                                ModifyCard: new(
                                    new Choice(
                                        new ChoiceSource(orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                ActionContext = new(
                                                    ActionContextCardsOfMoveCard: new(
                                                        "move",
                                                        ActionContextCardsOfMoveCard.ValueType.Moved))
                                            }
                                        })),
                                    Toughness: new(
                                        NumValueModifier.ValueModifierOperator.Replace,
                                        new NumValue(NumValueCalculator: new(
                                            NumValueCalculator.ValueType.CardBaseToughness,
                                            new Choice(
                                                new ChoiceSource(orCardConditions: new[]
                                                {
                                                    new CardCondition()
                                                    {
                                                        ActionContext = new(
                                                            ActionContextCardsOfMoveCard: new(
                                                                "move",
                                                                ActionContextCardsOfMoveCard.ValueType.Moved))
                                                    }
                                                })))))))
                        }
                    )
                });

        public static CardDef Recycle
            => SampleCards.Sorcery(1, "リサイクル", "墓地のカードを1枚選択する。それのコピーをあなたの手札に加える。",
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction(
                                AddCard: new(
                                    new Choice(
                                        new ChoiceSource(orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                ZoneCondition = new(new ZoneValue(
                                                    new[]{ ZonePrettyName.YouCemetery, ZonePrettyName.OpponentCemetery })),
                                            }
                                        }),
                                        Choice.ChoiceHow.Choose,
                                        1),
                                    new ZoneValue(new[]{ ZonePrettyName.YouHand })
                                    )),
                        }
                    )
                });

        public static CardDef SimpleReborn
            => SampleCards.Sorcery(1, "簡易蘇生", "墓地のクリーチャーをランダムに1体選択する。それをあなたの場に出す。それのタフネスを1にする。",
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                ZoneCondition = new(new ZoneValue(
                                                    new[]{ZonePrettyName.YouCemetery, ZonePrettyName.OpponentCemetery })),
                                                TypeCondition = new(new[]{ CardType.Creature }),
                                            }
                                        }),
                                        Choice.ChoiceHow.Random,
                                        1),
                                    ZonePrettyName.YouField,
                                    Name: "move"
                                    )),
                            new EffectAction(
                                ModifyCard: new(
                                    new Choice(
                                        new ChoiceSource(orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                ActionContext = new(
                                                    ActionContextCardsOfMoveCard: new(
                                                        "move",
                                                        ActionContextCardsOfMoveCard.ValueType.Moved))
                                            }
                                        })),
                                    Toughness: new(
                                        NumValueModifier.ValueModifierOperator.Replace,
                                        new NumValue(1))))
                        }
                    )
                });

        public static CardDef Sword
            => SampleCards.Sorcery(1, "剣", "あなたの場にあるクリチャー1体を選択する。それは+1/+0 の修整を受ける。",
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
            => SampleCards.Sorcery(1, "盾", "あなたの場にあるクリチャー1体を選択する。それは+0/+1 の修整を受ける。",
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
                                new Choice(new ChoiceSource(
                                    orCardConditions: new[]
                                    {
                                        new CardCondition()
                                        {
                                            ZoneCondition = new(new ZoneValue(new[]{ ZonePrettyName.CardPool })),
                                            CardSetCondition = new(CardSetCondition.ConditionType.This),
                                            NameCondition = new TextCondition(
                                                new TextValue(Hit.Name),
                                                TextCondition.CompareValue.Equality)
                                        },
                                        new CardCondition()
                                        {
                                            ZoneCondition = new(new ZoneValue(new[]{ ZonePrettyName.CardPool })),
                                            CardSetCondition = new(CardSetCondition.ConditionType.This),
                                            NameCondition = new TextCondition(
                                                new TextValue(Heal.Name),
                                                TextCondition.CompareValue.Equality)
                                        },
                                    }),
                                    how: Choice.ChoiceHow.Choose,
                                    numPicks: 1
                                    ),
                                new ZoneValue(new[]{ ZonePrettyName.YouHand })
                                ))
                        })
                });

        public static CardDef Parasite
            => SampleCards.Creature(1, "寄生虫", "このカードがデッキから手札に移動したとき、このカードをあなたの場に出す。このカードがあなたの場にある限り、あなたのターン終了時に、あなたは1ダメージを受ける。",
                1, 1, isToken: true,
                effects: new[]
                {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouHand,
                            new EffectWhen(new EffectTiming(
                                MoveCard: new(EffectTimingMoveCardEvent.EventSource.This,
                                    ZonePrettyName.YouDeck, ZonePrettyName.YouHand)))),
                        new[]
                        {
                            new EffectAction(MoveCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                ContextCondition = CardCondition.ContextConditionValue.This
                                            }
                                        })),
                                ZonePrettyName.YouField)),
                        }),
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                EndTurn: new(EffectTimingEndTurnEvent.EventSource.You)))),
                        new[]
                        {
                            new EffectAction(Damage: new(
                                new NumValue(1),
                                new Choice(
                                    new ChoiceSource(
                                        orPlayerConditions: new[]
                                        {
                                            new PlayerCondition(
                                                Type: PlayerCondition.PlayerConditionType.You)
                                        }))))
                        }),
                });

        public static CardDef Insector
            => SampleCards.Creature(1, "虫つかい", "このカードをプレイしたとき、相手のデッキの1番上に「寄生虫」を1枚追加する。",
                1, 1,
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction(AddCard: new(
                                new Choice(
                                    new ChoiceSource(
                                        orCardConditions: new[]
                                        {
                                            new CardCondition()
                                            {
                                                CardSetCondition = new(CardSetCondition.ConditionType.This),
                                                NameCondition = new(
                                                    new TextValue(Parasite.Name),
                                                    TextCondition.CompareValue.Equality),
                                                ZoneCondition = new(new ZoneValue(new[]{ ZonePrettyName.CardPool }))
                                            }
                                        })),
                                new ZoneValue(new[]{ ZonePrettyName.OpponentDeck }),
                                InsertCardPosition: new(InsertCardPosition.PositionTypeValue.Top, 1))),
                        }),
                });

        public static CardDef EmergencyFood
            => SampleCards.Sorcery(1, "非常食", "あなたはランダムに手札を1枚捨てる。あなたはXのライフを得る。X=捨てたカードのコスト",
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
                                Name: "moveCard"
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

        public static CardDef Gather
            => SampleCards.Sorcery(1, "集合", "あなたの手札に「ゴブリン」を3枚加える。",
                effects: new[] {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction()
                            {
                                AddCard = new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ZoneCondition = new(new(new[]{ ZonePrettyName.CardPool })),
                                                    CardSetCondition = new(CardSetCondition.ConditionType.This),
                                                    NameCondition = new(
                                                        new TextValue(Goblin.Name),
                                                        TextCondition.CompareValue.Equality
                                                    )
                                                },
                                            })),
                                    new ZoneValue(new[]{ ZonePrettyName.YouHand }),
                                    NumOfAddCards: 3)
                            }
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
                                        1),
                                    new ZoneValue(new[]{ ZonePrettyName.YouHand })
                                    )
                            }
                        }
                    )
                });

        public static CardDef FirstAttack
            => SampleCards.Sorcery(2, "ゴブリンの一撃", "相手プレイヤーか、あなたの場か、相手の場にあるクリーチャー1体を選択する。それに1ダメージを与える。あなたの手札に「ゴブリンの二撃」1枚を加える。",
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
                                                        TextCondition.CompareValue.Equality)
                                                }
                                            }),
                                        Choice.ChoiceHow.All,
                                        1),
                                    new ZoneValue(new[]{ ZonePrettyName.YouHand })
                                    )
                            }
                        }
                    )
                });

        public static CardDef SecondAttack
            => SampleCards.Sorcery(2, "ゴブリンの二撃", "相手プレイヤーか、あなたの場か、相手の場にあるクリーチャー1体を選択する。それに2ダメージを与える。", isToken: true,
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
            => SampleCards.Sorcery(2, "聖なる盾", "あなたの場にある他のカードに次の効果を付与する。「ターン終了時まで、受けるダメージは0になる。」",
                effects: new[]
                {
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
                                                    TypeCondition = new(new[]{ CardType.Creature }),
                                                    ContextCondition = CardCondition.ContextConditionValue.Others,
                                                }
                                            })),
                                    new[]
                                    {
                                        new CardEffect(
                                            new EffectCondition(ZonePrettyName.YouField,
                                                new EffectWhen(new EffectTiming(DamageBefore: new(
                                                    Source: EffectTimingDamageBeforeEvent.EventSource.Take,
                                                    CardCondition: new(){
                                                        ContextCondition = CardCondition.ContextConditionValue.This,
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
                                                                        ContextCondition = CardCondition.ContextConditionValue.This,
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

        public static CardDef ChangeHands => SampleCards.Sorcery(2, "手札入れ替え", "あなたは手札をすべて捨てる。あなたは捨てた枚数カードをドローする。",
            effects: new[]
            {
                new CardEffect(
                    SampleCards.Spell,
                    new[]
                    {
                        // 手札の枚数をとっとく
                        new EffectAction(SetVariable: new(
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
            => SampleCards.Sorcery(2, "袋叩き", "相手の場にあるクリーチャー1体を選択する。それにXダメージを与える。X=あなたの場にあるクリーチャーの数",
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

        public static CardDef BounceHand
            => SampleCards.Sorcery(2, "手札へ戻す", "場のカード1枚を選択する。選択したカードを持ち主の手札に移動する。",
                effects: new[] {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ZoneCondition = new(new ZoneValue(
                                                        new[]{ ZonePrettyName.YouField, ZonePrettyName.OpponentField }))
                                                }
                                            }),
                                        Choice.ChoiceHow.Choose,
                                        1),
                                    ZonePrettyName.OwnerHand))
                        })
                });

        public static CardDef BounceDeck
            => SampleCards.Sorcery(2, "デッキへ戻す", "場のカード1枚を選択する。選択したカードを持ち主のデッキのランダムな位置に移動する。",
                effects: new[] {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ZoneCondition = new(new ZoneValue(
                                                        new[]{ ZonePrettyName.YouField, ZonePrettyName.OpponentField }))
                                                }
                                            }),
                                        Choice.ChoiceHow.Choose,
                                        1),
                                    ZonePrettyName.OwnerDeck,
                                    new InsertCardPosition(InsertCardPosition.PositionTypeValue.Random)))
                        })
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
                                    new ZoneValue(new[]{ ZonePrettyName.YouHand }),
                                    NumOfAddCards: 2)
                            }
                        }
                    )
                });

        public static CardDef FullAttack
            => SampleCards.Sorcery(3, "一斉射撃", "相手プレイヤーか、相手の場にあるクリーチャーすべてに、1ダメージを与える。",
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

        public static CardDef Search
            => SampleCards.Sorcery(3, "探索", "あなたのデッキからランダムなカード1枚を、あなたの手札に加える。そのカードのコストを半分にする。",
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction(
                                MoveCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ZoneCondition = new(new ZoneValue(new[]{ ZonePrettyName.YouDeck })),
                                                }
                                            }),
                                        Choice.ChoiceHow.Random,
                                        numPicks: 1),
                                    ZonePrettyName.YouHand,
                                    Name: "search_card")
                            ),
                            new EffectAction(
                                ModifyCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ActionContext = new(ActionContextCardsOfMoveCard: new(
                                                        "search_card",
                                                        ActionContextCardsOfMoveCard.ValueType.Moved
                                                        ))
                                                }
                                            }
                                        )),
                                    Cost: new(
                                        NumValueModifier.ValueModifierOperator.Div,
                                        new NumValue(2)
                                        ))
                            )
                        }
                    )
                });

        public static CardDef GoblinCaptureJar
            => SampleCards.Sorcery(4, "ゴブリン封印の壺", "あなたの場か、相手の場にあるクリーチャーのうち、名前に「ゴブリン」を含むカードすべてのパワーを1にし、「封印」状態にする。",
                effects: new[]
                {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                Play: new(EffectTimingPlayEvent.SourceValue.This)))),
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
                                                        TextCondition.CompareValue.Contains),
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
            => SampleCards.Artifact(1, "ぼろの盾", "あなたの場にあるクリーチャーがダメージを受けるとき、そのダメージを1軽減する。その後、このカードを破壊する。",
                effects: new[]
                {
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
                                        ContextCondition = CardCondition.ContextConditionValue.Others
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
                                                    ContextCondition = CardCondition.ContextConditionValue.EventSource,
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
                                                ContextCondition = CardCondition.ContextConditionValue.This
                                            }
                                        }))
                                )
                            }
                        }
                    )
                });

        public static CardDef OldWall
            => SampleCards.Artifact(1, "ぼろの壁", "あなたか、あなたの場にあるクリーチャーがダメージを受けるとき、そのダメージを1軽減する。その後、このカードを破壊する。",
                effects: new[]
                {
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
                                        ContextCondition = CardCondition.ContextConditionValue.Others
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
                                                ContextCondition = CardCondition.ContextConditionValue.EventSource,
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
                                                ContextCondition = CardCondition.ContextConditionValue.This
                                            }
                                        }))
                                )
                            }
                        }
                    )
                });

        public static CardDef GoblinStatue
            => SampleCards.Artifact(4, "呪いのゴブリン像", "あなたのターン終了時、もしあなたの墓地が30枚以上なら、相手プレイヤー、相手の場にあるクリーチャーすべてに6ダメージを与える。",
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
            => SampleCards.Artifact(4, "癒やしの像", "あなたの場にあるクリーチャーすべては+0/+1 の修整を受ける。あなたが場にクリーチャーを出したとき、それは+0/+1 の修整を受ける。",
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
                                                ContextCondition = CardCondition.ContextConditionValue.EventSource,
                                            }
                                        })),
                                Toughness: new NumValueModifier(
                                    NumValueModifier.ValueModifierOperator.Add,
                                    new NumValue(1))
                            )
                        }
                    })
            });

        public static CardDef WarStatue
            => SampleCards.Artifact(1, "戦いの像",
                "あなたのターン開始時に、場にあるこのカードを墓地に移動する。場にあるこのカードが墓地に移動されたとき、「勝利への道」1枚をあなたの場に追加する。",
                effects: new[]
                {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                StartTurn: new(EffectTimingStartTurnEvent.EventSource.You)))),
                        new[]
                        {
                            new EffectAction(
                                DestroyCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ContextCondition = CardCondition.ContextConditionValue.This
                                                }
                                            }))))
                        }),
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.None,
                            new EffectWhen(new EffectTiming(
                                MoveCard: new(
                                    EffectTimingMoveCardEvent.EventSource.This,
                                    ZonePrettyName.YouField,
                                    ZonePrettyName.YouCemetery)))),
                        new[]
                        {
                            new EffectAction(
                                AddCard: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    NameCondition = new(
                                                        new TextValue(VictoryRoad.Name),
                                                        TextCondition.CompareValue.Equality),
                                                    ZoneCondition = new(
                                                        new ZoneValue(new[]
                                                        {
                                                            ZonePrettyName.CardPool
                                                        }))
                                                }
                                            })),
                                    new ZoneValue(new[]
                                    {
                                        ZonePrettyName.YouField
                                    })))
                        })
                });

        public static CardDef VictoryRoad
            => SampleCards.Artifact(0, "勝利への道",
                "あなたのターン開始時に、このカードがあなたの場にあるとき、あなたはゲームに勝利する。",
                isToken: true,
                effects: new[]
                {
                    new CardEffect(
                        new EffectCondition(ZonePrettyName.YouField,
                            new EffectWhen(new EffectTiming(
                                StartTurn: new(EffectTimingStartTurnEvent.EventSource.You)))),
                        new[]
                        {
                            new EffectAction(
                                Win: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition(
                                                    Type: PlayerCondition.PlayerConditionType.You)
                                            }))))
                        }),
                });
    }
}
