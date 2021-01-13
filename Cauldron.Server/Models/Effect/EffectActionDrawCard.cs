using Cauldron.Server.Models.Effect.Value;
using System.Linq;

namespace Cauldron.Server.Models.Effect
{
    public record EffectActionDrawCard(NumValue NumCards, PlayerCondition PlayerCondition) : IEffectAction
    {
        public (bool, EffectEventArgs) Execute(Card effectOwnerCard, EffectEventArgs args)
        {
            // 対象のプレイヤー一覧
            // 順序はアクティブプレイヤー優先
            var targetPlayers = args.GameMaster.PlayersById.Values
                .Where(p => this.PlayerCondition.IsMatch(effectOwnerCard, p, args))
                .OrderBy(p => p.Id == args.GameMaster.ActivePlayer.Id);

            var numCards = this.NumCards.Calculate(effectOwnerCard, args);

            foreach (var p in targetPlayers)
            {

                args.GameMaster.Draw(p.Id, numCards);
            }

            return (true, args);
        }
    }
}
