using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionAddCard
    {
        public ZoneValue ZoneToAddCard { get; set; }
        public Choice Choice { get; set; }

        public EffectActionAddCard(ZoneValue ZoneToAddCard, Choice choice)
        {
            this.ZoneToAddCard = ZoneToAddCard;
            this.Choice = choice;
        }
    }
}
