using System;
using System.Runtime.Serialization;

namespace UnstableSort.Crudless.Exceptions
{
    [Serializable]
    public class FailedToFindException : Exception
    {
        public Type EntityTypeProperty { get; set; }

        public FailedToFindException()
        {
        }

        public FailedToFindException(string message)
            : base(message)
        {
        }

        public FailedToFindException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected FailedToFindException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            EntityTypeProperty = (Type)info.GetValue(nameof(EntityTypeProperty), typeof(Type));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(EntityTypeProperty), EntityTypeProperty);

            base.GetObjectData(info, context);
        }
    }
}
