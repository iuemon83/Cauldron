using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class NumValueVariableCalculator
    {
        public string Name { get; }

        public NumValueVariableCalculator(string Name)
        {
            this.Name = Name;
        }
    }
}
