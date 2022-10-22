using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class PositionConditionExtentions
    {
        public static async ValueTask<bool> IsMatch(this PositionCondition _this, Card targetCard,
            Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var choiceResult = await effectEventArgs.GameMaster
                .Choice(effectOwnerCard, _this.ChoiceBaseCard, effectEventArgs);

            var targetCardIndex = effectEventArgs.GameMaster.Get(targetCard.Zone.PlayerId)?
                .Field.Index(targetCard) ?? -1;

            if (targetCardIndex == -1)
            {
                return false;
            }

            foreach (var card in choiceResult.CardList)
            {
                var isMatched = _this.IsMatch(card, targetCard, targetCardIndex, effectEventArgs);

                if (isMatched)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsMatch(this PositionCondition _this,
            Card basePositionCard, Card targetCard, int targetCardIndex,
            EffectEventArgs effectEventArgs)
        {
            var basePositionCardIndex = effectEventArgs.GameMaster.Get(basePositionCard.Zone.PlayerId)?
                .Field.Index(basePositionCard) ?? -1;

            return IsMatch(
                basePositionCard.Zone, basePositionCardIndex,
                targetCard.Zone, targetCardIndex,
                _this.X, _this.Y);

            //if (basePositionCard.Zone.ZoneName != targetCard.Zone.ZoneName)
            //{
            //    return false;
            //}

            //switch (basePositionCard.Zone.ZoneName)
            //{
            //    case ZoneName.Field:
            //        if (_this.Y == 0 && basePositionCard.Zone.PlayerId != basePositionCard.Zone.PlayerId)
            //        {
            //            return false;
            //        }

            //        if (_this.Y == 1 && basePositionCard.Zone.PlayerId == basePositionCard.Zone.PlayerId)
            //        {
            //            return false;
            //        }

            //        var basePositionCardIndex = effectEventArgs.GameMaster.Get(basePositionCard.Zone.PlayerId)?
            //            .Field.Index(basePositionCard) ?? -1;

            //        if (basePositionCardIndex == -1)
            //        {
            //            return false;
            //        }

            //        return targetCardIndex - basePositionCardIndex == _this.X;

            //    default:
            //        return false;
            //}
        }

        public static bool IsMatch(Zone baseZone, int baseIndex, Zone zone, int index, int x, int y)
        {
            if (baseZone.ZoneName != zone.ZoneName)
            {
                return false;
            }

            if (baseIndex == -1)
            {
                return false;
            }

            switch (baseZone.ZoneName)
            {
                case ZoneName.Field:
                    if (y == 0 && baseZone.PlayerId != zone.PlayerId)
                    {
                        return false;
                    }

                    if (y == 1 && baseZone.PlayerId == zone.PlayerId)
                    {
                        return false;
                    }

                    return index - baseIndex == x;

                default:
                    return false;
            }
        }
    }
}
