using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using UnstableSort.Crudless.Mediator.Tests.Fakes;

namespace UnstableSort.Crudless.Mediator.Tests
{
    [TestFixture]
    public class DynamicMediatorPipelineTests
    {
        private Container Container { get; set; }

        private IMediator Mediator => Container.GetInstance<IMediator>();

        [SetUp]
        public void SetUp()
        {
            Container = new Container();

            Container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            Crudless.CreateInitializer(Container)
                .WithAssemblies(GetType().GetTypeInfo().Assembly)
                .UseDynamicMediator()
                .UseEntityFramework()
                .UseFluentValidation()
                .UseTransactions()
                .ValidateAllRequests(false)
                .Initialize();

            Container.RegisterInstance<DbContext>(new FakeDbContext());
        }

        [Test]
        public async Task Handle_WithoutResponseData_ReturnsResponseObjectWithoutData()
        {
            // Act
            var response = await Mediator.HandleAsync(new RequestWithoutResponse());
            await Mediator.HandleAsync(new RequestWithoutResponse());
            await Mediator.HandleAsync(new RequestWithoutResponse());

            // Assert
            Assert.IsFalse(response.HasErrors);
            Assert.IsNull(response.Result);
        }

        [Test]
        public async Task Handle_WithResponseData_ReturnsResponseObjectWithData()
        {
            // Act
            var response = await Mediator.HandleAsync(new RequestWithResponse());
            await Mediator.HandleAsync(new RequestWithResponse());
            await Mediator.HandleAsync(new RequestWithResponse());

            // Assert
            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual("Bar", response.Result);
        }
    }
}