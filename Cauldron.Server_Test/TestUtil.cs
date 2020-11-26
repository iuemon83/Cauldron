﻿using Cauldron.Server.Models;
using Cauldron.Server.Models.Effect;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Cauldron.Server_Test
{
    class TestUtil
    {
        public static void AssertPhase(Func<(bool, string)> phase)
        {
            var (isSucceeded, errorMessage) = phase();
            Assert.True(isSucceeded, errorMessage);
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

        public static void AssertChoiceResult(ChoiceResult expected, ChoiceResult actual)
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

            TestUtil.AssertChoiceResult(expected, actual,
                expected.PlayerIdList.Count
                + expected.CardDefList.Count
                + expected.CardList.Count);
        }

        public static void AssertChoiceResult(ChoiceResult candidatesExpected, ChoiceResult actual, int numOfAny)
        {
            // ぜんぶ候補に含まれている
            Assert.True(actual.PlayerIdList.All(a => candidatesExpected.PlayerIdList.Contains(a)));
            Assert.True(actual.CardDefList.All(a => candidatesExpected.CardDefList.Select(ec => ec.Id).Contains(a.Id)));
            Assert.True(actual.CardList.All(a => candidatesExpected.CardList.Select(ec => ec.Id).Contains(a.Id)));

            // 選ばれた数が正しい
            var actualChoiceCount = actual.PlayerIdList.Count
                + actual.CardDefList.Count
                + actual.CardList.Count;
            Assert.Equal(numOfAny, actualChoiceCount);
        }

        public static Card NewCardAndPlayFromHand(GameMaster testGameMaster, Guid playerId, Guid cardDefId)
        {
            var newCard = testGameMaster.GenerateNewCard(cardDefId, playerId);
            testGameMaster.AddHand(testGameMaster.CurrentPlayer, newCard);
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(playerId, newCard.Id));

            return newCard;
        }

        public static void Turn(GameMaster gameMaster, Action<GameMaster, Guid> turnAction)
        {
            gameMaster.StartTurn(gameMaster.CurrentPlayer.Id);
            turnAction(gameMaster, gameMaster.CurrentPlayer.Id);
            gameMaster.EndTurn(gameMaster.CurrentPlayer.Id);
        }

        public static T Turn<T>(GameMaster gameMaster, Func<GameMaster, Guid, T> turnAction)
        {
            gameMaster.StartTurn(gameMaster.CurrentPlayer.Id);
            var cards = turnAction(gameMaster, gameMaster.CurrentPlayer.Id);
            gameMaster.EndTurn(gameMaster.CurrentPlayer.Id);

            return cards;
        }
    }
}
