﻿using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using System;
using System.Threading.Tasks;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    public class EffectActionModifyCounterExecuter : IEffectActionExecuter
    {
        private readonly EffectActionModifyCounter _this;

        public EffectActionModifyCounterExecuter(EffectActionModifyCounter _this)
        {
            this._this = _this;
        }

        public async ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, EffectEventArgs args)
        {
            if (_this.NumCountersModifier == null)
            {
                return (true, args);
            }

            var targetCards = await args.GameMaster.Choice(effectOwnerCard, _this.TargetsChoice, args);

            var totalNumBeforeCounters = 0;
            var totalNumAfterCounters = 0;

            foreach (var pid in targetCards.PlayerIdList)
            {
                var beforeNumCounters = 0;
                totalNumBeforeCounters += beforeNumCounters;
                var modifyNum = await _this.NumCountersModifier.Modify(effectOwnerCard, args, beforeNumCounters)
                    - beforeNumCounters;
                await args.GameMaster.ModifyCounter(pid, _this.CounterName, modifyNum);

                totalNumAfterCounters += modifyNum;
            }

            foreach (var c in targetCards.CardList)
            {
                var beforeNumCounters = c.GetCounter(_this.CounterName);
                totalNumBeforeCounters += beforeNumCounters;
                var modifyNum = await _this.NumCountersModifier.Modify(effectOwnerCard, args, beforeNumCounters)
                    - beforeNumCounters;
                await args.GameMaster.ModifyCounter(c, _this.CounterName, modifyNum);

                totalNumAfterCounters += modifyNum;
            }

            if (!string.IsNullOrEmpty(_this.Name))
            {
                var context = new ActionContext(ModifyCounter: new(
                    targetCards.CardList,
                    Math.Max(0, totalNumBeforeCounters),
                    Math.Max(0, totalNumAfterCounters)
                    ));
                args.GameMaster.SetActionContext(effectOwnerCard.Id, _this.Name, context);
            }

            return (true, args);
        }
    }
}
