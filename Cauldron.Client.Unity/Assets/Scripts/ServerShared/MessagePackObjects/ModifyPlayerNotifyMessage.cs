using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ModifyPlayerNotifyMessage
    {
        public PlayerId PlayerId { get; }

        public ModifyPlayerNotifyMessage(PlayerId PlayerId)
        {
            this.PlayerId = PlayerId;
        }
    }
}
