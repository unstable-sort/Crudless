using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Requests;

namespace UnstableSort.Crudless.Tests.Fakes
{
    public class FakeInjectable
    {
    }

    public class TestTypeRequestHook : RequestHook<TestHooksRequest>
    {
        public TestTypeRequestHook(FakeInjectable injectable)
        {
            if (injectable == null)
                throw new System.Exception("Injection Failed");
        }

        public override Task Run(TestHooksRequest request, CancellationToken token)
        {
            request.Items.ForEach(i => i.RequestHookMessage += "r4/");
            return Task.CompletedTask;
        }
    }

    public class TestTypeEntityHook : EntityHook<TestHooksRequest, HookEntity>
    {
        public TestTypeEntityHook(FakeInjectable injectable)
        {
            if (injectable == null)
                throw new System.Exception("Injection Failed");
        }

        public override Task Run(TestHooksRequest request, HookEntity entity, CancellationToken token)
        {
            entity.EntityHookMessage += "e4/";
            return Task.CompletedTask;
        }
    }

    public class TestTypeItemHook : ItemHook<TestHooksRequest, HookDto>
    {
        public TestTypeItemHook(FakeInjectable injectable)
        {
            if (injectable == null)
                throw new System.Exception("Injection Failed");
        }

        public override Task<HookDto> Run(TestHooksRequest request, HookDto item, CancellationToken token)
        {
            item.ItemHookMessage += "i4/";
            return Task.FromResult(item);
        }
    }

    public class TestTypeResultHook : ResultHook<TestHooksRequest, string>
    {
        public TestTypeResultHook(FakeInjectable injectable)
        {
            if (injectable == null)
                throw new System.Exception("Injection Failed");
        }

        public override Task<string> Run(TestHooksRequest request, string result, CancellationToken token)
        {
            return Task.FromResult(result + "t4/");
        }
    }

    public class TestInstanceRequestHook : RequestHook<TestHooksRequest>
    {
        public override Task Run(TestHooksRequest request, CancellationToken token)
        {
            request.Items.ForEach(i => i.RequestHookMessage += "r3/");
            return Task.CompletedTask;
        }
    }

    public class TestInstanceEntityHook : EntityHook<TestHooksRequest, HookEntity>
    {
        public override Task Run(TestHooksRequest request, HookEntity entity, CancellationToken token)
        {
            entity.EntityHookMessage += "e3/";
            return Task.CompletedTask;
        }
    }

    public class TestInstanceItemHook : ItemHook<TestHooksRequest, HookDto>
    {
        public override Task<HookDto> Run(TestHooksRequest request, HookDto item, CancellationToken token)
        {
            item.ItemHookMessage += "i3/";
            return Task.FromResult(item);
        }
    }

    public class TestInstanceResultHook : ResultHook<TestHooksRequest, string>
    {
        public override Task<string> Run(TestHooksRequest request, string result, CancellationToken token)
        {
            return Task.FromResult(result + "t3/");
        }
    }

    public class TestContravariantRequestHook : RequestHook<ICrudlessRequest>
    {
        public override Task Run(ICrudlessRequest request, CancellationToken token)
        {
            ((ITestHooksRequest)request).Items.ForEach(i => i.RequestHookMessage += "r5");
            return Task.CompletedTask;
        }
    }

    public class TestContravariantEntityHook : EntityHook<ICrudlessRequest, IEntity>
    {
        public override Task Run(ICrudlessRequest request, IEntity entity, CancellationToken token)
        {
            if (!(request is TestHooksRequest))
                throw new System.Exception("Contravariance Failed");

            ((HookEntity)entity).EntityHookMessage += "e5";
            return Task.CompletedTask;
        }
    }

    public class TestContravariantItemHook : ItemHook<ICrudlessRequest, HookDto>
    {
        public override Task<HookDto> Run(ICrudlessRequest request, HookDto item, CancellationToken token)
        {
            if (!(request is TestHooksRequest))
                throw new System.Exception("Contravariance Failed");

            item.ItemHookMessage += "i5";
            return Task.FromResult(item);
        }
    }

    public class TestContravariantResultHook : ResultHook<ICrudlessRequest, string>
    {
        public override Task<string> Run(ICrudlessRequest request, string result, CancellationToken token)
        {
            if (!(request is TestHooksRequest))
                throw new System.Exception("Contravariance Failed");

            return Task.FromResult(result + "t5");
        }
    }
}
