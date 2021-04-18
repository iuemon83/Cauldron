using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.Services;
using MagicOnion;
using MagicOnion.Server;
using System;

namespace Cauldron.Server.Services
{
    public class CauldronService : ServiceBase<ICauldronService>, ICauldronService
    {
#pragma warning disable CS1998 // 非同期メソッドは、'await' 演算子がないため、同期的に実行されます
        public async UnaryResult<GameMasterStatusCode> AnswerChoice(Guid questionId, ChoiceResult answer)
#pragma warning restore CS1998 // 非同期メソッドは、'await' 演算子がないため、同期的に実行されます
        {
            if (answer == null)
            {
                return GameMasterStatusCode.InvalidQuestionId;
            }

            return QuestionManager.SetAnswer(questionId, answer)
                ? GameMasterStatusCode.OK
                : GameMasterStatusCode.InvalidQuestionId;
        }
    }
}
