using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionModifyPlayer
    {
        public Choice Choice { get; set; }
        public PlayerModifier PlayerModifier { get; set; }

        public EffectActionModifyPlayer(Choice Choice, PlayerModifier PlayerModifier)
        {
            this.Choice = Choice;
            this.PlayerModifier = PlayerModifier;
        }
    }
}
