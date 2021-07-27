using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ModifyPlayerContext
    {
        public PlayerId PlayerId { get; }
        public PlayerModifier PlayerModifier { get; }

        public ModifyPlayerContext(PlayerId playerId, PlayerModifier playerModifier)
        {
            this.PlayerId = playerId;
            this.PlayerModifier = playerModifier;
        }
    }
}
