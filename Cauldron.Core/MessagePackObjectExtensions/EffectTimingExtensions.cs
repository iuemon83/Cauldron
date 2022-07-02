﻿using Cauldron.Core.Entities;
using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectTiming effectTiming, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            return eventArgs.GameEvent switch
            {
                GameEvent.OnStartTurn => effectTiming.StartTurn != null
                    && await effectTiming.StartTurn.IsMatch(effectOwnerCard, eventArgs),
                GameEvent.OnEndTurn => effectTiming.EndTurn != null
                    && await effectTiming.EndTurn.IsMatch(effectOwnerCard, eventArgs),
                GameEvent.OnPlay => await (effectTiming.Play?.IsMatch(effectOwnerCard, eventArgs)
                    ?? ValueTask.FromResult(false)),
                GameEvent.OnDestroy => await (effectTiming.Destroy?.IsMatch(effectOwnerCard, eventArgs)
                    ?? ValueTask.FromResult(false)),
                GameEvent.OnDamageBefore => await (effectTiming.DamageBefore?.IsMatch(effectOwnerCard, eventArgs)
                    ?? ValueTask.FromResult(false)),
                GameEvent.OnDamage => await (effectTiming.DamageAfter?.IsMatch(effectOwnerCard, eventArgs)
                    ?? ValueTask.FromResult(false)),
                GameEvent.OnHealBefore => await (effectTiming.HealBefore?.IsMatch(effectOwnerCard, eventArgs)
                    ?? ValueTask.FromResult(false)),
                GameEvent.OnHeal => await (effectTiming.HealAfter?.IsMatch(effectOwnerCard, eventArgs)
                    ?? ValueTask.FromResult(false)),
                GameEvent.OnAttackBefore => await (effectTiming.AttackBefore?.IsMatch(effectOwnerCard, eventArgs)
                    ?? ValueTask.FromResult(false)),
                GameEvent.OnAttack => await (effectTiming.AttackAfter?.IsMatch(effectOwnerCard, eventArgs)
                    ?? ValueTask.FromResult(false)),
                GameEvent.OnMoveCard => await (effectTiming.MoveCard?.IsMatch(effectOwnerCard, eventArgs)
                    ?? ValueTask.FromResult(false)),
                GameEvent.OnExclude => await (effectTiming.ExcludeCard?.IsMatch(effectOwnerCard, eventArgs)
                    ?? ValueTask.FromResult(false)),
                GameEvent.OnModifyCounter => await (effectTiming.ModifyCounter?.IsMatch(effectOwnerCard, eventArgs)
                    ?? ValueTask.FromResult(false)),
                GameEvent.OnModifyPlayer => await (effectTiming.ModifyPlayer?.IsMatch(effectOwnerCard, eventArgs)
                    ?? ValueTask.FromResult(false)),
                GameEvent.OnModifyCard => await (effectTiming.ModifyCard?.IsMatch(effectOwnerCard, eventArgs)
                    ?? ValueTask.FromResult(false)),
                _ => false,
            };
        }

        public static GameEvent ToGameEvent(this EffectTiming effectTiming)
        {
            if (effectTiming.AttackAfter != null)
            {
                return GameEvent.OnAttack;
            }
            else if (effectTiming.AttackBefore != null)
            {
                return GameEvent.OnAttackBefore;
            }
            else if (effectTiming.DamageAfter != null)
            {
                return GameEvent.OnDamage;
            }
            else if (effectTiming.DamageBefore != null)
            {
                return GameEvent.OnDamageBefore;
            }
            else if (effectTiming.HealAfter != null)
            {
                return GameEvent.OnHeal;
            }
            else if (effectTiming.HealBefore != null)
            {
                return GameEvent.OnHealBefore;
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
            else if (effectTiming.ExcludeCard != null)
            {
                return GameEvent.OnExclude;
            }
            else if (effectTiming.Play != null)
            {
                return GameEvent.OnPlay;
            }
            else if (effectTiming.ModifyCounter != null)
            {
                return GameEvent.OnModifyCounter;
            }
            else if (effectTiming.StartTurn != null)
            {
                return GameEvent.OnStartTurn;
            }
            else if (effectTiming.ModifyPlayer != null)
            {
                return GameEvent.OnModifyPlayer;
            }
            else if (effectTiming.ModifyCard != null)
            {
                return GameEvent.OnModifyCard;
            }

            throw new InvalidOperationException("invalid card effect timing");
        }
    }
}
