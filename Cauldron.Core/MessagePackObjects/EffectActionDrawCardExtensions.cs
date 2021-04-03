using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionDrawCardExtensions
    {
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
    }
}
