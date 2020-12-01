using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Server.Models
{
    public class Card
    {
        public Guid Id { get; }

        public Guid OwnerId { get; set; }

        public Guid CardDefId { get; set; }

        public int BaseCost { get; set; } = 0;

        public int CostBuff { get; set; } = 0;

        public string Name { get; set; } = "";

        public string FlavorText { get; set; } = "";

        public bool IsToken { get; set; }

        public CardType Type { get; set; }

        public int BasePower { get; set; } = 0;
        public int BaseToughness { get; set; } = 0;

        public int PowerBuff { get; set; } = 0;
        public int ToughnessBuff { get; set; } = 0;

        public int Cost => this.BaseCost + this.CostBuff;
        public int Power => this.BasePower + this.PowerBuff;
        public int Toughness => this.BaseToughness + this.ToughnessBuff;

        public List<CreatureAbility> Abilities { get; set; } = new List<CreatureAbility>();

        public CardRequireToPlay Require = new CardRequireToPlay();

        //public Dictionary<CardEffectType, CardEffect> EffectsByType { get; set; } = new Dictionary<CardEffectType, CardEffect>();

        public List<CardEffect> Effects { get; set; } = new List<CardEffect>();

        /// <summary>
        /// 攻撃可能となるまでのターン数
        /// </summary>
        public int TurnCountToCanAttack { get; set; }

        /// <summary>
        /// フィールドに出てからのターン数
        /// </summary>
        public int TurnCountInField { get; set; }

        public Card(CardDef cardDef)
        {
            this.Id = Guid.NewGuid();
            this.CardDefId = cardDef.Id;
            this.BaseCost = cardDef.BaseCost;
            this.IsToken = cardDef.IsToken;
            this.Type = cardDef.Type;
            this.Name = cardDef.Name;
            this.FlavorText = cardDef.FlavorText;

            this.BasePower = cardDef.BasePower;
            this.BaseToughness = cardDef.BaseToughness;

            this.Abilities = cardDef.Abilities;
            this.Require = cardDef.Require;
            this.Effects = cardDef.Effects.ToList();
            this.TurnCountToCanAttack = cardDef.TurnCountToCanAttack;
            //this.EffectsByType = cardDef.Effects.ToDictionary(effect => effect.Type);
        }

        public void AddEffect(CardEffect effect)
        {
            //this.EffectsByType.Add(effect.Type, effect);
            this.Effects.Add(effect);
        }

        public void Damage(Card card)
        {
            this.ToughnessBuff -= Math.Max(0, card.Power);
        }

        public void Damage(int damage)
        {
            this.ToughnessBuff -= Math.Max(0, damage);
        }

        public override string ToString()
        {
            return this.Type switch
            {
                CardType.Artifact => $"{this.Name}[{this.Cost}]",
                CardType.Sorcery => $"{this.Name}[{this.Cost}]",
                _ => $"{this.Name}[{this.Cost},{this.Power},{this.Toughness}]",
            };
        }
    }
}
