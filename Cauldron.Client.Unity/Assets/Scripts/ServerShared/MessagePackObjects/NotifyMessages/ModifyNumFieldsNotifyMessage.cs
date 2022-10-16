#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ModifyNumFieldsNotifyMessage
    {
        public PlayerId PlayerId { get; }
        public int FieldIndex { get; }
        public bool IsActiveField { get; }
        public Card? EffectOwnerCard { get; }
        public CardEffectId? EffectId { get; }

        public ModifyNumFieldsNotifyMessage(
            PlayerId PlayerId,
             int FieldIndex,
             bool IsActiveField,
            Card? EffectOwnerCard = default,
            CardEffectId? EffectId = default
            )
        {
            this.PlayerId = PlayerId;
            this.FieldIndex = FieldIndex;
            this.IsActiveField = IsActiveField;
            this.EffectOwnerCard = EffectOwnerCard;
            this.EffectId = EffectId;
        }
    }
}
