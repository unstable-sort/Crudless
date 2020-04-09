﻿using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace UnstableSort.Crudless.Common.ServiceProvider
{
    [Serializable]
    public class FailedToCreateServiceException : Exception
    {
        public FailedToCreateServiceException()
        {
        }

        public FailedToCreateServiceException(string message)
            : base(message)
        {
        }

        public FailedToCreateServiceException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected FailedToCreateServiceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            
            base.GetObjectData(info, context);
        }
    }
}
