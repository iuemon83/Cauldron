using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CloseGameReply
    {
        public bool Result { get; }
        public string ErrorMessage { get; }

        public CloseGameReply(bool result, string errorMessage)
        {
            this.Result = result;
            this.ErrorMessage = errorMessage;
        }
    }
}
