using Cauldron.Shared.MessagePackObjects;
using MagicOnion;
using System;

namespace Cauldron.Shared.Services
{
    public interface ICauldronService : IService<ICauldronService>
    {
        UnaryResult<GameMasterStatusCode> AnswerChoice(Guid questionId, ChoiceResult choiceResult);
    }
}
