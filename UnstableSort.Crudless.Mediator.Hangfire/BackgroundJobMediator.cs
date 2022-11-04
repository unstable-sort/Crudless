using System;
using Hangfire;

namespace UnstableSort.Crudless.Mediator.Hangfire
{
    public class BackgroundJobMediator : IBackgroundJobMediator
    {
        private readonly BackgroundJobAdapter _jobAdapter;

        public BackgroundJobMediator(BackgroundJobAdapter jobAdapter)
        {
            _jobAdapter = jobAdapter;
        }

        public void Enqueue<TRequest, TResult>(TRequest command) 
            where TRequest : IRequest<TResult>
        {
            var job = _jobAdapter.Adapt(command);

            BackgroundJob.Enqueue<BackgroundJobExecutor<IBackgroundJob<TResult>, TResult>>(x => x.Execute(job));
        }

        public void Enqueue<TRequest>(TRequest command)
            where TRequest : IRequest
                => Enqueue<TRequest, NoResult>(command);

        public void Schedule<TRequest, TResult>(TRequest command, TimeSpan delay)
            where TRequest : IRequest<TResult>
        {
            var job = _jobAdapter.Adapt(command);

            BackgroundJob.Schedule<BackgroundJobExecutor<IBackgroundJob<TResult>, TResult>>(x => x.Execute(job), delay);
        }

        public void Schedule<TRequest>(TRequest command, TimeSpan delay)
            where TRequest : IRequest
                => Schedule<TRequest, NoResult>(command, delay);

        public void Schedule<TRequest, TResult>(string name, TRequest command, string cron)
            where TRequest : IRequest<TResult>
        {
            var job = _jobAdapter.Adapt(command);

            RecurringJob.AddOrUpdate<BackgroundJobExecutor<IBackgroundJob<TResult>, TResult>>(name, x => x.Execute(job), cron);
        }

        public void Schedule<TRequest>(string name, TRequest command, string cron)
            where TRequest : IRequest
                => Schedule<TRequest, NoResult>(name, command, cron);
    }
}