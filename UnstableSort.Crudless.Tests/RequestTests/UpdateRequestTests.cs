using AutoMapper;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Requests;
using UnstableSort.Crudless.Tests.Fakes;

namespace UnstableSort.Crudless.Tests.RequestTests
{
    [TestFixture]
    public class UpdateRequestTests : BaseUnitTest
    {
        User _user;

        [SetUp]
        public void SetUp()
        {
            _user = new User { Name = "TestUser" };

            Context.Add(_user);
            Context.SaveChanges();
        }

        [Test]
        public async Task Handle_UpdateUserByIdRequest_UpdatesUser()
        {
            var request = new UpdateUserByIdRequest
            {
                Id = _user.Id,
                Name = "NewName"
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(_user.Id, response.Result.Id);
            Assert.AreEqual("NewName", response.Result.Name);
        }

        [Test]
        public async Task Handle_UpdateUserByNameRequest_UpdatesUser()
        {
            var request = new UpdateUserByNameRequest
            {
                Name = _user.Name,
                Data = new UserDto { Name = "NewName" }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            var user = Context.Set<User>().FirstOrDefault();
            Assert.NotNull(user);
            Assert.AreEqual(_user.Id, user.Id);
            Assert.AreEqual("NewName", user.Name);
        }

        [Test]
        public async Task Handle_InvalidUpdateUserByNameRequest_ReturnsError()
        {
            var request = new UpdateUserByNameRequest { Name = "NonUser", Data = new UserDto() };
            var response = await Mediator.HandleAsync(request);
            
            Assert.IsTrue(response.HasErrors);
        }

        [Test]
        public async Task Handle_InvalidUpdateUserByIdRequest_ReturnsNull()
        {
            var request = new UpdateUserByIdRequest { Id = 100, Name = string.Empty };
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNull(response.Result);
        }

        [Test]
        public async Task Handle_DefaultUpdateWithoutResponseRequest_UpdatesUser()
        {
            var request = new UpdateRequest<User, UserGetDto>(new UserGetDto
            {
                Id = _user.Id,
                Name = "NewUser"
            });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            var user = Context.Set<User>().First();
            Assert.IsNotNull(user);
            Assert.AreEqual(_user.Id, user.Id);
            Assert.AreEqual(_user.Name, user.Name);
        }

        [Test]
        public async Task Handle_DefaultUpdateWithResponseRequest_UpdatesUser()
        {
            var request = new UpdateRequest<User, UserGetDto, UserGetDto>(new UserGetDto
            {
                Id = _user.Id,
                Name = "NewUser"
            });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(_user.Id, response.Result.Id);
            Assert.AreEqual(_user.Name, response.Result.Name);
        }
        
        [Test]
        public async Task Handle_UpdateRequestWithBuiltSelector_UpdatesUser()
        {
            var request = new UpdateRequest<User, int, UserDto, UserGetDto>(_user.Id, new UserDto { Name = "NewUser" });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(_user.Id, response.Result.Id);
            Assert.AreEqual(_user.Name, response.Result.Name);
        }

        [Test]
        public async Task Handle_UpdateByIdRequest_UpdatesUser()
        {
            var request = new UpdateByIdRequest<User, UserDto, UserGetDto>(
                _user.Id,
                new UserDto { Name = "NewUser" });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(_user.Id, response.Result.Id);
            Assert.AreEqual("NewUser", response.Result.Name);
        }
    }
    
    public class UpdateUserByIdRequest 
        : UserDto, IUpdateRequest<User, UserGetDto>
    {
        public int Id { get; set; }
    }
    
    public class UpdateUserByNameRequest
        : IUpdateRequest<User>
    {
        public string Name { get; set; }
        public UserDto Data { get; set; }
    }

    public class UpdateUserWithoutResponseRequestProfile 
        : RequestProfile<UpdateRequest<User, UserGetDto>>
    {
        public UpdateUserWithoutResponseRequestProfile()
        {
            ForEntity<User>()
                .UseKeys(r => r.Item.Id, e => e.Id);
        }
    }

    public class UpdateUserWithResponseRequestProfile 
        : RequestProfile<UpdateRequest<User, UserGetDto, UserGetDto>>
    {
        public UpdateUserWithResponseRequestProfile()
        {
            ForEntity<User>()
                .SelectWith(builder => builder.Single(request => request.Item.Id, entity => entity.Id));
        }
    }

    public class UpdateUserByIdProfile 
        : RequestProfile<UpdateUserByIdRequest>
    {
        public UpdateUserByIdProfile()
        {
            ForEntity<User>()
                .SelectWith(builder => builder.Single(request => entity => entity.Id == request.Id));

            ConfigureErrors(config => config.FailedToFindInUpdateIsError = false);
        }
    }

    public class UpdateUserByNameProfile 
        : RequestProfile<UpdateUserByNameRequest>
    {
        public UpdateUserByNameProfile()
        {
            ForEntity<User>()
                .SelectWith((Configuration.Builders.Select.SelectorBuilder<UpdateUserByNameRequest, User> builder) => builder.Single(
                    e => e.Name,
                    r => r.Name))
                .UpdateEntityWith((context, entity) =>
                {
                    return Task.FromResult(context.ServiceProvider
                        .ProvideInstance<IMapper>()
                        .Map(context.Request.Data, entity));
                });

            ConfigureErrors(config => config.FailedToFindInUpdateIsError = true);
        }
    }
}