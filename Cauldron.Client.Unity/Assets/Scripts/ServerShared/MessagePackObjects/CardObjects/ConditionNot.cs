using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ConditionNot
    {
        public ConditionWrap Condition { get; }

        public ConditionNot(ConditionWrap Condition)
        {
            this.Condition = Condition;
        }
    }
}
