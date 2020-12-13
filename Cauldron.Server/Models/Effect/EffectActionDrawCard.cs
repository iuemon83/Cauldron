using System.Linq;

namespace Cauldron.Server.Models.Effect
{
    public record EffectActionDrawCard(int NumCards, PlayerCondition PlayerCondition) : IEffectAction
    {
        public (bool, EffectEventArgs) Execute(Card effectOwnerCard, EffectEventArgs args)
        {
            // 対象のプレイヤー一覧
            // 順序はアクティブプレイヤー優先
            var targetPlayers = args.GameMaster.PlayersById.Values
                .Where(p => this.PlayerCondition.IsMatch(effectOwnerCard, p, args))
                .OrderBy(p => p.Id == args.GameMaster.ActivePlayer.Id);

            foreach (var p in targetPlayers)
            {

                args.GameMaster.Draw(p.Id, this.NumCards);
            }

            return (true, args);
        }
    }
}
