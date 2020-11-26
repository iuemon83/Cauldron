using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Server.Models
{
    public class CardDef
    {
        public static CardDef CreatureCard(int cost, string fullName, string name, string flavorText, int power, int toughness, IEnumerable<CreatureAbility> abilities = null, IEnumerable<CardEffect2> effects = null)
        {
            return new CardDef()
            {
                BaseCost = cost,
                Type = CardType.Creature,
                FullName = fullName,
                Name = name,
                FlavorText = flavorText,
                BasePower = power,
                BaseToughness = toughness,
                Abilities = abilities?.ToList() ?? new List<CreatureAbility>(),
                Effects = effects?.ToArray() ?? new CardEffect2[0]
            };
        }

        public static CardDef ArtifactCard(int cost, string fullName, string name, string flavorText, IEnumerable<CardEffect2> effects = null)
        {
            return new CardDef()
            {
                BaseCost = cost,
                Type = CardType.Artifact,
                FullName = fullName,
                Name = name,
                FlavorText = flavorText,
                Effects = effects?.ToArray() ?? new CardEffect2[0]
            };
        }

        public static CardDef SorceryCard(int cost, string fullName, string name, string flavorText, CardRequireToPlay require = null, IEnumerable<CardEffect2> effects = null)
        {
            return new CardDef()
            {
                BaseCost = cost,
                Type = CardType.Sorcery,
                FullName = fullName,
                Name = name,
                FlavorText = flavorText,
                Require = require ?? new CardRequireToPlay(),
                Effects = effects?.ToArray() ?? new CardEffect2[0]
            };
        }
        public static CardDef TokenCard(int cost, string fullName, string name, string flavorText, int power, int toughness, IEnumerable<CreatureAbility> abilities = null, IEnumerable<CardEffect2> effects = null)
        {
            return new CardDef()
            {
                BaseCost = cost,
                Type = CardType.Token,
                FullName = fullName,
                Name = name,
                FlavorText = flavorText,
                BasePower = power,
                BaseToughness = toughness,
                Abilities = abilities?.ToList() ?? new List<CreatureAbility>(),
                Effects = effects?.ToArray() ?? new CardEffect2[0]
            };
        }

        public Guid Id { get; }

        public int BaseCost { get; set; }

        public string FullName { get; set; } = "";

        public string Name { get; set; } = "";

        public string FlavorText { get; set; } = "";

        public CardType Type { get; set; }

        public int BasePower { get; set; } = 0;
        public int BaseToughness { get; set; } = 0;

        public List<CreatureAbility> Abilities { get; set; } = new List<CreatureAbility>();

        public CardRequireToPlay Require { get; set; } = new CardRequireToPlay(_ => true);

        public IReadOnlyList<CardEffect2> Effects { get; set; } = new CardEffect2[0];

        /// <summary>
        /// 攻撃可能となるまでのターン数
        /// </summary>
        public int TurnCountToCanAttack { get; set; }

        public CardDef()
        {
            this.Id = Guid.NewGuid();
        }
    }
}
