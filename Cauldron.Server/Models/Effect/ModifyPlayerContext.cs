using System;

namespace Cauldron.Server.Models.Effect
{
    public record ModifyPlayerContext(Guid PlayerId, PlayerModifier PlayerModifier)
    {
    }
}
