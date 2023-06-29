using System;
using System.Linq;
using System.Reflection;
using Hangfire;
using UnstableSort.Crudless.Common.ServiceProvider;

namespace UnstableSort.Crudless.Mediator.Hangfire
{
    public sealed class BackgroundMediator : IBackgroundMediator
    {
        private readonly ServiceProviderContainer _container;

        public BackgroundMediator(ServiceProviderContainer container)
        {
            _container = container;
        }

        public void Enqueue<TRequest, TResult>(TRequest request)
            where TRequest : IRequest<TResult>
                => InvokeHangfire<TRequest, TResult>(nameof(InvokeEnqueue), request);

        public void Enqueue<TRequest>(TRequest request)
            where TRequest : IRequest
                => Enqueue<TRequest, NoResult>(request);

        public void Schedule<TRequest, TResult>(TRequest request, TimeSpan delay)
            where TRequest : IRequest<TResult>
                => InvokeHangfire<TRequest, TResult>(nameof(InvokeSchedule), request, delay);

        public void Schedule<TRequest>(TRequest request, TimeSpan delay)
            where TRequest : IRequest
                => Schedule<TRequest, NoResult>(request, delay);

        public void Schedule<TRequest, TResult>(string name, TRequest request, string cron)
            where TRequest : IRequest<TResult>
                => InvokeHangfire<TRequest, TResult>(nameof(InvokeRecurringSchedule), request, name, cron);

        public void Schedule<TRequest>(string name, TRequest command, string cron)
            where TRequest : IRequest
                => Schedule<TRequest, NoResult>(name, command, cron);

        private void InvokeHangfire<TRequest, TResult>(string internalMethod, TRequest request, params object[] additionalParams)
            where TRequest : IRequest<TResult>
        {
            var options = _container.ProvideInstance<CrudlessHangfireOptions>();

            var jobAdapter = _container.ProvideInstance<BackgroundJobAdapter>();
            var job = jobAdapter.Adapt<TRequest, TResult>(request);
            var jobType = job.GetType();

            var executorType = options.BackgroundJobExecutorType
                .MakeGenericType(jobType, typeof(TRequest), typeof(TResult));

            var parameters = new object[] { job }.Concat(additionalParams).ToArray();

            typeof(BackgroundMediator)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(x => x.Name == internalMethod)
                .MakeGenericMethod(jobType, executorType, typeof(TRequest), typeof(TResult))
                .Invoke(this, parameters);
        }

        private void InvokeEnqueue<TJob, TExecutor, TRequest, TResult>(TJob job)
            where TRequest : IRequest<TResult>
            where TJob : IBackgroundJob<TRequest, TResult>
            where TExecutor : BackgroundJobExecutor<TJob, TRequest, TResult>
                => BackgroundJob.Enqueue<TExecutor>(x => x.Execute(job));

        private void InvokeSchedule<TJob, TExecutor, TRequest, TResult>(TJob job, TimeSpan delay)
            where TRequest : IRequest<TResult>
            where TJob : IBackgroundJob<TRequest, TResult>
            where TExecutor : BackgroundJobExecutor<TJob, TRequest, TResult>
                => BackgroundJob.Schedule<TExecutor>(x => x.Execute(job), delay);

        private void InvokeRecurringSchedule<TJob, TExecutor, TRequest, TResult>(TJob job, string name, string cron)
            where TRequest : IRequest<TResult>
            where TJob : IBackgroundJob<TRequest, TResult>
            where TExecutor : BackgroundJobExecutor<TJob, TRequest, TResult>
                => RecurringJob.AddOrUpdate<TExecutor>(name, x => x.Execute(job), cron);
    }
}