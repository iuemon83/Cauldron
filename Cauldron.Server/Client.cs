using Cauldron.Core.Entities;
using System.Net;

namespace Cauldron.Server
{
    public record Client(
        string Id,
        IPAddress Ip,
        PlayerDef PlayerDef
        );
}
