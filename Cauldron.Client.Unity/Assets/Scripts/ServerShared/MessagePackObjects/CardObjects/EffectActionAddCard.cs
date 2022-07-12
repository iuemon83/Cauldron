#nullable enable

using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionAddCard
    {
        public Choice Choice { get; }

        public ZoneValue ZoneToAddCard { get; }

        public InsertCardPosition? InsertCardPosition { get; }

        /// <summary>
        /// 選択したカードを加える数
        /// 
        /// ex. 
        /// 1枚選択して、それを2枚加える => 2
        /// 2枚選択して、それぞれ1枚ずつ加える => 1
        /// </summary>
        public NumValue NumOfAddCards { get; }
        public string? Name { get; }

        public EffectActionAddCard(
            Choice Choice,
            ZoneValue ZoneToAddCard,
            InsertCardPosition? InsertCardPosition = null,
            NumValue? NumOfAddCards = null,
            string? Name = null
            )
        {
            this.Choice = Choice;
            this.ZoneToAddCard = ZoneToAddCard;
            this.InsertCardPosition = InsertCardPosition;
            this.NumOfAddCards = NumOfAddCards ?? new NumValue(1);
            this.Name = Name;
        }
    }
}
