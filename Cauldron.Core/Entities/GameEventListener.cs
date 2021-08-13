using Cauldron.Shared.MessagePackObjects;
using System;
using System.Threading.Tasks;

namespace Cauldron.Core.Entities
{
    public record GameEventListener(
        Action<PlayerId, GameContext> OnStartTurn,
        Action<PlayerId, GameContext, AddCardNotifyMessage> OnAddCard,
        Action<PlayerId, GameContext, ExcludeCardNotifyMessage> OnExcludeCard,
        Action<PlayerId, GameContext, MoveCardNotifyMessage> OnMoveCard,
        Action<PlayerId, GameContext, ModifyCardNotifyMessage> OnModifyCard,
        Action<PlayerId, GameContext, ModifyPlayerNotifyMessage> OnModifyPlayer,
        Action<PlayerId, GameContext, BattleNotifyMessage> OnBattle,
        Action<PlayerId, GameContext, DamageNotifyMessage> OnDamage,
        Action<PlayerId, GameContext> OnEndGame,
        Func<PlayerId, ChoiceCandidates, int, ValueTask<ChoiceAnswer>> AskCardAction
        );
}
