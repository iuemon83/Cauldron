﻿using System;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingModifyPlayerEventExtensions
    {
        public static bool IsMatch(this EffectTimingModifyPlayerEvent effectTimingModifyPlayerEvent, PlayerId modifyPlayerId, Card ownerCard)
        {
            return effectTimingModifyPlayerEvent.Source switch
            {
                EffectTimingModifyPlayerEvent.SourceValue.All => true,
                EffectTimingModifyPlayerEvent.SourceValue.Owner => modifyPlayerId == ownerCard.OwnerId,
                EffectTimingModifyPlayerEvent.SourceValue.Other => modifyPlayerId != ownerCard.OwnerId,
                _ => throw new InvalidOperationException($"{nameof(effectTimingModifyPlayerEvent.Source)}={effectTimingModifyPlayerEvent.Source}"),
            };
        }
    }
}