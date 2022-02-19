using Cauldron.Core.Entities.Effect;
using Cauldron.Shared;
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

        private readonly Dictionary<GameEvent, List<(CardEffect Effect, Card Owner)>> reservedEffectsByGameEvent = new();

        private readonly Dictionary<GameEvent, Dictionary<(Card, EffectWhile), int>> effectWhileCounter = new();

        public EffectManager(ILogger logger)
        {
            this.logger = logger;

            foreach (var gameEvent in Enum.GetValues<GameEvent>())
            {
                this.reservedEffectsByGameEvent.Add(gameEvent, new());
                this.effectWhileCounter.Add(gameEvent, new());
            }
        }

        public bool IsMatchedWhile(EffectWhile effectWhile, Card owner)
        {
            var gameEvent = effectWhile.Timing?.ToGameEvent();
            if (gameEvent is { } g)
            {
                var key = (owner, effectWhile);
                if (!this.effectWhileCounter[g].ContainsKey(key))
                {
                    return false;
                }

                return (effectWhile.Skip == 0 || this.effectWhileCounter[g][key] >= effectWhile.Skip)
                    && this.effectWhileCounter[g][key] < effectWhile.Take + effectWhile.Skip;
            }

            return false;
        }

        public async void EndGameEvent(GameEvent gameEvent, EffectEventArgs effectEventArgs)
        {
            foreach (var key in this.effectWhileCounter[gameEvent].Keys)
            {
                var (o, w) = key;
                if (await w.Timing.IsMatch(o, effectEventArgs))
                {
                    this.effectWhileCounter[gameEvent][key]++;
                }
            }
        }

        public void OnMoveCard(Card owner)
        {
            this.RegisterOrRemoveEffectWhile(owner);
        }

        /// <summary>
        /// 効果が発動する領域に移動したらwhileを登録する
        /// </summary>
        /// <param name="owner"></param>
        private void RegisterOrRemoveEffectWhile(Card owner)
        {
            foreach (var ef in owner.Effects)
            {
                this.RegisterOrRemoveEffectWhile(ef, owner);
            }
        }

        public void RegisterOrRemoveEffectWhile(CardEffect cardEffect, Card owner)
        {
            var condition = cardEffect.Condition.ByNotPlay
                ?? (EffectCondition?)cardEffect.Condition.Reserve;
            if (condition == null)
            {
                return;
            }

            var w = condition.While;
            var whileGameEvent = w?.Timing?.ToGameEvent();

            if (w != null && whileGameEvent is GameEvent g)
            {
                var key = (owner, w);
                if (condition.Zone == ZonePrettyName.Any
                    || condition.Zone == owner.Zone.AsZonePrettyName(owner))
                {
                    this.effectWhileCounter[g].TryAdd(key, 0);
                }
                else
                {
                    this.effectWhileCounter[g].Remove(key);
                }
            }
        }

        public void ReserveAnyZoneEffect(CardEffect cardEffect, Card owner)
        {
            var reserveCondition = cardEffect.Condition?.Reserve;
            if (reserveCondition == null)
            {
                return;
            }

            var whenGameEvent = reserveCondition.When?.Timing?.ToGameEvent();
            if (whenGameEvent is GameEvent g)
            {
                this.reservedEffectsByGameEvent[g].Add((cardEffect, owner));
            }

            this.RegisterOrRemoveEffectWhile(cardEffect, owner);
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

                    await effectEventArgs.GameMaster.DestroyDeadCards();
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
            foreach (var (ef, owner) in this.reservedEffectsByGameEvent[effectEventArgs.GameEvent])
            {
                var (done, args) = await ef.DoReservedEffectIfMatched(owner, newEffectEventArgs);

                if (done)
                {
                    newEffectEventArgs = args;
                    this.logger.LogInformation($"効果: {effectEventArgs.GameEvent} {owner.Name}");

                    await effectEventArgs.GameMaster.DestroyDeadCards();
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

                        await effectEventArgs.GameMaster.DestroyDeadCards();
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

                            await effectEventArgs.GameMaster.DestroyDeadCards();
                        }
                    }
                }
            }

            return newEffectEventArgs;
        }
    }
}
