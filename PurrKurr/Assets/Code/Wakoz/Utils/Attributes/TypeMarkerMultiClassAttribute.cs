using System;

namespace Code.Wakoz.Utils.Attributes {

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class TypeMarkerMultiClassAttribute : Attribute {

        public Type type { get; }

        public TypeMarkerMultiClassAttribute(Type attributeType) {
            type = attributeType;
        }
    }
}
