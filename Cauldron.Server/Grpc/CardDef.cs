using System;
using System.Linq;

namespace Cauldron.Grpc.Models
{
    public partial class CardDef
    {
        public CardDef(Server.Models.CardDef source)
        {
            this.Id = source.Id.ToString();
            this.Power = source.BasePower;
            this.Toughness = source.BaseToughness;
            this.Abilities.AddRange(source.Abilities.Select(ability => (Types.Ability)ability));
            this.Name = source.Name;
            this.FlavorText = source.FlavorText;
            this.Cost = source.BaseCost;
            this.CardType = Enum.Parse<Types.Type>(source.Type.ToString());
            //this.EffectText = source.Effects;
        }

        //public Server.Models.CardDef ToServerModel()
        //{
        //    return new Server.Models.CardDef()
        //    {
        //        Abilitiy = Enum.Parse<CreatureAbility>(this.Ability.ToString()),
        //        BaseCost = this.Cost,
        //        BasePower = this.Power,
        //        BaseToughness = this.Toughness,
        //        //Effects = ,
        //        FlavorText = this.FlavorText,
        //        Name = this.Name,
        //        //Require = ,
        //        Type = Enum.Parse<CardType>(this.CardType.ToString()),
        //    };
        //}
    }
}
