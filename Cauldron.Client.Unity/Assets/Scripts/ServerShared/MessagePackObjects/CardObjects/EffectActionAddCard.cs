using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionAddCard
    {
        public ZoneValue ZoneToAddCard { get; }

        /// <summary>
        /// 選択したカードを加える数
        /// 
        /// ex. 
        /// 1枚選択して、それを2枚加える => 2
        /// 2枚選択して、それぞれ1枚ずつ加える => 1
        /// </summary>
        public int NumOfAddCards { get; }

        public Choice Choice { get; }

        public EffectActionAddCard(Choice choice, ZoneValue ZoneToAddCard, int NumOfAddCards = 1)
        {
            this.ZoneToAddCard = ZoneToAddCard;
            this.NumOfAddCards = NumOfAddCards;
            this.Choice = choice;
        }
    }
}
