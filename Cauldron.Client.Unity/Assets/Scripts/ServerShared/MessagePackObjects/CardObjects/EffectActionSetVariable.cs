using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionSetVariable
    {
        public string Name { get; set; }
        public NumValue NumValue { get; set; }

        public EffectActionSetVariable(string Name, NumValue NumValue)
        {
            this.Name = Name;
            this.NumValue = NumValue;
        }
    }
}
