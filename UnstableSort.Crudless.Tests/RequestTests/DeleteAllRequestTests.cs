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
    public class DeleteAllRequestTests : BaseUnitTest
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

            Context.AddRange(_users);
            Context.SaveChanges();
        }

        [Test]
        public async Task Handle_DeleteAllUsersByIdRequest_DeletesAllFilteredUsers()
        {
            var request = new DeleteAllUsersByIdRequest
            {
                Ids = new List<int>
                {
                    _users[0].Id,
                    _users[2].Id
                }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.IsNotNull(response.Result.Items);
            Assert.AreEqual(2, response.Result.Items.Count);
            Assert.IsTrue(response.Result.Items[0].IsDeleted);
            Assert.IsTrue(response.Result.Items[1].IsDeleted);
            Assert.IsFalse(Context.Set<User>().First(x => x.Name == "TestUser2").IsDeleted);
            Assert.IsFalse(Context.Set<User>().First(x => x.Name == "TestUser4").IsDeleted);
        }

        [Test]
        public async Task Handle_DeleteAllByIdRequest_DeletesAllFilteredUsers()
        {
            var request = new DeleteAllByIdRequest<User, UserGetDto>
            (
                new List<int>
                {
                    _users[0].Id,
                    _users[2].Id
                }
            );

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.IsNotNull(response.Result.Items);
            Assert.AreEqual(2, response.Result.Items.Count);
            Assert.IsTrue(response.Result.Items[0].IsDeleted);
            Assert.IsTrue(response.Result.Items[1].IsDeleted);
            Assert.IsFalse(Context.Set<User>().First(x => x.Name == "TestUser2").IsDeleted);
            Assert.IsFalse(Context.Set<User>().First(x => x.Name == "TestUser4").IsDeleted);
        }
    }
    
    public class DeleteAllUsersByIdRequest
        : IDeleteAllRequest<User, UserGetDto>
    {
        public List<int> Ids { get; set; }
    }
    
    public class DeleteAllUsersByIdProfile 
        : RequestProfile<DeleteAllUsersByIdRequest>
    {
        public DeleteAllUsersByIdProfile()
        {
            ForEntity<User>()
                .AddContainsFilter(r => r.Ids, e => e.Id);

            UseErrorConfiguration(config => config.FailedToFindInDeleteIsError = false);
        }
    }
}