#nullable enable

using MessagePack;
using System;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingStartTurnEvent
    {
        public PlayerCondition[] OrPlayerConditions { get; }

        public EffectTimingStartTurnEvent(PlayerCondition[]? OrPlayerConditions = null)
        {
            this.OrPlayerConditions = OrPlayerConditions ?? Array.Empty<PlayerCondition>();
        }
    }
}
