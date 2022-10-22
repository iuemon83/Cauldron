using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class PositionCondition
    {
        public Choice ChoiceBaseCard { get; }
        public int X { get; }
        public int Y { get; }
        public bool Not { get; }

        public PositionCondition(Choice ChoiceBaseCard, int X, int Y, bool Not = false)
        {
            this.ChoiceBaseCard = ChoiceBaseCard;
            this.X = X;
            this.Y = Y;
            this.Not = Not;
        }
    }
}
