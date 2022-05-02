using Cauldron.Core.Entities.Effect;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    public class EffectActionMoveCardExecuter : IEffectActionExecuter
    {
        private readonly EffectActionMoveCard _this;

        public EffectActionMoveCardExecuter(EffectActionMoveCard _this)
        {
            this._this = _this;
        }

        public async ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, CardEffectId effectId, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.Choice(effectOwnerCard, _this.CardsChoice, args);
            var targets = choiceResult.CardList;

            var done = false;
            var opponentId = args.GameMaster.GetOpponent(effectOwnerCard.OwnerId).Id;
            foreach (var card in targets)
            {
                var (success, toZone) = _this.To.TryGetZone(
                    effectOwnerCard.OwnerId,
                    opponentId,
                    card.OwnerId);
                if (success)
                {
                    var moveContext = new MoveCardContext(card.Zone, toZone, _this.InsertCardPosition);
                    await args.GameMaster.MoveCard(card.Id, moveContext, effectOwnerCard, effectId);
                    done = true;
                }
            }

            if (!string.IsNullOrEmpty(_this.Name))
            {
                var context = new ActionContext(MoveCard: new(targets));
                args.GameMaster.SetActionContext(effectOwnerCard.Id, _this.Name, context);
            }

            return (done, args);
        }
    }
}
