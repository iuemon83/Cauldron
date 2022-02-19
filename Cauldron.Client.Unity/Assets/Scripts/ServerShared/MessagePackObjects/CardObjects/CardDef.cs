using MessagePack;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CardDef
    {
        public static CardDef Empty => new CardDef();

        [JsonIgnore]
        public CardDefId Id { get; }

        public int Cost { get; set; }

        [JsonIgnore]
        public string CardSetName { get; set; } = "";

        public string Name { get; set; } = "";

        [JsonIgnore]
        public string FullName => $"{this.CardSetName}.{this.Name}";

        public string FlavorText { get; set; } = "";

        public IReadOnlyList<string> Annotations { get; set; } = Array.Empty<string>();

        public bool IsToken { get; set; }

        public CardType Type { get; set; }

        public int Power { get; set; } = 0;

        public int Toughness { get; set; } = 0;

        public IReadOnlyList<CreatureAbility> Abilities { get; set; } = Array.Empty<CreatureAbility>();

        public string EffectText { get; set; } = "";

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

        public CardDef(
            CardDefId Id,
            int Cost,
            string CardSetName,
            string Name,
            string FlavorText,
            IReadOnlyList<string> Annotations,
            bool IsToken,
            CardType Type,
            int Power,
            int Toughness,
            IReadOnlyList<CreatureAbility> Abilities,
            string EffectText,
            IReadOnlyList<CardEffect> Effects,
            int? NumTurnsToCanAttack,
            int? NumAttacksLimitInTurn
            )
        {
            this.Id = Id;
            this.Cost = Cost;
            this.CardSetName = CardSetName;
            this.Name = Name;
            this.FlavorText = FlavorText;
            this.Annotations = Annotations;
            this.IsToken = IsToken;
            this.Type = Type;
            this.Power = Power;
            this.Toughness = Toughness;
            this.Abilities = Abilities;
            this.EffectText = EffectText;
            this.Effects = Effects;
            this.NumTurnsToCanAttack = NumTurnsToCanAttack;
            this.NumAttacksLimitInTurn = NumAttacksLimitInTurn;
        }
    }
}
