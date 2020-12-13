namespace Cauldron.Server.Models.Effect
{
    public interface IEffectAction
    {
        public (bool, EffectEventArgs) Execute(Card ownerCard, EffectEventArgs effectEventArgs);
    }
}
