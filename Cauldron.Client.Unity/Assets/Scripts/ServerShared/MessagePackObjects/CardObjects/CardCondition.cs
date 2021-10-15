using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;
using System.Collections.Generic;

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
        public ContextConditionValue ContextCondition { get; }
        public ActionContextCards ActionContext { get; }
        public NumCondition CostCondition { get; }
        public NumCondition PowerCondition { get; }
        public NumCondition ToughnessCondition { get; }
        public CardSetCondition CardSetCondition { get; }
        public TextCondition NameCondition { get; }
        public CardAnnotationCondition AnnotationCondition { get; }
        public CardTypeCondition TypeCondition { get; }
        public ZoneCondition ZoneCondition { get; }
        public OwnerConditionValue OwnerCondition { get; }
        public IReadOnlyCollection<CreatureAbility> AbilityCondition { get; }
        public CounterCondition CounterCondition { get; }

        public CardCondition(
            ContextConditionValue ContextCondition = ContextConditionValue.Any,
            ActionContextCards ActionContext = default,
            NumCondition CostCondition = default,
            NumCondition PowerCondition = default,
            NumCondition ToughnessCondition = default,
            CardSetCondition CardSetCondition = default,
            TextCondition NameCondition = default,
            CardAnnotationCondition AnnotationCondition = default,
            CardTypeCondition TypeCondition = default,
            ZoneCondition ZoneCondition = default,
            OwnerConditionValue OwnerCondition = OwnerConditionValue.Any,
            IReadOnlyCollection<CreatureAbility> AbilityCondition = default,
            CounterCondition CounterCondition = default
            )
        {
            this.ContextCondition = ContextCondition;
            this.ActionContext = ActionContext;
            this.CostCondition = CostCondition;
            this.PowerCondition = PowerCondition;
            this.ToughnessCondition = ToughnessCondition;
            this.CardSetCondition = CardSetCondition;
            this.NameCondition = NameCondition;
            this.AnnotationCondition = AnnotationCondition;
            this.TypeCondition = TypeCondition;
            this.ZoneCondition = ZoneCondition;
            this.OwnerCondition = OwnerCondition;
            this.AbilityCondition = AbilityCondition;
            this.CounterCondition = CounterCondition;
        }
    }
}
