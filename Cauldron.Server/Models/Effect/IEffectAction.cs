using System;

namespace Cauldron.Server.Models.Effect
{
    public interface IEffectAction
    {
        public Action<EffectEventArgs> Execute(Card ownerCard);

        public void Execute(GameMaster gameMaster, Card ownerCard, Card eventSource);
    }
}
