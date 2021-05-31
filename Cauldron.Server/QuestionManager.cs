using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Concurrent;

namespace Cauldron.Server
{
    public class QuestionManager
    {
        private static readonly ConcurrentDictionary<Guid, byte> QuestionIdList = new();

        private static readonly ConcurrentDictionary<Guid, ChoiceAnswer> Answers = new();

        public static Guid AddNewQuestion()
        {
            var newId = Guid.NewGuid();
            QuestionIdList.TryAdd(newId, default);

            return newId;
        }

        public static bool SetAnswer(Guid questionId, ChoiceAnswer answer)
        {
            if (answer == null)
            {
                return false;
            }

            if (QuestionIdList.TryRemove(questionId, out _))
            {
                Answers.TryAdd(questionId, answer);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool TryGetAnswer(Guid questionId, out ChoiceAnswer answer)
        {
            if (Answers.TryGetValue(questionId, out var result))
            {
                Answers.TryRemove(questionId, out _);
                answer = result;
                return true;
            }
            else
            {
                answer = default;
                return false;
            }
        }
    }
}
