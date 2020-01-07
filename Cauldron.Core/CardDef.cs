using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Core
{
    public class CardDef
    {
        public static CardDef CreatureCard(int cost, string name, string flavorText, int power, int toughness, CreatureAbility ability = CreatureAbility.None, IEnumerable<CardEffect> effects = null)
        {
            return new CardDef()
            {
                BaseCost = cost,
                Type = CardType.Creature,
                Name = name,
                FlavorText = flavorText,
                BasePower = power,
                BaseToughness = toughness,
                Abilitiy = ability,
                Effects = effects?.ToArray() ?? new CardEffect[0]
            };
        }

        public static CardDef ArtifactCard(int cost, string name, string flavorText, IEnumerable<CardEffect> effects = null)
        {
            return new CardDef()
            {
                BaseCost = cost,
                Type = CardType.Artifact,
                Name = name,
                FlavorText = flavorText,
                Effects = effects?.ToArray() ?? new CardEffect[0]
            };
        }

        public static CardDef SorceryCard(int cost, string name, string flavorText, CardRequireToPlay require = null, IEnumerable<CardEffect> effects = null)
        {
            return new CardDef()
            {
                BaseCost = cost,
                Type = CardType.Sorcery,
                Name = name,
                FlavorText = flavorText,
                Require = require ?? new CardRequireToPlay(),
                Effects = effects?.ToArray() ?? new CardEffect[0]
            };
        }
        public static CardDef TokenCard(int cost, string name, string flavorText, int power, int toughness, CreatureAbility ability = CreatureAbility.None, IEnumerable<CardEffect> effects = null)
        {
            return new CardDef()
            {
                BaseCost = cost,
                Type = CardType.Token,
                Name = name,
                FlavorText = flavorText,
                BasePower = power,
                BaseToughness = toughness,
                Abilitiy = ability,
                Effects = effects?.ToArray() ?? new CardEffect[0]
            };
        }

        public Guid Id { get; }

        public int BaseCost { get; set; }

        public string Name { get; set; } = "";

        public string FlavorText { get; set; } = "";

        public CardType Type { get; set; }

        public int BasePower { get; set; } = 0;
        public int BaseToughness { get; set; } = 0;

        public CreatureAbility Abilitiy { get; set; } = CreatureAbility.None;

        public CardRequireToPlay Require { get; set; } = new CardRequireToPlay(_ => true);

        public IReadOnlyList<CardEffect> Effects { get; set; } = new CardEffect[0];

        public CardDef()
        {
            this.Id = Guid.NewGuid();
        }
    }
}
