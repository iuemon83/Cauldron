using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.Entities.Effect
{
    public record ModifyCounterContext(
        string CounterName,
        EffectTimingModifyCounterOnCardEvent.OperatorValue Operator
        );
}
