﻿using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace UnstableSort.Crudless.Exceptions
{
    [Serializable]
    public class UpdateEntityFailedException : Exception
    {
        public object ItemProperty { get; set; }

        public object EntityProperty { get; set; }

        public UpdateEntityFailedException()
        {
        }

        public UpdateEntityFailedException(string message)
            : base(message)
        {
        }

        public UpdateEntityFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected UpdateEntityFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ItemProperty = JsonConvert.DeserializeObject(info.GetString(nameof(ItemProperty)));
            EntityProperty = JsonConvert.DeserializeObject(info.GetString(nameof(EntityProperty)));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(ItemProperty), JsonConvert.SerializeObject(ItemProperty));
            info.AddValue(nameof(EntityProperty), JsonConvert.SerializeObject(EntityProperty));

            base.GetObjectData(info, context);
        }
    }
}
