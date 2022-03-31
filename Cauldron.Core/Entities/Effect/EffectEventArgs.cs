using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.Entities.Effect
{
    public record EffectEventArgs(
        GameEvent GameEvent,
        GameMaster GameMaster,
        Player? SourcePlayer = null,
        Card? SourceCard = null,
        Player? ActionTargetPlayer = null,
        IReadOnlyList<Player> ActionTargetPlayers = null!,
        Card? ActionTargetCard = null,
        IReadOnlyList<Card> ActionTargetCards = null!,
        BattleContext? BattleContext = null,
        DamageContext? DamageContext = null,
        MoveCardContext? MoveCardContext = null,
        ModifyCounterContext? ModifyCounterContext = null,
        ExcludeCardContext? ExcludeCardContext = null
        )
    {
        public IReadOnlyList<Player> ActionTargetPlayers { get; init; } = ActionTargetPlayers ?? Array.Empty<Player>();
        public IReadOnlyList<Card> ActionTargetCards { get; init; } = ActionTargetCards ?? Array.Empty<Card>();
    }
}
