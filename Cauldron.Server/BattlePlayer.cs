using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Server
{
    public record BattlePlayer(
        GameId GameId,
        PlayerId PlayerId,
        string Name,
        string[] CardNamesInDeck,
        int PlayOrder,
        string Ip
        );
}
