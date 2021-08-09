using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// ダメージの修整
    /// </summary>
    [MessagePackObject(true)]
    public class EffectActionModifyDamage
    {
        public NumValueModifier Value { get; }

        public EffectActionModifyDamage(NumValueModifier Value)
        {
            this.Value = Value;
        }
    }
}
