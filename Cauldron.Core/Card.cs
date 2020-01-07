using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Core
{
    public class Card
    {
        public Guid Id { get; }

        public Guid OwnerId { get; set; }

        public Guid ClassId { get; set; }

        public int BaseCost { get; set; } = 0;

        public int CostBuff { get; set; } = 0;

        public string Name { get; set; } = "";

        public string FlavorText { get; set; } = "";

        public CardType Type { get; set; }

        public int BasePower { get; set; } = 0;
        public int BaseToughness { get; set; } = 0;

        public int PowerBuff { get; set; } = 0;
        public int ToughnessBuff { get; set; } = 0;

        public int Cost => this.BaseCost + this.CostBuff;
        public int Power => this.BasePower + this.PowerBuff;
        public int Toughness => this.BaseToughness + this.ToughnessBuff;

        public CreatureAbility Abilitiy { get; set; } = CreatureAbility.None;

        public CardRequireToPlay Require = new CardRequireToPlay();

        public Dictionary<CardEffectType, CardEffect> EffectsByType { get; set; } = new Dictionary<CardEffectType, CardEffect>();

        public Card(CardDef cardDef)
        {
            this.Id = Guid.NewGuid();
            this.ClassId = cardDef.Id;
            this.BaseCost = cardDef.BaseCost;
            this.Type = cardDef.Type;
            this.Name = cardDef.Name;
            this.FlavorText = cardDef.FlavorText;

            this.BasePower = cardDef.BasePower;
            this.BaseToughness = cardDef.BaseToughness;

            this.Abilitiy = cardDef.Abilitiy;
            this.Require = cardDef.Require;
            this.EffectsByType = cardDef.Effects.ToDictionary(effect => effect.Type);
        }

        public void AddEffect(CardEffect effect)
        {
            this.EffectsByType.Add(effect.Type, effect);
        }

        public void Damage(Card card)
        {
            this.ToughnessBuff -= Math.Max(0, card.Power);
        }

        public void Damage(int damage)
        {
            this.ToughnessBuff -= Math.Max(0, damage);
        }

        public CardEffect GetEffenct(CardEffectType effectType)
        {
            return this.EffectsByType.TryGetValue(effectType, out var effect)
                ? effect
                : null;
        }
    }
}
