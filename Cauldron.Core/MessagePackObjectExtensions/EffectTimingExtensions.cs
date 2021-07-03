using Cauldron.Core.Entities;
using Cauldron.Core.Entities.Effect;
using System;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectTiming effectTiming, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            return eventArgs.GameEvent switch
            {
                GameEvent.OnStartTurn => effectTiming.StartTurn?.IsMatch(eventArgs.GameMaster.ActivePlayer.Id, effectOwnerCard) ?? false,
                GameEvent.OnEndTurn => effectTiming.EndTurn?.IsMatch(eventArgs.GameMaster.ActivePlayer.Id, effectOwnerCard) ?? false,
                GameEvent.OnPlay => await (effectTiming.Play?.IsMatch(effectOwnerCard, eventArgs.SourceCard, eventArgs)
                    ?? ValueTask.FromResult(false)),
                GameEvent.OnDestroy => effectTiming.Destroy?.IsMatch(effectOwnerCard, eventArgs.SourceCard) ?? false,
                GameEvent.OnDamageBefore => await (effectTiming.DamageBefore?.IsMatch(effectOwnerCard, eventArgs)
                    ?? ValueTask.FromResult(false)),
                GameEvent.OnDamage => await (effectTiming.DamageAfter?.IsMatch(effectOwnerCard, eventArgs)
                    ?? ValueTask.FromResult(false)),
                GameEvent.OnMoveCard => effectTiming.MoveCard?.IsMatch(effectOwnerCard, eventArgs) ?? false,
                _ => false,
            };
        }

        public static GameEvent ToGameEvent(this EffectTiming effectTiming)
        {
            if (effectTiming.DamageAfter != null)
            {
                return GameEvent.OnDamage;
            }
            else if (effectTiming.DamageBefore != null)
            {
                return GameEvent.OnDamageBefore;
            }
            else if (effectTiming.Destroy != null)
            {
                return GameEvent.OnDestroy;
            }
            else if (effectTiming.EndTurn != null)
            {
                return GameEvent.OnEndTurn;
            }
            else if (effectTiming.MoveCard != null)
            {
                return GameEvent.OnMoveCard;
            }
            else if (effectTiming.Play != null)
            {
                return GameEvent.OnPlay;
            }
            else if (effectTiming.StartTurn != null)
            {
                return GameEvent.OnStartTurn;
            }

            throw new InvalidOperationException("invalid card effect timing");
        }
    }
}
