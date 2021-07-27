using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class NumValueCalculator
    {
        public enum ValueType
        {
            [DisplayText("カウント")]
            Count,
            [DisplayText("カードのコスト")]
            CardCost,
            [DisplayText("カードの元々のコスト")]
            CardBaseCost,
            [DisplayText("カードのパワー")]
            CardPower,
            [DisplayText("カードの元々のパワー")]
            CardBasePower,
            [DisplayText("カードのタフネス")]
            CardToughness,
            [DisplayText("カードの元々のタフネス")]
            CardBaseToughness,
        }

        public NumValueCalculator.ValueType Type { get; }

        public Choice CardsChoice { get; }

        public NumValueCalculator(
            NumValueCalculator.ValueType Type,
            Choice CardsChoice)
        {
            this.Type = Type;
            this.CardsChoice = CardsChoice;
        }
    }
}
