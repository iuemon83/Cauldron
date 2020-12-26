using System.Linq;

namespace Cauldron.Grpc.Models
{
    public partial class PublicPlayerInfo
    {
        public PublicPlayerInfo(Server.Models.PublicPlayerInfo serverModel)
        {
            this.Id = serverModel.Id.ToString();
            this.Name = serverModel.Name;
            this.NumDeckCards = serverModel.DeckCount;
            this.Cemetery.AddRange(serverModel.Cemetery.AllCards.Select(card => new Card(card)));
            this.NumHands = serverModel.HandsCount;
            this.Field.AddRange(serverModel.Field.AllCards.Select(card => new Card(card)));
            this.MaxHp = serverModel.MaxHp;
            this.CurrentHp = serverModel.CurrentHp;
            this.MaxMp = serverModel.MaxMp;
            this.CurrentMp = serverModel.CurrentMp;
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
