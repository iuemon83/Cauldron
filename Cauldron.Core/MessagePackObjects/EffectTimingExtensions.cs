using Cauldron.Core.Entities;
using Cauldron.Core.Entities.Effect;
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
                GameEvent.OnPlay => effectTiming.Play?.IsMatch(effectOwnerCard, eventArgs.SourceCard) ?? false,
                GameEvent.OnDestroy => effectTiming.Destroy?.IsMatch(effectOwnerCard, eventArgs.SourceCard) ?? false,
                GameEvent.OnDamageBefore => await (effectTiming.DamageBefore?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(false)),
                GameEvent.OnDamage => await (effectTiming.DamageAfter?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(false)),
                GameEvent.OnMoveCard => effectTiming.MoveCard?.IsMatch(effectOwnerCard, eventArgs) ?? false,
                _ => false,
            };
        }
    }
}
