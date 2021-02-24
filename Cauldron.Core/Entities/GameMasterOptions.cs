using Cauldron.Shared.MessagePackObjects;
using Microsoft.Extensions.Logging;

namespace Cauldron.Core.Entities
{
    public record GameMasterOptions(
        RuleBook RuleBook,
        CardRepository CardFactory,
        ILogger Logger,
        //Func<PlayerId, ChoiceResult, int, ChoiceResult> AskCardAction,
        GameEventListener EventListener

        //Action<PlayerId, ReadyGameReply> NotifyClient,
        //Action<PlayerId, GameContext> OnStartTurn,
        //Action<PlayerId, GameContext, AddCardNotifyMessage> OnAddCard,
        //Action<PlayerId, GameContext, MoveCardNotifyMessage> OnMoveCard,
        //Action<PlayerId, GameContext, ModifyCardNotifyMessage> OnModifyCard,
        //Action<PlayerId, GameContext, ModifyPlayerNotifyMessage> OnModifyPlayer,
        //Action<PlayerId, GameContext, DamageNotifyMessage> OnDamage
        )
    {
    }
}
