using System;
using Hangfire;

namespace UnstableSort.Crudless.Mediator.Hangfire
{
    public class BackgroundJobMediator : IBackgroundJobMediator
    {
        public void Enqueue<TRequest, TResult>(TRequest command) 
            where TRequest : IRequest<TResult>
        {
            BackgroundJob.Enqueue<BackgroundJobExecutor<TRequest, TResult>>(x => x.Execute(command));
        }


        public void Enqueue<TRequest>(TRequest command)
            where TRequest : IRequest
            => Enqueue<TRequest, NoResult>(command);

        public void Schedule<TRequest, TResult>(TRequest command, TimeSpan delay)
            where TRequest : IRequest<TResult>
        {
            BackgroundJob.Schedule<BackgroundJobExecutor<TRequest, TResult>>(x => x.Execute(command), delay);
        }

        public void Schedule<TRequest>(TRequest command, TimeSpan delay)
            where TRequest : IRequest
            => Schedule<TRequest, NoResult>(command, delay);

        public void Schedule<TRequest, TResult>(string name, TRequest command, string cron)
            where TRequest : IRequest<TResult>
        {
            RecurringJob.AddOrUpdate<BackgroundJobExecutor<TRequest, TResult>>(name, x => x.Execute(command), cron);
        }

        public void Schedule<TRequest>(string name, TRequest command, string cron)
            where TRequest : IRequest
            => Schedule<TRequest, NoResult>(name, command, cron);
    }
}