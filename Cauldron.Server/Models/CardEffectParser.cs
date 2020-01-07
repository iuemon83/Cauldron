using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Server.Models
{
    public class CardEffectParser
    {
        enum CardEffectActionType
        {
            Summon,
            Damage,
            Add,
            Buff,
        }

        enum DamageTargetType
        {
            Player,
            Creature,
            PlayerAndCreature
        }

        //enum ChooseType
        //{
        //    Self,
        //    That,
        //    Random,
        //    Choose,
        //    All,
        //    Specify
        //}

        enum TargetPlayerType
        {
            You,
            Opponent,
            TurnOwner
        }

        public static IEnumerable<CardEffect> Parse(string effectText)
        {
            if (string.IsNullOrWhiteSpace(effectText))
            {
                return new CardEffect[0];
            }

            return effectText.Split(';')
                .Select(line => ParseLine(line));
        }

        public static CardEffect ParseLine(string effectTextLine)
        {
            var elms = effectTextLine.Split(' ');
            if (!Enum.TryParse<CardEffectType>(elms[0], true, out var effectType))
            {
                throw new InvalidOperationException(elms[0]);
            }

            switch (effectType)
            {
                case CardEffectType.OnPlay:
                case CardEffectType.OnDestroy:
                case CardEffectType.OnEveryPlay:
                case CardEffectType.OnEveryDestroy:
                    {
                        var action = ParseAction(elms[1..]);
                        return new CardEffect(effectType, action);
                    }

                default:
                    throw new InvalidOperationException(elms[0]);
            }
        }

        public static Action<GameMaster, Card> ParseAction(string[] elms)
        {
            if (!Enum.TryParse<CardEffectActionType>(elms[0], true, out var effectActionType))
            {
                throw new InvalidOperationException(elms[0]);
            }

            switch (effectActionType)
            {
                case CardEffectActionType.Summon:
                    {
                        var creatureFullName = elms[1];

                        return (gameMaster, ownerCard) =>
                        {
                            var owner = gameMaster.PlayersById[ownerCard.OwnerId];
                            var playingCard = gameMaster.GenerateNewCard(creatureFullName, owner.Id);
                            gameMaster.PlayDirect(ownerCard.OwnerId, playingCard.Id);
                        };
                    }

                case CardEffectActionType.Damage:
                    {
                        return ParseDamageAction(elms[1..]);
                    }

                case CardEffectActionType.Add:
                    {
                        return ParseAddAction(elms[1..]);
                    }

                case CardEffectActionType.Buff:
                    {
                        return ParseBuffAction(elms[1..]);
                    }

                default:
                    throw new InvalidOperationException(elms[0]);
            }
        }

        public static Action<GameMaster, Card> ParseDamageAction(string[] elms)
        {
            if (!Enum.TryParse<DamageTargetType>(elms[0], true, out var damageTargetType))
            {
                throw new InvalidOperationException(elms[0]);
            }

            if (!Enum.TryParse<ChooseType>(elms[1], true, out var chooseType))
            {
                throw new InvalidOperationException(elms[1]);
            }

            if (!int.TryParse(elms[3], out var damageValue))
            {
                throw new InvalidOperationException(elms[3]);
            }

            switch (damageTargetType)
            {
                case DamageTargetType.Creature:
                    {
                        switch (chooseType)
                        {
                            case ChooseType.Random:
                                {
                                    if (!Enum.TryParse<ZoneType>(elms[2], true, out var zoneType))
                                    {
                                        throw new InvalidOperationException(elms[2]);
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

                                                    var targetCreature = Program.RandomPick(candidates);
                                                    gameMaster.HitCreature(ownerCard.OwnerId, targetCreature.Id, damageValue);
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

                                                    var targetCreature = Program.RandomPick(candidates);
                                                    gameMaster.HitCreature(ownerCard.OwnerId, targetCreature.Id, damageValue);
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

                                                    var targetCreature = Program.RandomPick(candidates);
                                                    gameMaster.HitCreature(ownerCard.OwnerId, targetCreature.Id, damageValue);
                                                };
                                            }

                                        default:
                                            throw new InvalidOperationException();
                                    }
                                }

                            default:
                                throw new InvalidOperationException();
                        }
                    }

                case DamageTargetType.Player:
                    {
                        switch (chooseType)
                        {
                            case ChooseType.Specify:
                                {
                                    if (!Enum.TryParse<TargetPlayerType>(elms[2], true, out var targetPlayerType))
                                    {
                                        throw new InvalidOperationException(elms[2]);
                                    }

                                    switch (targetPlayerType)
                                    {
                                        case TargetPlayerType.Opponent:
                                            {
                                                return (gameMaster, ownerCard) =>
                                                {
                                                    var opponent = gameMaster.GetOpponent(ownerCard.OwnerId);
                                                    gameMaster.HitPlayer(opponent.Id, damageValue);
                                                };
                                            }

                                        case TargetPlayerType.You:
                                            {
                                                return (gameMaster, ownerCard) =>
                                                {
                                                    var you = gameMaster.PlayersById[ownerCard.OwnerId];
                                                    gameMaster.HitPlayer(you.Id, damageValue);
                                                };
                                            }

                                        case TargetPlayerType.TurnOwner:
                                            {
                                                return (gameMaster, ownerCard) =>
                                                {
                                                    var turnPlayer = gameMaster.CurrentPlayer;
                                                    gameMaster.HitPlayer(turnPlayer.Id, damageValue);
                                                };
                                            }

                                        default:
                                            throw new InvalidOperationException();
                                    }
                                }

                            default:
                                throw new InvalidOperationException();
                        }
                    }

                default:
                    throw new InvalidOperationException();
            }
        }

        enum CardEffectAddActionType
        {
            YouHand,
            OpponentHand,
            YouDeck,
            OpponentDeck,
            YouCemetery,
            OpponentCemetery,
        }

        public static Action<GameMaster, Card> ParseAddAction(string[] elms)
        {
            if (!Enum.TryParse<CardEffectAddActionType>(elms[0], true, out var effectAddActionType))
            {
                throw new InvalidOperationException(elms[0]);
            }

            switch (effectAddActionType)
            {
                case CardEffectAddActionType.YouHand:
                    {
                        var creatureFullName = elms[1];

                        return (gameMaster, ownerCard) =>
                        {
                            var owner = gameMaster.PlayersById[ownerCard.OwnerId];
                            var newCard = gameMaster.GenerateNewCard(creatureFullName, owner.Id);
                            gameMaster.AddHand(owner, newCard);
                        };
                    }

                case CardEffectAddActionType.OpponentHand:
                    {
                        var creatureFullName = elms[1];

                        return (gameMaster, ownerCard) =>
                        {
                            var opponent = gameMaster.GetOpponent(ownerCard.OwnerId);
                            var newCard = gameMaster.GenerateNewCard(creatureFullName, opponent.Id);
                            gameMaster.AddHand(opponent, newCard);
                        };
                    }

                //case CardEffectAddActionType.YouDeck:
                //    {
                //        var creatureFullName = elms[1];

                //        return (gameMaster, ownerCard) =>
                //        {
                //            var owner = gameMaster.PlayersById[ownerCard.OwnerId];
                //            var newCard = gameMaster.GenerateNewCard(creatureFullName, owner.Id);
                //            gameMaster.AddHand(owner, newCard);
                //        };
                //    }

                //case CardEffectAddActionType.OpponentDeck:
                //    {
                //        var creatureFullName = elms[1];

                //        return (gameMaster, ownerCard) =>
                //        {
                //            var owner = gameMaster.PlayersById[ownerCard.OwnerId];
                //            var newCard = gameMaster.GenerateNewCard(creatureFullName, owner.Id);
                //            gameMaster.AddHand(owner, newCard);
                //        };
                //    }

                //case CardEffectAddActionType.YouCemetery:
                //    {
                //        var creatureFullName = elms[1];

                //        return (gameMaster, ownerCard) =>
                //        {
                //            var owner = gameMaster.PlayersById[ownerCard.OwnerId];
                //            var newCard = gameMaster.GenerateNewCard(creatureFullName, owner.Id);
                //            gameMaster.AddHand(owner, newCard);
                //        };
                //    }

                //case CardEffectAddActionType.OpponentCemetery:
                //    {
                //        var creatureFullName = elms[1];

                //        return (gameMaster, ownerCard) =>
                //        {
                //            var owner = gameMaster.PlayersById[ownerCard.OwnerId];
                //            var newCard = gameMaster.GenerateNewCard(creatureFullName, owner.Id);
                //            gameMaster.AddHand(owner, newCard);
                //        };
                //    }

                default:
                    throw new InvalidOperationException(elms[0]);
            }
        }

        public static Action<GameMaster, Card> ParseBuffAction(string[] elms)
        {
            if (!Enum.TryParse<ChooseType>(elms[0], true, out var chooseType))
            {
                throw new InvalidOperationException(elms[0]);
            }

            switch (chooseType)
            {
                case ChooseType.Self:
                case ChooseType.That:
                    {
                        if (!int.TryParse(elms[1], out var powerBuff))
                        {
                            throw new InvalidOperationException(elms[2]);
                        }

                        if (!int.TryParse(elms[2], out var toughnessBuff))
                        {
                            throw new InvalidOperationException(elms[3]);
                        }

                        return (gameMaster, ownerCard) =>
                        {
                            if (ownerCard.Type == CardType.Creature)
                            {
                                gameMaster.Buff(ownerCard, powerBuff, toughnessBuff);
                            }
                        };
                    }
                case ChooseType.All:
                case ChooseType.Choose:
                case ChooseType.Random:
                    {
                        if (!Enum.TryParse<ZoneType>(elms[1], true, out var zoneType))
                        {
                            throw new InvalidOperationException(elms[1]);
                        }

                        if (!int.TryParse(elms[2], out var powerBuff))
                        {
                            throw new InvalidOperationException(elms[2]);
                        }

                        if (!int.TryParse(elms[3], out var toughnessBuff))
                        {
                            throw new InvalidOperationException(elms[3]);
                        }

                        switch (zoneType)
                        {
                            case ZoneType.YouField:
                                {
                                    return (gameMaster, ownerCard) =>
                                    {
                                        var owner = gameMaster.PlayersById[ownerCard.OwnerId];
                                        var candidates = owner.Field.AllCards
                                            .Where(c => c.Type == CardType.Creature || c.Type == CardType.Token)
                                            .ToArray();

                                        var targetCreatures = chooseType switch
                                        {
                                            ChooseType.Random => new[] { Program.RandomPick(candidates) },
                                            ChooseType.All => candidates,
                                            _ => throw new InvalidOperationException()
                                        };

                                        if (!targetCreatures.Any()) return;

                                        foreach (var targetCreature in targetCreatures)
                                        {
                                            gameMaster.Buff(targetCreature, powerBuff, toughnessBuff);
                                        }
                                    };
                                }

                            case ZoneType.OpponentHand:
                                {
                                    return (gameMaster, ownerCard) =>
                                    {
                                        var opponent = gameMaster.GetOpponent(ownerCard.OwnerId);
                                        var candidates = opponent.Field.AllCards
                                            .Where(c => c.Type == CardType.Creature || c.Type == CardType.Token)
                                            .ToArray();

                                        var targetCreature = chooseType switch
                                        {
                                            ChooseType.Random => Program.RandomPick(candidates),
                                            _ => throw new InvalidOperationException()
                                        };

                                        if (targetCreature == default) return;

                                        gameMaster.Buff(targetCreature, powerBuff, toughnessBuff);
                                    };
                                }

                            default:
                                throw new InvalidOperationException(elms[0]);
                        }
                    }

                default:
                    throw new InvalidOperationException(elms[0]);
            }
        }
    }
}
