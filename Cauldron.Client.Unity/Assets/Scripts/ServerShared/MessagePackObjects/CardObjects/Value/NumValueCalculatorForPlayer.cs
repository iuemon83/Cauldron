using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class NumValueCalculatorForPlayer
    {
        public enum TypeValue
        {
            None,
            [DisplayText("カウント")]
            Count,
            [DisplayText("HP")]
            PlayerCurrentHp,
            [DisplayText("MP")]
            PlayerCurrentMp,
        }

        public TypeValue Type { get; }

        public Choice PlayersChoice { get; }

        public NumValueCalculatorForPlayer(TypeValue Type, Choice PlayersChoice)
        {
            this.Type = Type;
            this.PlayersChoice = PlayersChoice;
        }
    }
}
