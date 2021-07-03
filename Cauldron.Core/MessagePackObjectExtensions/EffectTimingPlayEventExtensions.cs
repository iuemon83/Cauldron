using Cauldron.Core.Entities.Effect;
using System;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingPlayEventExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectTimingPlayEvent effectTimingPlayEvent, Card effectOwnerCard, Card sourceCard, EffectEventArgs args)
        {
            bool IsMatchedSource(EffectTimingPlayEvent.SourceValue value)
                => value switch
                {
                    EffectTimingPlayEvent.SourceValue.This => effectOwnerCard.Id == sourceCard.Id,
                    EffectTimingPlayEvent.SourceValue.Other => effectOwnerCard.Id != sourceCard.Id,
                    _ => throw new InvalidOperationException()
                };

            async ValueTask<bool> IsMatchedCardCondition(CardCondition cardCondition, Card card)
            {
                if (cardCondition == null)
                {
                    return true;
                }

                return await cardCondition.IsMatch(card, effectOwnerCard, args);
            }

            return IsMatchedSource(effectTimingPlayEvent.Source)
                && await IsMatchedCardCondition(effectTimingPlayEvent.CardCondition, sourceCard);
        }
    }
}
