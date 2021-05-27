using System;

namespace Assets.Scripts.ServerShared.MessagePackObjects
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DisplayTextAttribute : Attribute
    {
        public string Value { get; private set; }

        public DisplayTextAttribute(string value)
        {
            this.Value = value;
        }
    }
}
