using System;

namespace Cauldron.Core
{
    public class CardRequireToPlay
    {
        private readonly Func<GameEnvironment, bool> isPlayableAction;

        public CardRequireToPlay()
        {
            this.isPlayableAction = _ => true;
        }

        public CardRequireToPlay(Func<GameEnvironment, bool> action)
        {
            this.isPlayableAction = action;
        }

        public bool IsPlayable(GameEnvironment environment)
        {
            return this.isPlayableAction(environment);
        }
    }
}
