using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionDrawCardExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionDrawCard _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            // 対象のプレイヤー一覧
            // 順序はアクティブプレイヤー優先
            var targetPlayers = args.GameMaster.playerRepository.AllPlayers
                .Where(p => _this.PlayerCondition.IsMatch(effectOwnerCard, args, p))
                .OrderBy(p => p.Id == args.GameMaster.ActivePlayer.Id);

            var numCards = await _this.NumCards.Calculate(effectOwnerCard, args);

            var drawnCards = new List<Card>();
            foreach (var p in targetPlayers)
            {
                var (status, cards) = await args.GameMaster.Draw(p.Id, numCards);
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
