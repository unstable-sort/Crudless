using System.Reflection;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Mediator.Tests.Fakes;
using UnstableSort.Crudless.ServiceProvider.SimpleInjector;
using UnstableSort.Crudless.Tests;

namespace UnstableSort.Crudless.Mediator.Tests
{
    public class BaseUnitTest
    {
        protected Container Container { get; set; }

        protected IServiceProvider Provider { get; set; }

        protected IMediator Mediator => Container.GetInstance<IMediator>();
        
        [SetUp]
        public void BaseSetUp()
        {
            Container = new Container();

            Container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            Container.ConfigureAutoMapper();

            var providerContainer = Container.AsServiceProvider();
            
            Crudless.CreateInitializer(providerContainer)
                .WithAssemblies(GetType().GetTypeInfo().Assembly)
                .Initialize();

            Container.RegisterInstance<DbContext>(new FakeDbContext());

            Provider = providerContainer.CreateProvider();
        }

        [TearDown]
        public void BaseTearDown()
        {
            Provider.Dispose();
        }
    }
}