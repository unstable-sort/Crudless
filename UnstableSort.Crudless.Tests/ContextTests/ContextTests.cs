﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using NUnit.Framework;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Context;
using UnstableSort.Crudless.Context.Utilities;
using UnstableSort.Crudless.Mediator;
using UnstableSort.Crudless.Requests;
using UnstableSort.Crudless.Integration.SimpleInjector;
using UnstableSort.Crudless.Tests.Fakes;

namespace UnstableSort.Crudless.Tests.ContextTests
{
    [TestFixture]
    public class ContextTests
    {
        private Scope _scope;

        protected Container Container;

        protected ServiceProviderContainer Provider { get; set; }

        protected IMediator Mediator { get; private set; }

        protected IEntityContext Context { get; private set; }

        protected IObjectMapper ObjectMapper { get; private set; }

        protected IMapper Mapper { get; private set; }
        
        [SetUp]
        public void SetUp()
        {
            var assemblies = new[] { typeof(UnitTestSetUp).Assembly };
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.Options.ResolveUnregisteredConcreteTypes = false;

            Provider = container.AsServiceProvider();

            container.ConfigureAutoMapper(assemblies);
            
            UnitTestSetUp.ConfigureDatabase(container);
            
            Crudless.CreateInitializer(Provider, assemblies)
                .UseAutoMapper()
                .Initialize();

            container.Options.AllowOverridingRegistrations = true;

            container.Register(typeof(FakeInjectable));

            container.Register<IEntityContext, InMemoryContext>();

            var dataAgent = new InMemoryDataAgent();
            container.RegisterInstance<ICreateDataAgent>(dataAgent);
            container.RegisterInstance<IUpdateDataAgent>(dataAgent);
            container.RegisterInstance<IDeleteDataAgent>(dataAgent);
            container.RegisterInstance<IBulkCreateDataAgent>(dataAgent);
            container.RegisterInstance<IBulkUpdateDataAgent>(dataAgent);
            container.RegisterInstance<IBulkDeleteDataAgent>(dataAgent);

            container.Options.AllowOverridingRegistrations = false;

            _scope = AsyncScopedLifestyle.BeginScope(container);

            Mediator = _scope.GetInstance<IMediator>();
            Context = _scope.GetInstance<IEntityContext>();
            ObjectMapper = _scope.GetInstance<IObjectMapper>();
            Mapper = _scope.GetInstance<IMapper>();

            InMemoryContext.Clear();

            Container = container;
        }

        [TearDown]
        public void TearDown()
        {
            Provider.Dispose();
            _scope.Dispose();
        }

        [Test]
        public void QueryProvider_OnCustomContext_IsAsyncQueryProvider()
        {
            var query = Context.Set<User>() as IQueryable<User>;

            Assert.IsTrue(typeof(IAsyncQueryProvider).IsAssignableFrom(query.Provider.GetType()));
        }

        [Test]
        public void QueryProvider_OnCustomContext_IsAdaptedToAsync()
        {
            var query = Context.Set<User>() as IQueryable<User>;

            Assert.IsAssignableFrom(typeof(AsyncQueryProviderExtensions.AsyncAdaptedQueryProvider),
                query.Provider);
        }

        [Test]
        public void Query_FromCustomContext_IsAsyncEnumerable()
        {
            var query = Context.Set<User>().Where(x => true);

            Assert.IsTrue(typeof(IAsyncEnumerable<User>).IsAssignableFrom(query.GetType()));
        }

        [Test]
        public async Task GetEnumerator_FromCustomContextQuery_AllowsIteration()
        {
            await Context.Set<User>().CreateAsync(new DataContext<User>(null), 
                new[]
                {
                    new User { Name = "User1" },
                    new User { Name = "User2" },
                    new User { Name = "User3" },
                });

            var enumerator = Context.Set<User>().Where(x => x.Name.StartsWith("User")).GetEnumerator();
            var results = new List<string>();

            while (enumerator.MoveNext())
                results.Add(enumerator.Current.Name);
            
            Assert.AreEqual(3, results.Count);
            Assert.IsTrue(results.Contains("User1"));
            Assert.IsTrue(results.Contains("User2"));
            Assert.IsTrue(results.Contains("User3"));
        }

