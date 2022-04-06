using Cauldron.Core.Entities;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Cauldron.Core_Test
{
    class TestUtil
    {
        public static RuleBook TestRuleBook => new(
            InitialPlayerHp: 10, MaxPlayerHp: 99, MinPlayerHp: 0,
            MaxNumDeckCards: 99, MinNumDeckCards: 1,
            InitialNumHands: 5, MaxNumHands: 10,
            InitialMp: 1, MaxLimitMp: 10, MinMp: 1, LimitMpToIncrease: 1,
            MaxNumFieldCards: 5,
            DefaultNumTurnsToCanAttack: 0,
            DefaultNumAttacksLimitInTurn: 1
            );

        public static CardDef CardDef(string name)
        {
            var t = Shared.MessagePackObjects.CardDef.Empty;
            t.Name = name;

            return t;
        }

        public static GameMasterOptions GameMasterOptions(
            RuleBook ruleBook = null,
            CardRepository cardRepository = null,
            ILogger Logger = null,
            GameEventListener EventListener = null
            ) => new(
                ruleBook ?? TestUtil.TestRuleBook,
                cardRepository ?? new CardRepository(ruleBook ?? TestUtil.TestRuleBook),
                Logger ?? new TestLogger(),
                EventListener ?? TestUtil.GameEventListener()
                );

        public static GameEventListener GameEventListener(
            Action<PlayerId, GameContext, StartTurnNotifyMessage> OnStartTurn = null,
            Action<PlayerId, GameContext, PlayCardNotifyMessage> OnPlayCard = null,
            Action<PlayerId, GameContext, AddCardNotifyMessage> OnAddCard = null,
            Action<PlayerId, GameContext, ExcludeCardNotifyMessage> OnExcludeCard = null,
            Action<PlayerId, GameContext, MoveCardNotifyMessage> OnMoveCard = null,
            Action<PlayerId, GameContext, ModifyCardNotifyMessage> OnModifyCard = null,
            Action<PlayerId, GameContext, ModifyPlayerNotifyMessage> OnModifyPlayer = null,
            Action<PlayerId, GameContext, DamageNotifyMessage> OnDamage = null,
            Action<PlayerId, GameContext, BattleNotifyMessage> OnBattleStart = null,
            Action<PlayerId, GameContext, BattleNotifyMessage> OnBattleEnd = null,
            Action<PlayerId, GameContext, ModifyCounterNotifyMessage> OnModityCounter = null,
            Action<PlayerId, GameContext, EndGameNotifyMessage> OnEndGame = null,
            Func<PlayerId, ChoiceCandidates, int, ValueTask<ChoiceAnswer>> AskCardAction = null
            ) => new(
                OnStartTurn, OnPlayCard, OnAddCard, OnExcludeCard, OnMoveCard, OnModifyCard, OnModifyPlayer,
                OnBattleStart, OnBattleEnd, OnDamage, OnModityCounter, OnEndGame, AskCardAction
                );

        public static (TestAnswer, Func<PlayerId, ChoiceCandidates, int, ValueTask<ChoiceAnswer>>) TestAskCardAction()
        {
            var testAnswer = new TestAnswer();

            var func = (PlayerId p, ChoiceCandidates c, int i) =>
            {
                if (testAnswer.ExpectedPlayerIdList != null)
                {
                    AssertCollection(testAnswer.ExpectedPlayerIdList, c.PlayerIdList);
                }
                if (testAnswer.ExpectedCardIdList != null)
                {
                    AssertCollection(testAnswer.ExpectedCardIdList, c.CardList.Select(c => c.Id).ToArray());
                }
                if (testAnswer.ExpectedCardDefIdList != null)
                {
                    AssertCollection(testAnswer.ExpectedCardDefIdList, c.CardDefList.Select(c => c.Id).ToArray());
                }

                return ValueTask.FromResult(new ChoiceAnswer(
                    testAnswer.ChoicePlayerIdList,
                    testAnswer.ChoiceCardIdList,
                    testAnswer.ChoiceCardDefIdList));
            };

            return (testAnswer, func);
        }

        /// <summary>
        /// 自分に1ダメージ
        /// </summary>
        public static EffectAction TestEffectAction => new(
            Damage: new(
                new NumValue(1),
                new Choice(new ChoiceSource(
                    orPlayerConditions: new[]{
                        new PlayerCondition(PlayerCondition.ContextValue.You)
                    }))));

        public static void AssertGameAction(Func<(bool, string)> phase)
        {
            var (isSucceeded, errorMessage) = phase();
            Assert.True(isSucceeded, errorMessage);
        }

        public static async ValueTask AssertGameAction(Func<ValueTask<GameMasterStatusCode>> phase)
        {
            var statusCode = await phase();
            Assert.Equal(GameMasterStatusCode.OK, statusCode);
        }

        public static void AssertCollection<T>(IReadOnlyList<T> expected, IReadOnlyList<T> actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

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
            var actualResult = new ChoiceResult(
                actual.PlayerIdList,
                actual.CardList,
                actual.CardDefList
                );

            TestUtil.AssertChoiceResult(expected, actualResult,
                expected.PlayerIdList.Length
                + expected.CardList.Length);
        }

        public static void AssertChoiceResult(ChoiceCandidates candidatesExpected, ChoiceResult actual, int numOfAny)
        {
            // ぜんぶ候補に含まれている
            Assert.True(actual.PlayerIdList.All(a => candidatesExpected.PlayerIdList.Contains(a)));
            Assert.True(actual.CardDefList.All(a => candidatesExpected.CardDefList.Select(ec => ec.Id).Contains(a.Id)));
            Assert.True(actual.CardList.All(a => candidatesExpected.CardList.Select(ec => ec.Id).Contains(a.Id)));

            // 選ばれた数が正しい
            var actualChoiceCount = actual.PlayerIdList.Length
                + actual.CardDefList.Length
                + actual.CardList.Length;
            Assert.Equal(numOfAny, actualChoiceCount);
        }

        public static void AssertChoiceResult(ChoiceResult candidatesExpected, ChoiceResult actual)
        {
            // ぜんぶ候補に含まれている
            Assert.True(actual.PlayerIdList.All(a => candidatesExpected.PlayerIdList.Contains(a)));
            Assert.True(actual.CardDefList.All(a => candidatesExpected.CardDefList.Select(ec => ec.Id).Contains(a.Id)));
            Assert.True(actual.CardList.All(a => candidatesExpected.CardList.Select(ec => ec.Id).Contains(a.Id)));

            // 選ばれた数が正しい
            var actualChoiceCount = actual.PlayerIdList.Length
                + actual.CardDefList.Length
                + actual.CardList.Length;
        }

        public static async ValueTask<Card> NewCardAndPlayFromHand(GameMaster testGameMaster, PlayerId playerId, CardDefId cardDefId)
        {
            var newCard = await testGameMaster.GenerateNewCard(cardDefId, new Zone(playerId, ZoneName.Hand), null, default);
            if (newCard == null)
            {
                throw new Exception("カードの生成に失敗");
            }

            await TestUtil.AssertGameAction(async () => await testGameMaster.PlayFromHand(playerId, newCard.Id));

            return newCard;
        }

        public static async ValueTask Turn(GameMaster gameMaster, Action<GameMaster, PlayerId> turnAction)
        {
            await gameMaster.StartTurn();
            turnAction(gameMaster, gameMaster.ActivePlayer.Id);
            await gameMaster.EndTurn();
        }

        public static async ValueTask Turn(GameMaster gameMaster, Func<GameMaster, PlayerId, ValueTask> turnAction)
        {
            await gameMaster.StartTurn();
            await turnAction(gameMaster, gameMaster.ActivePlayer.Id);
            await gameMaster.EndTurn();
        }

        public static async ValueTask<T> Turn<T>(GameMaster gameMaster, Func<GameMaster, PlayerId, ValueTask<T>> turnAction)
        {
            await gameMaster.StartTurn();
            var cards = await turnAction(gameMaster, gameMaster.ActivePlayer.Id);
            await gameMaster.EndTurn();

            return cards;
        }

        public record TestContext(GameMaster GameMaster, Player Player1, Player Player2, CardRepository CardRepository, TestAnswer TestAnswer);

        public static async ValueTask<TestContext> InitTest(IEnumerable<CardDef> cardpool, ITestOutputHelper output = null)
            => await InitTest(cardpool, Enumerable.Repeat(cardpool.First(c => !c.IsToken), 40), TestUtil.GameMasterOptions(), output);

        public static async ValueTask<TestContext> InitTest(IEnumerable<CardDef> cardpool, IEnumerable<CardDef> deck, ITestOutputHelper output = null)
            => await InitTest(cardpool, deck, TestUtil.GameMasterOptions(), output);

        public static async ValueTask<TestContext> InitTest(IEnumerable<CardDef> cardpool, GameMasterOptions options, ITestOutputHelper output = null)
            => await InitTest(cardpool, Enumerable.Repeat(cardpool.First(c => !c.IsToken), 40), options, output);

        public static async ValueTask<TestContext> InitTest(IEnumerable<CardDef> cardpool, IEnumerable<CardDef> deck, GameMasterOptions options, ITestOutputHelper output)
        {
            options.CardRepository.SetCardPool(new CardPool(new[] { new CardSet(SampleCards2.CardsetName, cardpool.ToArray()) }));

            TestAnswer testAnswer = null;
            if (options.EventListener.AskCardAction == null)
            {
                var x = TestAskCardAction();

                testAnswer = x.Item1;
                options = options with { EventListener = options.EventListener with { AskCardAction = x.Item2 } };
            }

            options = options with { Logger = new TestLogger(output) };

            var testGameMaster = new GameMaster(options);
            var deckCardDefIdList = deck.Select(c => c.Id);

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", deckCardDefIdList);
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", deckCardDefIdList);

            await testGameMaster.StartGame(player1Id);

            var (_, player1) = testGameMaster.playerRepository.TryGet(player1Id);
            var (_, player2) = testGameMaster.playerRepository.TryGet(player2Id);

            return new(testGameMaster, player1, player2, options.CardRepository, testAnswer);
        }
    }
}
