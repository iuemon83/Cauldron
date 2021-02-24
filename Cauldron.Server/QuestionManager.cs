using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;

namespace Cauldron.Server
{
    public class QuestionManager
    {
        private static HashSet<Guid> QuestionIdList = new();

        private static Dictionary<Guid, ChoiceResult> Answers = new();

        public static Guid AddNewQuestion()
        {
            var newId = Guid.NewGuid();
            QuestionIdList.Add(newId);

            return newId;
        }

        public static bool SetAnswer(Guid questionId, ChoiceResult answer)
        {
            if (QuestionIdList.Remove(questionId))
            {
                Answers.Add(questionId, answer);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool TryGetAnswer(Guid questionId, out ChoiceResult answer)
        {
            if (Answers.TryGetValue(questionId, out var result))
            {
                Answers.Remove(questionId);
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
