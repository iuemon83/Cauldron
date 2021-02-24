using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectIf
    {
        public NumCondition NumCondition { get; }
        public NumValue NumValue { get; }

        public EffectIf(NumCondition NumCondition, NumValue NumValue)
        {
            this.NumCondition = NumCondition;
            this.NumValue = NumValue;
        }
    }
}
