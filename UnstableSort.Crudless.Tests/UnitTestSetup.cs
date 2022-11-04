using System;
using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Integration.EntityFrameworkExtensions;
using UnstableSort.Crudless.Integration.SimpleInjector;
using UnstableSort.Crudless.Tests.Fakes;
using UnstableSort.Crudless.Tests.Utilities;

namespace UnstableSort.Crudless.Tests
{
    [SetUpFixture]
    public class UnitTestSetUp
    {
        public static Container Container { get; private set; }
        
        public static ServiceProviderContainer Provider { get; private set; }

        [OneTimeSetUp]
        public static void UnitTestOneTimeSetUp()
        {
            var assemblies = new[] { typeof(UnitTestSetUp).Assembly };
            Container = new Container();

            Container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            Container.Options.ResolveUnregisteredConcreteTypes = false;

            Container.ConfigureAutoMapper();

            Provider = Container.AsServiceProvider();

            ConfigureDatabase(Container);
            Container.ConfigureAutoMapper(assemblies);
            ConfigureFluentValidation(Container, assemblies);

            Container.Register(typeof(FakeInjectable));

            // NOTE: License removed from repository

            //if (!LicenseManager.ValidateLicense(out var licenseErrorMessage))
            //{
            //    throw new Exception(licenseErrorMessage);
            //}

            Crudless.CreateInitializer(Provider, assemblies)
                .ValidateAllRequests(false)
                .UseDynamicMediator(false)
                .UseFluentValidation()
                .UseEntityFrameworkExtensions(BulkExtensions.None)
                .AddInitializer(new SoftDeleteInitializer())
                .Initialize();

            Container.Verify();
        }

        [OneTimeTearDown]
        public static void UnitTestOneTimeTearDown()
        {
            Provider.Dispose();
            Provider = null;
        }

        public static void ConfigureDatabase(Container container)
        {
            container.Register<DbContext>(() =>
            {
                var options = new DbContextOptionsBuilder<FakeDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .Options;

                return new FakeDbContext(options);
            }, 
            Lifestyle.Scoped);
        }

        public static void ConfigureFluentValidation(Container container, Assembly[] assemblies)
        {
            container.Register(typeof(IValidator<>), assemblies);

            ValidatorOptions.Global.CascadeMode = CascadeMode.Stop;
        }
    }
}
