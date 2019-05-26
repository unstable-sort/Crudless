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
    public class MergeRequestTests : BaseUnitTest
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
        public async Task Handle_MergeUsersByIdRequest_MergesAllProvidedUsers()
        {
            var request = new MergeUsersByIdRequest
            {
                Items = new List<UserGetDto>
                {
                    new UserGetDto { Id = _users[0].Id, Name = string.Concat(_users[0].Name, "_New") },
                    new UserGetDto { Id = _users[1].Id, Name = string.Concat(_users[1].Name, "_New") },
                    new UserGetDto { Id = 9999, Name = "NewUser1" },
                    new UserGetDto { Id = 0, Name = "NewUser2" },
                    new UserGetDto { Name = "NewUser3" },
                    null,
                }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.IsNotNull(response.Result.Items);
            Assert.AreEqual(5, response.Result.Items.Count);
            Assert.AreEqual(_users[0].Id, response.Result.Items[0].Id);
            Assert.AreEqual("TestUser1_New", response.Result.Items[0].Name);
            Assert.AreEqual(_users[1].Id, response.Result.Items[1].Id);
            Assert.AreEqual("TestUser2_New", response.Result.Items[1].Name);
            Assert.AreNotEqual(0, response.Result.Items[2].Id);
            Assert.AreEqual("NewUser1", response.Result.Items[2].Name);
            Assert.AreNotEqual(0, response.Result.Items[3].Id);
            Assert.AreEqual("NewUser2", response.Result.Items[3].Name);
            Assert.AreNotEqual(0, response.Result.Items[4].Id);
            Assert.AreEqual("NewUser3", response.Result.Items[4].Name);
            Assert.AreEqual(7, Context.Set<User>().Count());
        }

        [Test]
        public async Task Handle_GenericMergeByIdRequest_UpdatesAllProvidedUsers()
        {
            var request = new MergeByIdRequest<User, UserGetDto, UserGetDto>(new List<UserGetDto>
            {
                new UserGetDto { Id = _users[0].Id, Name = string.Concat(_users[0].Name, "_New") },
                    new UserGetDto { Id = _users[1].Id, Name = string.Concat(_users[1].Name, "_New") },
                    new UserGetDto { Id = 9999, Name = "NewUser1" },
                    new UserGetDto { Id = 0, Name = "NewUser2" },
                    new UserGetDto { Name = "NewUser3" },
                    null,
            });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.IsNotNull(response.Result.Items);
            Assert.AreEqual(5, response.Result.Items.Count);
            Assert.AreEqual(_users[0].Id, response.Result.Items[0].Id);
            Assert.AreEqual("TestUser1_New", response.Result.Items[0].Name);
            Assert.AreEqual(_users[1].Id, response.Result.Items[1].Id);
            Assert.AreEqual("TestUser2_New", response.Result.Items[1].Name);
            Assert.AreNotEqual(0, response.Result.Items[2].Id);
            Assert.AreEqual("NewUser1", response.Result.Items[2].Name);
            Assert.AreNotEqual(0, response.Result.Items[3].Id);
            Assert.AreEqual("NewUser2", response.Result.Items[3].Name);
            Assert.AreNotEqual(0, response.Result.Items[4].Id);
            Assert.AreEqual("NewUser3", response.Result.Items[4].Name);
            Assert.AreEqual(7, Context.Set<User>().Count());
        }
    }
    
    public class MergeUsersByIdRequest
        : IMergeRequest<User, UserGetDto>
    {
        public List<UserGetDto> Items { get; set; }
    }

    public class MergeUsersByIdProfile
        : BulkRequestProfile<MergeUsersByIdRequest, UserGetDto>
    {
        public MergeUsersByIdProfile() : base(request => request.Items)
        {
            ForEntity<User>().UseKeys("Id");
        }
    }
}