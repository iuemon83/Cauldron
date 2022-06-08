using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ListAllowedClientVersionsReply
    {
        public string[] AllowedClientVersions { get; }

        public ListAllowedClientVersionsReply(string[] AllowedClientVersions)
        {
            this.AllowedClientVersions = AllowedClientVersions;
        }
    }
}
