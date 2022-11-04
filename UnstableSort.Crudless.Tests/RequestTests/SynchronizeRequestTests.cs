﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using NUnit.Framework;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Requests;
using UnstableSort.Crudless.Tests.Fakes;

namespace UnstableSort.Crudless.Tests.RequestTests
{
    [TestFixture]
    public class SynchronizeRequestTests : BaseUnitTest
    {
        User[] _users;

        [SetUp]
        public void SetUp()
        {
            _users = new[]
            {
                new User { Name = "TestUser1" },
                new User { Name = "TestUser2" },
                new User { Name = "TestUser3Filtered" },
                new User { Name = "TestUser4" },
                new User { Name = "TestUser5Filtered" }
            };

            Context.AddRange(_users.Cast<object>());
            Context.SaveChanges();
        }
        
        [Test]
        public async Task Handle_SynchronizeUsersByIdRequest_SynchronizesAllProvidedUsers()
        {
            var request = new SynchronizeUsersByIdRequest
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
            Assert.AreEqual(3, Context.Set<User>().Count(x => x.IsDeleted));
            Assert.AreEqual(5, Context.Set<User>().Count(x => !x.IsDeleted));
        }

        [Test]
        public async Task Handle_GenericSynchronizeByIdRequest_UpdatesAllProvidedUsers()
        {
            var request = new SynchronizeByIdRequest<User, UserGetDto, UserGetDto>(new List<UserGetDto>
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
            Assert.AreEqual(3, Context.Set<User>().Count(x => x.IsDeleted));
            Assert.AreEqual(5, Context.Set<User>().Count(x => !x.IsDeleted));
        }

        [Test]
        public async Task Handle_SynchronizeUserClaimsRequest_SynchronizesWithoutDeletingOtherUsersClaims()
        {
            var user1Claims = new[]
            {
                new UserClaim { UserId = _users[0].Id, Claim = "TestClaim1", Value = "Before", IsDeleted = false },
                new UserClaim { UserId = _users[0].Id, Claim = "TestClaim2", Value = "Before", IsDeleted = false }
            };

            Context.AddRange(user1Claims.Cast<object>());

            var user2Claims = new[]
            {
                new UserClaim { UserId = _users[1].Id, Claim = "TestClaim1", Value = "Before", IsDeleted = false },
                new UserClaim { UserId = _users[1].Id, Claim = "TestClaim2", Value = "Before", IsDeleted = false }
            };

            Context.AddRange(user2Claims.Cast<object>());
            Context.SaveChanges();

            var request = new SynchronizeUserClaimsRequest
            {
                UserId = _users[1].Id,
                Claims = new List<UserClaimDto>
                {
                    // TODO: Should throw an exception if EFE ops are disabled, but we're still relying on a configuration
                    // For example, the following relies on the UserId being ignored from the bulk profile (but if you disable
                    // EFE for Inserts/Updates, the user id in the db will be set to 99 and not ignored)
                    //new UserClaimDto { UserId = 99, Claim = "TestClaim2", Value = "After" },
                    //new UserClaimDto { UserId = 99, Claim = "TestClaim3", Value = "After" }
                    new UserClaimDto { UserId = _users[1].Id, Claim = "TestClaim2", Value = "After" },
                    new UserClaimDto { UserId = _users[1].Id, Claim = "TestClaim3", Value = "After" }
                }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.IsNotNull(response.Result.Items);

            Assert.AreEqual(2, response.Result.Items.Count);

            Assert.AreEqual(4, response.Result.Items[0].Id);
            Assert.AreEqual("TestClaim2", response.Result.Items[0].Claim);
            Assert.AreEqual("After", response.Result.Items[0].Value);
            ///////Assert.AreEqual(99, response.Result.Items[0].UserId);
            Assert.AreEqual(_users[1].Id, response.Result.Items[0].UserId);

            Assert.AreEqual(5, response.Result.Items[1].Id);
            Assert.AreEqual("TestClaim3", response.Result.Items[1].Claim);
            Assert.AreEqual("After", response.Result.Items[1].Value);
            Assert.AreEqual(_users[1].Id, response.Result.Items[1].UserId);

            var claims = Context.Set<UserClaim>()
                .AsQueryable()
                .OrderBy(x => x.Id)
                .ToArray();

            Assert.AreEqual(5, claims.Length);

            Assert.AreEqual(1, claims[0].Id);
            Assert.AreEqual("TestClaim1", claims[0].Claim);
            Assert.AreEqual("Before", claims[0].Value);
            Assert.AreEqual(_users[0].Id, claims[0].UserId);
            Assert.IsFalse(claims[0].IsDeleted);

            Assert.AreEqual(2, claims[1].Id);
            Assert.AreEqual("TestClaim2", claims[1].Claim);
            Assert.AreEqual("Before", claims[1].Value);
            Assert.AreEqual(_users[0].Id, claims[1].UserId);
            Assert.IsFalse(claims[1].IsDeleted);

            Assert.AreEqual(3, claims[2].Id);
            Assert.AreEqual("TestClaim1", claims[2].Claim);
            Assert.AreEqual("Before", claims[2].Value);
            Assert.AreEqual(_users[1].Id, claims[2].UserId);
            Assert.IsTrue(claims[2].IsDeleted);

            Assert.AreEqual(4, claims[3].Id);
            Assert.AreEqual("TestClaim2", claims[3].Claim);
            Assert.AreEqual("After", claims[3].Value);
            Assert.AreEqual(_users[1].Id, claims[3].UserId);
            Assert.IsFalse(claims[3].IsDeleted);

            Assert.AreEqual(5, claims[4].Id);
            Assert.AreEqual("TestClaim3", claims[4].Claim);
            Assert.AreEqual("After", claims[4].Value);
            Assert.AreEqual(_users[1].Id, claims[4].UserId);
            Assert.IsFalse(claims[4].IsDeleted);
        }
    }
    
    public class SynchronizeUsersByIdRequest
        : ISynchronizeRequest<User, UserGetDto>
    {
        public List<UserGetDto> Items { get; set; }
    }

    public class SynchronizeUsersByIdProfile
        : BulkRequestProfile<SynchronizeUsersByIdRequest, UserGetDto>
    {
        public SynchronizeUsersByIdProfile() : base(request => request.Items)
        {
            ForEntity<User>().UseKeys("Id");
        }
    }

    public class SynchronizeUserClaimsRequest
        : ISynchronizeRequest<UserClaim, UserClaimGetDto>
    {
        public int UserId { get; set; }

        public List<UserClaimDto> Claims { get; set; }
    }

    public class NotDeletedFilter : Filter<ICrudlessRequest, IEntity>
    {
        public override IQueryable<IEntity> Apply(ICrudlessRequest request, IQueryable<IEntity> queryable)
        {
            return queryable.Where(x => !x.IsDeleted);
        }
    }
    
    public class SynchronizeUserClaimsProfile
        : BulkRequestProfile<SynchronizeUserClaimsRequest, UserClaimDto>
    {
        public SynchronizeUserClaimsProfile() : base(request => request.Claims)
        {
            ForEntity<UserClaim>()
                .UseKeys("Claim")
                .AddFilter(new NotDeletedFilter())
                .AddFilter((request, claim) => request.UserId == claim.UserId)
                .CreateEntityWith((context, claim) =>
                {
                    var result = context.ServiceProvider
                        .ProvideInstance<IMapper>()
                        .Map<UserClaim>(claim);

                    result.UserId = context.Request.UserId;
                    return result;
                })
                .BulkUpdateWith(config => config.IgnoreColumns(x => x.UserId));
        }
    }
}