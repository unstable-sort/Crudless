using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Requests;
using UnstableSort.Crudless.Tests.Fakes;

namespace UnstableSort.Crudless.Tests.RequestTests
{
    [TestFixture]
    public class CreateAllRequestTests : BaseUnitTest
    {
        [Test]
        public async Task Handle_WithoutResponse_CreatesUsers()
        {
            var request = new CreateUsersWithoutResponseRequest
            {
                Users = new[]
                {
                    new UserDto { Name = "TestUser1" },
                    new UserDto { Name = "TestUser2" }
                }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            
            var users = await Context.Set<User>().ToListAsync();
            Assert.IsNotNull(users);
            Assert.AreEqual(2, users.Count);
            Assert.AreEqual("TestUser1", users[0].Name);
            Assert.AreEqual("TestUser2", users[1].Name);
        }
        
        [Test]
        public async Task Handle_WithResponse_CreatesUsersAndReturnsDtos()
        {
            var request = new CreateUsersWithResponseRequest
            {
                Users = new[]
                {
                    new UserDto { Name = "TestUser1" },
                    new UserDto { Name = "TestUser2" }
                }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);

            Assert.IsNotNull(response.Result);
            Assert.IsNotNull(response.Result.Items);
            Assert.AreEqual(2, response.Result.Items.Count);
            Assert.AreEqual("TestUser1", response.Result.Items[0].Name);
            Assert.AreEqual("TestUser2", response.Result.Items[1].Name);
        }

        [Test]
        public async Task Handle_DefaultWithoutResponse_CreatesUsers()
        {
            var request = new CreateAllRequest<User, UserDto>(new List<UserDto>
            {
                new UserDto { Name = "TestUser1" },
                new UserDto { Name = "TestUser2" }
            });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);

            var users = await Context.Set<User>().ToListAsync();
            Assert.IsNotNull(users);
            Assert.AreEqual(2, users.Count);
            Assert.AreEqual("TestUser1", users[0].Name);
            Assert.AreEqual("TestUser2", users[1].Name);
        }

        [Test]
        public async Task Handle_DefaultWithResponse_CreatesUsersAndReturnsDtos()
        {
            var request = new CreateAllRequest<User, UserDto, UserGetDto>(new List<UserDto>
            {
                new UserDto { Name = "TestUser1" },
                new UserDto { Name = "TestUser2" }
            });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);

            Assert.IsNotNull(response.Result);
            Assert.IsNotNull(response.Result.Items);
            Assert.AreEqual(2, response.Result.Items.Count);
            Assert.AreEqual("TestUser1", response.Result.Items[0].Name);
            Assert.AreEqual("TestUser2", response.Result.Items[1].Name);
        }
    }

    public interface ICreateAllCommon : ICreateAllRequest, ICrudlessRequest
    {
        UserDto[] Users { get; set; }
    }
    
    public class CreateUsersWithResponseRequest : ICreateAllCommon, ICreateAllRequest<User, UserGetDto>
    {
        public UserDto[] Users { get; set; }
    }
    
    public class CreateUsersWithoutResponseRequest : ICreateAllCommon, ICreateAllRequest<User>
    {
        public UserDto[] Users { get; set; }
    }
    
    public class CreateUsersRequestProfile : BulkRequestProfile<ICreateAllCommon, UserDto>
    {
        public CreateUsersRequestProfile()
        {
            ForEntity<User>()
                .CreateEntityWith((context, user) =>
                {
                    return context.ServiceProvider
                        .ProvideInstance<IMapper>()
                        .Map<User>(user);
                })
                .UseRequestItems(request => request.Users);
        }
    }
}