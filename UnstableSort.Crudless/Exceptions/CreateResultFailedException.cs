using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace UnstableSort.Crudless.Exceptions
{
    [Serializable]
    public class CreateResultFailedException : Exception
    {
        public object EntityProperty { get; set; }

        public CreateResultFailedException()
        {
        }

        public CreateResultFailedException(string message)
            : base(message)
        {
        }

        public CreateResultFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected CreateResultFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            EntityProperty = JsonConvert.DeserializeObject(info.GetString(nameof(EntityProperty)));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            
            info.AddValue(nameof(EntityProperty), JsonConvert.SerializeObject(EntityProperty));

            base.GetObjectData(info, context);
        }
    }
}
