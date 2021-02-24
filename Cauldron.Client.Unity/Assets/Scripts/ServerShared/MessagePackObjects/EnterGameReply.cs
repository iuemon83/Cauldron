using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EnterGameReply
    {
        public PlayerId PlayerId { get; }

        public EnterGameReply(PlayerId PlayerId)
        {
            this.PlayerId = PlayerId;
        }
    }
}
