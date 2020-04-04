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
    public class DeleteRequestTests : BaseUnitTest
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
        public async Task Handle_DeleteUserByIdRequest_DeletesUser()
        {
            var request = new DeleteUserByIdRequest
            {
                Id = _user.Id
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.IsTrue(response.Result.IsDeleted);
        }

        [Test]
        public async Task Handle_DeleteUserByNameRequest_DeletesUser()
        {
            var request = new DeleteUserByNameRequest
            {
                Name = _user.Name
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            var user = Context.Set<User>().FirstOrDefault();
            Assert.NotNull(user);
            Assert.IsTrue(user.IsDeleted);
        }

        [Test]
        public async Task Handle_InvalidDeleteUserByNameRequest_ReturnsError()
        {
            var request = new DeleteUserByNameRequest { Name = "NonUser" };
            var response = await Mediator.HandleAsync(request);
            
            Assert.IsTrue(response.HasErrors);
            Assert.IsFalse(Context.Set<User>().First().IsDeleted);
        }

        [Test]
        public async Task Handle_InvalidDeleteUserByIdRequest_ReturnsNull()
        {
            var request = new DeleteUserByIdRequest { Id = 100 };
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsFalse(Context.Set<User>().First().IsDeleted);
            Assert.IsNull(response.Result);
        }

        [Test]
        public async Task Handle_DefaultDeleteByIdWithoutResponse_DeletesUser()
        {
            var request = new DeleteByIdRequest<User>(_user.Id);

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            var user = Context.Set<User>().First();
            Assert.IsNotNull(user);
            Assert.AreEqual(_user.Id, user.Id);
            Assert.IsTrue(user.IsDeleted);
        }

        [Test]
        public async Task Handle_DefaultDeleteByIdWithResponse_DeletesUserAndReturnsDto()
        {
            var request = new DeleteByIdRequest<User, UserGetDto>(_user.Id);

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(_user.Id, response.Result.Id);
            Assert.IsTrue(response.Result.IsDeleted);
        }
    }
    
    public class DeleteUserByIdRequest 
        : IDeleteRequest<User, UserGetDto>
    {
        public int Id { get; set; }
    }
    
    public class DeleteUserByNameRequest
        : IDeleteRequest<User>
    {
        public string Name { get; set; }
    }
    
    public class DeleteUserByIdProfile 
        : RequestProfile<DeleteUserByIdRequest>
    {
        public DeleteUserByIdProfile()
        {
            Entity<User>().SelectBy("Id");

            ConfigureErrors(config => config.FailedToFindInDeleteIsError = false);
        }
    }

    public class DeleteUserByNameProfile 
        : RequestProfile<DeleteUserByNameRequest>
    {
        public DeleteUserByNameProfile()
        {
            Entity<User>()
                .SelectBy(request => entity => string.Equals(entity.Name, request.Name, StringComparison.InvariantCultureIgnoreCase));

            ConfigureErrors(config => config.FailedToFindInDeleteIsError = true);
        }
    }
}