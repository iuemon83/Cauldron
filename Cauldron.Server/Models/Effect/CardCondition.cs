namespace Cauldron.Server.Models.Effect
{
    public class CardCondition
    {
        public enum CardConditionContext
        {
            All,
            Me,
            Others,
            EventSource,
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
        public ZoneType ZoneCondition { get; set; } = ZoneType.All;

        public bool IsMatch(Card ownerCard, Card card, Card eventSource)
        {
            return
                (this.Context switch
                {
                    CardConditionContext.Me => card.Id == ownerCard.Id,
                    CardConditionContext.Others => card.Id != ownerCard.Id,
                    CardConditionContext.EventSource => card.Id == eventSource.Id,
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
