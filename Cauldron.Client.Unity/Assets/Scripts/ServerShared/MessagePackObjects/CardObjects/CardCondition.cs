#nullable enable

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
            /// 自分と同じ種類のカード
            /// </summary>
            [DisplayText("同じ種類のカード")]
            SameDefs,

            /// <summary>
            /// 自分とは別の種類のカード
            /// </summary>
            [DisplayText("別の種類のカード")]
            OtherDefs,

            /// <summary>
            /// イベントの発生元
            /// </summary>
            [DisplayText("イベントの発生源")]
            EventSource,

            /// <summary>
            /// EffectActionの対象となるカード
            /// </summary>
            [DisplayText("アクションの対象となるカード")]
            ActionTarget,

            /// <summary>
            /// EffectActionの対象となるすべてのカード
            /// </summary>
            [DisplayText("アクションの対象となるすべてのカード")]
            ActionTargetAll,
        }

        /// <summary>
        /// 戦闘イベントに関連する条件
        /// </summary>
        public enum BattleEventContextConditionValue
        {
            /// <summary>
            /// いずれか
            /// </summary>
            [DisplayText("いずれか")]
            Any,

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
        }

        /// <summary>
        /// ダメージイベントに関連する条件
        /// </summary>
        public enum DamageEventContextConditionValue
        {
            /// <summary>
            /// いずれか
            /// </summary>
            [DisplayText("いずれか")]
            Any,

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

        /// <summary>
        /// 所有者に関する条件
        /// </summary>
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

        public ContextConditionValue ContextCondition { get; }
        public BattleEventContextConditionValue BattleEventContextCondition { get; }
        public DamageEventContextConditionValue DamageEventContextCondition { get; }
        public ActionContextCards? ActionContext { get; }
        public NumCompare? CostCondition { get; }
        public NumCompare? PowerCondition { get; }
        public NumCompare? ToughnessCondition { get; }
        public NumCompare? DiffToughnessCondition { get; }
        public CardSetCondition? CardSetCondition { get; }
        public TextCompare? NameCondition { get; }
        public CardAnnotationCondition? AnnotationCondition { get; }
        public CardTypeCondition? TypeCondition { get; }
        public ZoneCondition? ZoneCondition { get; }
        public OwnerConditionValue OwnerCondition { get; }
        public IReadOnlyCollection<CreatureAbility>? AbilityCondition { get; }
        public CounterCondition? CounterCondition { get; }

        public CardCondition(
            ContextConditionValue ContextCondition = ContextConditionValue.Any,
            BattleEventContextConditionValue BattleEventContextCondition = BattleEventContextConditionValue.Any,
            DamageEventContextConditionValue DamageEventContextCondition = DamageEventContextConditionValue.Any,
            ActionContextCards? ActionContext = default,
            NumCompare? CostCondition = default,
            NumCompare? PowerCondition = default,
            NumCompare? ToughnessCondition = default,
            NumCompare? DiffToughnessCondition = default,
            CardSetCondition? CardSetCondition = default,
            TextCompare? NameCondition = default,
            CardAnnotationCondition? AnnotationCondition = default,
            CardTypeCondition? TypeCondition = default,
            ZoneCondition? ZoneCondition = default,
            OwnerConditionValue OwnerCondition = OwnerConditionValue.Any,
            IReadOnlyCollection<CreatureAbility>? AbilityCondition = default,
            CounterCondition? CounterCondition = default
            )
        {
            this.ContextCondition = ContextCondition;
            this.BattleEventContextCondition = BattleEventContextCondition;
            this.DamageEventContextCondition = DamageEventContextCondition;
            this.ActionContext = ActionContext;
            this.CostCondition = CostCondition;
            this.PowerCondition = PowerCondition;
            this.ToughnessCondition = ToughnessCondition;
            this.DiffToughnessCondition = DiffToughnessCondition;
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
