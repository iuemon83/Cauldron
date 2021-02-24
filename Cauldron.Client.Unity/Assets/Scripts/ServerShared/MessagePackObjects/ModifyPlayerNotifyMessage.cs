using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ModifyPlayerNotifyMessage
    {
        public PlayerId PlayerId { get; set; }
    }
}
