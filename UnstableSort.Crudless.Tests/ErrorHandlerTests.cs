﻿using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Errors;
using UnstableSort.Crudless.Integration.SimpleInjector;
using UnstableSort.Crudless.Mediator;
using UnstableSort.Crudless.Requests;
using UnstableSort.Crudless.Tests.Fakes;

namespace UnstableSort.Crudless.Tests
{
    [TestFixture]
    public class DefaultErrorHandlerTests
    {
        private Scope _scope;

        protected Container Container;

        protected ServiceProviderContainer Provider;

        protected IMediator Mediator { get; private set; }

        protected DbContext Context { get; private set; }

        [TearDown]
        public void TearDown()
        {
            _scope.Dispose();

            Provider.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            var assemblies = new[] { typeof(UnitTestSetUp).Assembly };
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.Options.ResolveUnregisteredConcreteTypes = false;

            Provider = container.AsServiceProvider();

            UnitTestSetUp.ConfigureDatabase(container);

            container.ConfigureAutoMapper(assemblies);

            Crudless.CreateInitializer(Provider, assemblies)
                .UseAutoMapper()
                .UseEntityFramework()
                .Initialize();
            
            container.Options.AllowOverridingRegistrations = true;
            container.Register<IErrorHandler, TestErrorHandler>(Lifestyle.Singleton);
            container.Options.AllowOverridingRegistrations = false;

            container.Register(typeof(FakeInjectable));

            _scope = AsyncScopedLifestyle.BeginScope(container);

            Mediator = _scope.GetInstance<IMediator>();
            Context = _scope.GetInstance<DbContext>();
            Context.Database.EnsureDeleted();
        
            Container = container;
        }

        [Test]
        public async Task Handle_FailingRequest_UsesTestErrorHandler()
        {
            Exception exception = null;

            try
            {
                var request = new GetByIdRequest<NonEntity, NonEntity>(1);
                await Mediator.HandleAsync(request);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual("Failed to find entity.", exception.Message);
        }
    }

    [TestFixture]
    public class CustomErrorHandlerTests : BaseUnitTest
    {
        [Test]
        public async Task Handle_UseDefaultErrorHandler_UsesDefaultErrorHandler()
        {
            var request = new UseDefaultErrorHandler { Id = 1 };
            var response = await Mediator.HandleAsync(request);
            
            Assert.IsTrue(response.HasErrors);
            Assert.AreEqual("Failed to find entity.", response.Errors[0].ErrorMessage);
        }

