﻿using Cauldron.Core.Entities.Effect;
using System;
using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ActionContextCardsOfModifyCardExtensions
    {
        public static IEnumerable<Card> GetRsult(this ActionContextCardsOfModifyCard _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            return args.GameMaster.TryGetActionContext(effectOwnerCard.Id, _this.ActionName, out var value)
                ? value?.ActionModifyCardContext?.GetCards(_this.Type)
                : Array.Empty<Card>();
        }
    }
}