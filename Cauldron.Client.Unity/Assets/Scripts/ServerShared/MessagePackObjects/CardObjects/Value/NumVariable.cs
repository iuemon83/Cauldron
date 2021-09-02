using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class NumVariable
    {
        public string Name { get; }

        public NumVariable(string Name)
        {
            this.Name = Name;
        }
    }
}
