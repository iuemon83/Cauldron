using MessagePack;
using System;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingStartTurnEvent
    {
        public PlayerCondition[] OrPlayerCondition { get; }

        public EffectTimingStartTurnEvent(PlayerCondition[] OrPlayerCondition = null)
        {
            this.OrPlayerCondition = OrPlayerCondition ?? Array.Empty<PlayerCondition>();
        }
    }
}
