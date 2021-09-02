using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カウンターの変更（カード）
    /// </summary>
    [MessagePackObject(true)]
    public class EffectTimingModifyCounterOnCardEvent
    {
        public enum OperatorValue
        {
            Add,
            Remove
        }

        public CardCondition[] OrCardConditions { get; }

        public string Countername { get; }

        public OperatorValue Operator { get; }

        public EffectTimingModifyCounterOnCardEvent(
            CardCondition[] OrCardConditions,
            string Countername,
            OperatorValue Operator
            )
        {
            this.OrCardConditions = OrCardConditions;
            this.Countername = Countername;
            this.Operator = Operator;
        }
    }
}
