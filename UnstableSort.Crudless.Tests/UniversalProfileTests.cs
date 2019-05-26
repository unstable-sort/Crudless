using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Tests
{
    [TestFixture]
    public class UniversalProfileTests : BaseUnitTest
    {
        [Test]
        public async Task Handle_NonCrudRequest_RunsHooksInCorrectOrder()
        {
            var request = new TestUniversalRequest
            {
                RequestHookMessage = string.Empty
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);

            Assert.NotNull(response.Result);
            Assert.AreEqual("r1/r2", response.Result.RequestHookMessage);
            Assert.AreEqual("t1/t2/t3", response.Result.ResultHookMessage);
        }
    }

    public class UniversalDto
    {
        public string RequestHookMessage { get; set; }

        public string ResultHookMessage { get; set; }
    }
    
    [DoNotValidate]
    public class TestUniversalRequest : IRequest<UniversalDto>
    {
        public string RequestHookMessage { get; set; }
    }

    public class TestUniversalRequestHandler : IRequestHandler<TestUniversalRequest, UniversalDto>
    {
        public Task<Response<UniversalDto>> HandleAsync(TestUniversalRequest request, CancellationToken token)
        {
            var result = new UniversalDto { RequestHookMessage = request.RequestHookMessage };
            return result.AsResponseAsync();
        }
    }
    
    public class TestUniversalProfile
        : UniversalRequestProfile<TestUniversalRequest>
    {
        public TestUniversalProfile()
        {
            AddRequestHook(r =>
            {
                r.RequestHookMessage = "r1/";
            });

            AddRequestHook(r =>
            {
                r.RequestHookMessage += "r2";
                return Task.CompletedTask;
            });

            AddResultHook<UniversalDto>((r, t) => 
            {
                t.ResultHookMessage += "t2/";
                return t;
            });

            AddResultHook<UniversalDto>((r, t) => 
            {
                t.ResultHookMessage += "t3";
                return Task.FromResult(t);
            });
        }
    }

    public class TestProfile<T> : UniversalRequestProfile<IRequest<T>>
    {
        public TestProfile()
        {
            AddResultHook<T>((request, result) =>
            {
                if (request is TestUniversalRequest testRequest)
                    (result as UniversalDto).ResultHookMessage = "t1/";
                    
                return result;
            });
        }
    }
}