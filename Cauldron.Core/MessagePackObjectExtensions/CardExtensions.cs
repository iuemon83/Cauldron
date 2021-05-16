using System;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class CardExtensions
    {
        public static void Damage(this Card card, int damage)
        {
            card.ToughnessBuff -= Math.Max(0, damage);
        }
    }
}
