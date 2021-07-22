using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionWin
    {
        public Choice ChoicePlayers { get; }

        public EffectActionWin(Choice ChoicePlayers)
        {
            this.ChoicePlayers = ChoicePlayers;
        }
    }
}
