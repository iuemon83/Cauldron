#nullable enable

using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class PlayerModifier
    {
        public NumValueModifier? MaxHp { get; }
        public NumValueModifier? Hp { get; }
        public NumValueModifier? MaxMp { get; }
        public NumValueModifier? Mp { get; }

        public PlayerModifier(
            NumValueModifier? MaxHp = null,
            NumValueModifier? Hp = null,
            NumValueModifier? MaxMp = null,
            NumValueModifier? Mp = null
            )
        {
            this.MaxHp = MaxHp;
            this.Hp = Hp;
            this.MaxMp = MaxMp;
            this.Mp = Mp;
        }
    }
}
