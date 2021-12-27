using Cauldron.Core.Entities.Effect;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectWhileExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectWhile effectWhile, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            return eventArgs.GameMaster.IsMatchedWhile(effectWhile, effectOwnerCard);
        }
    }
}
