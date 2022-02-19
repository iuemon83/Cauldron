#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EventContextNum
    {
        public ActionContextCardsOfAddEffect? ActionContextCardsOfAddEffect { get; }

        public EventContextNum(
            ActionContextCardsOfAddEffect? ActionContextCardsOfAddEffect = null
            )
        {
            this.ActionContextCardsOfAddEffect = ActionContextCardsOfAddEffect;
        }
    }
}
