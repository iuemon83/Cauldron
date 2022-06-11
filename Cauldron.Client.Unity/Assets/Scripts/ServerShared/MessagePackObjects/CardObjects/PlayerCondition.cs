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
            EventSource,

            /// <summary>
            /// EffectActionの対象となるカード
            /// </summary>
            [DisplayText("アクションの対象となるプレイヤー")]
            ActionTarget,

            /// <summary>
            /// EffectActionの対象となるすべてのカード
            /// </summary>
            [DisplayText("アクションの対象となるすべてのプレイヤー")]
            ActionTargetAll,
        }

        public PlayerCondition.ContextValue Context { get; }

        /// <summary>
        /// 先攻ならTrue
        /// </summary>
        public bool? IsFirst { get; }

        public PlayerCondition(
            PlayerCondition.ContextValue Context = ContextValue.Any,
            bool? IsFirst = null
            )
        {
            this.Context = Context;
            this.IsFirst = IsFirst;
        }
    }
}
