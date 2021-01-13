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

        public ActionContextCards ActionContext { get; set; }

        public NumCondition CostCondition { get; set; }
        public NumCondition PowerCondition { get; set; }
        public NumCondition ToughnessCondition { get; set; }
        public TextCondition NameCondition { get; set; }
        public CardTypeCondition TypeCondition { get; set; }
        public ZoneCondition ZoneCondition { get; set; }

        public bool IsMatch(Card effectOwnerCard, Card cardToMatch, EffectEventArgs eventArgs)
        {
            return
                (this.Context switch
                {
                    CardConditionContext.This => cardToMatch.Id == effectOwnerCard.Id,
                    CardConditionContext.Others => cardToMatch.Id != effectOwnerCard.Id,
                    CardConditionContext.EventSource => cardToMatch.Id == eventArgs.SourceCard.Id,
                    CardConditionContext.Attack => cardToMatch.Id == eventArgs.BattleContext.AttackCard.Id,
                    CardConditionContext.Guard => cardToMatch.Id == eventArgs.BattleContext.GuardCard.Id,
                    _ => true
                })
                && (this.CostCondition?.IsMatch(cardToMatch.Cost) ?? true)
                && (this.PowerCondition?.IsMatch(cardToMatch.Power) ?? true)
                && (this.ToughnessCondition?.IsMatch(cardToMatch.Toughness) ?? true)
                && (this.NameCondition?.IsMatch(cardToMatch.Name) ?? true)
                && (this.TypeCondition?.IsMatch(cardToMatch.Type) ?? true);
        }

        public bool IsMatch(CardDef cardDefToMatch)
        {
            return
                (this.CostCondition?.IsMatch(cardDefToMatch.BaseCost) ?? true)
                && (this.PowerCondition?.IsMatch(cardDefToMatch.BasePower) ?? true)
                && (this.ToughnessCondition?.IsMatch(cardDefToMatch.BaseToughness) ?? true)
                && (this.NameCondition?.IsMatch(cardDefToMatch.FullName) ?? true)
                && (this.TypeCondition?.IsMatch(cardDefToMatch.Type) ?? true);
        }
    }
}
