using System;

namespace Cauldron.Core
{
    public class CardEffect
    {
        private Action<GameMaster, Card> Action { get; }

        public CardEffectType Type { get; }

        public CardEffect(CardEffectType type, Action<GameMaster, Card> action)
        {
            this.Type = type;
            this.Action = action;
        }

        public void Execute(GameMaster gameMaster, Card owner) => this.Action(gameMaster, owner);
    }
}
