using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CardCondition
    {
        public enum CardConditionContext
        {
            /// <summary>
            /// いずれか
            /// </summary>
            Any,

            /// <summary>
            /// 自分自身
            /// </summary>
            This,

            /// <summary>
            /// 自分以外
            /// </summary>
            Others,

            /// <summary>
            /// イベントの発生元
            /// </summary>
            EventSource,

            /// <summary>
            /// 攻撃クリーチャー
            /// </summary>
            Attack,

            /// <summary>
            /// 防御クリーチャー
            /// </summary>
            Guard,

            /// <summary>
            /// ダメージの発生源
            /// </summary>
            DamageFrom,

            /// <summary>
            /// ダメージを受ける側
            /// </summary>
            DamageTo,
        }

        /// <summary>
        /// 自分自身かそれ以外か
        /// </summary>
        public CardConditionContext Context { get; set; }
        public ActionContextCards ActionContext { get; set; }
        public NumCondition CostCondition { get; set; }
        public NumCondition PowerCondition { get; set; }
        public NumCondition ToughnessCondition { get; set; }
        public TextCondition NameCondition { get; set; }
        public CardTypeCondition TypeCondition { get; set; }
        public ZoneCondition ZoneCondition { get; set; }
    }
}
