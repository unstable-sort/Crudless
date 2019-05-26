using System.Reflection;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SimpleInjector;
using UnstableSort.Crudless.Mediator.Tests.Fakes;

namespace UnstableSort.Crudless.Mediator.Tests
{
    public class BaseUnitTest
    {
        protected Container Container { get; set; }

        protected IMediator Mediator => Container.GetInstance<IMediator>();
        
        [SetUp]
        public void SetUp()
        {
            Container = new Container();
            
            Crudless.CreateInitializer(Container)
                .WithAssemblies(GetType().GetTypeInfo().Assembly)
                .Initialize();

            Container.RegisterInstance<DbContext>(new FakeDbContext());
        }
    }
}