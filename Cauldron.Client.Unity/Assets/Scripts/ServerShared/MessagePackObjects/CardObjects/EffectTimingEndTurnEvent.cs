using MessagePack;
using System;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingEndTurnEvent
    {
        public PlayerCondition[] OrPlayerCondition { get; }

        public EffectTimingEndTurnEvent(PlayerCondition[] OrPlayerCondition = null)
        {
            this.OrPlayerCondition = OrPlayerCondition ?? Array.Empty<PlayerCondition>();
        }
    }
}
