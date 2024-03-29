﻿using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace UnstableSort.Crudless.Exceptions
{
    [Serializable]
    public class RequestFailedException : Exception
    {
        public Type RequestTypeProperty { get; set; }

        public object ResponseData { get; set; }
        
        public RequestFailedException()
        {
        }

        public RequestFailedException(string message)
            : base(message)
        {
        }

        public RequestFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected RequestFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            RequestTypeProperty = (Type) info.GetValue(nameof(RequestTypeProperty), typeof(Type));
            ResponseData = JsonConvert.DeserializeObject(info.GetString(nameof(ResponseData)));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(RequestTypeProperty), RequestTypeProperty);
            info.AddValue(nameof(ResponseData), JsonConvert.SerializeObject(ResponseData));

            base.GetObjectData(info, context);
        }
    }
}
