using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class OpenNewGameRequest
    {
        public RuleBook RuleBook { get; }

        public OpenNewGameRequest(RuleBook ruleBook)
        {
            this.RuleBook = ruleBook;
        }
    }
}
