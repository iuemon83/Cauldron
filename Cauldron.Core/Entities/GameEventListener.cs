using Cauldron.Shared.MessagePackObjects;
using System;
using System.Threading.Tasks;

namespace Cauldron.Core.Entities
{
    public record GameEventListener(
        Action<PlayerId, GameContext, StartTurnNotifyMessage> OnStartTurn,
        Action<PlayerId, GameContext, PlayCardNotifyMessage> OnPlay,
        Action<PlayerId, GameContext, AddCardNotifyMessage> OnAddCard,
        Action<PlayerId, GameContext, ExcludeCardNotifyMessage> OnExcludeCard,
        Action<PlayerId, GameContext, MoveCardNotifyMessage> OnMoveCard,
        Action<PlayerId, GameContext, ModifyCardNotifyMessage> OnModifyCard,
        Action<PlayerId, GameContext, ModifyPlayerNotifyMessage> OnModifyPlayer,
        Action<PlayerId, GameContext, BattleNotifyMessage> OnBattleStart,
        Action<PlayerId, GameContext, BattleNotifyMessage> OnBattleEnd,
        Action<PlayerId, GameContext, DamageNotifyMessage> OnDamage,
        Action<PlayerId, GameContext, ModifyCounterNotifyMessage> OnModityCounter,
        Action<PlayerId, GameContext> OnEndGame,
        Func<PlayerId, ChoiceCandidates, int, ValueTask<ChoiceAnswer>> AskCardAction
        );
}
