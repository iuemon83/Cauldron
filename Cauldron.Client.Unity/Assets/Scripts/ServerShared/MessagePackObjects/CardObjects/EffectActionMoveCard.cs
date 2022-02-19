#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionMoveCard
    {
        public Choice CardsChoice { get; }
        public ZonePrettyName To { get; }
        public InsertCardPosition? InsertCardPosition { get; }
        public string? Name { get; } = null;

        public EffectActionMoveCard(Choice CardsChoice, ZonePrettyName To,
            InsertCardPosition? InsertCardPosition = null,
            string? Name = null)
        {
            this.CardsChoice = CardsChoice;
            this.To = To;
            this.InsertCardPosition = InsertCardPosition;
            this.Name = Name;
        }
    }
}
