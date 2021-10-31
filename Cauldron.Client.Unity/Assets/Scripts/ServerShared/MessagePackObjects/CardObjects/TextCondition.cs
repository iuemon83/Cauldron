using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class TextCondition
    {
        public TextValue Value { get; }
        public TextCompare Compare { get; }

        public TextCondition(
            TextValue Value,
            TextCompare Compare
            )
        {
            this.Value = Value;
            this.Compare = Compare;
        }
    }
}
