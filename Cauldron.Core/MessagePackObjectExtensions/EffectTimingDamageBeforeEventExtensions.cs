﻿using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingDamageBeforeEventExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectTimingDamageBeforeEvent effectTimingDamageBeforeEvent, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            if (eventArgs.DamageContext == null)
            {
                return false;
            }

            if (effectTimingDamageBeforeEvent.Type == EffectTimingDamageBeforeEvent.TypeValue.Battle
                && !eventArgs.DamageContext.IsBattle)
            {
                return false;
            }

            var playerMatch = PlayerIsMatch(effectTimingDamageBeforeEvent, effectOwnerCard, eventArgs);

            var cardMatch = await CardIsMatch(effectTimingDamageBeforeEvent, effectOwnerCard, eventArgs);

            return playerMatch || cardMatch;

            static bool PlayerIsMatch(EffectTimingDamageBeforeEvent effectTimingDamageBeforeEvent, Card effectOwnerCard, EffectEventArgs eventArgs)
            {
                return effectTimingDamageBeforeEvent.Source switch
                {
                    EffectTimingDamageBeforeEvent.SourceValue.Any => eventArgs.DamageContext.GuardPlayer != null
                        && (effectTimingDamageBeforeEvent.PlayerCondition?.IsMatch(effectOwnerCard, eventArgs, eventArgs.DamageContext.GuardPlayer)
                            ?? false),
                    EffectTimingDamageBeforeEvent.SourceValue.Take => eventArgs.DamageContext.GuardPlayer != null
                        && (effectTimingDamageBeforeEvent.PlayerCondition?.IsMatch(effectOwnerCard, eventArgs, eventArgs.DamageContext.GuardPlayer)
                            ?? false),
                    _ => false
                };
            }

            static async ValueTask<bool> CardIsMatch(EffectTimingDamageBeforeEvent effectTimingDamageBeforeEvent, Card effectOwnerCard, EffectEventArgs eventArgs)
            {
                async ValueTask<bool> IsMatchDamageSource()
                {
                    var damageSource = eventArgs.DamageContext.DamageSourceCard;
                    if (damageSource == null
                        || effectTimingDamageBeforeEvent.CardCondition == null)
                    {
                        return false;
                    }

                    return await effectTimingDamageBeforeEvent.CardCondition.IsMatch(damageSource, effectOwnerCard, eventArgs);
                }

                async ValueTask<bool> IsMatchTake()
                {
                    var guard = eventArgs.DamageContext.GuardCard;
                    if (guard == null
                        || effectTimingDamageBeforeEvent.CardCondition == null)
                    {
                        return false;
                    }

                    return await effectTimingDamageBeforeEvent.CardCondition.IsMatch(guard, effectOwnerCard, eventArgs);
                }

                return effectTimingDamageBeforeEvent.Source switch
                {
                    EffectTimingDamageBeforeEvent.SourceValue.Any => await IsMatchDamageSource() || await IsMatchTake(),
                    EffectTimingDamageBeforeEvent.SourceValue.DamageSource => await IsMatchDamageSource(),
                    EffectTimingDamageBeforeEvent.SourceValue.Take => await IsMatchTake(),
                    _ => false
                };
            }
        }
    }
}