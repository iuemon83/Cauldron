namespace Cauldron.Server.Models.Effect
{
    public class CardCondition
    {
        public enum CardConditionContext
        {
            All,

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
        public CardConditionContext Context { get; set; } = CardConditionContext.All;

        public NumCondition CostCondition { get; set; }
        public NumCondition PowerCondition { get; set; }
        public NumCondition ToughnessCondition { get; set; }
        public TextCondition NameCondition { get; set; }
        public CardTypeCondition TypeCondition { get; set; }
        public ZoneCondition ZoneCondition { get; set; }

        public bool IsMatch(Card effectOwnerCard, Card card, EffectEventArgs eventArgs)
        {
            return
                (this.Context switch
                {
                    CardConditionContext.This => card.Id == effectOwnerCard.Id,
                    CardConditionContext.Others => card.Id != effectOwnerCard.Id,
                    CardConditionContext.EventSource => card.Id == eventArgs.SourceCard.Id,
                    CardConditionContext.Attack => card.Id == eventArgs.BattleContext.AttackCard.Id,
                    CardConditionContext.Guard => card.Id == eventArgs.BattleContext.GuardCard.Id,
                    _ => true
                })
                && (this.CostCondition?.IsMatch(card.Cost) ?? true)
                && (this.PowerCondition?.IsMatch(card.Power) ?? true)
                && (this.ToughnessCondition?.IsMatch(card.Toughness) ?? true)
                && (this.NameCondition?.IsMatch(card.Name) ?? true)
                && (this.TypeCondition?.IsMatch(card.Type) ?? true);
        }

        public bool IsMatch(CardDef cardDef)
        {
            return
                (this.CostCondition?.IsMatch(cardDef.BaseCost) ?? true)
                && (this.PowerCondition?.IsMatch(cardDef.BasePower) ?? true)
                && (this.ToughnessCondition?.IsMatch(cardDef.BaseToughness) ?? true)
                && (this.NameCondition?.IsMatch(cardDef.FullName) ?? true)
                && (this.TypeCondition?.IsMatch(cardDef.Type) ?? true);
        }
    }
}
