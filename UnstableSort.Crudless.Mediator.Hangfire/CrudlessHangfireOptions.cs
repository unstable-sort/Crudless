using System;
using UnstableSort.Crudless.Mediator.Hangfire.Extensions;

namespace UnstableSort.Crudless.Mediator.Hangfire
{
    public class CrudlessHangfireOptions
    {
        private Type _backgroundJobAdapterType = typeof(SimpleBackgroundJobAdapter);
        private Type _backgroundJobExecutorType = typeof(SimpleBackgroundJobExecutor<,,>);

        public Type BackgroundJobAdapterType 
        { 
            get => _backgroundJobAdapterType;
            set
            {
                _backgroundJobAdapterType = value ?? typeof(SimpleBackgroundJobAdapter);
                if (!typeof(BackgroundJobAdapter).IsAssignableFrom(_backgroundJobAdapterType))
                    throw new ArgumentException($"'{_backgroundJobAdapterType.GetType()}' cannot be used as a background job adapter.");
            } 
        }

        public Type BackgroundJobExecutorType 
        { 
            get => _backgroundJobExecutorType; 
            set
            {
                _backgroundJobExecutorType = value ?? typeof(SimpleBackgroundJobExecutor<,,>);
                if (!_backgroundJobExecutorType.IsSubclassOfGenericType(typeof(BackgroundJobExecutor<,,>)))
                    throw new ArgumentException($"'{_backgroundJobExecutorType.GetType()}' cannot be used as a background job executor.");
            }
        }
    }
}
