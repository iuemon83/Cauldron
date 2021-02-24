using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionMoveCard
    {
        public Choice CardsChoice { get; set; }
        public ZonePrettyName To { get; set; }
        public string Name { get; set; } = null;

        public EffectActionMoveCard(Choice CardsChoice, ZonePrettyName To, string Name = null)
        {
            this.CardsChoice = CardsChoice;
            this.To = To;
            this.Name = Name;
        }
    }
}
