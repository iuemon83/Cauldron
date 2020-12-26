using System;
using System.Linq;

namespace Cauldron.Grpc.Models
{
    public partial class Card
    {
        public Card(Server.Models.Card source)
        {
            this.Id = source.Id.ToString();
            this.OwnerId = source.OwnerId.ToString();
            this.Power = source.Power;
            this.Toughness = source.Toughness;
            this.Abilities.AddRange(source.Abilities.Select(ability => (CardDef.Types.Ability)ability));
            this.Name = source.Name;
            this.FlavorText = source.FlavorText;
            this.Cost = source.Cost;
            this.CardType = Enum.Parse<CardDef.Types.Type>(source.Type.ToString());
            this.TurnNumInField = source.NumTurnsInField;
            //this.EffectText = source.Effects;
        }

        //public Server.Models.Card ToServerModel()
        //{
        //    var cardDef = this.CardDef.ToServerModel();

        //    return new Server.Models.Card(cardDef)
        //    {
        //        Abilitiy = Enum.Parse<CreatureAbility>(this.Ability.ToString()),
        //        FlavorText = this.FlavorText,
        //        Name = this.Name,
        //        Type = Enum.Parse<CardType>(this.CardType.ToString()),
        //        CostBuff = this.Cost - cardDef.BaseCost,
        //        OwnerId = Guid.Parse(this.OwnerId),
        //        PowerBuff = this.Power - cardDef.BasePower,
        //        ToughnessBuff = this.Toughness - cardDef.BaseToughness,

        //    };
        //}
    }
}
