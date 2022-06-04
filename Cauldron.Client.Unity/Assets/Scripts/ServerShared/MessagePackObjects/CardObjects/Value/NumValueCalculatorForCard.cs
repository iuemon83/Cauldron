using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class NumValueCalculatorForCard
    {
        public enum TypeValue
        {
            None,
            [DisplayText("カウント")]
            Count,
            [DisplayText("種類のカウント")]
            DefCount,
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

        public TypeValue Type { get; }

        public Choice CardsChoice { get; }

        public NumValueCalculatorForCard(TypeValue Type, Choice CardsChoice)
        {
            this.Type = Type;
            this.CardsChoice = CardsChoice;
        }
    }
}
