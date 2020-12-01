namespace Cauldron.Server.Models.Effect
{
    public interface IEffectAction
    {
        public bool Execute(Card ownerCard, EffectEventArgs effectEventArgs);
    }
}
