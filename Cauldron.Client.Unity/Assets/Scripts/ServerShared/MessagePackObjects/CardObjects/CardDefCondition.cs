using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CardDefCondition
    {
        public OutZoneCondition OutZoneCondition { get; }

        public NumCompare CostCondition { get; }
        public NumCompare PowerCondition { get; }
        public NumCompare ToughnessCondition { get; }
        public CardSetCondition CardSetCondition { get; }
        public TextCompare NameCondition { get; }
        public CardTypeCondition TypeCondition { get; }

        public CardDefCondition(
            OutZoneCondition OutZoneCondition,
            NumCompare CostCondition = default,
            NumCompare PowerCondition = default,
            NumCompare ToughnessCondition = default,
            CardSetCondition CardSetCondition = default,
            TextCompare NameCondition = default,
            CardTypeCondition TypeCondition = default
            )
        {
            this.OutZoneCondition = OutZoneCondition;
            this.CostCondition = CostCondition;
            this.PowerCondition = PowerCondition;
            this.ToughnessCondition = ToughnessCondition;
            this.CardSetCondition = CardSetCondition;
            this.NameCondition = NameCondition;
            this.TypeCondition = TypeCondition;
        }
    }
}
