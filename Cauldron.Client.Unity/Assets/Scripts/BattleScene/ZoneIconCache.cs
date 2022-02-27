using Cauldron.Shared;
using System;
using System.Collections.Generic;
using UnityEngine;

class ZoneIconCache
{
    public static Lazy<Sprite> CardPoolIcon { get; }
        = new Lazy<Sprite>(() => Resources.Load<Sprite>("Images/include_card_icon"), true);
    public static Lazy<Sprite> FieldIcon { get; }
        = new Lazy<Sprite>(() => Resources.Load<Sprite>("Images/field_icon"), true);
    public static Lazy<Sprite> HandIcon { get; }
        = new Lazy<Sprite>(() => Resources.Load<Sprite>("Images/hands_icon"), true);
    public static Lazy<Sprite> DeckIcon { get; }
        = new Lazy<Sprite>(() => Resources.Load<Sprite>("Images/deck_icon"), true);
    public static Lazy<Sprite> CemeteryIcon { get; }
        = new Lazy<Sprite>(() => Resources.Load<Sprite>("Images/cemetery_icon"), true);
    public static Lazy<Sprite> ExcludeIcon { get; }
        = new Lazy<Sprite>(() => Resources.Load<Sprite>("Images/exclude_card_icon"), true);

    private static readonly Dictionary<ZoneName, Sprite> dic = new Dictionary<ZoneName, Sprite>();

    public static (bool, Sprite) TryGet(ZoneName zoneName)
    {
        if (!dic.TryGetValue(zoneName, out var value))
        {
            value = zoneName switch
            {
                ZoneName.None => throw new NotImplementedException(),
                ZoneName.CardPool => CardPoolIcon.Value,
                ZoneName.Field => FieldIcon.Value,
                ZoneName.Hand => HandIcon.Value,
                ZoneName.Deck => DeckIcon.Value,
                ZoneName.Cemetery => CemeteryIcon.Value,
                ZoneName.Excluded => ExcludeIcon.Value,
                _ => null
            };

            if (value == null)
            {
                return (false, default);
            }
        }

        return (true, value);
    }
}
