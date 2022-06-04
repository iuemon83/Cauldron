using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Cauldron.Core_Test
{
    public class CreatureAbility_Test
    {
        private readonly ITestOutputHelper output;

        public CreatureAbility_Test(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task Cover()
        {
            var normalcardDef = SampleCards1.Vanilla;
            normalcardDef.Cost = 0;
            normalcardDef.NumTurnsToCanAttackToCreature = 0;

            var testCardDef = SampleCards1.Vanilla;
            testCardDef.Cost = 0;
            testCardDef.Abilities = new[] { CreatureAbility.Cover };

            var c = await TestUtil.InitTest(new[] { normalcardDef, testCardDef }, this.output);

            var (normal, test) = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var normalCard = await TestUtil.NewCardAndPlayFromHand(g, pid, normalcardDef.Id);
                var covercard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                return (normalCard, covercard);
            });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var normal2 = await TestUtil.NewCardAndPlayFromHand(g, pid, normalcardDef.Id);

                // カバーがいるので攻撃できない
                var status = await g.AttackToCreature(pid, normal2.Id, normal.Id);
                Assert.Equal(GameMasterStatusCode.CantAttack, status);

                // カバーには攻撃できる
                status = await g.AttackToCreature(pid, normal2.Id, test.Id);
                Assert.Equal(GameMasterStatusCode.OK, status);
            });
        }

        [Fact]
        public async Task Deadly_攻撃する()
        {
            var normalcardDef = SampleCards1.Creature(0, "t", 1, 10);
            var deadlyCardDef = SampleCards1.Creature(0, "t2", 0, 2,
                abilities: new[] { CreatureAbility.Deadly });

            var c = await TestUtil.InitTest(new[] { normalcardDef, deadlyCardDef }, this.output);

            var normal1 = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var normal1 = await TestUtil.NewCardAndPlayFromHand(g, pid, normalcardDef.Id);

                return normal1;
            });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var deadlyCard = await TestUtil.NewCardAndPlayFromHand(g, pid, deadlyCardDef.Id);

                // 必殺なので倒せる、自分もダメージは受ける
                await TestUtil.AssertGameAction(() => g.AttackToCreature(pid, deadlyCard.Id, normal1.Id));
                Assert.Equal(ZoneName.Cemetery, normal1.Zone.ZoneName);
                Assert.Equal(deadlyCard.BaseToughness - normal1.Power, deadlyCard.Toughness);
            });
        }

        [Fact]
        public async Task Deadly_攻撃して自分が破壊される()
        {
            var normalcardDef = SampleCards1.Creature(0, "t", 1, 10);
            var deadlyCardDef = SampleCards1.Creature(0, "t2", 0, 1,
                abilities: new[] { CreatureAbility.Deadly });

            var c = await TestUtil.InitTest(new[] { normalcardDef, deadlyCardDef }, this.output);

            var normal1 = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var normal1 = await TestUtil.NewCardAndPlayFromHand(g, pid, normalcardDef.Id);
                var normal2 = await TestUtil.NewCardAndPlayFromHand(g, pid, normalcardDef.Id);

                return normal1;
            });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var deadlyCard = await TestUtil.NewCardAndPlayFromHand(g, pid, deadlyCardDef.Id);

                // 自分が破壊されても必殺なので倒せる
                await TestUtil.AssertGameAction(() => g.AttackToCreature(pid, deadlyCard.Id, normal1.Id));
                Assert.Equal(ZoneName.Cemetery, deadlyCard.Zone.ZoneName);
                Assert.Equal(ZoneName.Cemetery, normal1.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task Deadly_攻撃される()
        {
            var normalcardDef = SampleCards1.Creature(0, "t", 0, 10);
            var deadlyCardDef = SampleCards1.Creature(0, "t2", 0, 1,
                abilities: new[] { CreatureAbility.Deadly });

            var c = await TestUtil.InitTest(new[] { normalcardDef, deadlyCardDef }, this.output);

            var deadlyCard = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var deadlyCard = await TestUtil.NewCardAndPlayFromHand(g, pid, deadlyCardDef.Id);

                return deadlyCard;
            });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var normal = await TestUtil.NewCardAndPlayFromHand(g, pid, normalcardDef.Id);

                // 相手が必殺なので倒される、ただし攻撃はできる
                var status = await g.AttackToCreature(pid, normal.Id, deadlyCard.Id);
                Assert.Equal(GameMasterStatusCode.OK, status);
                Assert.Equal(ZoneName.Cemetery, normal.Zone.ZoneName);

                Assert.Equal(deadlyCard.BaseToughness - normal.Power, deadlyCard.Toughness);
            });
        }

        [Fact]
        public async Task Deadly_攻撃されて破壊される()
        {
            var normalcardDef = SampleCards1.Creature(0, "t", 1, 10);
            var deadlyCardDef = SampleCards1.Creature(0, "t2", 0, 1,
                abilities: new[] { CreatureAbility.Deadly });

            var c = await TestUtil.InitTest(new[] { normalcardDef, deadlyCardDef }, this.output);

            var deadlyCard = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var deadlyCard = await TestUtil.NewCardAndPlayFromHand(g, pid, deadlyCardDef.Id);

                return deadlyCard;
            });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var normal = await TestUtil.NewCardAndPlayFromHand(g, pid, normalcardDef.Id);

                // 相手が必殺なので倒される、ただし攻撃はできる
                await TestUtil.AssertGameAction(() => g.AttackToCreature(pid, normal.Id, deadlyCard.Id));
                Assert.Equal(ZoneName.Cemetery, deadlyCard.Zone.ZoneName);
                Assert.Equal(ZoneName.Cemetery, normal.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task Stealth_攻撃対象にならない()
        {
            var stealthCardDef = SampleCards1.Vanilla;
            stealthCardDef.Cost = 0;
            stealthCardDef.Toughness = 10;
            stealthCardDef.Abilities = new[] { CreatureAbility.Stealth };

            var normalcardDef = SampleCards1.Vanilla;
            normalcardDef.Cost = 0;
            normalcardDef.Toughness = 10;
            normalcardDef.NumTurnsToCanAttackToCreature = 0;

            var c = await TestUtil.InitTest(
                new[] { normalcardDef, stealthCardDef, });

            var stealthCard = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var stealthCard = await TestUtil.NewCardAndPlayFromHand(g, pid, stealthCardDef.Id);

                return stealthCard;
            });

            var normal = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var normal = await TestUtil.NewCardAndPlayFromHand(g, pid, normalcardDef.Id);

                // ステルスなので攻撃できない
                var status = await g.AttackToCreature(pid, normal.Id, stealthCard.Id);
                Assert.Equal(GameMasterStatusCode.CantAttack, status);

                return normal;
            });
        }

        [Fact]
        public async Task Stealth_相手カードの効果の選択対象にならない()
        {
            var stealthCardDef = SampleCards1.Vanilla;
            stealthCardDef.Cost = 0;
            stealthCardDef.Toughness = 10;
            stealthCardDef.Abilities = new[] { CreatureAbility.Stealth };

            var choiceCarddef = SampleCards1.SelectDamage;
            choiceCarddef.Cost = 0;

            Card[] expectedAskCardLsit = Array.Empty<Card>();
            var called = false;
            ValueTask<ChoiceAnswer> assertAskAction(PlayerId _, ChoiceCandidates c, int i)
            {
                called = true;

                Assert.Equal(1, i);
                TestUtil.AssertCollection(
                    expectedAskCardLsit.Select(c => c.Id).ToArray(),
                    c.CardList.Select(c => c.Id).ToArray());
                Assert.Empty(c.CardDefList);

                return ValueTask.FromResult(new ChoiceAnswer(
                    c.PlayerIdList.Take(1).ToArray(),
                    Array.Empty<CardId>(),
                    Array.Empty<CardDefId>()));
            }

            var c = await TestUtil.InitTest(
                new[] { stealthCardDef, choiceCarddef },
                TestUtil.GameMasterOptions(
                    EventListener: TestUtil.GameEventListener(AskCardAction: assertAskAction)
                ));

            // 先攻
            var stealthCard = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var stealthCard = await TestUtil.NewCardAndPlayFromHand(g, pid, stealthCardDef.Id);

                return stealthCard;
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                // 相手のカードの効果の対象にならない
                await TestUtil.NewCardAndPlayFromHand(g, pid, choiceCarddef.Id);
            });

            Assert.True(called);
        }

        [Fact]
        public async Task Stealth_自分のカードの効果の選択対象になる()
        {
            var stealthCardDef = SampleCards1.Vanilla;
            stealthCardDef.Cost = 0;
            stealthCardDef.Toughness = 10;
            stealthCardDef.Abilities = new[] { CreatureAbility.Stealth };

            var choiceCarddef = SampleCards1.SelectDamage;
            choiceCarddef.Cost = 0;

            Card[] expectedAskCardLsit = default;
            var called = false;
            ValueTask<ChoiceAnswer> assertAskAction(PlayerId _, ChoiceCandidates c, int i)
            {
                called = true;
                Assert.Equal(1, i);
                TestUtil.AssertCollection(
                    expectedAskCardLsit.Select(c => c.Id).ToArray(),
                    c.CardList.Select(c => c.Id).ToArray());
                Assert.Empty(c.CardDefList);

                return ValueTask.FromResult(new ChoiceAnswer(
                    c.PlayerIdList.Take(1).ToArray(),
                    Array.Empty<CardId>(),
                    Array.Empty<CardDefId>()));
            }

            var c = await TestUtil.InitTest(
                new[] { stealthCardDef, choiceCarddef },
                TestUtil.GameMasterOptions(
                    EventListener: TestUtil.GameEventListener(AskCardAction: assertAskAction)
                ));

            // 先攻
            var stealthCard = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var stealthCard = await TestUtil.NewCardAndPlayFromHand(g, pid, stealthCardDef.Id);

                return stealthCard;
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, (g, pid) =>
            {
            });

            expectedAskCardLsit = new[] { stealthCard };

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                // 自分のカードの効果の対象になる
                await TestUtil.NewCardAndPlayFromHand(g, pid, choiceCarddef.Id);
            });

            Assert.True(called);
        }

        [Fact]
        public async Task Stealth_相手カードのランダム選択の対象になる()
        {
            var stealthCardDef = SampleCards1.Vanilla;
            stealthCardDef.Cost = 0;
            stealthCardDef.Toughness = 10;
            stealthCardDef.Abilities = new[] { CreatureAbility.Stealth };

            var randomChoiceCarddef = SampleCards1.RandomDamage;
            randomChoiceCarddef.Cost = 0;

            var c = await TestUtil.InitTest(
                new[] { stealthCardDef, randomChoiceCarddef });

            // 先攻
            var stealthCard = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pid, stealthCardDef.Id);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                // 相手カードのランダム選択の対象になる
                await TestUtil.NewCardAndPlayFromHand(g, pid, randomChoiceCarddef.Id);
                Assert.Equal(stealthCard.BaseToughness - 2, stealthCard.Toughness);
            });
        }

        [Fact]
        public async Task Stealth_攻撃後はステルスがなくなる()
        {
            var stealthCardDef = SampleCards1.Vanilla;
            stealthCardDef.Cost = 0;
            stealthCardDef.Toughness = 10;
            stealthCardDef.Abilities = new[] { CreatureAbility.Stealth };

            var normalcardDef = SampleCards1.Vanilla;
            normalcardDef.Cost = 0;
            normalcardDef.Toughness = 10;
            normalcardDef.NumTurnsToCanAttackToCreature = 0;

            var c = await TestUtil.InitTest(
                new[] { normalcardDef, stealthCardDef, });

            var (stealthCard1, stealthCard2) = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var stealthCard1 = await TestUtil.NewCardAndPlayFromHand(g, pid, stealthCardDef.Id);
                var stealthCard2 = await TestUtil.NewCardAndPlayFromHand(g, pid, stealthCardDef.Id);

                return (stealthCard1, stealthCard2);
            });

            var normal = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var normal = await TestUtil.NewCardAndPlayFromHand(g, pid, normalcardDef.Id);

                return normal;
            });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                // クリーチャーに攻撃
                {
                    // 攻撃前はステルスがある
                    Assert.Contains(CreatureAbility.Stealth, stealthCard1.Abilities);

                    var status = await g.AttackToCreature(pid, stealthCard1.Id, normal.Id);
                    Assert.Equal(GameMasterStatusCode.OK, status);

                    // 攻撃後はステルスがなくなる
                    Assert.DoesNotContain(CreatureAbility.Stealth, stealthCard1.Abilities);
                }

                // プレイヤーに攻撃
                {
                    // 攻撃前はステルスがある
                    Assert.Contains(CreatureAbility.Stealth, stealthCard2.Abilities);

                    var status = await g.AttackToPlayer(pid, stealthCard2.Id, g.GetOpponent(pid).Id);
                    Assert.Equal(GameMasterStatusCode.OK, status);

                    // 攻撃後はステルスがなくなる
                    Assert.DoesNotContain(CreatureAbility.Stealth, stealthCard2.Abilities);
                }
            });
        }

        [Fact]
        public async Task Stealth_墓地から復活したらステルスも復活する()
        {
            var stealthCardDef = SampleCards1.Creature(0, "a", 1, 1,
                abilities: new[] { CreatureAbility.Stealth }
                );

            var spellDef = SampleCards1.SelectDeathDamage;
            spellDef.Cost = 0;

            var c = await TestUtil.InitTest(
                new[] { stealthCardDef, spellDef },
                output);

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var stealthCard1 = await TestUtil.NewCardAndPlayFromHand(g, pid, stealthCardDef.Id);

                // 攻撃前はステルスがある
                Assert.Contains(CreatureAbility.Stealth, stealthCard1.Abilities);

                var status = await g.AttackToPlayer(pid, stealthCard1.Id, g.GetOpponent(pid).Id);
                Assert.Equal(GameMasterStatusCode.OK, status);

                // 攻撃後はステルスがなくなる
                Assert.DoesNotContain(CreatureAbility.Stealth, stealthCard1.Abilities);

                // 墓地に移動した後はステルスが復活する
                c.TestAnswer.ChoiceCardIdList = new[] { stealthCard1.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pid, spellDef.Id);

                Assert.Equal(ZoneName.Cemetery, stealthCard1.Zone.ZoneName);
                Assert.Contains(CreatureAbility.Stealth, stealthCard1.Abilities);
            });
        }
    }
}
