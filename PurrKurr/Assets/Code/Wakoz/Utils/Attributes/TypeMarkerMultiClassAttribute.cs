using System;

namespace Code.Wakoz.Utils.Attributes {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TypeMarkerMultiClassAttribute : Attribute {

        public Type type { get; }

        public TypeMarkerMultiClassAttribute(Type objectiveType) {
            type = objectiveType;
        }
    }
}
