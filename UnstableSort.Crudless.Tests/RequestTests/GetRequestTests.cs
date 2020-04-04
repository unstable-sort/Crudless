using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Requests;
using UnstableSort.Crudless.Tests.Fakes;

namespace UnstableSort.Crudless.Tests.RequestTests
{
    [TestFixture]
    public class GetRequestTests : BaseUnitTest
    {
        User _user;
        Site _site;

        [SetUp]
        public void SetUp()
        {
            _user = new User { Name = "TestUser" };
            _site = new Site { Guid = Guid.NewGuid() };

            Context.Add(_user);
            Context.Add(_site);

            Context.SaveChanges();
        }

        [Test]
        public async Task Handle_GenericGetByIdRequest_GetsUser()
        {
            var request = new GetByIdRequest<User, UserGetDto>(_user.Id);
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(_user.Id, response.Result.Id);
            Assert.AreEqual(_user.Name, response.Result.Name);
        }

        [Test]
        public async Task Handle_GenericGetByGuidRequest_GetsSite()
        {
            var request = new GetByGuidRequest<Site, SiteGetDto>(_site.Guid);
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(_site.Id, response.Result.Id);
            Assert.AreEqual(_site.Guid, response.Result.Guid);
        }

        [Test]
        public async Task Handle_GetUserByIdRequest_GetsUser()
        {
            var request = new GetUserByIdRequest { Id = _user.Id };
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(_user.Id, response.Result.Id);
            Assert.AreEqual(_user.Name, response.Result.Name);
        }

        [Test]
        public async Task Handle_GetUserByNameRequest_GetsUser()
        {
            var request = new GetUserByNameRequest { Name = _user.Name.ToUpper() };
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(_user.Id, response.Result.Id);
            Assert.AreEqual(_user.Name, response.Result.Name);
        }

        [Test]
        public async Task Handle_InvalidGetUserByIdRequest_ReturnsError()
        {
            var request = new GetUserByIdRequest { Id = 100 };
            var response = await Mediator.HandleAsync(request);
            
            Assert.IsTrue(response.HasErrors);
        }

        [Test]
        public async Task Handle_InvalidGetUserByNameRequest_ReturnsDefault()
        {
            var request = new GetUserByNameRequest { Name = "NoSuchUser" };
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual("DefaultUser", response.Result.Name);
        }

        [Test]
        public async Task Handle_DefaultGetRequest_GetsUser()
        {
            var request = new GetByIdRequest<User, UserGetDto>(_user.Id);
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(_user.Id, response.Result.Id);
            Assert.AreEqual(_user.Name, response.Result.Name);
        }

        [Test]
        public async Task Handle_GetUserByPrimaryKeyRequest_GetsUser()
        {
            var request = new GetUserByKeyRequest { Id = _user.Id, Name = _user.Name };
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(_user.Id, response.Result.Id);
            Assert.AreEqual(_user.Name, response.Result.Name);
        }
    }
    
    public class GetUserByIdRequest : IGetRequest<User, UserGetDto>
    {
        public int Id { get; set; }
    }
    
    public class GetUserByNameRequest : IGetRequest<User, UserGetDto>
    {
        public string Name { get; set; }
    }
    
    public class GetUserByKeyRequest : IGetRequest<User, UserGetDto>
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class GetUserByIdProfile 
        : RequestProfile<GetUserByIdRequest>
    {
        public GetUserByIdProfile()
        {
            Entity<User>()
                .SelectUsing((r, e) => r.Id == e.Id)
                .WithDefault(new User { Name = "DefaultUser" });
        }
    }

    public class GetUserByNameProfile 
        : RequestProfile<GetUserByNameRequest>
    {
        public GetUserByNameProfile()
        {
            Entity<User>()
                .WithDefault(new User { Name = "DefaultUser" })
                .SelectWith(builder =>
                    builder.Single((request, entity) =>
                        string.Equals(entity.Name, request.Name, StringComparison.InvariantCultureIgnoreCase)));

            ConfigureErrors(config => config.FailedToFindInGetIsError = false);
        }
    }

    public class GetUserByKeyProfile 
        : RequestProfile<GetUserByKeyRequest>
    {
        public GetUserByKeyProfile()
        {
            Entity<User>().UseKeys("Id");
        }
    }
}