using MessagePack;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CardDef
    {
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
        public CardDef(
            CardDefId Id,
             int BaseCost,
             string FullName,
             string Name,
             string FlavorText,
             bool IsToken,
             CardType Type,
             int BasePower,
             int BaseToughness,
             List<CreatureAbility> Abilities,
             IReadOnlyList<CardEffect> Effects,
             int? NumTurnsToCanAttack,
              int? NumAttacksLimitInTurn
            )
        {
            this.Id = Id;
            this.BaseCost = BaseCost;
            this.FullName = FullName;
            this.Name = Name;
            this.FlavorText = FlavorText;
            this.IsToken = IsToken;
            this.Type = Type;
            this.BasePower = BasePower;
            this.BaseToughness = BaseToughness;
            this.Abilities = Abilities;
            this.Effects = Effects;
            this.NumTurnsToCanAttack = NumTurnsToCanAttack;
            this.NumAttacksLimitInTurn = NumAttacksLimitInTurn;
        }
    }
}
