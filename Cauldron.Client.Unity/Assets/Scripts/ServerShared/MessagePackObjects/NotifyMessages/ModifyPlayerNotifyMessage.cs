#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ModifyPlayerNotifyMessage
    {
        public PlayerId PlayerId { get; }
        public Card? EffectOwnerCard { get; }

        public ModifyPlayerNotifyMessage(
            PlayerId PlayerId,
            Card? EffectOwnerCard = default
            )
        {
            this.PlayerId = PlayerId;
            this.EffectOwnerCard = EffectOwnerCard;
        }
    }
}
