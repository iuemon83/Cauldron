#nullable enable

using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// 場の数を変更するアクション
    /// </summary>
    [MessagePackObject(true)]
    public class EffectActionModifyNumFields
    {
        public Choice ChoicePlayers { get; }

        /// <summary>
        /// 場の数の変化量
        /// </summary>
        public NumValueModifier DiffNum { get; }

        public string? Name { get; }

        public EffectActionModifyNumFields(
            Choice ChoicePlayers,
            NumValueModifier DiffNum,
            string? Name = null
            )
        {
            this.ChoicePlayers = ChoicePlayers;
            this.DiffNum = DiffNum;
            this.Name = Name;
        }
    }
}
