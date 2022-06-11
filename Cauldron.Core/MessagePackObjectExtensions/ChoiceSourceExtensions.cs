using Cauldron.Core.Entities;
using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ChoiceSourceExtensions
    {
        public static async ValueTask<ChoiceCandidates> ChoiceCandidates(this ChoiceSource choiceSource,
            Card effectOwnerCard, EffectEventArgs effectEventArgs, PlayerRepository playerRepository,
            CardRepository cardRepository, Choice.HowValue choiceHow, int numDuplicates)
        {
            var playerList = (await choiceSource
                .ListMatchedPlayers(effectOwnerCard, effectEventArgs, playerRepository))
                .Select(p => p.Id)
                .ToArray();

            var cardList = await choiceSource.ListMatchedCards(effectOwnerCard, effectEventArgs, cardRepository);

            if (choiceHow == Choice.HowValue.Choose)
            {
                cardList = cardList.Where(c =>
                {
                    var isStealth = c.OwnerId != effectOwnerCard.OwnerId
                        && c.EnableAbility(CreatureAbility.Stealth);

                    return !isStealth;
                });
            }

            var cardDefList = await choiceSource.ListMatchedCardDefs(
                effectOwnerCard, effectEventArgs, cardRepository, numDuplicates);

            return new ChoiceCandidates(
                playerList,
                cardList.ToArray(),
                cardDefList.ToArray()
            );
        }

        public static async ValueTask<IEnumerable<Player>> ListMatchedPlayers(this ChoiceSource choiceSource, Card effectOwnerCard, EffectEventArgs eventArgs, PlayerRepository playerRepository)
        {
            var matchedList = new Dictionary<PlayerId, Player>();
            foreach (var cond in choiceSource.OrPlayerConditions)
            {
                var players = await cond.ListMatchedPlayers(effectOwnerCard, eventArgs, playerRepository);
                foreach (var p in players)
                {
                    // 重複はいらない
                    matchedList.TryAdd(p.Id, p);
                }
            }

            return matchedList.Values;
        }

        public static async ValueTask<IEnumerable<Card>> ListMatchedCards(this ChoiceSource choiceSource,
            Card effectOwnerCard, EffectEventArgs eventArgs, CardRepository cardRepository)
        {
            var mathedCards = new Dictionary<CardId, Card>();
            foreach (var cond in choiceSource.OrCardConditions)
            {
                var cards = await cond.ListMatchedCards(effectOwnerCard, eventArgs, cardRepository);
                foreach (var c in cards)
                {
                    // 重複はいらない
                    mathedCards.TryAdd(c.Id, c);
                }
            }

            return mathedCards.Values
                .OrderBy(x => x.Name);
        }

        public static async ValueTask<IEnumerable<CardDef>> ListMatchedCardDefs(this ChoiceSource choiceSource,
            Card effectOwnerCard, EffectEventArgs eventArgs, CardRepository cardRepository, int numDuplicates)
        {
            var mathedCards = new List<CardDef>();
            foreach (var cond in choiceSource.OrCardDefConditions)
            {
                var carddefs = await cond.ListMatchedCardDefs(effectOwnerCard, eventArgs, cardRepository);
                mathedCards.AddRange(carddefs);
            }

            return mathedCards
                .SelectMany(c => Enumerable.Repeat(c, numDuplicates))
                .OrderBy(x => x.Name);
        }
    }
}
