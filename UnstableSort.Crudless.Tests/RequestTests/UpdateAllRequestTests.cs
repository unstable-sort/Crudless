using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Requests;
using UnstableSort.Crudless.Tests.Fakes;

namespace UnstableSort.Crudless.Tests.RequestTests
{
    [TestFixture]
    public class UpdateAllRequestTests : BaseUnitTest
    {
        User[] _users;

        [SetUp]
        public void SetUp()
        {
            _users = new[]
            {
                new User { Name = "TestUser1" },
                new User { Name = "TestUser2" },
                new User { Name = "TestUser3" },
                new User { Name = "TestUser4" }
            };

            Context.AddRange(_users.Cast<object>());
            Context.SaveChanges();
        }

        [Test]
        public async Task Handle_UpdateAllUsersByIdRequest_UpdatesAllProvidedUsers()
        {
            var request = new UpdateAllUsersByIdRequest
            {
                Items = new List<UserGetDto>
                {
                    new UserGetDto { Id = _users[0].Id, Name = string.Concat(_users[0].Name, "_New") },
                    new UserGetDto { Id = _users[1].Id, Name = string.Concat(_users[1].Name, "_New") },
                    new UserGetDto { Id = 9999, Name = "Invalid Id" },
                    new UserGetDto { Id = _users[3].Id, Name = _users[3].Name }
                }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.IsNotNull(response.Result.Items);
            Assert.AreEqual(3, response.Result.Items.Count);
            Assert.AreEqual(_users[0].Id, response.Result.Items[0].Id);
            Assert.AreEqual("TestUser1_New", response.Result.Items[0].Name);
            Assert.AreEqual(_users[1].Id, response.Result.Items[1].Id);
            Assert.AreEqual("TestUser2_New", response.Result.Items[1].Name);
            Assert.AreEqual(_users[3].Id, response.Result.Items[2].Id);
            Assert.AreEqual("TestUser4", response.Result.Items[2].Name);
        }

        [Test]
        public async Task Handle_GenericUpdateAllByIdRequest_UpdatesAllProvidedUsers()
        {
            var request = new UpdateAllByIdRequest<User, UserGetDto, UserGetDto>(new List<UserGetDto>
            {
                new UserGetDto { Id = _users[0].Id, Name = string.Concat(_users[0].Name, "_New") },
                new UserGetDto { Id = _users[1].Id, Name = string.Concat(_users[1].Name, "_New") },
                new UserGetDto { Id = 9999, Name = "Invalid Id" },
                new UserGetDto { Id = _users[3].Id, Name = _users[3].Name }
            });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.IsNotNull(response.Result.Items);
            Assert.AreEqual(3, response.Result.Items.Count);
            Assert.AreEqual(_users[0].Id, response.Result.Items[0].Id);
            Assert.AreEqual("TestUser1_New", response.Result.Items[0].Name);
            Assert.AreEqual(_users[1].Id, response.Result.Items[1].Id);
            Assert.AreEqual("TestUser2_New", response.Result.Items[1].Name);
            Assert.AreEqual(_users[3].Id, response.Result.Items[2].Id);
            Assert.AreEqual("TestUser4", response.Result.Items[2].Name);
        }
    }
    
    public class UpdateAllUsersByIdRequest
        : IUpdateAllRequest<User, UserGetDto>
    {
        public List<UserGetDto> Items { get; set; }
    }

    public class UpdateAllUsersByIdProfile 
        : BulkRequestProfile<UpdateAllUsersByIdRequest, UserGetDto>
    {
        public UpdateAllUsersByIdProfile()
        {
            ForEntity<User>()
                .WithRequestItems(request => request.Items)
                .UseKeys(item => item.Id, entity => entity.Id);
        }
    }
}