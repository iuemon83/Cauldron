using Cauldron.Core.Entities.Effect;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Cauldron.Core_Test
{
    public class CardCondition_Test
    {
        private static Card TestCard(
            CardId Id = default,
            CardDefId CardDefId = default,
            int BaseCost = default,
            bool IsToken = default,
            CardType Type = default,
            string CardSetName = default,
            string Name = default,
            string FlavorText = default,
            IReadOnlyList<string> Annotations = default,
            int BasePower = default,
            int BaseToughness = default,
            IReadOnlyList<CreatureAbility> Abilities = default,
            IReadOnlyList<CardEffect> Effects = default,
            int NumTurnsToCanAttack = default,
            int NumAttacksLimitInTurn = default
            )
        {
            return new Card(Id, CardDefId, BaseCost, IsToken, Type, CardSetName, Name,
                FlavorText, Annotations ?? Array.Empty<string>(), BasePower, BaseToughness,
                Abilities ?? Array.Empty<CreatureAbility>(),
                Effects ?? Array.Empty<CardEffect>(),
                NumTurnsToCanAttack, NumAttacksLimitInTurn);
        }

        private readonly ITestOutputHelper output;

        public CardCondition_Test(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task 条件なし()
        {
            var testCondition = new CardCondition();

            var actual = await testCondition.IsMatch(
                TestCard(),
                new EffectEventArgs(default, default),
                TestCard(Type: CardType.Sorcery)
                );
            Assert.True(actual);
        }

        [Fact]
        public async Task カードタイプ_true()
        {
            var testCondition = new CardCondition(
                TypeCondition: new(new[] { CardType.Creature })
                );

            var actual = await testCondition.IsMatch(
                TestCard(),
                new EffectEventArgs(default, default),
                TestCard(Type: CardType.Creature)
                );
            Assert.True(actual);
        }

        [Fact]
        public async Task カードタイプ_false()
        {
            var testCondition = new CardCondition(
                TypeCondition: new(new[] { CardType.Creature })
                );

            var actual = await testCondition.IsMatch(
                TestCard(),
                new EffectEventArgs(default, default),
                TestCard(Type: CardType.Sorcery)
                );
            Assert.False(actual);
        }
    }
}
