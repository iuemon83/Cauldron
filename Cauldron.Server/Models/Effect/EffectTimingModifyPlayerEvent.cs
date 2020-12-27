using System;

namespace Cauldron.Server.Models.Effect
{
    public record EffectTimingModifyPlayerEvent(
        EffectTimingModifyPlayerEvent.EventSource Source,
        bool ModifyMaxHp,
        bool ModifyHp,
        bool ModifyMaxMp,
        bool ModifyMp
        )
    {
        public enum EventSource
        {
            Owner,
            Other,
            All
        }

        public bool IsMatch(PlayerId modifyPlayerId, Card ownerCard)
        {
            return this.Source switch
            {
                EventSource.All => true,
                EventSource.Owner => modifyPlayerId == ownerCard.OwnerId,
                EventSource.Other => modifyPlayerId != ownerCard.OwnerId,
                _ => throw new InvalidOperationException($"{nameof(this.Source)}={this.Source}"),
            };
        }
    }
}
