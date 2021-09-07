using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionWin
    {
        public Choice ChoicePlayers { get; }
        public string Name { get; } = null;

        public EffectActionWin(Choice ChoicePlayers,
            string Name = null)
        {
            this.ChoicePlayers = ChoicePlayers;
            this.Name = Name;
        }
    }
}
