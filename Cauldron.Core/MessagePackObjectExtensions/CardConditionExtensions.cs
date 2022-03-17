using Cauldron.Core.Entities;
using Cauldron.Core.Entities.Effect;


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
                var isMathed = await cardCondition.IsMatch(effectOwnerCard, eventArgs, card);

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
            Card effectOwnerCard, EffectEventArgs effectEventArgs, Card cardToMatch)
        {
            static bool ContextConditionIsMatch(CardCondition.ContextConditionValue value,
                Card cardToMatch, Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                return value switch
                {
                    CardCondition.ContextConditionValue.This => cardToMatch.Id == effectOwnerCard.Id,
                    CardCondition.ContextConditionValue.Others => cardToMatch.Id != effectOwnerCard.Id,
                    CardCondition.ContextConditionValue.SameDefs => cardToMatch.CardDefId == effectOwnerCard.CardDefId,
                    CardCondition.ContextConditionValue.OtherDefs => cardToMatch.CardDefId != effectOwnerCard.CardDefId,
                    CardCondition.ContextConditionValue.EventSource => cardToMatch.Id == effectEventArgs.SourceCard?.Id,
                    CardCondition.ContextConditionValue.ActionTarget => cardToMatch.Id == effectEventArgs.ActionTargetCard?.Id,
                    CardCondition.ContextConditionValue.ActionTargetAll => effectEventArgs.ActionTargetCards.Select(x => x.Id).Contains(cardToMatch.Id),
                    _ => true
                };
            }

            static bool DamageEventContextConditionIsMatch(
                CardCondition.DamageEventContextConditionValue value, Card cardToMatch, EffectEventArgs effectEventArgs)
            {
                return value switch
                {
                    CardCondition.DamageEventContextConditionValue.DamageFrom =>
                        cardToMatch.Id == effectEventArgs.DamageContext?.DamageSourceCard?.Id,
                    CardCondition.DamageEventContextConditionValue.DamageTo =>
                        cardToMatch.Id == effectEventArgs.DamageContext?.GuardCard?.Id,
                    _ => true
                };
            }

            static bool BattleEventContextConditionIsMatch(
                CardCondition.BattleEventContextConditionValue value, Card cardToMatch, EffectEventArgs effectEventArgs)
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
                    CardCondition.BattleEventContextConditionValue.Attack => IsMatchesAttackCard(effectEventArgs, cardToMatch),
                    CardCondition.BattleEventContextConditionValue.Guard => IsMatchesGuardCard(effectEventArgs, cardToMatch),
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
                IReadOnlyCollection<CreatureAbility>? value, Card cardToMatch)
            {
                if (value == default)
                {
                    return true;
                }

                return value.Any(x => cardToMatch.Abilities.Contains(x));
            }

            // await じゃないものを先
            return
                ContextConditionIsMatch(_this.ContextCondition, cardToMatch, effectOwnerCard, effectEventArgs)
                && DamageEventContextConditionIsMatch(_this.DamageEventContextCondition, cardToMatch, effectEventArgs)
                && BattleEventContextConditionIsMatch(_this.BattleEventContextCondition, cardToMatch, effectEventArgs)
                && OwnerConditionIsMatch(_this.OwnerCondition, cardToMatch, effectOwnerCard)
                && (_this.CostCondition?.IsMatch(cardToMatch.Cost) ?? true)
                && (_this.PowerCondition?.IsMatch(cardToMatch.Power) ?? true)
                && (_this.ToughnessCondition?.IsMatch(cardToMatch.Toughness) ?? true)
                && AbilitiesConditionIsMatch(_this.AbilityCondition, cardToMatch)
                && (_this.CounterCondition?.IsMatch(cardToMatch) ?? true)
                && (_this.AnnotationCondition?.IsMatch(cardToMatch.Annotations) ?? true)
                && (await (_this.CardSetCondition?.IsMatch(effectOwnerCard, effectEventArgs, cardToMatch)
                    ?? ValueTask.FromResult(true)))
                && (await (_this.NameCondition?.IsMatch(effectOwnerCard, effectEventArgs, cardToMatch.Name)
                    ?? ValueTask.FromResult(true)))
                && (_this.TypeCondition?.IsMatch(cardToMatch.Type) ?? true)
                && (await (_this.ZoneCondition?.IsMatch(effectOwnerCard, effectEventArgs, cardToMatch.Zone)
                    ?? ValueTask.FromResult(true)))
                ;
        }
    }
}
