using Cauldron.Shared;
using System;
using System.Collections.Generic;
using UnityEngine;

class AbilityIconCache
{
    public static Lazy<Sprite> CoverIcon { get; }
        = new Lazy<Sprite>(() => Resources.Load<Sprite>("Images/ability_cover_icon"), true);
    public static Lazy<Sprite> StealthIcon { get; }
        = new Lazy<Sprite>(() => Resources.Load<Sprite>("Images/ability_stealth_icon"), true);
    public static Lazy<Sprite> DeadlyIcon { get; }
        = new Lazy<Sprite>(() => Resources.Load<Sprite>("Images/ability_deadly_icon"), true);
    public static Lazy<Sprite> SealedIcon { get; }
        = new Lazy<Sprite>(() => Resources.Load<Sprite>("Images/ability_sealed_icon"), true);
    public static Lazy<Sprite> CantattackIcon { get; }
        = new Lazy<Sprite>(() => Resources.Load<Sprite>("Images/ability_cantattack_icon"), true);

    private static readonly Dictionary<CreatureAbility, Sprite> dic = new Dictionary<CreatureAbility, Sprite>();

    public static (bool, Sprite) TryGet(CreatureAbility creatureAbility)
    {
        if (!dic.TryGetValue(creatureAbility, out var value))
        {
            value = creatureAbility switch
            {
                CreatureAbility.Cover => CoverIcon.Value,
                CreatureAbility.Stealth => StealthIcon.Value,
                CreatureAbility.Deadly => DeadlyIcon.Value,
                CreatureAbility.Sealed => SealedIcon.Value,
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
