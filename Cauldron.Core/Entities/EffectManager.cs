using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Core.Entities
{
    public class EffectManager
    {
        private readonly ILogger logger;

        private readonly Dictionary<GameEvent, List<(CardEffect Effect, Card Owner)>> anyZoneEffectsByGameEvent = new();

        public EffectManager(ILogger logger)
        {
            this.logger = logger;

            foreach (var gameEvent in Enum.GetValues<GameEvent>())
            {
                this.anyZoneEffectsByGameEvent.Add(gameEvent, new());
            }
        }

        public void RegisterEffectIfNeeded(Card owner)
        {
            foreach (var ef in owner.Effects)
            {
                var isAnyZoneEffect = ef.ShouldRegisterEffect();
                if (isAnyZoneEffect)
                {
                    this.RegisterEffectByEvent(ef, owner);
                }
            }
        }

        public void RegisterEffectByEvent(CardEffect cardEffect, Card owner)
        {
            var gameEvent = cardEffect.Condition?.ByNotPlay?.When?.Timing?.ToGameEvent();
            if (gameEvent is { } g)
            {
                this.anyZoneEffectsByGameEvent[g].Add((cardEffect, owner));
            }
        }

        public async ValueTask<EffectEventArgs> DoEffectByPlaying(Card playedCard, EffectEventArgs effectEventArgs)
        {
            // プレイしたときの条件で判断する
            var matchedEffectList = new List<CardEffect>(playedCard.Effects.Count);
            foreach (var ef in playedCard.Effects)
            {
                var isMatched = await ef.IsMatchedByPlaying(playedCard, effectEventArgs);
                if (isMatched)
                {
                    matchedEffectList.Add(ef);
                }
            }

            var newEffectEventArgs = effectEventArgs;
            foreach (var ef in matchedEffectList)
            {
                var (done, args) = await ef.DoActionByPlaying(playedCard, newEffectEventArgs);

                if (done)
                {
                    newEffectEventArgs = args;
                    this.logger.LogInformation($"効果: {effectEventArgs.GameEvent} {playedCard.Name}");
                }
            }

            return newEffectEventArgs;
        }

        public async ValueTask<EffectEventArgs> DoEffect(EffectEventArgs effectEventArgs)
        {
            // カードの効果の発動順は
            // anyZoneEffect → アクティブプレイヤー → 相手プレイヤー
            // 場 → 手札
            static Card[] getCards(Player player) =>
                player.Field.AllCards
                    .Concat(player.Hands.AllCards)
                    .Concat(player.Cemetery.AllCards)
                    .Concat(player.Deck.AllCards)
                    .ToArray();

            var newEffectEventArgs = effectEventArgs;

            // anyZoneEffect
            foreach (var (ev, effectList) in this.anyZoneEffectsByGameEvent)
            {
                foreach (var (ef, owner) in effectList)
                {
                    var (done, args) = await ef.DoIfMatchedAnyZone(owner, newEffectEventArgs);

                    if (done)
                    {
                        newEffectEventArgs = args;
                        this.logger.LogInformation($"効果: {effectEventArgs.GameEvent} {owner.Name}");
                    }
                }
            }

            // アクティブプレイヤー
            var activePlayerCards = getCards(newEffectEventArgs.GameMaster.ActivePlayer);
            foreach (var card in activePlayerCards)
            {
                foreach (var ef in card.Effects)
                {
                    var (done, args) = await ef.DoIfMatched(card, newEffectEventArgs);

                    if (done)
                    {
                        newEffectEventArgs = args;
                        this.logger.LogInformation($"効果: {effectEventArgs.GameEvent} {card.Name}");
                    }
                }
            }

            // 相手プレイヤー
            foreach (var player in newEffectEventArgs.GameMaster.NonActivePlayers)
            {
                var nonActivePlayerCards = getCards(player);
                foreach (var card in nonActivePlayerCards)
                {
                    foreach (var ef in card.Effects)
                    {
                        var (done, args) = await ef.DoIfMatched(card, newEffectEventArgs);

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
