﻿using Cauldron.Core.Entities.Effect;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    public class EffectActionDamageExecuter : IEffectActionExecuter
    {
        private readonly EffectActionDamage _this;

        public EffectActionDamageExecuter(EffectActionDamage _this)
        {
            this._this = _this;
        }

        public async ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, CardEffectId effectId, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.Choice(effectOwnerCard, _this.Choice, args);

            var targetPlayers = args.GameMaster.playerRepository.TryList(choiceResult.PlayerIdList).ToArray();

            var done = false;
            foreach (var guardPlayer in targetPlayers)
            {
                var newArgs = args with
                {
                    ActionTargetPlayers = targetPlayers,
                    ActionTargetPlayer = guardPlayer
                };

                var damageValue = await _this.Value.Calculate(effectOwnerCard, newArgs);

                var damageContext = new DamageContext(
                    DamageNotifyMessage.ReasonValue.Effect,
                    effectOwnerCard,
                    Value: damageValue,
                    GuardPlayer: guardPlayer
                    );

                await args.GameMaster.DamagePlayer(damageContext, effectOwnerCard, effectId);

                done = true;
            }

            foreach (var card in choiceResult.CardList)
            {
                var newArgs = args with
                {
                    ActionTargetCards = choiceResult.CardList,
                    ActionTargetCard = card
                };
                var damageValue = await _this.Value.Calculate(effectOwnerCard, newArgs);

                var damageContext = new DamageContext(
                    DamageNotifyMessage.ReasonValue.Effect,
                    effectOwnerCard,
                    Value: damageValue,
                    GuardCard: card
                    );
                await args.GameMaster.DamageCreature(damageContext, effectOwnerCard, effectId);

                done = true;
            }

            if (!string.IsNullOrEmpty(_this.Name))
            {
                var context = new ActionContext(Damage: new(choiceResult.CardList));
                args.GameMaster.SetActionContext(effectOwnerCard.Id, _this.Name, context);
            }

            return (done, args);
        }
    }
}
