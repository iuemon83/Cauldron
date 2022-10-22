#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionChoice
    {
        public Choice Choice { get; }

        public string? Name { get; }

        public EffectActionChoice(
            Choice Choice,
            string? Name = null
            )
        {
            this.Choice = Choice;
            this.Name = Name;
        }
    }
}
