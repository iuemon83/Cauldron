using Cauldron.Core.Entities.Effect;
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

            if (effectTimingDamageBeforeEvent.Type == EffectTimingDamageBeforeEvent.DamageType.Battle
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
                    EffectTimingDamageBeforeEvent.EventSource.Any => eventArgs.DamageContext.GuardPlayer != null
                        && (effectTimingDamageBeforeEvent.PlayerCondition?.IsMatch(effectOwnerCard, eventArgs, eventArgs.DamageContext.GuardPlayer)
                            ?? false),
                    EffectTimingDamageBeforeEvent.EventSource.Take => eventArgs.DamageContext.GuardPlayer != null
                        && (effectTimingDamageBeforeEvent.PlayerCondition?.IsMatch(effectOwnerCard, eventArgs, eventArgs.DamageContext.GuardPlayer)
                            ?? false),
                    _ => false
                };
            }

            static async ValueTask<bool> CardIsMatch(EffectTimingDamageBeforeEvent effectTimingDamageBeforeEvent, Card effectOwnerCard, EffectEventArgs eventArgs)
            {
                async ValueTask<bool> SwitchDamageSource()
                {
                    var damageSource = eventArgs.DamageContext.DamageSourceCard;
                    if (damageSource == null
                        || effectTimingDamageBeforeEvent.CardCondition == null)
                    {
                        return false;
                    }

                    return await effectTimingDamageBeforeEvent.CardCondition.IsMatch(effectOwnerCard, eventArgs, damageSource);
                }

                async ValueTask<bool> SwitchTake()
                {
                    var guard = eventArgs.DamageContext.GuardCard;
                    if (guard == null
                        || effectTimingDamageBeforeEvent.CardCondition == null)
                    {
                        return false;
                    }

                    return await effectTimingDamageBeforeEvent.CardCondition.IsMatch(effectOwnerCard, eventArgs, guard);
                }

                return effectTimingDamageBeforeEvent.Source switch
                {
                    EffectTimingDamageBeforeEvent.EventSource.Any => await SwitchDamageSource() || await SwitchTake(),
                    EffectTimingDamageBeforeEvent.EventSource.DamageSource => await SwitchDamageSource(),
                    EffectTimingDamageBeforeEvent.EventSource.Take => await SwitchTake(),
                    _ => false
                };
            }
        }
    }
}
