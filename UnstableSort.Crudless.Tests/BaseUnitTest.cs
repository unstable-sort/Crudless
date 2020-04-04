using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Mediator;
using UnstableSort.Crudless.Tests.Fakes;

namespace UnstableSort.Crudless.Tests
{
    public class BaseUnitTest
    {
        private Scope _scope;

        protected Container Container => UnitTestSetUp.Container;

        protected ServiceProviderContainer Provider => UnitTestSetUp.Provider;

        protected IMediator Mediator { get; private set; }

        protected DbContext Context { get; private set; }

        [SetUp]
        public void SetUpBase()
        {
            _scope = AsyncScopedLifestyle.BeginScope(Container);
            
            Mediator = _scope.GetInstance<IMediator>();
            Context = _scope.GetInstance<DbContext>();
            Context.Database.EnsureDeleted();
        }

        [TearDown]
        public async Task BaseTearDown()
        {
            if (Context is FakeDbContext dbContext)
                await dbContext.Clear();

            Context.ResetValueGenerators();

            _scope.Dispose();
        }
    }
}