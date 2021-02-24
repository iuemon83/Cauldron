using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class PlayerModifier
    {
        public NumValueModifier MaxHp { get; set; } = null;
        public NumValueModifier Hp { get; set; } = null;
        public NumValueModifier MaxMp { get; set; } = null;
        public NumValueModifier Mp { get; set; } = null;

        public PlayerModifier(
            NumValueModifier MaxHp = null,
            NumValueModifier Hp = null,
            NumValueModifier MaxMp = null,
            NumValueModifier Mp = null
            )
        {
            this.MaxHp = MaxHp;
            this.Hp = Hp;
            this.MaxMp = MaxMp;
            this.Mp = Mp;
        }
    }
}
