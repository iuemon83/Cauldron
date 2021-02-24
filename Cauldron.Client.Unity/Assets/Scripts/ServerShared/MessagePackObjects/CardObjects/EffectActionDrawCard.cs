using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionDrawCard
    {
        public NumValue NumCards { get; set; }
        public PlayerCondition PlayerCondition { get; set; }

        public EffectActionDrawCard(NumValue NumCards, PlayerCondition PlayerCondition)
        {
            this.NumCards = NumCards;
            this.PlayerCondition = PlayerCondition;
        }
    }
}
