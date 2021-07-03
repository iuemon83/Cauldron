using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CardCondition
    {
        public enum ContextConditionValue
        {
            /// <summary>
            /// いずれか
            /// </summary>
            [DisplayText("いずれか")]
            Any,

            /// <summary>
            /// 自分自身
            /// </summary>
            [DisplayText("このカード")]
            This,

            /// <summary>
            /// 自分以外
            /// </summary>
            [DisplayText("他のカード")]
            Others,

            /// <summary>
            /// イベントの発生元
            /// </summary>
            [DisplayText("イベントの発生源")]
            EventSource,

            /// <summary>
            /// 攻撃クリーチャー
            /// </summary>
            [DisplayText("攻撃クリーチャー")]
            Attack,

            /// <summary>
            /// 防御クリーチャー
            /// </summary>
            [DisplayText("防御クリーチャー")]
            Guard,

            /// <summary>
            /// ダメージの発生源
            /// </summary>
            [DisplayText("ダメージの発生源")]
            DamageFrom,

            /// <summary>
            /// ダメージを受ける側
            /// </summary>
            [DisplayText("ダメージを受ける側")]
            DamageTo,
        }

        public enum OwnerConditionValue
        {
            /// <summary>
            /// いずれか
            /// </summary>
            [DisplayText("いずれか")]
            Any,

            /// <summary>
            /// あなた
            /// </summary>
            [DisplayText("あなた")]
            You,

            /// <summary>
            /// 相手
            /// </summary>
            [DisplayText("相手")]
            Opponent
        }

        /// <summary>
        /// 自分自身かそれ以外か
        /// </summary>
        public ContextConditionValue ContextCondition { get; set; }
        public ActionContextCards ActionContext { get; set; }
        public NumCondition CostCondition { get; set; }
        public NumCondition PowerCondition { get; set; }
        public NumCondition ToughnessCondition { get; set; }
        public CardSetCondition CardSetCondition { get; set; }
        public TextCondition NameCondition { get; set; }
        public CardTypeCondition TypeCondition { get; set; }
        public ZoneCondition ZoneCondition { get; set; }
        public OwnerConditionValue OwnerCondition { get; set; }
    }
}
