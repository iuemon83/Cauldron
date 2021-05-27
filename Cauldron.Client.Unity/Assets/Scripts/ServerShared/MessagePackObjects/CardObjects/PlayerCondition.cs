using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class PlayerCondition
    {
        public enum PlayerConditionContext
        {
            [DisplayText("いずれか")]
            Any,
            [DisplayText("イベントの発生源")]
            EventSource
        }

        public enum PlayerConditionType
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
        }

        public PlayerCondition.PlayerConditionContext Context { get; set; }
        public PlayerCondition.PlayerConditionType Type { get; set; }

        public PlayerCondition(
            PlayerCondition.PlayerConditionContext Context = PlayerConditionContext.Any,
            PlayerCondition.PlayerConditionType Type = PlayerConditionType.Any)
        {
            this.Context = Context;
            this.Type = Type;
        }
    }
}