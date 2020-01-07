using System.Linq;

namespace Cauldron.Grpc.Models
{
    public partial class PrivatePlayerInfo
    {
        public PrivatePlayerInfo(Server.Models.PrivatePlayerInfo source)
        {
            this.PublicPlayerInfo = new PublicPlayerInfo(source);

            this.Hands.Clear();
            this.Hands.AddRange(source.Hands.AllCards.Select(card => new Card(card)));
        }

        //public Server.Models.PrivatePlayerInfo ToServerModel()
        //{
        //    var publicPlayerInfo = this.PublicPlayerInfo.ToServerModel();

        //    var hands = new Server.Models.Hands();
        //    foreach (var card in this.Hands.Select(card => card.ToServerModel()))
        //    {
        //        hands.Add(card);
        //    }

        //    return new Server.Models.PrivatePlayerInfo(publicPlayerInfo, hands);
        //}
    }
}
