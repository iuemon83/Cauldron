using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Threading.Tasks;


namespace Cauldron.Shared.MessagePackObjects
{
    public static class CardConditionExtensions
    {
        public static async ValueTask<bool> IsMatch(this CardCondition cardCondition, Card effectOwnerCard, Card cardToMatch, EffectEventArgs effectEventArgs)
        {
            return
                (cardCondition.Context switch
                {
                    CardCondition.CardConditionContext.This => cardToMatch.Id == effectOwnerCard.Id,
                    CardCondition.CardConditionContext.Others => cardToMatch.Id != effectOwnerCard.Id,
                    CardCondition.CardConditionContext.EventSource => cardToMatch.Id == effectEventArgs.SourceCard.Id,
                    CardCondition.CardConditionContext.DamageFrom => cardToMatch.Id == effectEventArgs.DamageContext.DamageSourceCard.Id,
                    CardCondition.CardConditionContext.DamageTo => cardToMatch.Id == effectEventArgs.DamageContext.GuardCard.Id,
                    _ => true
                })
                && (cardCondition.CostCondition?.IsMatch(cardToMatch.Cost) ?? true)
                && (cardCondition.PowerCondition?.IsMatch(cardToMatch.Power) ?? true)
                && (cardCondition.ToughnessCondition?.IsMatch(cardToMatch.Toughness) ?? true)
                && (await (cardCondition.NameCondition?.IsMatch(effectOwnerCard, effectEventArgs, cardToMatch.Name) ?? ValueTask.FromResult(true)))
                && (cardCondition.TypeCondition?.IsMatch(cardToMatch.Type) ?? true)
                && (await (cardCondition.ZoneCondition?.IsMatch(effectOwnerCard, effectEventArgs, cardToMatch.Zone) ?? ValueTask.FromResult(true)))
                ;
        }

        public static async ValueTask<bool> IsMatch(this CardCondition cardCondition, Card effectOwnerCard, EffectEventArgs effectEventArgs, CardDef cardDefToMatch)
        {
            return
                (cardCondition.CostCondition?.IsMatch(cardDefToMatch.Cost) ?? true)
                && (cardCondition.PowerCondition?.IsMatch(cardDefToMatch.Power) ?? true)
                && (cardCondition.ToughnessCondition?.IsMatch(cardDefToMatch.Toughness) ?? true)
                && (await (cardCondition.NameCondition?.IsMatch(effectOwnerCard, effectEventArgs, cardDefToMatch.FullName) ?? ValueTask.FromResult(true)))
                && (cardCondition.TypeCondition?.IsMatch(cardDefToMatch.Type) ?? true);
        }
    }
}
