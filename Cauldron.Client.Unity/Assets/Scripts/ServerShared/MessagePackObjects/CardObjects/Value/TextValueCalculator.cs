using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class TextValueCalculator
    {
        public enum TypeValue
        {
            [DisplayText("カード名")]
            CardName,
        }

        public TypeValue Type { get; }
        public Choice CardsChoice { get; }

        public TextValueCalculator(
            TypeValue Type,
            Choice CardsChoice
            )
        {
            this.Type = Type;
            this.CardsChoice = CardsChoice;
        }
    }
}
