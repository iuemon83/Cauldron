using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class PlayFromHandReply
    {
        public bool Result { get; }
        public string ErrorMessage { get; }
        public GameContext GameContext { get; }

        public PlayFromHandReply(bool Result, string ErrorMessage, GameContext GameContext)
        {
            this.Result = Result;
            this.ErrorMessage = ErrorMessage;
            this.GameContext = GameContext;
        }
    }
}
