﻿using Cauldron.Core.Entities.Effect;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    public class EffectActionAddEffectExecuter : IEffectActionExecuter
    {
        private readonly EffectActionAddEffect _this;

        public EffectActionAddEffectExecuter(EffectActionAddEffect _this)
        {
            this._this = _this;
        }

        public async ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, CardEffectId effectId, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.Choice(effectOwnerCard, _this.CardsChoice, args);
            var targets = choiceResult.CardList;

            var done = false;
            foreach (var card in targets)
            {
                args.GameMaster.AddEffect(card, _this.EffectToAdd, effectOwnerCard, effectId);
            }

            if (!string.IsNullOrEmpty(_this.Name))
            {
                var context = new ActionContext(AddEffect: new(targets));
                args.GameMaster.SetActionContext(effectOwnerCard.Id, _this.Name, context);
            }

            return (done, args);
        }
    }
}
