using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.Services;
using MagicOnion;
using MagicOnion.Server;
using System;

namespace Cauldron.Server.Services
{
    public class CauldronService : ServiceBase<ICauldronService>, ICauldronService
    {
        public async UnaryResult<GameMasterStatusCode> AnswerChoice(Guid questionId, ChoiceResult answer)
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
