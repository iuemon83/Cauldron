using Cauldron.Core.Entities;
using Cauldron.Core.Entities.Effect;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Cauldron.Shared.MessagePackObjects
{
    public static class CardConditionExtensions
    {
        public static async ValueTask<Card[]> ListMatchedCards(this CardCondition cardCondition, Card effectOwnerCard, EffectEventArgs eventArgs, CardRepository cardRepository)
        {
            var source = cardCondition.ChoiceCandidateSourceCards(effectOwnerCard, eventArgs, cardRepository);

            var mathedCards = new List<Card>();
            foreach (var card in source)
            {
                var isMathed = await cardCondition.IsMatch(card, effectOwnerCard, eventArgs);

                if (isMathed)
                {
                    mathedCards.Add(card);
                }
            }

            return mathedCards.ToArray();
        }

        private static IEnumerable<Card> ChoiceCandidateSourceCards(this CardCondition cardCondition, Card effectOwnerCard, EffectEventArgs eventArgs, CardRepository cardRepository)
        {
            var actionContext = cardCondition?.ActionContext;
            if (actionContext != null)
            {
                return actionContext.GetCards(effectOwnerCard, eventArgs);
            }
            else
            {
                return cardRepository.AllCards;
            }
        }

        public static async ValueTask<bool> IsMatch(this CardCondition _this,
            Card cardToMatch, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            static bool ContextConditionIsMatch(CardCondition.ContextConditionValue value,
                Card cardToMatch, Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                static bool IsMatchesAttackCard(EffectEventArgs effectEventArgs, Card cardToMatch)
                {
                    if (effectEventArgs.DamageContext != null)
                    {
                        return effectEventArgs.DamageContext.IsBattle
                            && cardToMatch.Id == effectEventArgs.DamageContext.DamageSourceCard?.Id;
                    }
                    else if (effectEventArgs.BattleContext != null)
                    {
                        return cardToMatch.Id == effectEventArgs.BattleContext.AttackCard?.Id;
                    }

                    return false;
                }

                static bool IsMatchesGuardCard(EffectEventArgs effectEventArgs, Card cardToMatch)
                {
                    if (effectEventArgs.DamageContext != null)
                    {
                        return effectEventArgs.DamageContext.IsBattle
                            && cardToMatch.Id == effectEventArgs.DamageContext.GuardCard?.Id;
                    }
                    else if (effectEventArgs.BattleContext != null)
                    {
                        return cardToMatch.Id == effectEventArgs.BattleContext.GuardCard?.Id;
                    }

                    return false;
                }

                return value switch
                {
                    CardCondition.ContextConditionValue.This => cardToMatch.Id == effectOwnerCard.Id,
                    CardCondition.ContextConditionValue.Others => cardToMatch.Id != effectOwnerCard.Id,
                    CardCondition.ContextConditionValue.EventSource => cardToMatch.Id == effectEventArgs.SourceCard.Id,
                    CardCondition.ContextConditionValue.DamageFrom =>
                        cardToMatch.Id == effectEventArgs.DamageContext?.DamageSourceCard?.Id,
                    CardCondition.ContextConditionValue.DamageTo =>
                        cardToMatch.Id == effectEventArgs.DamageContext?.GuardCard?.Id,
                    CardCondition.ContextConditionValue.Attack => IsMatchesAttackCard(effectEventArgs, cardToMatch),
                    CardCondition.ContextConditionValue.Guard => IsMatchesGuardCard(effectEventArgs, cardToMatch),
                    _ => true
                };
            }

            static bool OwnerConditionIsMatch(
                CardCondition.OwnerConditionValue value, Card cardToMatch, Card effectOwnerCard)
            {
                return value switch
                {
                    CardCondition.OwnerConditionValue.You => effectOwnerCard.OwnerId == cardToMatch.OwnerId,
                    CardCondition.OwnerConditionValue.Opponent => effectOwnerCard.OwnerId != cardToMatch.OwnerId,
                    _ => true
                };
            }

            static bool AbilitiesConditionIsMatch(
                IReadOnlyCollection<CreatureAbility> value, Card cardToMatch)
            {
                if (value == default)
                {
                    return true;
                }

                return value.Any(x => cardToMatch.Abilities.Contains(x));
            }

            return
                ContextConditionIsMatch(_this.ContextCondition, cardToMatch, effectOwnerCard, effectEventArgs)
                && OwnerConditionIsMatch(_this.OwnerCondition, cardToMatch, effectOwnerCard)
                && (_this.CostCondition?.IsMatch(cardToMatch.Cost) ?? true)
                && (_this.PowerCondition?.IsMatch(cardToMatch.Power) ?? true)
                && (_this.ToughnessCondition?.IsMatch(cardToMatch.Toughness) ?? true)
                && (await (_this.CardSetCondition?.IsMatch(effectOwnerCard, effectEventArgs, cardToMatch)
                    ?? ValueTask.FromResult(true)))
                && (await (_this.NameCondition?.IsMatch(effectOwnerCard, effectEventArgs, cardToMatch.Name)
                    ?? ValueTask.FromResult(true)))
                && (_this.TypeCondition?.IsMatch(cardToMatch.Type) ?? true)
                && (await (_this.ZoneCondition?.IsMatch(effectOwnerCard, effectEventArgs, cardToMatch.Zone)
                    ?? ValueTask.FromResult(true)))
                && AbilitiesConditionIsMatch(_this.AbilityCondition, cardToMatch)
                ;
        }
    }
}
