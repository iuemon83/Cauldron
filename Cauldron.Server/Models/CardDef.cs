using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Cauldron.Server.Models
{
    public class CardDef
    {
        public static CardDef Creature(int cost, string fullName, string name, string flavorText, int power, int toughness,
            int? numTurnsToCanAttack = null, int? numAttacksInTurn = null, bool isToken = false,
            IEnumerable<CreatureAbility> abilities = null, IEnumerable<CardEffect> effects = null)
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
                NumTurnsToCanAttack = numTurnsToCanAttack,
                NumAttacksLimitInTurn = numAttacksInTurn,
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

        public static CardDef Sorcery(int cost, string fullName, string name, string flavorText, IEnumerable<CardEffect> effects = null)
        {
            return new CardDef()
            {
                BaseCost = cost,
                Type = CardType.Sorcery,
                FullName = fullName,
                Name = name,
                FlavorText = flavorText,
                Effects = effects?.ToArray() ?? Array.Empty<CardEffect>()
            };
        }

        [JsonIgnore]
        public CardDefId Id { get; }

        public int BaseCost { get; set; }

        [JsonIgnore]
        public string FullName { get; set; } = "";

        public string Name { get; set; } = "";

        public string FlavorText { get; set; } = "";

        public bool IsToken { get; set; }

        public CardType Type { get; set; }

        public int BasePower { get; set; } = 0;

        public int BaseToughness { get; set; } = 0;

        public List<CreatureAbility> Abilities { get; set; } = new List<CreatureAbility>();

        public IReadOnlyList<CardEffect> Effects { get; set; } = Array.Empty<CardEffect>();

        /// <summary>
        /// 攻撃可能となるまでのターン数
        /// </summary>
        public int? NumTurnsToCanAttack { get; set; }

        /// <summary>
        /// 1ターン中に攻撃可能な回数
        /// </summary>
        public int? NumAttacksLimitInTurn { get; set; }

        public CardDef()
        {
            this.Id = CardDefId.NewId();
        }
    }
}
