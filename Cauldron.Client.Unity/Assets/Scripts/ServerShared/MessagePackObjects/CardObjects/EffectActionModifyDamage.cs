#nullable enable

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
        public string? Name { get; } = null;

        public EffectActionModifyDamage(NumValueModifier Value,
            string? Name = null)
        {
            this.Value = Value;
            this.Name = Name;
        }
    }
}