        [Test]
        public async Task Handle_UseCustomErrorHandlerForRequest_UsesCustomErrorHandler()
        {
            Exception exception = null;

            try
            {
                var request = new UseCustomErrorHandlerForRequest { Id = 1 };
                await Mediator.HandleAsync(request);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual("Failed to find entity.", exception.Message);
        }

        [Test]
        public async Task Handle_UseCustomErrorHandlerForEntity_UsesCustomErrorHandler()
        {
            Exception exception = null;

            try
            {
                var request = new UseCustomErrorHandlerForEntity { Id = 1 };
                await Mediator.HandleAsync(request);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual("Failed to find entity.", exception.Message);
        }
    }

    [TestFixture]
    public class TestTypeErrorHandlerTests : BaseUnitTest
    {
        public static ErrorTypeTestHandler TypeTestHandler { get; }
            = new ErrorTypeTestHandler();

        [Test]
        public async Task Handle_WithFindFailure_DispatchesFailedToFindError()
        {
            TypeTestHandler.Clear();

            var request = new FindFailureTestRequest();
            await Mediator.HandleAsync(request);

            Assert.NotNull(TypeTestHandler.LastError);
            Assert.AreEqual(typeof(FailedToFindError), TypeTestHandler.LastError.GetType());
            Assert.IsNull(TypeTestHandler.LastError.Exception);
        }

        [Test]
        public async Task Handle_WithHookFailure_DispatchesHookFailedError()
        {
            TypeTestHandler.Clear();

            var request = new HookFailureTestRequest();
            await Mediator.HandleAsync(request);

            Assert.NotNull(TypeTestHandler.LastError);
            Assert.AreEqual(typeof(HookFailedError), TypeTestHandler.LastError.GetType());
            Assert.AreEqual("HookTest", TypeTestHandler.LastErrorMessage);
        }

        [Test]
        public async Task Handle_WithCreateResultFailure_DispatchesCreateResultFailedError()
        {
            TypeTestHandler.Clear();

            var request = new CreateResultFailureTestRequest();
            await Mediator.HandleAsync(request);

            Assert.NotNull(TypeTestHandler.LastError);
            Assert.AreEqual(typeof(CreateResultFailedError), TypeTestHandler.LastError.GetType());
            Assert.AreEqual("CreateResultTest", TypeTestHandler.LastErrorMessage);
        }

        [Test]
        public async Task Handle_WithCreateEntityFailure_DispatchesCreateResultFailedError()
        {
            TypeTestHandler.Clear();

            var request = new CreateEntityFailureTestRequest();
            await Mediator.HandleAsync(request);

            Assert.NotNull(TypeTestHandler.LastError);
            Assert.AreEqual(typeof(CreateEntityFailedError), TypeTestHandler.LastError.GetType());
            Assert.AreEqual("CreateEntityTest", TypeTestHandler.LastErrorMessage);
        }

        [Test]
        public async Task Handle_WithUpdateEntityFailure_DispatchesCreateResultFailedError()
        {
            TypeTestHandler.Clear();

            var entity = Context.Set<NonEntity>().Add(new NonEntity()).Entity;
            await Context.SaveChangesAsync();

            var request = new UpdateEntityFailureTestRequest { Id = entity.Id };
            await Mediator.HandleAsync(request);

            Assert.NotNull(TypeTestHandler.LastError);
            Assert.AreEqual(typeof(UpdateEntityFailedError), TypeTestHandler.LastError.GetType());
            Assert.AreEqual("UpdateEntityTest", TypeTestHandler.LastErrorMessage);
        }

        [Test]
        public async Task Handle_WithCancelation_DispatchesRequestCanceledError()
        {
            TypeTestHandler.Clear();

            var request = new RequestCanceledTestRequest { Items = new[] { 1, 2, 3 } };
            await Mediator.HandleAsync(request);

            Assert.NotNull(TypeTestHandler.LastError);
            Assert.AreEqual(typeof(RequestCanceledError), TypeTestHandler.LastError.GetType());
            Assert.AreEqual("CancelTest", TypeTestHandler.LastErrorMessage);
        }
    }

    [DoNotValidate]
    public class UseDefaultErrorHandler
        : IGetRequest<NonEntity, NonEntity>
    {
        public int Id { get; set; }
    }

    public class UseDefaultErrorHandlerProfile
        : RequestProfile<UseDefaultErrorHandler>
    {
        public UseDefaultErrorHandlerProfile()
        {
            ForEntity<NonEntity>()
                .SelectBy("Id");
        }
    }

    [DoNotValidate]
    public class UseCustomErrorHandlerForRequest
        : IGetRequest<NonEntity, NonEntity>
    {
        public int Id { get; set; }
    }

    public class UseCustomErrorHandlerForRequestProfile
        : RequestProfile<UseCustomErrorHandlerForRequest>
    {
        public UseCustomErrorHandlerForRequestProfile()
        {
            UseErrorConfiguration(config => 
                config.ErrorHandlerFactory = () => new TestErrorHandler());

            ForEntity<NonEntity>()
                .SelectBy("Id");
        }
    }

    [DoNotValidate]
    public class UseCustomErrorHandlerForEntity
        : IGetRequest<NonEntity, NonEntity>
    { 
        public int Id { get; set; }
    }

    public class UseCustomErrorHandlerForEntityProfile
        : RequestProfile<UseCustomErrorHandlerForEntity>
    {
        public UseCustomErrorHandlerForEntityProfile()
        {
            ForEntity<NonEntity>()
                .SelectBy("Id")
                .UseErrorHandlerFactory(() => new TestErrorHandler());
        }
    }

    [DoNotValidate]
    public class FindFailureTestRequest
        : IGetRequest<NonEntity, NonEntity>
    {
        public int Id { get; set; }
    }

    public class FindFailureTestProfile
        : RequestProfile<FindFailureTestRequest>
    {
        public FindFailureTestProfile()
        {
            UseErrorConfiguration(config => config.FailedToFindInGetIsError = true);

            ForEntity<NonEntity>()
                .UseKeys("Id")
                .UseErrorHandlerFactory(() => TestTypeErrorHandlerTests.TypeTestHandler)
                .AddEntityHook((r, e) => throw new InvalidOperationException("FindTest"));
        }
    }

    [DoNotValidate]
    public class HookFailureTestRequest
        : IGetRequest<NonEntity, NonEntity>
    {
        public int Id { get; set; }
    }

    public class HookFailureTestProfile
        : RequestProfile<HookFailureTestRequest>
    {
        public HookFailureTestProfile()
        {
            UseErrorConfiguration(config => config.FailedToFindInGetIsError = false);

            ForEntity<NonEntity>()
                .UseKeys("Id")
                .UseErrorHandlerFactory(() => TestTypeErrorHandlerTests.TypeTestHandler)
                .AddEntityHook((r, e) => throw new InvalidOperationException("HookTest"));
        }
    }

    [DoNotValidate]
    public class CreateResultFailureTestRequest
        : IGetRequest<NonEntity, NonEntity>
    {
        public int Id { get; set; }
    }

    public class CreateResultFailureTestProfile
        : RequestProfile<CreateResultFailureTestRequest>
    {
        public CreateResultFailureTestProfile()
        {
            NonEntity createResult(RequestContext<CreateResultFailureTestRequest> context, NonEntity _) 
                => throw new InvalidOperationException("CreateResultTest");

            UseErrorConfiguration(config => config.FailedToFindInGetIsError = false);

            ForEntity<NonEntity>()
                .UseKeys("Id")
                .UseErrorHandlerFactory(() => TestTypeErrorHandlerTests.TypeTestHandler)
                .CreateResultWith(createResult);
        }
    }

    [DoNotValidate]
    public class CreateEntityFailureTestRequest
        : ICreateRequest<NonEntity, NonEntity>
    {
        public int Id { get; set; }
    }

    public class CreateEntityFailureTestProfile
        : RequestProfile<CreateEntityFailureTestRequest>
    {
        public CreateEntityFailureTestProfile()
        {
            Func<RequestContext<CreateEntityFailureTestRequest>, NonEntity> createEntity
                = r => throw new InvalidOperationException("CreateEntityTest");
            
            ForEntity<NonEntity>()
                .UseErrorHandlerFactory(() => TestTypeErrorHandlerTests.TypeTestHandler)
                .CreateEntityWith(createEntity);
        }
    }

    [DoNotValidate]
    public class UpdateEntityFailureTestRequest
        : IUpdateRequest<NonEntity, NonEntity>
    {
        public int Id { get; set; }
    }

    public class UpdateEntityFailureTestProfile
        : RequestProfile<UpdateEntityFailureTestRequest>
    {
        public UpdateEntityFailureTestProfile()
        {
            Func<RequestContext<UpdateEntityFailureTestRequest>, NonEntity, NonEntity> updateEntity
                = (r, e) => throw new InvalidOperationException("UpdateEntityTest");
            
            ForEntity<NonEntity>()
                .UseKeys("Id")
                .UseErrorHandlerFactory(() => TestTypeErrorHandlerTests.TypeTestHandler)
                .UpdateEntityWith(updateEntity);
        }
    }

    [DoNotValidate]
    public class RequestCanceledTestRequest
        : ICreateAllRequest<NonEntity, NonEntity>
    {
        public int[] Items { get; set; } = Array.Empty<int>();
    }

    public class RequestCanceledTestProfile
        : BulkRequestProfile<RequestCanceledTestRequest, int>
    {
        public RequestCanceledTestProfile()
        {
            Func<int, NonEntity> createEntity 
                = i => throw new OperationCanceledException("CancelTest");

            ForEntity<NonEntity>()
                .UseRequestItems(x => x.Items)
                .CreateEntityWith(createEntity)
                .UseErrorHandlerFactory(() => TestTypeErrorHandlerTests.TypeTestHandler);
        }
    }

    public class ErrorTypeTestHandler : ErrorHandler
    {
        public CrudlessError LastError { get; private set; }

        public string LastErrorMessage { get; private set; }

        public void Clear()
        {
            LastError = null;
            LastErrorMessage = null;
        }

        private Response GenericHandle(CrudlessError error)
        {
            LastError = error;
            LastErrorMessage = error.Exception?.Message;
            
            return Response.Success();
        }

        protected override Response HandleError(FailedToFindError error)
            => GenericHandle(error);

        protected override Response HandleError(CrudlessError error)
            => GenericHandle(error);

        protected override Response HandleError(HookFailedError error)
            => GenericHandle(error);

        protected override Response HandleError(RequestCanceledError error)
            => GenericHandle(error);

        protected override Response HandleError(RequestFailedError error)
            => GenericHandle(error);

        protected override Response HandleError(CreateEntityFailedError error)
            => GenericHandle(error);

        protected override Response HandleError(UpdateEntityFailedError error)
            => GenericHandle(error);

        protected override Response HandleError(CreateResultFailedError error)
            => GenericHandle(error);
    }
}
