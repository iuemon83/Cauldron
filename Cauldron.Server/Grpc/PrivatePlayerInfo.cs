using System.Linq;

namespace Cauldron.Grpc.Models
{
    public partial class PrivatePlayerInfo
    {
        public PrivatePlayerInfo(Server.Models.PrivatePlayerInfo source)
        {
            this.PublicPlayerInfo = new PublicPlayerInfo(source.PublicPlayerInfo);

            this.Hands.Clear();
            this.Hands.AddRange(source.Hands.AllCards.Select(card => new Card(card)));
        }
    }
}
