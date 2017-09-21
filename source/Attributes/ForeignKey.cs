using System;

namespace Generic.LightDataTable.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ForeignKey : Attribute
    {
        public Type Type { get; private set; }

        public ForeignKey() { }

        public ForeignKey(Type type)
        {
            Type = type;
        }


    }
}
