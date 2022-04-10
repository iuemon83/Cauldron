﻿using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    public class EffectActionDrawCardExecuter : IEffectActionExecuter
    {
        private readonly EffectActionDrawCard _this;

        public EffectActionDrawCardExecuter(EffectActionDrawCard _this)
        {
            this._this = _this;
        }

        public async ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, EffectEventArgs args)
        {
            // 対象のプレイヤー一覧
            // 順序はアクティブプレイヤー優先
            var targetPlayers = args.GameMaster.playerRepository.AllPlayers
                .Where(p => _this.PlayerCondition.IsMatch(effectOwnerCard, args, p))
                .OrderBy(p => p.Id == args.GameMaster.ActivePlayer.Id)
                .ToArray();

            var drawnCards = new List<Card>();
            foreach (var p in targetPlayers)
            {
                var newArgs = args with
                {
                    ActionTargetPlayers = targetPlayers,
                    ActionTargetPlayer = p
                };

                var numCards = await _this.NumCards.Calculate(effectOwnerCard, newArgs);

                var (status, cards) = await args.GameMaster.Draw(p.Id, numCards, effectOwnerCard);
                drawnCards.AddRange(cards);
            }

            if (!string.IsNullOrEmpty(_this.Name))
            {
                var context = new ActionContext(DrawCard: new(drawnCards));
                args.GameMaster.SetActionContext(effectOwnerCard.Id, _this.Name, context);
            }

            return (true, args);
        }
    }
}