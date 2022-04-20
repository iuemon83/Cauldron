#nullable enable

using MessagePack;
using System;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingEndTurnEvent
    {
        public PlayerCondition[] OrPlayerConditions { get; }

        public EffectTimingEndTurnEvent(PlayerCondition[]? OrPlayerConditions = null)
        {
            this.OrPlayerConditions = OrPlayerConditions ?? Array.Empty<PlayerCondition>();
        }
    }
}
