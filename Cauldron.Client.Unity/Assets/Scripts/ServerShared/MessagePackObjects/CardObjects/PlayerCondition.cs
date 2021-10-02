using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class PlayerCondition
    {
        public enum ContextValue
        {
            [DisplayText("いずれか")]
            Any,
            [DisplayText("あなた")]
            You,
            [DisplayText("相手")]
            Opponent,
            [DisplayText("アクティブターンプレイヤー")]
            Active,
            [DisplayText("ノンアクティブターンプレイヤー")]
            NonActive,
            [DisplayText("イベントの発生源")]
            EventSource
        }

        public PlayerCondition.ContextValue Context { get; }

        public PlayerCondition(
            PlayerCondition.ContextValue Context = ContextValue.Any)
        {
            this.Context = Context;
        }
    }
}
