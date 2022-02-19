#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionModifyPlayer
    {
        public Choice Choice { get; }
        public PlayerModifier PlayerModifier { get; }
        public string? Name { get; } = null;

        public EffectActionModifyPlayer(Choice Choice, PlayerModifier PlayerModifier,
            string? Name = null)
        {
            this.Choice = Choice;
            this.PlayerModifier = PlayerModifier;
            this.Name = Name;
        }
    }
}
