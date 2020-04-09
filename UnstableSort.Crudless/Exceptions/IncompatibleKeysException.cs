using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace UnstableSort.Crudless.Exceptions
{
    [Serializable]
    public class IncompatibleKeysException : Exception
    {
        public Type RequestType { get; set; }

        public Type EntityType { get; set; }

        public IncompatibleKeysException(Type requestType, Type entityType) 
            : base(GetMessage(requestType, entityType))
        {
            RequestType = requestType;
            EntityType = entityType;
        }

        public IncompatibleKeysException(Type requestType, Type entityType, Exception inner)
            : base(GetMessage(requestType, entityType), inner)
        {
            RequestType = requestType;
            EntityType = entityType;
        }

        protected IncompatibleKeysException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            RequestType = info.GetValue(nameof(RequestType), typeof(Type)) as Type;
            EntityType = info.GetValue(nameof(EntityType), typeof(Type)) as Type;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(RequestType), RequestType);
            info.AddValue(nameof(EntityType), EntityType);

            base.GetObjectData(info, context);
        }

        private static string GetMessage(Type requestType, Type entityType)
            => $"Incompatible keys defined for '{requestType}' and '{entityType}'";
    }
}
