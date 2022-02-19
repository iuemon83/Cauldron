#nullable enable

using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionDrawCard
    {
        public NumValue NumCards { get; }
        public PlayerCondition PlayerCondition { get; }
        public string? Name { get; }
        public EffectActionDrawCard(NumValue NumCards, PlayerCondition PlayerCondition,
            string? Name = null)
        {
            this.NumCards = NumCards;
            this.PlayerCondition = PlayerCondition;
            this.Name = Name;
        }
    }
}
