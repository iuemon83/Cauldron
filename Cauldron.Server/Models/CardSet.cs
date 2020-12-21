using System.Collections.Generic;

namespace Cauldron.Server.Models
{
    public class CardSet
    {
        public string Name { get; set; }

        public CardDefJson[] Cards { get; set; }

        public IEnumerable<CardDef> AsCardDefs()
        {
            return System.Array.Empty<CardDef>();

            //return this.Cards.Select(c =>
            //{
            //    return c.Type switch
            //    {
            //        CardType.Creature => CardDef.CreatureCard(
            //            c.Cost, $"{this.Name}.{c.Name}", c.Name,
            //            c.FlavorText, c.Power, c.Toughness, c.Abilities,
            //            CardEffectParser.Parse(c.EffectText)),

            //        CardType.Artifact => CardDef.ArtifactCard(
            //            c.Cost, $"{this.Name}.{c.Name}", c.Name,
            //            c.FlavorText,
            //            CardEffectParser.Parse(c.EffectText)),

            //        _ => null
            //    };
            //});
        }
    }
}
