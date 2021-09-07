﻿using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionAddCardExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionAddCard _this,
            Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var (exists, owner) = effectEventArgs.GameMaster.playerRepository.TryGet(effectOwnerCard.OwnerId);
            if (!exists)
            {
                return (false, effectEventArgs);
            }

            var opponent = effectEventArgs.GameMaster.GetOpponent(effectOwnerCard.OwnerId);

            var zonePrettyNames = await _this.ZoneToAddCard.Calculate(effectOwnerCard, effectEventArgs);

            if (!zonePrettyNames.Any())
            {
                return (false, effectEventArgs);
            }

            var (success, defaultZone) = zonePrettyNames[0].TryGetZone(owner.Id, opponent.Id, owner.Id);
            if (!success)
            {
                return (false, effectEventArgs);
            }

            var choiceResult = await effectEventArgs.GameMaster
                .Choice(effectOwnerCard, _this.Choice, effectEventArgs);

            var cardDefAndZones = choiceResult.CardList
                .Select(c => (
                    effectEventArgs.GameMaster.TryGet(c.CardDefId),
                    zonePrettyNames[0].TryGetZone(owner.Id, opponent.Id, c.OwnerId)))
                .Where(x => x.Item1.Exists && x.Item2.Success)
                .Select(x => (x.Item1.CardDef, x.Item2.Zone))
                .Concat(choiceResult.CardDefList.Select(cd => (CardDef: cd, Zone: defaultZone)));

            var addedCards = new List<Card>();
            foreach (var (cardDef, zone) in cardDefAndZones)
            {
                foreach (var cd in Enumerable.Repeat(cardDef, _this.NumOfAddCards))
                {
                    var card = await effectEventArgs.GameMaster.GenerateNewCard(cd.Id, zone, _this.InsertCardPosition);
                    addedCards.Add(card);
                }
            }

            if (!string.IsNullOrEmpty(_this.Name))
            {
                var context = new ActionContext(AddCard: new(addedCards));
                effectEventArgs.GameMaster.SetActionContext(effectOwnerCard.Id, _this.Name, context);
            }

            return (true, effectEventArgs);
        }
    }
}
