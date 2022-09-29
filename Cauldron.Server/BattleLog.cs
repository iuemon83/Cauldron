using Cauldron.Core;
using Cauldron.Shared.MessagePackObjects;
using System;

namespace Cauldron.Server
{
    public record BattleLog(
        GameId GameId,
        PlayerId PlayerId,
        PlayerId WinnerPlayerId,
        NotifyEvent NotifyEvent,
        GameContext GameContext,
        string MessageJson
        )
    {
        public static BattleLog AsStartTurnEvent(GameId GameId, PlayerId playerId, GameContext gameContext, StartTurnNotifyMessage message)
        {
            return new BattleLog(GameId, playerId, new PlayerId(Guid.Empty), NotifyEvent.OnStartTurn, gameContext, JsonConverter.Serialize(message));
        }
        public static BattleLog AsPlayEvent(GameId GameId, PlayerId playerId, GameContext gameContext, PlayCardNotifyMessage message)
        {
            return new BattleLog(GameId, playerId, new PlayerId(Guid.Empty), NotifyEvent.OnPlay, gameContext, JsonConverter.Serialize(message));
        }
        public static BattleLog AsAddCardEvent(GameId GameId, PlayerId playerId, GameContext gameContext, AddCardNotifyMessage message)
        {
            return new BattleLog(GameId, playerId, new PlayerId(Guid.Empty), NotifyEvent.OnAddCard, gameContext, JsonConverter.Serialize(message));
        }
        public static BattleLog AsExcludeCardEvent(GameId GameId, PlayerId playerId, GameContext gameContext, ExcludeCardNotifyMessage message)
        {
            return new BattleLog(GameId, playerId, new PlayerId(Guid.Empty), NotifyEvent.OnExcludeCard, gameContext, JsonConverter.Serialize(message));
        }
        public static BattleLog AsBattleStartEvent(GameId GameId, PlayerId playerId, GameContext gameContext, BattleNotifyMessage message)
        {
            return new BattleLog(GameId, playerId, new PlayerId(Guid.Empty), NotifyEvent.OnBattleStart, gameContext, JsonConverter.Serialize(message));
        }
        public static BattleLog AsBattleEndEvent(GameId GameId, PlayerId playerId, GameContext gameContext, BattleNotifyMessage message)
        {
            return new BattleLog(GameId, playerId, new PlayerId(Guid.Empty), NotifyEvent.OnBattleEnd, gameContext, JsonConverter.Serialize(message));
        }
        public static BattleLog AsDamageEvent(GameId GameId, PlayerId playerId, GameContext gameContext, DamageNotifyMessage message)
        {
            return new BattleLog(GameId, playerId, new PlayerId(Guid.Empty), NotifyEvent.OnDamage, gameContext, JsonConverter.Serialize(message));
        }
        public static BattleLog AsHealEvent(GameId GameId, PlayerId playerId, GameContext gameContext, HealNotifyMessage message)
        {
            return new BattleLog(GameId, playerId, new PlayerId(Guid.Empty), NotifyEvent.OnHeal, gameContext, JsonConverter.Serialize(message));
        }
        public static BattleLog AsModifyCardEvent(GameId GameId, PlayerId playerId, GameContext gameContext, ModifyCardNotifyMessage message)
        {
            return new BattleLog(GameId, playerId, new PlayerId(Guid.Empty), NotifyEvent.OnModifyCard, gameContext, JsonConverter.Serialize(message));
        }
        public static BattleLog AsModifyPlayerEvent(GameId GameId, PlayerId playerId, GameContext gameContext, ModifyPlayerNotifyMessage message)
        {
            return new BattleLog(GameId, playerId, new PlayerId(Guid.Empty), NotifyEvent.OnModifyPlayer, gameContext, JsonConverter.Serialize(message));
        }
        public static BattleLog AsMoveCardEvent(GameId GameId, PlayerId playerId, GameContext gameContext, MoveCardNotifyMessage message)
        {
            return new BattleLog(GameId, playerId, new PlayerId(Guid.Empty), NotifyEvent.OnMoveCard, gameContext, JsonConverter.Serialize(message));
        }
        public static BattleLog AsModityCounterEvent(GameId GameId, PlayerId playerId, GameContext gameContext, ModifyCounterNotifyMessage message)
        {
            return new BattleLog(GameId, playerId, new PlayerId(Guid.Empty), NotifyEvent.OnModityCounter, gameContext, JsonConverter.Serialize(message));
        }
        public static BattleLog AsEndGameEvent(GameId GameId, PlayerId playerId, PlayerId WinnerPlayerId, GameContext gameContext, EndGameNotifyMessage message)
        {
            return new BattleLog(GameId, playerId, WinnerPlayerId, NotifyEvent.OnEndGame, gameContext, JsonConverter.Serialize(message));
        }

        public static BattleLog AsStartGameEvent(GameId GameId)
        {
            return new BattleLog(GameId, new PlayerId(Guid.Empty), new PlayerId(Guid.Empty), NotifyEvent.OnStartGame, default, "");
        }
    }
}
