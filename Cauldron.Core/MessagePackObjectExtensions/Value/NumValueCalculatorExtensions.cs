﻿using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    public static class NumValueCalculatorExtensions
    {
        public static async ValueTask<int> Calculate(this NumValueCalculator _this,
            Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            if (_this.EventContext != NumValueCalculator.EventContextValue.None)
            {
                return _this.EventContext switch
                {
                    NumValueCalculator.EventContextValue.DamageValue
                        => effectEventArgs.DamageContext?.Value ?? 0,
                    _ => 0,
                };
            }

            if (_this.Random != null)
            {
                return _this.Random.Calculate();
            }

            if (_this.ForPlayer != null)
            {
                return await _this.ForPlayer.Calculate(effectOwnerCard, effectEventArgs);
            }

            if (_this.ForCard != null)
            {
                return await _this.ForCard.Calculate(effectOwnerCard, effectEventArgs);
            }

            if (_this.ForCounter != null)
            {
                return await _this.ForCounter.Calculate(effectOwnerCard, effectEventArgs);
            }

            return 0;
        }
    }
}
