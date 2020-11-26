using Cauldron.Server.Models.Effect;
using System;
using System.Collections.Generic;

namespace Cauldron.Server.Models
{
    public class CardEffect2
    {
        public EffectTiming Timing { get; set; }

        public IReadOnlyList<IEffectAction> Actions { get; set; }

        public void Execute(GameEvent effectType, GameMaster gameMaster, Card owner, Card eventSource)
        {
            if (!this.Timing.Match(effectType, gameMaster.CurrentPlayer.Id, owner, eventSource)) return;

            foreach (var action in this.Actions)
            {
                action.Execute(gameMaster, owner, eventSource);
            }
        }

        public Action<EffectEventArgs> Execute(Card ownerCard) => args =>
        {
            this.Execute(args.EffectType, args.GameMaster, ownerCard, args.Source);
        };

        public bool MatchTiming(GameEvent gameEvent)
        {
            return gameEvent switch
            {
                GameEvent.OnStartTurn => this.Timing.StartTurn != null,
                GameEvent.OnEndTurn => this.Timing.EndTurn != null,
                GameEvent.OnPlay => this.Timing.Play != null,
                GameEvent.OnDestroy => this.Timing.Destroy != null,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
