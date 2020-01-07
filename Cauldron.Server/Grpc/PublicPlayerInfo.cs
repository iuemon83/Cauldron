using System.Linq;

namespace Cauldron.Grpc.Models
{
    public partial class PublicPlayerInfo
    {
        public PublicPlayerInfo(Server.Models.PublicPlayerInfo serverModel)
        {
            this.DeckCount = serverModel.DeckCount;
            this.Field.AddRange(serverModel.Field.AllCards.Select(card => new Card(card)));
            this.Hp = serverModel.Hp;
            this.Id = serverModel.Id.ToString();
            this.MaxMp = serverModel.MaxMp;
            this.Name = serverModel.Name;
            this.UsedMp = serverModel.UsedMp;
        }

        //public Server.Models.PublicPlayerInfo ToServerModel()
        //{
        //    return new Server.Models.PublicPlayerInfo(
        //        id: Guid.Parse(this.Id),
        //        name: this.Name,
        //        field: null,
        //        deckCount: this.DeckCount,
        //        hp: this.Hp,
        //        maxMp: this.MaxMp,
        //        usedMp: this.UsedMp
        //        );
        //}
    }
}
