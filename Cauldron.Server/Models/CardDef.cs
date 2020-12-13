using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Server.Models
{
    public class CardDef
    {
        public static CardDef Creature(int cost, string fullName, string name, string flavorText, int power, int toughness,
            int turnCountToCanAttack, bool isToken = false, IEnumerable<CreatureAbility> abilities = null, IEnumerable<CardEffect> effects = null)
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
                TurnCountToCanAttack = turnCountToCanAttack,
                IsToken = isToken,
                Abilities = abilities?.ToList() ?? new List<CreatureAbility>(),
                Effects = effects?.ToArray() ?? Array.Empty<CardEffect>()
            };
        }

        public static CardDef Artifact(int cost, string fullName, string name, string flavorText, bool isToken = false, IEnumerable<CardEffect> effects = null)
        {
            return new CardDef()
            {
                BaseCost = cost,
                IsToken = isToken,
                Type = CardType.Artifact,
                FullName = fullName,
                Name = name,
                FlavorText = flavorText,
                Effects = effects?.ToArray() ?? Array.Empty<CardEffect>()
            };
        }

        public static CardDef Sorcery(int cost, string fullName, string name, string flavorText, CardRequireToPlay require = null, IEnumerable<CardEffect> effects = null)
        {
            return new CardDef()
            {
                BaseCost = cost,
                Type = CardType.Sorcery,
                FullName = fullName,
                Name = name,
                FlavorText = flavorText,
                Require = require ?? new CardRequireToPlay(),
                Effects = effects?.ToArray() ?? Array.Empty<CardEffect>()
            };
        }

        public Guid Id { get; }

        public int BaseCost { get; set; }

        public string FullName { get; set; } = "";

        public string Name { get; set; } = "";

        public string FlavorText { get; set; } = "";

        public bool IsToken { get; set; }

        public CardType Type { get; set; }

        public int BasePower { get; set; } = 0;
        public int BaseToughness { get; set; } = 0;

        public List<CreatureAbility> Abilities { get; set; } = new List<CreatureAbility>();

        public CardRequireToPlay Require { get; set; } = new CardRequireToPlay(_ => true);

        public IReadOnlyList<CardEffect> Effects { get; set; } = Array.Empty<CardEffect>();

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
