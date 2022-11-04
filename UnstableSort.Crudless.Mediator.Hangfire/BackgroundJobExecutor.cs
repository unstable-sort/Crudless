using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UnstableSort.Crudless.Mediator.Hangfire
{
    public abstract class BackgroundJobExecutor<TJob, TResult> 
        where TJob : IBackgroundJob<TResult>
    {
        protected BackgroundJobExecutor(IMediator mediator)
        {
            Mediator = mediator;
        }

        protected IMediator Mediator { get; }

        public async Task Execute(TJob job)
        {
            await PreExecute(job);

            var response = await ExecuteRequest(job.Request);

            await PostExecute(job, response);
        }

        private Task<Response<TResult>> ExecuteRequest(IRequest<TResult> request) 
            => Mediator.HandleAsync(request);

        protected abstract Task PreExecute(TJob job);

        protected abstract Task PostExecute(TJob job, Response<TResult> response);
    }

    public class SimpleBackgroundJobExecutor<TJob, TResult>
        : BackgroundJobExecutor<TJob, TResult> 
        where TJob : IBackgroundJob<TResult>
    {
        public SimpleBackgroundJobExecutor(IMediator mediator)
            : base(mediator)
        {
        }

        protected override Task PreExecute(TJob job)
        {
            return Task.CompletedTask;
        }

        protected override Task PostExecute(TJob job, Response<TResult> response)
        {
            if (response.HasErrors)
                throw new Exception(JsonConvert.SerializeObject(response.Errors));

            return Task.CompletedTask;
        }
    }
}
