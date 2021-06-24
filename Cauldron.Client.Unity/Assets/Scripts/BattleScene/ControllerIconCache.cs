using System;
using System.Collections.Generic;
using UnityEngine;

class ControllerIconCache
{
    public enum IconType
    {
        You,
        Opponent
    }

    public static Lazy<Sprite> YouIcon { get; }
        = new Lazy<Sprite>(() => Resources.Load<Sprite>("Images/you_icon"), true);
    public static Lazy<Sprite> OpponentIcon { get; }
        = new Lazy<Sprite>(() => Resources.Load<Sprite>("Images/opponent_icon"), true);

    private static readonly Dictionary<IconType, Sprite> dic = new Dictionary<IconType, Sprite>();

    public static (bool, Sprite) TryGet(IconType iconType)
    {
        if (!dic.TryGetValue(iconType, out var value))
        {
            value = iconType switch
            {
                IconType.You => YouIcon.Value,
                IconType.Opponent => OpponentIcon.Value,
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
