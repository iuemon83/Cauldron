using Cauldron.Core.Entities.Effect;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Core.Entities
{
    public static class MessageObjectExtensions
    {
        public static async ValueTask<int> Calculate(this NumValue numValue, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            async ValueTask<int> CalcBaseValue()
            {
                if (numValue.PureValue != null)
                {
                    return numValue.PureValue.Value;
                }
                else if (numValue.NumValueCalculator != null)
                {
                    return await numValue.NumValueCalculator.Calculate(effectOwnerCard, effectEventArgs);
                }
                else if (numValue.NumValueVariableCalculator != null)
                {
                    return numValue.NumValueVariableCalculator.Calculate(effectOwnerCard, effectEventArgs);
                }
                else
                {
                    return 0;
                }
            }

            var baseValue = await CalcBaseValue();

            //var baseValue = numValue.PureValue
            //    ?? numValue.NumValueCalculator?.Calculate(effectOwnerCard, effectEventArgs)
            //    ?? numValue.NumValueVariableCalculator?.Calculate(effectOwnerCard, effectEventArgs)
            //    ?? 0;

            return await (numValue.NumValueModifier?.Modify(effectOwnerCard, effectEventArgs, baseValue)
                ?? ValueTask.FromResult(baseValue));
        }


        public static async ValueTask<int> Calculate(this NumValueCalculator numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return numValueCalculator.Type switch
            {
                NumValueCalculator.ValueType.Count => await numValueCalculator.CalculateCount(effectOwnerCard, effectEventArgs),
                NumValueCalculator.ValueType.CardCost => await numValueCalculator.CalculateCardCost(effectOwnerCard, effectEventArgs),
                _ => throw new InvalidOperationException($"{nameof(numValueCalculator.Type)}: {numValueCalculator.Type}")
            };
        }

        private static async ValueTask<int> CalculateCount(this NumValueCalculator numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var pickCards = await effectEventArgs.GameMaster.ChoiceCards(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);
            return pickCards.CardList.Length;
        }

        private static async ValueTask<int> CalculateCardCost(this NumValueCalculator numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var pickCards = await effectEventArgs.GameMaster
                .ChoiceCards(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);

            return pickCards.CardList.Sum(c => c.Cost);
        }

        public static async ValueTask<int> Modify(this NumValueModifier numValueModifier, Card effectOwnerCard, EffectEventArgs effectEventArgs, int value)
        {
            var baseValue = await numValueModifier.Value.Calculate(effectOwnerCard, effectEventArgs);

            return numValueModifier.Operator switch
            {
                NumValueModifier.ValueModifierOperator.Add => value + baseValue,
                NumValueModifier.ValueModifierOperator.Sub => value - baseValue,
                NumValueModifier.ValueModifierOperator.Multi => value * baseValue,
                NumValueModifier.ValueModifierOperator.Div => value / baseValue,
                NumValueModifier.ValueModifierOperator.Replace => baseValue,
                _ => throw new InvalidOperationException()
            };
        }

        public static int Calculate(this NumValueVariableCalculator numValueVariableCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return effectEventArgs.GameMaster.TryGetNumVariable(effectOwnerCard.Id, numValueVariableCalculator.Name, out var value)
                ? value
                : default;
        }

        public static async ValueTask<string> Calculate(this TextValue textValue, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return textValue.PureValue
                ?? await (textValue.TextValueCalculator?.Calculate(effectOwnerCard, effectEventArgs)
                    ?? ValueTask.FromResult(""));
        }

        public static async ValueTask<string> Calculate(this TextValueCalculator textValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return textValueCalculator.Type switch
            {
                TextValueCalculator.ValueType.CardName => await textValueCalculator.CalculateName(effectOwnerCard, effectEventArgs),
                _ => throw new InvalidOperationException($"{nameof(textValueCalculator.Type)}: {textValueCalculator.Type}")
            };
        }

        private static async ValueTask<string> CalculateName(this TextValueCalculator textValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var choiceResult = await effectEventArgs.GameMaster
                .ChoiceCards(effectOwnerCard, textValueCalculator.CardsChoice, effectEventArgs);

            var pickCards = choiceResult.CardList;

            return pickCards.Any() ? pickCards[0].FullName : "";
        }

        public static async ValueTask<ZonePrettyName[]> Calculate(this ZoneValue zoneValue, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            async ValueTask<ZonePrettyName[]> FromValueCalculator()
            {
                if (zoneValue.ZoneValueCalculator == null)
                {
                    return null;
                }

                var zones = await zoneValue.ZoneValueCalculator.Calculate(effectOwnerCard, effectEventArgs);
                return zones.Select(zone => zone.AsZonePrettyName(effectOwnerCard)).ToArray();
            }

            return zoneValue.PureValue
                ?? await FromValueCalculator()
                //?? zoneValue.ZoneValueCalculator?.Calculate(effectOwnerCard, effectEventArgs)
                //    .Select(zone => zone.AsZonePrettyName(effectOwnerCard)).ToArray()
                ?? Array.Empty<ZonePrettyName>();
        }

        public static async ValueTask<IEnumerable<Zone>> Calculate(this ZoneValueCalculator zoneValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var choiceResult = await effectEventArgs.GameMaster
                .ChoiceCards(effectOwnerCard, zoneValueCalculator.CardsChoice, effectEventArgs);

            return choiceResult.CardList.Select(c => c.Zone);
        }

        public static IEnumerable<Card> GetCards(this ActionContextCards actionContextCards, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            if (actionContextCards?.ActionContextCardsOfAddEffect != null)
            {
                return actionContextCards.ActionContextCardsOfAddEffect.GetRsult(effectOwnerCard, eventArgs);
            }
            if (actionContextCards?.ActionContextCardsOfDestroyCard != null)
            {
                return actionContextCards.ActionContextCardsOfDestroyCard.GetRsult(effectOwnerCard, eventArgs);
            }
            else if (actionContextCards?.ActionContextCardsOfMoveCard != null)
            {
                return actionContextCards.ActionContextCardsOfMoveCard.GetRsult(effectOwnerCard, eventArgs);
            }
            else
            {
                return Array.Empty<Card>();
            }
        }

        public static IEnumerable<Card> GetRsult(this ActionContextCardsOfAddEffect actionContextCardsOfAddEffect, Card effectOwnerCard, EffectEventArgs args)
        {
            return args.GameMaster.TryGetActionContext(effectOwnerCard.Id, actionContextCardsOfAddEffect.ActionName, out var value)
                ? value?.ActionAddEffectContext?.GetCards(actionContextCardsOfAddEffect.Type)
                : Array.Empty<Card>();
        }


        public static IEnumerable<Card> GetRsult(this ActionContextCardsOfDestroyCard actionContextCardsOfDestroyCard, Card effectOwnerCard, EffectEventArgs args)
        {
            return args.GameMaster.TryGetActionContext(effectOwnerCard.Id, actionContextCardsOfDestroyCard.ActionName, out var value)
                ? value?.ActionDestroyCardContext?.GetCards(actionContextCardsOfDestroyCard.Type)
                : System.Array.Empty<Card>();
        }

        public static IEnumerable<Card> GetRsult(this ActionContextCardsOfMoveCard actionContextCardsOfMoveCard, Card effectOwnerCard, EffectEventArgs args)
        {
            return args.GameMaster.TryGetActionContext(effectOwnerCard.Id, actionContextCardsOfMoveCard.ActionName, out var value)
                ? value?.ActionMoveCardContext?.GetCards(actionContextCardsOfMoveCard.Type)
                : Array.Empty<Card>();
        }

        public static async ValueTask<bool> IsMatch(this CardCondition cardCondition, Card effectOwnerCard, Card cardToMatch, EffectEventArgs effectEventArgs)
        {
            return
                (cardCondition.Context switch
                {
                    CardCondition.CardConditionContext.This => cardToMatch.Id == effectOwnerCard.Id,
                    CardCondition.CardConditionContext.Others => cardToMatch.Id != effectOwnerCard.Id,
                    CardCondition.CardConditionContext.EventSource => cardToMatch.Id == effectEventArgs.SourceCard.Id,
                    CardCondition.CardConditionContext.Attack => cardToMatch.Id == effectEventArgs.BattleContext.AttackCard.Id,
                    CardCondition.CardConditionContext.Guard => cardToMatch.Id == effectEventArgs.BattleContext.GuardCard.Id,
                    _ => true
                })
                && (cardCondition.CostCondition?.IsMatch(cardToMatch.Cost) ?? true)
                && (cardCondition.PowerCondition?.IsMatch(cardToMatch.Power) ?? true)
                && (cardCondition.ToughnessCondition?.IsMatch(cardToMatch.Toughness) ?? true)
                && (await (cardCondition.NameCondition?.IsMatch(effectOwnerCard, effectEventArgs, cardToMatch.Name) ?? ValueTask.FromResult(true)))
                && (cardCondition.TypeCondition?.IsMatch(cardToMatch.Type) ?? true);
        }

        public static async ValueTask<bool> IsMatch(this CardCondition cardCondition, Card effectOwnerCard, EffectEventArgs effectEventArgs, CardDef cardDefToMatch)
        {
            return
                (cardCondition.CostCondition?.IsMatch(cardDefToMatch.BaseCost) ?? true)
                && (cardCondition.PowerCondition?.IsMatch(cardDefToMatch.BasePower) ?? true)
                && (cardCondition.ToughnessCondition?.IsMatch(cardDefToMatch.BaseToughness) ?? true)
                && (await (cardCondition.NameCondition?.IsMatch(effectOwnerCard, effectEventArgs, cardDefToMatch.FullName) ?? ValueTask.FromResult(true)))
                && (cardCondition.TypeCondition?.IsMatch(cardDefToMatch.Type) ?? true);
        }

        public static async ValueTask<(bool, EffectEventArgs)> DoIfMatched(this CardEffect cardEffect, Card effectOwnerCard, EffectEventArgs args)
        {
            if (!await cardEffect.Condition.IsMatch(effectOwnerCard, args)) return (false, args);

            var done = false;
            var newArgs = args;
            foreach (var action in cardEffect.Actions)
            {
                var (done2, newArgs2) = await action.Execute(effectOwnerCard, newArgs);

                done = done || done2;
                newArgs = newArgs2;
            }

            return (done, newArgs);
        }

        public static bool IsMatch(this CardTypeCondition cardTypeCondition, CardType checkValue)
        {
            var result = cardTypeCondition.Value.Contains(checkValue);
            return cardTypeCondition.Not ? !result : result;
        }

        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionAddCard effectActionAddCard, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var choiceResult = await effectEventArgs.GameMaster.ChoiceCards(effectOwnerCard, effectActionAddCard.Choice, effectEventArgs);
            var newCardDefs = choiceResult.CardDefList;

            var owner = effectEventArgs.GameMaster.PlayersById[effectOwnerCard.OwnerId];
            var opponent = effectEventArgs.GameMaster.GetOpponent(effectOwnerCard.OwnerId);

            var zonePrettyNames = await effectActionAddCard.ZoneToAddCard.Calculate(effectOwnerCard, effectEventArgs);

            if (!zonePrettyNames.Any())
            {
                return (false, effectEventArgs);
            }

            var zone = FromPrettyName(owner.Id, opponent.Id, zonePrettyNames[0]);

            var newCards = newCardDefs.Select(cd => effectEventArgs.GameMaster.GenerateNewCard(cd.Id, zone)).ToArray();

            return (true, effectEventArgs);
        }

        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionAddEffect effectActionAddEffect, Card effectOwnerCard, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.ChoiceCards(effectOwnerCard, effectActionAddEffect.CardsChoice, args);
            var targets = choiceResult.CardList;

            var done = false;
            foreach (var card in targets)
            {
                args.GameMaster.AddEffect(card, effectActionAddEffect.EffectToAdd);
            }

            if (!string.IsNullOrEmpty(effectActionAddEffect.Name))
            {
                var context = new ActionContext(ActionAddEffectContext: new(targets));
                args.GameMaster.SetActionContext(effectOwnerCard.Id, effectActionAddEffect.Name, context);
            }

            return (done, args);
        }

        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionDamage effectActionDamage, Card effectOwnerCard, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.ChoiceCards(effectOwnerCard, effectActionDamage.Choice, args);

            var done = false;

            foreach (var playerId in choiceResult.PlayerIdList)
            {
                var damageContext = new DamageContext(
                    effectOwnerCard,
                    Value: effectActionDamage.Value,
                    GuardPlayer: args.GameMaster.PlayersById[playerId]
                    );

                await args.GameMaster.HitPlayer(damageContext);

                done = true;
            }

            foreach (var card in choiceResult.CardList)
            {
                var damageContext = new DamageContext(
                    effectOwnerCard,
                    Value: effectActionDamage.Value,
                    GuardCard: card
                    );
                await args.GameMaster.HitCreature(damageContext);

                done = true;
            }

            return (done, args);
        }

        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionDestroyCard effectActionDestroyCard, Card effectOwnerCard, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.ChoiceCards(effectOwnerCard, effectActionDestroyCard.Choice, args);

            var done = false;
            foreach (var card in choiceResult.CardList)
            {
                await args.GameMaster.DestroyCard(card);

                done = true;
            }

            if (!string.IsNullOrEmpty(effectActionDestroyCard.Name))
            {
                var context = new ActionContext(ActionDestroyCardContext: new(choiceResult.CardList));
                args.GameMaster.SetActionContext(effectOwnerCard.Id, effectActionDestroyCard.Name, context);
            }

            return (done, args);
        }

        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionDrawCard effectActionDrawCard, Card effectOwnerCard, EffectEventArgs args)
        {
            // 対象のプレイヤー一覧
            // 順序はアクティブプレイヤー優先
            var targetPlayers = args.GameMaster.PlayersById.Values
                .Where(p => effectActionDrawCard.PlayerCondition.IsMatch(effectOwnerCard, p, args))
                .OrderBy(p => p.Id == args.GameMaster.ActivePlayer.Id);

            var numCards = await effectActionDrawCard.NumCards.Calculate(effectOwnerCard, args);

            foreach (var p in targetPlayers)
            {

                await args.GameMaster.Draw(p.Id, numCards);
            }

            return (true, args);
        }

        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionModifyCard effectActionModifyCard, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var choiceResult = await effectEventArgs.GameMaster.ChoiceCards(effectOwnerCard, effectActionModifyCard.Choice, effectEventArgs);
            var targets = choiceResult.CardList;

            var done = false;
            var buffPower = await (effectActionModifyCard.Power?.Calculate(effectOwnerCard, effectEventArgs) ?? ValueTask.FromResult(0));
            var buffToughness = await (effectActionModifyCard.Toughness?.Calculate(effectOwnerCard, effectEventArgs) ?? ValueTask.FromResult(0));
            foreach (var card in targets)
            {
                await effectEventArgs.GameMaster.Buff(card, buffPower, buffToughness);

                done = true;
            }

            return (done, effectEventArgs);
        }
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionModifyDamage effectActionModifyDamage, Card effectOwnerCard, EffectEventArgs args)
        {
            var done = false;

            var result = args;

            if (args.BattleContext != null)
            {
                var value = await effectActionModifyDamage.Value.Modify(effectOwnerCard, args, args.BattleContext.Value);
                result = args with
                {
                    BattleContext = args.BattleContext with
                    {
                        Value = Math.Max(0, value)
                    }
                };

                done = true;
            }

            if (args.DamageContext != null)
            {
                var value = await effectActionModifyDamage.Value.Modify(effectOwnerCard, args, args.DamageContext.Value);
                result = args with
                {
                    DamageContext = args.DamageContext with
                    {
                        Value = Math.Max(0, value)
                    }
                };

                done = true;
            }

            return (done, result);
        }

        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionModifyPlayer effectActionModifyPlayer, Card effectOwnerCard, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.ChoiceCards(effectOwnerCard, effectActionModifyPlayer.Choice, args);
            var targets = choiceResult.PlayerIdList;

            var done = false;
            foreach (var playerId in targets)
            {
                await args.GameMaster.ModifyPlayer(new(playerId, effectActionModifyPlayer.PlayerModifier), effectOwnerCard, args);

                done = true;
            }

            return (done, args);
        }

        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionMoveCard effectActionMoveCard, Card effectOwnerCard, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.ChoiceCards(effectOwnerCard, effectActionMoveCard.CardsChoice, args);
            var targets = choiceResult.CardList;

            var done = false;
            foreach (var card in targets)
            {
                var toZone = MessageObjectExtensions.FromPrettyName(effectOwnerCard.OwnerId, args.GameMaster.GetOpponent(effectOwnerCard.OwnerId).Id, effectActionMoveCard.To);
                await args.GameMaster.MoveCard(card.Id, new(card.Zone, toZone));

                done = true;
            }

            if (!string.IsNullOrEmpty(effectActionMoveCard.Name))
            {
                var context = new ActionContext(ActionMoveCardContext: new(targets));
                args.GameMaster.SetActionContext(effectOwnerCard.Id, effectActionMoveCard.Name, context);
            }

            return (done, args);
        }

        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionSetVariable effectActionSetVariable, Card effectOwnerCard, EffectEventArgs args)
        {
            if (effectActionSetVariable.NumValue != null)
            {
                var value = await effectActionSetVariable.NumValue.Calculate(effectOwnerCard, args);
                args.GameMaster.SetVariable(effectOwnerCard.Id, effectActionSetVariable.Name, value);
            }

            return (true, args);
        }

        public static readonly EffectCondition Spell
            = new(ZonePrettyName.YouField, new(new(Play: new(EffectTimingPlayEvent.EventSource.This))));

        public static async ValueTask<bool> IsMatch(this EffectCondition effectCondition, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            return effectCondition.IsMatchedZone(effectOwnerCard, eventArgs)
                && (await (effectCondition.While?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(true)))
                && await effectCondition.When.IsMatch(effectOwnerCard, eventArgs)
                && (await (effectCondition.If?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(true)));
        }

        private static bool IsMatchedZone(this EffectCondition effectCondition, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            var zone = MessageObjectExtensions.FromPrettyName(effectOwnerCard.OwnerId,
                eventArgs.GameMaster.GetOpponent(effectOwnerCard.OwnerId).Id, effectCondition.ZonePrettyName);
            return effectOwnerCard.Zone == zone;
        }

        public static async ValueTask<bool> IsMatch(this EffectIf effectIf, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            var value = await effectIf.NumValue.Calculate(effectOwnerCard, eventArgs);
            return effectIf.NumCondition.IsMatch(value);
        }

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
                GameEvent.OnBattleBefore => await (effectTiming.BattleBefore?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(false)),
                GameEvent.OnBattle => await (effectTiming.BattleAfter?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(false)),
                GameEvent.OnMoveCard => effectTiming.MoveCard?.IsMatch(effectOwnerCard, eventArgs) ?? false,
                _ => false,
            };
        }

        public static async ValueTask<bool> IsMatch(this EffectTimingBattleBeforeEvent effectTimingBattleBeforeEvent, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            var playerMatch = effectTimingBattleBeforeEvent.Source switch
            {
                EffectTimingBattleBeforeEvent.EventSource.All => (effectTimingBattleBeforeEvent.PlayerCondition?.IsMatch(effectOwnerCard, eventArgs.BattleContext.GuardPlayer, eventArgs) ?? false),
                EffectTimingBattleBeforeEvent.EventSource.Guard => effectTimingBattleBeforeEvent.PlayerCondition?.IsMatch(effectOwnerCard, eventArgs.BattleContext.GuardPlayer, eventArgs) ?? false,
                _ => false
            };

            var cardMatch = effectTimingBattleBeforeEvent.Source switch
            {
                EffectTimingBattleBeforeEvent.EventSource.All =>
                    (await (effectTimingBattleBeforeEvent.CardCondition?.IsMatch(effectOwnerCard, eventArgs.BattleContext.AttackCard, eventArgs)
                        ?? ValueTask.FromResult(false)))
                    || (await (effectTimingBattleBeforeEvent.CardCondition?.IsMatch(effectOwnerCard, eventArgs.BattleContext.GuardCard, eventArgs)
                        ?? ValueTask.FromResult(false)))
                ,
                EffectTimingBattleBeforeEvent.EventSource.Attack =>
                    await (effectTimingBattleBeforeEvent.CardCondition?.IsMatch(effectOwnerCard, eventArgs.BattleContext.AttackCard, eventArgs)
                        ?? ValueTask.FromResult(false)),
                EffectTimingBattleBeforeEvent.EventSource.Guard =>
                    await (effectTimingBattleBeforeEvent.CardCondition?.IsMatch(effectOwnerCard, eventArgs.BattleContext.GuardCard, eventArgs)
                        ?? ValueTask.FromResult(false)),
                _ => false
            };

            return playerMatch || cardMatch;
        }

        public static async ValueTask<bool> IsMatch(this EffectTimingDamageBeforeEvent effectTimingDamageBeforeEvent, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            var playerMatch = effectTimingDamageBeforeEvent.PlayerIsMatch(effectOwnerCard, eventArgs);

            var cardMatch = await effectTimingDamageBeforeEvent.CardIsMatch(effectOwnerCard, eventArgs);

            return playerMatch || cardMatch;
        }

        private static bool PlayerIsMatch(this EffectTimingDamageBeforeEvent effectTimingDamageBeforeEvent, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            return effectTimingDamageBeforeEvent.Source switch
            {
                EffectTimingDamageBeforeEvent.EventSource.All => eventArgs.DamageContext.GuardPlayer != null
                    && (effectTimingDamageBeforeEvent.PlayerCondition?.IsMatch(effectOwnerCard, eventArgs.DamageContext.GuardPlayer, eventArgs) ?? false),
                EffectTimingDamageBeforeEvent.EventSource.Guard => eventArgs.DamageContext.GuardPlayer != null
                    && (effectTimingDamageBeforeEvent.PlayerCondition?.IsMatch(effectOwnerCard, eventArgs.DamageContext.GuardPlayer, eventArgs) ?? false),
                _ => false
            };
        }

        private static async ValueTask<bool> CardIsMatch(this EffectTimingDamageBeforeEvent effectTimingDamageBeforeEvent, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            var damageSource = eventArgs.DamageContext.DamageSourceCard;
            var guard = eventArgs.DamageContext.GuardCard;

            return effectTimingDamageBeforeEvent.Source switch
            {
                EffectTimingDamageBeforeEvent.EventSource.All =>
                    (damageSource != null
                        && (await (effectTimingDamageBeforeEvent.CardCondition?.IsMatch(effectOwnerCard, damageSource, eventArgs)
                            ?? ValueTask.FromResult(false))))
                    || (guard != null
                        && (await (effectTimingDamageBeforeEvent.CardCondition?.IsMatch(effectOwnerCard, guard, eventArgs)
                            ?? ValueTask.FromResult(false))))
                ,
                EffectTimingDamageBeforeEvent.EventSource.DamageSource => damageSource != null
                    && (await (effectTimingDamageBeforeEvent.CardCondition?.IsMatch(effectOwnerCard, damageSource, eventArgs)
                        ?? ValueTask.FromResult(false))),
                EffectTimingDamageBeforeEvent.EventSource.Guard => guard != null
                    && (await (effectTimingDamageBeforeEvent.CardCondition?.IsMatch(effectOwnerCard, guard, eventArgs)
                        ?? ValueTask.FromResult(false))),
                _ => false
            };
        }

        public static bool IsMatch(this EffectTimingDestroyEvent effectTimingDestroyEvent, Card ownerCard, Card source)
        {
            return effectTimingDestroyEvent.Source switch
            {
                EffectTimingDestroyEvent.EventSource.This => ownerCard.Id == source.Id,
                EffectTimingDestroyEvent.EventSource.Other => ownerCard.Id != source.Id,
                _ => throw new InvalidOperationException()
            };
        }

        public static bool IsMatch(this EffectTimingEndTurnEvent effectTimingEndTurnEvent, PlayerId turnPlayerId, Card ownerCard)
        {
            return effectTimingEndTurnEvent.Source switch
            {
                EffectTimingEndTurnEvent.EventSource.Both => true,
                EffectTimingEndTurnEvent.EventSource.You => turnPlayerId == ownerCard.OwnerId,
                EffectTimingEndTurnEvent.EventSource.Opponent => turnPlayerId != ownerCard.OwnerId,
                _ => throw new InvalidOperationException($"{nameof(effectTimingEndTurnEvent.Source)}={effectTimingEndTurnEvent.Source}"),
            };
        }

        public static bool IsMatch(this EffectTimingModifyPlayerEvent effectTimingModifyPlayerEvent, PlayerId modifyPlayerId, Card ownerCard)
        {
            return effectTimingModifyPlayerEvent.Source switch
            {
                EffectTimingModifyPlayerEvent.EventSource.All => true,
                EffectTimingModifyPlayerEvent.EventSource.Owner => modifyPlayerId == ownerCard.OwnerId,
                EffectTimingModifyPlayerEvent.EventSource.Other => modifyPlayerId != ownerCard.OwnerId,
                _ => throw new InvalidOperationException($"{nameof(effectTimingModifyPlayerEvent.Source)}={effectTimingModifyPlayerEvent.Source}"),
            };
        }

        public static bool IsMatch(this EffectTimingMoveCardEvent effectTimingMoveCardEvent, Card ownerCard, EffectEventArgs args)
        {
            var matchSource = effectTimingMoveCardEvent.Source switch
            {
                EffectTimingMoveCardEvent.EventSource.This => ownerCard.Id == args.SourceCard.Id,
                EffectTimingMoveCardEvent.EventSource.Other => ownerCard.Id != args.SourceCard.Id,
                _ => throw new InvalidOperationException()
            };

            var opponentId = args.GameMaster.GetOpponent(ownerCard.OwnerId).Id;

            return matchSource
                && MessageObjectExtensions.FromPrettyName(ownerCard.OwnerId, opponentId, effectTimingMoveCardEvent.From) == args.MoveCardContext.From
                && MessageObjectExtensions.FromPrettyName(ownerCard.OwnerId, opponentId, effectTimingMoveCardEvent.To) == args.MoveCardContext.To;
        }

        public static bool IsMatch(this EffectTimingPlayEvent effectTimingPlayEvent, Card ownerCard, Card source)
        {
            return effectTimingPlayEvent.Source switch
            {
                EffectTimingPlayEvent.EventSource.This => ownerCard.Id == source.Id,
                EffectTimingPlayEvent.EventSource.Other => ownerCard.Id != source.Id,
                _ => throw new InvalidOperationException()
            };
        }

        public static bool IsMatch(this EffectTimingStartTurnEvent effectTimingStartTurnEvent, PlayerId turnPlayerId, Card ownerCard)
        {
            return effectTimingStartTurnEvent.Source switch
            {
                EffectTimingStartTurnEvent.EventSource.Both => true,
                EffectTimingStartTurnEvent.EventSource.You => turnPlayerId == ownerCard.OwnerId,
                EffectTimingStartTurnEvent.EventSource.Opponent => turnPlayerId != ownerCard.OwnerId,
                _ => throw new InvalidOperationException($"{nameof(effectTimingStartTurnEvent.Source)}={effectTimingStartTurnEvent.Source}"),
            };
        }

        public static async ValueTask<bool> IsMatch(this EffectWhen effectWhen, Card effectOwnerCard, EffectEventArgs eventArgs)
            => await effectWhen.Timing.IsMatch(effectOwnerCard, eventArgs);

        private static readonly Dictionary<EffectWhile, int> effectWhileCounter = new();

        public static async ValueTask<bool> IsMatch(this EffectWhile effectWhile, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            if (!effectWhileCounter.ContainsKey(effectWhile))
            {
                effectWhileCounter.Add(effectWhile, 0);
            }

            if (await effectWhile.Timing.IsMatch(effectOwnerCard, eventArgs))
            {
                effectWhileCounter[effectWhile]++;
            }

            return (effectWhile.Skip == 0 || effectWhileCounter[effectWhile] > effectWhile.Skip)
                && effectWhileCounter[effectWhile] <= effectWhile.Take + effectWhile.Skip;
        }

        public static bool IsMatch(this NumCondition numCondition, int checkValue)
        {
            var result = numCondition.Compare switch
            {
                NumCondition.ConditionCompare.Equality => checkValue == numCondition.Value,
                NumCondition.ConditionCompare.LessThan => checkValue <= numCondition.Value,
                NumCondition.ConditionCompare.GreaterThan => checkValue >= numCondition.Value,
                _ => throw new InvalidOperationException($"{nameof(numCondition.Compare)}: {numCondition.Compare}")
            };

            return result ^ numCondition.Not;
        }


        public static bool IsMatch(this PlayerCondition playerCondition, Card effectOwnerCard, Player player, EffectEventArgs eventArgs)
        {
            return
                playerCondition.Context switch
                {
                    PlayerCondition.PlayerConditionContext.EventSource => player.Id == eventArgs.SourcePlayer.Id,
                    _ => true
                }
                && playerCondition.Type switch
                {
                    PlayerCondition.PlayerConditionType.You => player.Id == effectOwnerCard.OwnerId,
                    PlayerCondition.PlayerConditionType.Opponent => player.Id != effectOwnerCard.OwnerId,
                    PlayerCondition.PlayerConditionType.Active => player.Id == eventArgs.GameMaster.ActivePlayer.Id,
                    PlayerCondition.PlayerConditionType.NonActive => player.Id != eventArgs.GameMaster.ActivePlayer.Id,
                    _ => true
                };
        }

        public static async ValueTask<bool> IsMatch(this TextCondition textCondition, Card effectOwnerCard, EffectEventArgs effectEventArgs, string checkValue)
        {
            var value = await textCondition.Value.Calculate(effectOwnerCard, effectEventArgs);

            var result = textCondition.Compare switch
            {
                TextCondition.ConditionCompare.Equality => checkValue == value,
                TextCondition.ConditionCompare.Like => checkValue.Contains(value),
                _ => throw new InvalidOperationException($"不正な入力値です: {textCondition.Compare}")
            };

            return textCondition.Not ? !result : result;
        }

        public static async ValueTask<bool> IsMatch(this ZoneCondition zoneCondition, Card effectOwnerCard, EffectEventArgs effectEventArgs, Zone checkValue)
        {
            var zones = await zoneCondition.Value.Calculate(effectOwnerCard, effectEventArgs);
            var opponentId = effectEventArgs.GameMaster.GetOpponent(effectOwnerCard.OwnerId).Id;
            var isMatch = zones.Select(z => MessageObjectExtensions.FromPrettyName(effectOwnerCard.OwnerId, opponentId, z))
                .Any(z => z == checkValue);

            return isMatch ^ zoneCondition.Not;
        }

        public static void Damage(this Card card, int damage)
        {
            card.ToughnessBuff -= Math.Max(0, damage);
        }

        public static CardDef Creature(int cost, string name, string flavorText, int power, int toughness,
            int? numTurnsToCanAttack = null, int? numAttacksInTurn = null, bool isToken = false,
            IEnumerable<CreatureAbility> abilities = null, IEnumerable<CardEffect> effects = null)
        {
            return new CardDef()
            {
                BaseCost = cost,
                Type = CardType.Creature,
                Name = name,
                FlavorText = flavorText,
                BasePower = power,
                BaseToughness = toughness,
                NumTurnsToCanAttack = numTurnsToCanAttack,
                NumAttacksLimitInTurn = numAttacksInTurn,
                IsToken = isToken,
                Abilities = abilities?.ToList() ?? new List<CreatureAbility>(),
                Effects = effects?.ToArray() ?? Array.Empty<CardEffect>()
            };
        }

        public static CardDef Artifact(int cost, string name, string flavorText, bool isToken = false, IEnumerable<CardEffect> effects = null)
        {
            return new CardDef()
            {
                BaseCost = cost,
                IsToken = isToken,
                Type = CardType.Artifact,
                Name = name,
                FlavorText = flavorText,
                Effects = effects?.ToArray() ?? Array.Empty<CardEffect>()
            };
        }

        public static CardDef Sorcery(int cost, string name, string flavorText, IEnumerable<CardEffect> effects = null)
        {
            return new CardDef()
            {
                BaseCost = cost,
                Type = CardType.Sorcery,
                Name = name,
                FlavorText = flavorText,
                Effects = effects?.ToArray() ?? Array.Empty<CardEffect>()
            };
        }

        public static Zone FromPrettyName(PlayerId cardOwnerId, PlayerId opponentId, ZonePrettyName zonePrettyName)
        {
            return zonePrettyName switch
            {
                ZonePrettyName.CardPool => new Zone(default, ZoneName.CardPool),
                ZonePrettyName.YouHand => new Zone(cardOwnerId, ZoneName.Hand),
                ZonePrettyName.OpponentHand => new Zone(opponentId, ZoneName.Hand),
                ZonePrettyName.YouField => new Zone(cardOwnerId, ZoneName.Field),
                ZonePrettyName.OpponentField => new Zone(opponentId, ZoneName.Field),
                ZonePrettyName.YouCemetery => new Zone(cardOwnerId, ZoneName.Cemetery),
                ZonePrettyName.OpponentCemetery => new Zone(opponentId, ZoneName.Cemetery),
                ZonePrettyName.YouDeck => new Zone(cardOwnerId, ZoneName.Deck),
                ZonePrettyName.OpponentDeck => new Zone(opponentId, ZoneName.Deck),
                _ => throw new InvalidOperationException()
            };
        }

        public static readonly ZoneName[] PublicZoneNames = new[] { ZoneName.Field, ZoneName.Cemetery, ZoneName.CardPool };

        public static bool IsPublic(this Zone zoneMessage) => PublicZoneNames.Contains(zoneMessage.ZoneName);

        public static ZonePrettyName AsZonePrettyName(this Zone zoneMessage, Card card)
        {
            return zoneMessage.ZoneName switch
            {
                ZoneName.CardPool => ZonePrettyName.CardPool,
                ZoneName.Hand => zoneMessage.PlayerId == card.OwnerId ? ZonePrettyName.YouHand : ZonePrettyName.OpponentHand,
                ZoneName.Field => zoneMessage.PlayerId == card.OwnerId ? ZonePrettyName.YouField : ZonePrettyName.OpponentField,
                ZoneName.Cemetery => zoneMessage.PlayerId == card.OwnerId ? ZonePrettyName.YouCemetery : ZonePrettyName.OpponentCemetery,
                ZoneName.Deck => zoneMessage.PlayerId == card.OwnerId ? ZonePrettyName.YouDeck : ZonePrettyName.OpponentDeck,
                _ => throw new InvalidOperationException()
            };
        }

        /// <summary>
        /// テストようにvirtual にしてる
        /// </summary>
        /// <param name="ownerCard"></param>
        /// <param name="effectEventArgs"></param>
        /// <returns></returns>
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectAction effectAction, Card ownerCard, EffectEventArgs effectEventArgs)
        {
            //TODO この順番もけっこう重要
            var actions = new Func<Card, EffectEventArgs, ValueTask<(bool, EffectEventArgs)>?>[]
            {
                (c,e) => effectAction.Damage?.Execute(c,e),
                (c,e) => effectAction.AddCard?.Execute(c,e),
                (c,e) => effectAction.ModifyCard?.Execute(c,e),
                (c,e) => effectAction.DestroyCard?.Execute(c,e),
                (c,e) => effectAction.ModifyDamage?.Execute(c,e),
                (c,e) => effectAction.ModifyPlayer?.Execute(c,e),
                (c,e) => effectAction.DrawCard?.Execute(c,e),
                (c,e) => effectAction.MoveCard?.Execute(c,e),
                (c,e) => effectAction.AddEffect?.Execute(c,e),
                (c,e) => effectAction.EffectActionSetVariable?.Execute(c,e),
            };

            var result = effectEventArgs;
            var done = false;
            foreach (var action in actions)
            {
                var task = action(ownerCard, result);
                if (task == null) continue;

                var (done2, result2) = await task.Value;

                done = done || done2;
                result = result2;
            }

            return (done, result);
        }
    }
}
