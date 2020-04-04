using UnstableSort.Crudless.Common.ServiceProvider;

namespace UnstableSort.Crudless.Requests
{
    public class BoxedRequestContext
    {
        internal object Request { get; set; }

        internal IServiceProvider ServiceProvider { get; set; }

        internal RequestContext<TRequest> Cast<TRequest>()
            => new RequestContext<TRequest> { Request = (TRequest)Request, ServiceProvider = ServiceProvider };
    }

    public class RequestContext<TRequest>
    {
        public TRequest Request { get; internal set; }

        public IServiceProvider ServiceProvider { get; internal set; }

        internal BoxedRequestContext Box()
            => new BoxedRequestContext { Request = Request, ServiceProvider = ServiceProvider };
    }
}
