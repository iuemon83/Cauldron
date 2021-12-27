using MessagePack;
using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// 効果を予約する
    /// </summary>
    [MessagePackObject(true)]
    public class EffectActionReserveEffect
    {
        public IEnumerable<CardEffect> EffectsToReserve { get; }

        public EffectActionReserveEffect(
            IEnumerable<CardEffect> EffectsToReserve
            )
        {
            this.EffectsToReserve = EffectsToReserve;
        }
    }
}