        [Test]
        public async Task GetBoxedEnumerator_FromCustomContextQuery_AllowsIteration()
        {
            await Context.Set<User>().CreateAsync(new DataContext<User>(null),
                new[]
                {
                    new User { Name = "User1" },
                    new User { Name = "User2" },
                    new User { Name = "User3" },
                });

            IEnumerator enumerator = ((IQueryable)Context.Set<User>().Where(x => x.Name.StartsWith("User"))).GetEnumerator();
            var results = new List<string>();

            while (enumerator.MoveNext())
                results.Add(((User)enumerator.Current).Name);

            Assert.AreEqual(3, results.Count);
            Assert.IsTrue(results.Contains("User1"));
            Assert.IsTrue(results.Contains("User2"));
            Assert.IsTrue(results.Contains("User3"));
        }
        
        [Test]
        public async Task Handle_OnCustomContext_CanProcessRequests()
        {
            var createRequest = new CreateRequest<User, UserDto, UserGetDto>(
                new UserDto { Name = "Test" });

            await Mediator.HandleAsync(createRequest);
            
            Assert.IsNotNull(Context.Set<User>());

            var users = await Context.Set<User>().CountAsync();
            Assert.AreEqual(1, users);

            var getRequest = new GetByIdRequest<User, UserGetDto>(1);
            var user = await Mediator.HandleAsync(getRequest);

            Assert.IsNotNull(user);
            Assert.IsNotNull(user.Result);
            Assert.AreEqual(createRequest.Item.Name, user.Result.Name);
        }

        [Test]
        public async Task FirstOrDefaultAsync_OnCustomContext_Succeeds()
        {
            Assert.IsNull(await Context.Set<User>().FirstOrDefaultAsync());

            await Context.Set<User>().CreateAsync(new DataContext<User>(null),
                new[]
                {
                    new User { Name = "User1" },
                    new User { Name = "User2" },
                    new User { Name = "User3" },
                });

            var name = (await Context.Set<User>().FirstOrDefaultAsync())?.Name;
            Assert.AreEqual("User1", name);

            name = (await Context.Set<User>().FirstOrDefaultAsync(x => x.Name == "User2"))?.Name;
            Assert.AreEqual("User2", name);
            
            Assert.IsNull(await Context.Set<User>().FirstOrDefaultAsync(x => x.Name == "NonExistent"));
        }

        [Test]
        public async Task SingleOrDefaultAsync_OnCustomContext_Succeeds()
        {
            Assert.IsNull(await Context.Set<User>().SingleOrDefaultAsync());

            await Context.Set<User>().CreateAsync(new DataContext<User>(null), new User { Name = "User1" });

            var name = (await Context.Set<User>().SingleOrDefaultAsync())?.Name;
            Assert.AreEqual("User1", name);

            await Context.Set<User>().CreateAsync(
                new DataContext<User>(null), 
                new[]
                {
                    new User { Name = "User2" },
                    new User { Name = "User3" },
                });

            name = (await Context.Set<User>().SingleOrDefaultAsync(x => x.Name == "User2"))?.Name;
            Assert.AreEqual("User2", name);

            Assert.Throws(typeof(InvalidOperationException),
                () => Context.Set<User>().SingleOrDefaultAsync());

            Assert.Throws(typeof(InvalidOperationException),
                () => Context.Set<User>().SingleOrDefaultAsync(x => x.Name.StartsWith("User")));
        }

        [Test]
        public async Task ProjectSingleOrDefaultAsync_OnCustomContext_Succeeds()
        {
            Assert.IsNull(await Context.Set<User>()
                .ProjectSingleOrDefaultAsync<User, PUser>(ObjectMapper));

            await Context.Set<User>().CreateAsync(new DataContext<User>(null), new User { Name = "User1" });

            var name = (await Context.Set<User>()
                .ProjectSingleOrDefaultAsync<User, PUser>(ObjectMapper))?.Name;

            Assert.AreEqual("User1", name);

            await Context.Set<User>().CreateAsync(new DataContext<User>(null), 
                new[]
                {
                    new User { Name = "User2" },
                    new User { Name = "User3" },
                });

            name = (await Context.Set<User>()
                .ProjectSingleOrDefaultAsync<User, PUser>(ObjectMapper, x => x.Name == "User2"))?.Name;

            Assert.AreEqual("User2", name);

            Assert.Throws(typeof(InvalidOperationException),
                () => Context.Set<User>()
                    .ProjectSingleOrDefaultAsync<User, PUser>(ObjectMapper));

            Assert.Throws(typeof(InvalidOperationException),
                () => Context.Set<User>()
                    .ProjectSingleOrDefaultAsync<User, PUser>(ObjectMapper, x => x.Name.StartsWith("User")));
        }

