using Cauldron.Core.Entities.Effect;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Microsoft.Extensions.Logging;

namespace Cauldron.Core.Entities
{
    public class EffectManager
    {
        private readonly ILogger logger;

        private readonly Dictionary<GameEvent, List<(CardEffect Effect, Card Owner)>> reservedEffectsByGameEvent = new();

        private readonly Dictionary<GameEvent, Dictionary<(Card, EffectWhile), int>> effectWhileCounter = new();

        /// <summary>
        /// 発動中の一番最初の効果のID
        /// </summary>
        private CardEffectId TopEffectId = default;

        /// <summary>
        /// このイベント中にすでに発動した効果のID
        /// </summary>
        private readonly List<CardEffectId> usedEffectIdList = new();

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

        public async void FinalyGameEvent(GameEvent gameEvent, EffectEventArgs effectEventArgs)
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
                await this.DoEffectInLoop(ef, playedCard, effectEventArgs, isPlayEvent: true);
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

            static async ValueTask<IReadOnlyList<(Card card, CardEffect effect)>>
                CandidateCards(EffectEventArgs effectEventArgs)
            {
                var candidateCards = getCards(effectEventArgs.GameMaster.ActivePlayer)
                    .Concat(effectEventArgs.GameMaster.NonActivePlayers.SelectMany(p => getCards(p)))
                    .SelectMany(card => card.Effects.Select(e => (card, effect: e)));

                var list = new List<(Card, CardEffect)>();
                foreach (var x in candidateCards)
                {
                    if (await x.effect.IsMatched(x.card, effectEventArgs))
                    {
                        list.Add(x);
                    }
                }

                return list;
            }

            // イベント発火時に発動条件を満たしていないとダメ
            // アクティブプレイヤーの領域→ノンアクティブプレイヤーの領域、のように途中で領域を移動されると、途中で条件を満たすようになる可能性がある
            var candidateCards = await CandidateCards(newEffectEventArgs);

            // anyZoneEffect
            foreach (var (ef, owner) in this.reservedEffectsByGameEvent[newEffectEventArgs.GameEvent])
            {
                newEffectEventArgs = await this.DoEffectInLoop(ef, owner, newEffectEventArgs, isPlayEvent: false);
            }

            // アクティブプレイヤー
            // 相手プレイヤー
            foreach (var (card, effect) in candidateCards)
            {
                newEffectEventArgs = await this.DoEffectInLoop(effect, card, newEffectEventArgs, isPlayEvent: false);
            }

            return newEffectEventArgs;
        }

        private async ValueTask<EffectEventArgs> DoEffectInLoop(
            CardEffect ef, Card owner, EffectEventArgs args, bool isPlayEvent)
        {
            if (this.usedEffectIdList.Contains(ef.Id))
            {
                // すでに発火されている効果をもう一度発火しない
                // ループ対策
                return args;
            }

            if (!isPlayEvent)
            {
                if (!await ef.IsMatched(owner, args))
                {
                    return args;
                }
            }

            if (this.TopEffectId == default)
            {
                this.TopEffectId = ef.Id;
                this.usedEffectIdList.Clear();
            }

            this.usedEffectIdList.Add(ef.Id);

            var (done, newArgs) = await ef.DoAction(owner, args);

            this.usedEffectIdList.Remove(ef.Id);

            if (this.TopEffectId == ef.Id)
            {
                // ルートの効果が処理完了したらぜんぶ削除する。
                this.TopEffectId = default;
                this.usedEffectIdList.Clear();
            }

            if (!done)
            {
                // ほぼありえないはず？
                // choiceの対象がいないとか？

                return args;
            }

            logger.LogInformation("効果: {GameEvent} {Name}", args.GameEvent, owner.Name);
            await args.GameMaster.DestroyDeadCards();

            return newArgs;
        }
    }
}
