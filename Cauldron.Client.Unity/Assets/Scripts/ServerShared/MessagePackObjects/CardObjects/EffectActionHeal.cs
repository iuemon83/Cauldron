#nullable enable

using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionHeal
    {
        public NumValue Value { get; }
        public Choice Choice { get; }
        public string? Name { get; }

        public EffectActionHeal(
            NumValue Value,
            Choice Choice,
            string? Name = null
            )
        {
            this.Value = Value;
            this.Choice = Choice;
            this.Name = Name;
        }
    }
}
