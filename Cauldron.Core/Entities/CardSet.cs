using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.Entities
{
    public record CardSet(string Name, CardDef[] Cards);
}
