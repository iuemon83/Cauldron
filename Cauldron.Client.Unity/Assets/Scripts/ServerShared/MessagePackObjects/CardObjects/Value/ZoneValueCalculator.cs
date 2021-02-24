using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class ZoneValueCalculator
    {
        public Choice CardsChoice { get; }

        public ZoneValueCalculator(Choice CardsChoice)
        {
            this.CardsChoice = CardsChoice;
        }
    }
}
