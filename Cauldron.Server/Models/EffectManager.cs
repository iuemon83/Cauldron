using Cauldron.Server.Models.Effect;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Cauldron.Server.Models
{
    public class EffectManager
    {
        private readonly ILogger logger;

        public EffectManager(ILogger logger)
        {
            this.logger = logger;
        }

        public EffectEventArgs DoEffect(EffectEventArgs effectEventArgs)
        {
            // カードの効果の発動順は
            // アクティブプレイヤー → 相手プレイヤー
            // 場 → 手札
            static Card[] getCards(Player player) =>
                player.Field.AllCards
                    .Concat(player.Hands.AllCards)
                    .Concat(player.Cemetery.AllCards)
                    .ToArray();

            var newEffectEventArgs = effectEventArgs;
            var activePlayerCards = getCards(newEffectEventArgs.GameMaster.ActivePlayer);
            foreach (var card in activePlayerCards)
            {
                foreach (var ef in card.Effects)
                {
                    var (done, args) = ef.DoIfMatched(card, newEffectEventArgs);

                    if (done)
                    {
                        newEffectEventArgs = args;
                        this.logger.LogInformation($"効果: {effectEventArgs.GameEvent} {card.Name}");
                    }
                }
            }

            foreach (var player in newEffectEventArgs.GameMaster.NonActivePlayers)
            {
                var nonActivePlayerCards = getCards(player);
                foreach (var card in nonActivePlayerCards)
                {
                    foreach (var ef in card.Effects)
                    {
                        var (done, args) = ef.DoIfMatched(card, newEffectEventArgs);

                        if (done)
                        {
                            newEffectEventArgs = args;
                            this.logger.LogInformation($"効果: {effectEventArgs.GameEvent} {card.Name}");
                        }
                    }
                }
            }

            return newEffectEventArgs;
        }
    }
}
