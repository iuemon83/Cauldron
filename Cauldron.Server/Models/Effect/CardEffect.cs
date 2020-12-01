using Cauldron.Server.Models.Effect;
using System.Collections.Generic;

namespace Cauldron.Server.Models
{
    public class CardEffect
    {
        public EffectTiming Timing { get; set; }

        public IReadOnlyList<IEffectAction> Actions { get; set; }

        public bool Execute(Card ownerCard, EffectEventArgs args)
        {
            if (!this.Timing.Match(args.EffectType, args.GameMaster.ActivePlayer.Id, ownerCard, args)) return false;

            var done = false;
            foreach (var action in this.Actions)
            {
                done = action.Execute(ownerCard, args) || done;
            }

            return done;
        }

        public bool MatchTiming(GameEvent gameEvent) => this.Timing.Match(gameEvent);
    }
}
