﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Configuration.Builders.Select;
using UnstableSort.Crudless.Requests;
using UnstableSort.Crudless.Tests.Fakes;

namespace UnstableSort.Crudless.Tests.RequestTests
{
    [TestFixture]
    public class CreateRequestTests : BaseUnitTest
    {
        [Test]
        public async Task Handle_WithoutResponse_CreatesUser()
        {
            var request = new CreateUserWithoutResponseRequest
            {
                User = new UserDto { Name = "TestUser" }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());

            var user = Context.Set<User>().FirstOrDefault();
            Assert.IsNotNull(user);
            Assert.AreEqual("TestUser", user.Name);
        }

        [Test]
        public async Task Handle_DerivedWithoutResponse_CreatesUserUsingBaseConfig()
        {
            var request = new DerivedCreateUserWithoutResponseRequest
            {
                User = new UserDto { Name = "TestUser" },
                OtherStuff = new object()
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());

            var user = Context.Set<User>().FirstOrDefault();
            Assert.IsNotNull(user);
            Assert.AreEqual("TestUser", user.Name);
        }

        [Test]
        public async Task Handle_WithResponse_CreatesUserAndReturnsDto()
        {
            var request = new CreateUserWithResponseRequest
            {
                Name = "TestUser"
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());
            Assert.IsNotNull(response.Result);
            Assert.AreEqual("TestUser_Modified", response.Result.Name);
            Assert.AreEqual(response.Result.Id, Context.Set<User>().First().Id);
        }

        [Test]
        public async Task Handle_DefaultWithoutResponse_CreatesUser()
        {
            var request = new CreateRequest<User, UserDto>(new UserDto
            {
                Name = "TestUser"
            });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());

            var user = Context.Set<User>().FirstOrDefault();
            Assert.IsNotNull(user);
            Assert.AreEqual("TestUser", user.Name);
        }

        [Test]
        public async Task Handle_DefaultWithResponse_CreatesUserAndReturnsDto()
        {
            var request = new CreateRequest<User, UserDto, UserGetDto>(new UserDto { Name = "TestUser" });
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());
            Assert.IsNotNull(response.Result);
            Assert.AreEqual("TestUser", response.Result.Name);
            Assert.AreEqual(response.Result.Id, Context.Set<User>().First().Id);

            var requestTest = new CreateUserWithoutResponseRequest();

            var kRequest = Key.MakeKeys<CreateUserWithoutResponseRequest, string>(x => x.User.Name);
            var kEntity = Key.MakeKeys<User, string>(x => x.Name);
            var selector = SelectorHelpers.BuildSingle<CreateUserWithoutResponseRequest, User>(kRequest[0], kEntity[0]);
            var test1 = selector.Get<User>()(new CreateUserWithoutResponseRequest());
            var test2 = Test(request => user => request.User.Name == user.Name, requestTest);
        }

        public Expression<Func<User, bool>> Test(Func<CreateUserWithoutResponseRequest, Expression<Func<User, bool>>> selectorTest, 
            CreateUserWithoutResponseRequest request)
        {
            return selectorTest(request);
        }
    }
    
    public class CreateUserWithResponseRequest : UserDto, ICreateRequest<User, UserGetDto>
    { }
    
    public class CreateUserWithoutResponseRequest : ICreateRequest<User>
    {
        public UserDto User { get; set; }
    }
    
    public class DerivedCreateUserWithoutResponseRequest : CreateUserWithoutResponseRequest
    {
        public object OtherStuff { get; set; }
    }

    public class CreateUserWithResponseProfile
        : RequestProfile<CreateUserWithResponseRequest>
    {
        public CreateUserWithResponseProfile()
        {
            ForEntity<User>()
                .CreateResultWith((ctx, user) =>
                {
                    var mapper = ctx.ServiceProvider.ProvideInstance<IObjectMapper>();
                    var result = mapper.Map<User, UserGetDto>(user);
                    result.Name += "_Modified";
                    return result;
                });
        }
    }

    public class CreateUserWithoutResponseProfile 
        : RequestProfile<CreateUserWithoutResponseRequest>
    {
        public CreateUserWithoutResponseProfile()
        {
            ForEntity<User>()
                .CreateEntityWith(context =>
                {
                    return context.ServiceProvider
                        .ProvideInstance<IObjectMapper>()
                        .Map<UserDto, User>(context.Request.User);
                });
        }
    }
}