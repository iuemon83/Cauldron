using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Server.Models
{
    public class CardEffectChooseActionParser
    {
        public static Func<GameMaster, Card, IEnumerable<Card>> Parse(string[] elms)
        {
            if (!Enum.TryParse<ChooseType>(elms[0], true, out var chooseType))
            {
                throw new InvalidOperationException(elms[0]);
            }

            if (!Enum.TryParse<ZoneType>(elms[1], true, out var zoneType)
               && !new[] { ZoneType.Field, ZoneType.YouField, ZoneType.OpponentField }.Contains(zoneType))
            {
                throw new InvalidOperationException(elms[1]);
            }

            switch (chooseType)
            {
                case ChooseType.Random:
                    return ParseAsRandom(zoneType, elms[2..]);

                case ChooseType.Choose:
                    return ParseAsRandom(zoneType, elms[2..]);

                case ChooseType.All:
                    return ParseAsRandom(zoneType, elms[2..]);

                default:
                    throw new InvalidOperationException();
            }
        }

        public static Func<GameMaster, Card, IEnumerable<Card>> ParseAsRandom(ZoneType zoneType, string[] elms)
        {
            // All 以外の時は選択するカードの数が必要
            if (!int.TryParse(elms[1], out var numChooseCards))
            {
                throw new InvalidOperationException(elms[1]);
            }

            switch (zoneType)
            {
                case ZoneType.Field:
                    {
                        return (gameMaster, ownerCard) =>
                        {
                            var candidates = gameMaster.PlayersById.Values
                                .SelectMany(player => player.Field.AllCards)
                                .Where(c => c.Type == CardType.Creature || c.Type == CardType.Token)
                                .ToArray();

                            return Enumerable.Range(0, numChooseCards)
                                .Select(_ => Program.RandomPick(candidates));
                        };
                    }

                case ZoneType.OpponentField:
                    {
                        return (gameMaster, ownerCard) =>
                        {
                            var opponent = gameMaster.GetOpponent(ownerCard.OwnerId);
                            var candidates = opponent.Field.AllCards
                                .Where(c => c.Type == CardType.Creature || c.Type == CardType.Token)
                                .ToArray();

                            return Enumerable.Range(0, numChooseCards)
                                .Select(_ => Program.RandomPick(candidates));
                        };
                    }

                case ZoneType.YouField:
                    {
                        return (gameMaster, ownerCard) =>
                        {
                            var you = gameMaster.PlayersById[ownerCard.OwnerId];
                            var candidates = you.Field.AllCards
                                .Where(c => c.Type == CardType.Creature || c.Type == CardType.Token)
                                .ToArray();

                            return Enumerable.Range(0, numChooseCards)
                                .Select(_ => Program.RandomPick(candidates));
                        };
                    }

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
