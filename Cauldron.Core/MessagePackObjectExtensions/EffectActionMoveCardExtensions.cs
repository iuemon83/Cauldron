﻿using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionMoveCardExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionMoveCard effectActionMoveCard, Card effectOwnerCard, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.ChoiceCards(effectOwnerCard, effectActionMoveCard.CardsChoice, args);
            var targets = choiceResult.CardList;

            var done = false;
            foreach (var card in targets)
            {
                var (success, toZone) = effectActionMoveCard.To.TryGetZone(effectOwnerCard.OwnerId, args.GameMaster.GetOpponent(effectOwnerCard.OwnerId).Id);
                if (success)
                {
                    var moveContext = new MoveCardContext(card.Zone, toZone, effectActionMoveCard.InsertCardPosition);
                    await args.GameMaster.MoveCard(card.Id, moveContext);
                    done = true;
                }
            }

            if (!string.IsNullOrEmpty(effectActionMoveCard.Name))
            {
                var context = new ActionContext(ActionMoveCardContext: new(targets));
                args.GameMaster.SetActionContext(effectOwnerCard.Id, effectActionMoveCard.Name, context);
            }

            return (done, args);
        }
    }
}
