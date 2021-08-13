using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CardDefCondition
    {
        public NumCondition CostCondition { get; }
        public NumCondition PowerCondition { get; }
        public NumCondition ToughnessCondition { get; }
        public CardSetCondition CardSetCondition { get; }
        public TextCondition NameCondition { get; }
        public CardTypeCondition TypeCondition { get; }
        public OutZoneCondition OutZoneCondition { get; }

        public CardDefCondition(
            NumCondition CostCondition = default,
            NumCondition PowerCondition = default,
            NumCondition ToughnessCondition = default,
            CardSetCondition CardSetCondition = default,
            TextCondition NameCondition = default,
            CardTypeCondition TypeCondition = default,
            OutZoneCondition OutZoneCondition = default
            )
        {
            this.CostCondition = CostCondition;
            this.PowerCondition = PowerCondition;
            this.ToughnessCondition = ToughnessCondition;
            this.CardSetCondition = CardSetCondition;
            this.NameCondition = NameCondition;
            this.TypeCondition = TypeCondition;
            this.OutZoneCondition = OutZoneCondition;
        }
    }
}
