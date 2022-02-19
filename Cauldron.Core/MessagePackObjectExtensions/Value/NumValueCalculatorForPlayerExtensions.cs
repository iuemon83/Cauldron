using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    public static class NumValueCalculatorForPlayerExtensions
    {
        public static async ValueTask<int> Calculate(this NumValueCalculatorForPlayer _this,
            Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            if (_this.Type == default
                || _this.PlayersChoice == default)
            {
                return 0;
            }

            return _this.Type switch
            {
                NumValueCalculatorForPlayer.TypeValue.Count => await CalcCount(_this, effectOwnerCard, effectEventArgs),
                NumValueCalculatorForPlayer.TypeValue.PlayerCurrentHp => await CalcPlayerCurrentHp(_this, effectOwnerCard, effectEventArgs),
                NumValueCalculatorForPlayer.TypeValue.PlayerCurrentMp => await CalcPlayerCurrentMp(_this, effectOwnerCard, effectEventArgs),
                _ => throw new InvalidOperationException($"{nameof(_this.Type)}: {_this.Type}")
            };

            static async ValueTask<int> CalcCount(NumValueCalculatorForPlayer _this,
                Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                var picked = await effectEventArgs.GameMaster
                    .Choice(effectOwnerCard, _this.PlayersChoice, effectEventArgs);
                return picked.PlayerIdList.Length;
            }

            static async ValueTask<int> CalcPlayerCurrentHp(NumValueCalculatorForPlayer _this,
                Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                var picked = await effectEventArgs.GameMaster
                    .Choice(effectOwnerCard, _this.PlayersChoice, effectEventArgs);

                return picked.PlayerIdList
                    .Sum(pid => effectEventArgs.GameMaster.Get(pid)?.CurrentHp ?? 0);
            }

            static async ValueTask<int> CalcPlayerCurrentMp(NumValueCalculatorForPlayer _this,
                Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                var picked = await effectEventArgs.GameMaster
                    .Choice(effectOwnerCard, _this.PlayersChoice, effectEventArgs);

                return picked.PlayerIdList
                    .Sum(pid => effectEventArgs.GameMaster.Get(pid)?.CurrentMp ?? 0);
            }
        }
    }
}
