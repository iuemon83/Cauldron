#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ActionContextPlayers
    {
        public ActionContextPlayersOfChoice? Choice { get; }

        public ActionContextPlayers(
            ActionContextPlayersOfChoice? Choice = null
            )
        {
            this.Choice = Choice;
        }
    }
}
