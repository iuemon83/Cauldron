using Cauldron.Core.Entities;
using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class PlayerConditionExtensions
    {
        public static async ValueTask<Player[]> ListMatchedPlayers(this PlayerCondition _this, Card effectOwnerCard, EffectEventArgs eventArgs, PlayerRepository playerRepository)
        {
            var matched = new List<Player>();

            foreach (var p in playerRepository.AllPlayers)
            {
                if (await _this.IsMatch(effectOwnerCard, eventArgs, p))
                {
                    matched.Add(p);
                }
            }

            return matched.ToArray();
        }

        public static async ValueTask<bool> IsMatch(this PlayerCondition _this, Card effectOwnerCard, EffectEventArgs eventArgs, Player playerToMatch)
        {
            return
                _this.Context switch
                {
                    PlayerCondition.ContextValue.EventSource => playerToMatch.Id == eventArgs.SourcePlayer?.Id,
                    PlayerCondition.ContextValue.You => playerToMatch.Id == effectOwnerCard.OwnerId,
                    PlayerCondition.ContextValue.Opponent => playerToMatch.Id != effectOwnerCard.OwnerId,
                    PlayerCondition.ContextValue.Active => playerToMatch.Id == eventArgs.GameMaster.ActivePlayer.Id,
                    PlayerCondition.ContextValue.NonActive => playerToMatch.Id != eventArgs.GameMaster.ActivePlayer.Id,
                    PlayerCondition.ContextValue.ActionTarget => playerToMatch.Id == eventArgs.ActionTargetPlayer?.Id,
                    PlayerCondition.ContextValue.ActionTargetAll => eventArgs.ActionTargetPlayers.Select(x => x.Id).Contains(playerToMatch.Id),
                    _ => true
                }
                && (_this.IsFirst == null || _this.IsFirst == playerToMatch.IsFirst)
                && (_this.MaxHpCondition == null || await _this.MaxHpCondition.IsMatch(playerToMatch.MaxHp, effectOwnerCard, eventArgs))
                && (_this.CurrentHpCondition == null || await _this.CurrentHpCondition.IsMatch(playerToMatch.CurrentHp, effectOwnerCard, eventArgs))
                && (_this.MaxMpCondition == null || await _this.MaxMpCondition.IsMatch(playerToMatch.MaxMp, effectOwnerCard, eventArgs))
                && (_this.CurrentMpCondition == null || await _this.CurrentMpCondition.IsMatch(playerToMatch.CurrentMp, effectOwnerCard, eventArgs))
                ;
        }
    }
}