        [Test]
        public async Task CountAsync_OnCustomContext_Succeeds()
        {
            Assert.AreEqual(0, await Context.Set<User>().CountAsync());

            await Context.Set<User>().CreateAsync(
                new DataContext<User>(null),
                new[]
                {
                    new User { Name = "User1" },
                    new User { Name = "User2" },
                    new User { Name = "AUser3" },
                });

            var count = await Context.Set<User>().CountAsync();
            Assert.AreEqual(3, count);

            count = await Context.Set<User>().CountAsync(x => x.Name.StartsWith("User"));
            Assert.AreEqual(2, count);

            count = await Context.Set<User>().CountAsync(x => x.Name == "NonExistent");
            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task ToArrayAsync_OnCustomContext_Succeeds()
        {
            await Context.Set<User>().CreateAsync(
                new DataContext<User>(null), 
                new[]
                {
                    new User { Name = "User1" },
                    new User { Name = "User2" },
                    new User { Name = "User3" },
                });

            var results = await Context.Set<User>().ToArrayAsync();

            Assert.AreEqual(3, results.Length);
            Assert.IsTrue(results.Select(x => x.Name).Contains("User1"));
            Assert.IsTrue(results.Select(x => x.Name).Contains("User2"));
            Assert.IsTrue(results.Select(x => x.Name).Contains("User3"));
        }

        [Test]
        public async Task ProjectToArrayAsync_OnCustomContext_Succeeds()
        {
            await Context.Set<User>().CreateAsync(
                new DataContext<User>(null), 
                new[]
                {
                    new User { Name = "User1" },
                    new User { Name = "User2" },
                    new User { Name = "User3" },
                });

            var results = await Context.Set<User>().ProjectToArrayAsync<User, PUser>(ObjectMapper);

            Assert.AreEqual(3, results.Length);
            Assert.AreEqual(typeof(PUser[]), results.GetType());
            Assert.IsTrue(results.Select(x => x.Name).Contains("User1"));
            Assert.IsTrue(results.Select(x => x.Name).Contains("User2"));
            Assert.IsTrue(results.Select(x => x.Name).Contains("User3"));
        }

        [Test]
        public async Task ToListAsync_OnCustomContext_Succeeds()
        {
            Assert.IsNull(await Context.Set<User>().FirstOrDefaultAsync());

            await Context.Set<User>().CreateAsync(
                new DataContext<User>(null), 
                new[]
                {
                    new User { Name = "User1" },
                    new User { Name = "User2" },
                    new User { Name = "User3" },
                });

            var results = await Context.Set<User>().ToListAsync();

            Assert.AreEqual(3, results.Count);
            Assert.IsTrue(results.Select(x => x.Name).Contains("User1"));
            Assert.IsTrue(results.Select(x => x.Name).Contains("User2"));
            Assert.IsTrue(results.Select(x => x.Name).Contains("User3"));
        }

        [Test]
        public async Task ProjectToListAsync_OnCustomContext_Succeeds()
        {
            await Context.Set<User>().CreateAsync(
                new DataContext<User>(null), 
                new[]
                {
                    new User { Name = "User1" },
                    new User { Name = "User2" },
                    new User { Name = "User3" },
                });

            var results = await Context.Set<User>().ProjectToListAsync<User, PUser>(ObjectMapper);

            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(typeof(List<PUser>), results.GetType());
            Assert.IsTrue(results.Select(x => x.Name).Contains("User1"));
            Assert.IsTrue(results.Select(x => x.Name).Contains("User2"));
            Assert.IsTrue(results.Select(x => x.Name).Contains("User3"));
        }

        public class PUser
        {
            public string Name { get; set; }
        }
    }

    public class ContextTestsProfiles : Profile
    {
        public ContextTestsProfiles()
        {
            CreateMap<User, ContextTests.PUser>();
        }
    }
}
