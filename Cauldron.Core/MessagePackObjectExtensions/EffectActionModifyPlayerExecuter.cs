using Cauldron.Core.Entities;
using Cauldron.Core.Entities.Effect;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    public class EffectActionModifyPlayerExecuter : IEffectActionExecuter
    {
        private readonly EffectActionModifyPlayer _this;

        public EffectActionModifyPlayerExecuter(EffectActionModifyPlayer _this)
        {
            this._this = _this;
        }

        public async ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, CardEffectId effectId, EffectEventArgs effectEventArgs)
        {
            var choiceResult = await effectEventArgs.GameMaster.Choice(effectOwnerCard, _this.Choice, effectEventArgs);
            var targets = effectEventArgs.GameMaster.playerRepository.TryList(choiceResult.PlayerIdList).ToArray();

            var done = false;
            foreach (var player in targets)
            {
                var newArgs = effectEventArgs with
                {
                    ActionTargetPlayers = targets,
                    ActionTargetPlayer = player
                };

                var modifyContext = await this.ModifyPlayerContext(player, effectOwnerCard, effectEventArgs);

                await effectEventArgs.GameMaster.ModifyPlayer(modifyContext, effectOwnerCard, effectId, effectEventArgs);

                done = true;
            }

            return (done, effectEventArgs);
        }

        private async ValueTask<ModifyPlayerContext> ModifyPlayerContext(Player targetPlayer, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var diffMaxHp = 0;
            if (_this.PlayerModifier.MaxHp != null)
            {
                diffMaxHp = await _this.PlayerModifier.MaxHp.Modify(effectOwnerCard, effectEventArgs, targetPlayer.MaxHp)
                    - targetPlayer.MaxHp;
            }

            var diffHp = 0;
            if (_this.PlayerModifier.Hp != null)
            {
                diffHp = await _this.PlayerModifier.Hp.Modify(effectOwnerCard, effectEventArgs, targetPlayer.CurrentHp)
                    - targetPlayer.CurrentHp;
            }

            var diffMaxMp = 0;
            if (_this.PlayerModifier.MaxMp != null)
            {
                diffMaxMp = await _this.PlayerModifier.MaxMp.Modify(effectOwnerCard, effectEventArgs, targetPlayer.MaxMp)
                    - targetPlayer.MaxMp;
            }

            var diffMp = 0;
            if (_this.PlayerModifier.Mp != null)
            {
                diffMp = await _this.PlayerModifier.Mp.Modify(effectOwnerCard, effectEventArgs, targetPlayer.CurrentMp)
                    - targetPlayer.CurrentMp;
            }

            return new ModifyPlayerContext(targetPlayer.Id, diffMaxHp, diffHp, diffMaxMp, diffMp);
        }
    }
}
