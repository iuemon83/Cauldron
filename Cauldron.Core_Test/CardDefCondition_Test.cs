using Cauldron.Core.Entities.Effect;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Cauldron.Core_Test
{
    public class CardDefCondition_Test
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

        public CardDefCondition_Test(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task カードタイプ_true()
        {
            var testCondition = new CardDefCondition(
                OutZoneCondition: new(Array.Empty<OutZonePrettyName>()),
                TypeCondition: new(new[] { CardType.Creature })
                );

            var actual = await testCondition.IsMatch(
                new CardDef()
                {
                    Type = CardType.Creature
                },
                TestCard(),
                new EffectEventArgs(default, default)
                );
            Assert.True(actual);
        }

        [Fact]
        public async Task カードタイプ_false()
        {
            var testCondition = new CardDefCondition(
                OutZoneCondition: new(Array.Empty<OutZonePrettyName>()),
                TypeCondition: new(new[] { CardType.Creature })
                );

            var actual = await testCondition.IsMatch(
                new CardDef()
                {
                    Type = CardType.Sorcery
                },
                TestCard(),
                new EffectEventArgs(default, default)
                );
            Assert.False(actual);
        }
    }
}
