using Cauldron.Core.Entities;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cauldron.Core_Test
{
    class TestUtil
    {
        public static RuleBook TestRuleBook => new RuleBook(10, 99, 0, 40, 40, 5, 10, 1, 10, 1, 1, 5, 0, 1);

        public static GameMasterOptions GameMasterOptions(
            RuleBook ruleBook = null,
            CardRepository cardRepository = null,
            ILogger Logger = null,
            GameEventListener EventListener = null
            ) => new GameMasterOptions(
                ruleBook ?? TestUtil.TestRuleBook,
                cardRepository ?? new CardRepository(ruleBook ?? TestUtil.TestRuleBook),
                Logger ?? new TestLogger(),
                EventListener ?? TestUtil.GameEventListener()
                );

        public static GameEventListener GameEventListener(
            Action<PlayerId, GameContext> OnStartTurn = null,
            Action<PlayerId, GameContext, AddCardNotifyMessage> OnAddCard = null,
            Action<PlayerId, GameContext, MoveCardNotifyMessage> OnMoveCard = null,
            Action<PlayerId, GameContext, ModifyCardNotifyMessage> OnModifyCard = null,
            Action<PlayerId, GameContext, ModifyPlayerNotifyMessage> OnModifyPlayer = null,
            Action<PlayerId, GameContext, DamageNotifyMessage> OnDamage = null,
            Func<PlayerId, ChoiceCandidates, int, ValueTask<ChoiceResult>> AskCardAction = null
            ) => new GameEventListener(
                OnStartTurn, OnAddCard, OnMoveCard, OnModifyCard, OnModifyPlayer,
                OnDamage, AskCardAction
                );

        public static EffectAction TestEffectAction => new EffectAction(
            Damage: new(
                1,
                new Choice()
                {
                    How = Choice.ChoiceHow.All,
                    PlayerCondition = new(Type: PlayerCondition.PlayerConditionType.You)
                }));

        public static void AssertGameAction(Func<(bool, string)> phase)
        {
            var (isSucceeded, errorMessage) = phase();
            Assert.True(isSucceeded, errorMessage);
        }

        public static async ValueTask AssertGameAction(Func<ValueTask<GameMasterStatusCode>> phase)
        {
            var statusCode = await phase();
            Assert.True(statusCode == GameMasterStatusCode.OK, statusCode.ToString());
        }

        public static void AssertCollection<T>(IReadOnlyList<T> expected, IReadOnlyList<T> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            var orderedExpected = expected.OrderBy(x => x);
            var orderedActual = actual.OrderBy(x => x);

            foreach (var (e, a) in orderedExpected.Zip(orderedActual))
            {
                Assert.Equal(e, a);
            }
        }

        public static void AssertChoiceResult(ChoiceCandidates expected, ChoiceCandidates actual)
        {
            //TestUtil.AssertCollection(
            //    expected.PlayerIdList,
            //    actual.PlayerIdList);

            //TestUtil.AssertCollection(
            //    expected.CardDefList.Select(c => c.Id).ToArray(),
            //    actual.CardDefList.Select(c => c.Id).ToArray());

            //TestUtil.AssertCollection(
            //    expected.CardList.Select(c => c.Id).ToArray(),
            //    actual.CardList.Select(c => c.Id).ToArray());

            var actualResult = new ChoiceResult2(
                actual.PlayerIdList,
                actual.CardList,
                actual.CardDefList
                );

            TestUtil.AssertChoiceResult(expected, actualResult,
                expected.PlayerIdList.Length
                //+ expected.CardDefList.Count
                + expected.CardList.Length);
        }

        public static void AssertChoiceResult(ChoiceCandidates candidatesExpected, ChoiceResult2 actual, int numOfAny)
        {
            // ぜんぶ候補に含まれている
            Assert.True(actual.PlayerIdList.All(a => candidatesExpected.PlayerIdList.Contains(a)));
            //Assert.True(actual.CardDefList.All(a => candidatesExpected.CardDefList.Select(ec => ec.Id).Contains(a.Id)));
            Assert.True(actual.CardList.All(a => candidatesExpected.CardList.Select(ec => ec.Id).Contains(a.Id)));

            // 選ばれた数が正しい
            var actualChoiceCount = actual.PlayerIdList.Length
                //+ actual.CardDefList.Count
                + actual.CardList.Length;
            Assert.Equal(numOfAny, actualChoiceCount);
        }

        public static void AssertChoiceResult(ChoiceResult2 candidatesExpected, ChoiceResult2 actual)
        {
            // ぜんぶ候補に含まれている
            Assert.True(actual.PlayerIdList.All(a => candidatesExpected.PlayerIdList.Contains(a)));
            //Assert.True(actual.CardDefList.All(a => candidatesExpected.CardDefList.Select(ec => ec.Id).Contains(a.Id)));
            Assert.True(actual.CardList.All(a => candidatesExpected.CardList.Select(ec => ec.Id).Contains(a.Id)));

            // 選ばれた数が正しい
            var actualChoiceCount = actual.PlayerIdList.Length
                //+ actual.CardDefList.Count
                + actual.CardList.Length;
        }

        public static async ValueTask<Card> NewCardAndPlayFromHand(GameMaster testGameMaster, PlayerId playerId, CardDefId cardDefId)
        {
            var newCard = testGameMaster.GenerateNewCard(cardDefId, new Zone(playerId, ZoneName.Hand));
            //testGameMaster.AddHand(testGameMaster.ActivePlayer, newCard);
            await TestUtil.AssertGameAction(async () => await testGameMaster.PlayFromHand(playerId, newCard.Id));

            return newCard;
        }

        public static async ValueTask Turn(GameMaster gameMaster, Action<GameMaster, PlayerId> turnAction)
        {
            await gameMaster.StartTurn();
            turnAction(gameMaster, gameMaster.ActivePlayer.Id);
            await gameMaster.EndTurn();
        }

        public static async ValueTask<T> Turn<T>(GameMaster gameMaster, Func<GameMaster, PlayerId, ValueTask<T>> turnAction)
        {
            await gameMaster.StartTurn();
            var cards = await turnAction(gameMaster, gameMaster.ActivePlayer.Id);
            await gameMaster.EndTurn();

            return cards;
        }

        public static ValueTask<ChoiceResult> TestPick(PlayerId _, ChoiceCandidates choiceCandidates, int __)
        {
            return ValueTask.FromResult(new ChoiceResult(
                choiceCandidates.PlayerIdList,
                choiceCandidates.CardList.Select(c => c.Id).ToArray(),
                choiceCandidates.CardDefList.Select(c => c.Id).ToArray()));
        }
    }
}
