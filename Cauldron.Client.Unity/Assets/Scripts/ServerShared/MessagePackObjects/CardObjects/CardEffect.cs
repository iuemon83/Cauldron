using MessagePack;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CardEffect
    {
        [JsonIgnore]
        public CardEffectId Id { get; }

        public string Description { get; }

        public EffectConditionWrap Condition { get; }

        public IReadOnlyList<EffectAction> Actions { get; }

        [JsonConstructor]
        public CardEffect(
            string Description,
            EffectConditionWrap Condition,
            IReadOnlyList<EffectAction> Actions
            ) : this(default, Description, Condition, Actions)
        {
        }

        public CardEffect(
            CardEffectId Id,
            string Description,
            EffectConditionWrap Condition,
            IReadOnlyList<EffectAction> Actions
            )
        {
            this.Id = Id;
            this.Description = Description;
            this.Condition = Condition;
            this.Actions = Actions ?? Array.Empty<EffectAction>();
        }

        public CardEffect CloneWithNewId()
        {
            return new CardEffect(
                CardEffectId.NewId(),
                this.Description,
                this.Condition,
                this.Actions
                );
        }
    }
}
