using System;
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
    public class SaveRequestTests : BaseUnitTest
    {
        [Test]
        public async Task Handle_SaveWithoutResponse_CreatesUser()
        {
            var request = new SaveUserWithoutResponseRequest
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
        public async Task Handle_SaveWithResponse_CreatesUser()
        {
            var request = new SaveUserWithResponseRequest
            {
                Name = "TestUser"
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());
            Assert.IsNotNull(response.Result);
            Assert.AreEqual("TestUser", response.Result.Name);
        }

        [Test]
        public async Task Handle_SaveExistingWithoutResponse_UpdatesUser()
        {
            var existing = new User { Name = "TestUser" };
            Context.Set<User>().Add(existing);

            await Context.SaveChangesAsync();

            var request = new SaveUserWithoutResponseRequest
            { 
                Id = existing.Id,
                User = new UserDto { Name = "NewUser" }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());

            var user = Context.Set<User>().FirstOrDefault();
            Assert.IsNotNull(user);
            Assert.AreEqual("NewUser", user.Name);
        }

        [Test]
        public async Task Handle_SaveExistingWithResponse_UpdatesUser()
        {
            var existing = new User { Name = "TestUser" };
            Context.Set<User>().Add(existing);

            await Context.SaveChangesAsync();

            var request = new SaveUserWithResponseRequest
            {
                Name = existing.Name
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());
            Assert.IsNotNull(response.Result);
            Assert.AreEqual("TestUser", response.Result.Name);
        }

        [Test]
        public async Task Handle_DefaultSaveWithoutResponseRequest_CreatesUser()
        {
            var request = new SaveRequest<User, UserGetDto>(new UserGetDto
            {
                Name = "NewUser"
            });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());
            var user = Context.Set<User>().First();
            Assert.IsNotNull(user);
            Assert.AreNotEqual(0, user.Id);
            Assert.AreEqual("NewUser", user.Name);
        }

        [Test]
        public async Task Handle_DefaultSaveWithResponseRequest_CreatesUser()
        {
            var request = new SaveRequest<User, UserGetDto, UserGetDto>(new UserGetDto
            {
                Name = "NewUser"
            });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());
            Assert.IsNotNull(response.Result);
            Assert.AreNotEqual(0, response.Result.Id);
            Assert.AreEqual("NewUser", response.Result.Name);
        }
        
        [Test]
        public async Task Handle_SaveByIdRequest_UpdatesUser()
        {
            var existing = new User { Name = "TestUser" };
            Context.Set<User>().Add(existing);

            await Context.SaveChangesAsync();

            var request = new SaveByIdRequest<User, UserDto, UserGetDto>(
                existing.Id,
                new UserDto { Name = "NewUser" });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(1, Context.Set<User>().Count());
            Assert.AreEqual(existing.Id, response.Result.Id);
            Assert.AreEqual("NewUser", response.Result.Name);
            Assert.AreEqual("NewUser", Context.Set<User>().First().Name);
        }

        [Test]
        public async Task Handle_SaveWithCompositeKey_CreatesEntity()
        {
            var existing = new CompositeKeyEntity { IntPart = 1, GuidPart = Guid.NewGuid(), Name = "TestComposite" };
            Context.Set<CompositeKeyEntity>().Add(existing);

            await Context.SaveChangesAsync();

            var request = new SaveCompositeKeyEntityRequest
            {
                IntPart = 1,
                GuidPart = Guid.NewGuid(),
                Name = "TestComposite2"
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(2, Context.Set<CompositeKeyEntity>().Count());
        }

        [Test]
        public async Task Handle_SaveWithCompositeKey_UpdatesEntity()
        {
            var existing = new CompositeKeyEntity { IntPart = 1, GuidPart = Guid.NewGuid(), Name = "TestComposite" };
            Context.Set<CompositeKeyEntity>().Add(existing);

            await Context.SaveChangesAsync();

            var request = new SaveCompositeKeyEntityRequest
            {
                IntPart = 1,
                GuidPart = existing.GuidPart,
                Name = "TestComposite2"
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<CompositeKeyEntity>().Count());
            Assert.AreEqual("TestComposite2", Context.Set<CompositeKeyEntity>().Single().Name);
        }
    }
    
    public class SaveUserWithResponseRequest : UserDto, ISaveRequest<User, UserGetDto>
    { }
    
    public class SaveUserWithoutResponseRequest : ISaveRequest<User>
    {
        public int Id { get; set; }

        public UserDto User { get; set; }
    }
    
    public class SaveUserWithoutResponseProfile 
        : RequestProfile<SaveUserWithoutResponseRequest>
    {
        public SaveUserWithoutResponseProfile()
        {
            ForEntity<User>()
                .SelectBy(r => r.Id, e => e.Id)
                .CreateEntityWith(context =>
                {
                    return context.ServiceProvider
                        .ProvideInstance<IMapper>()
                        .Map<User>(context.Request.User);
                })
                .UpdateEntityWith((context, entity) =>
                {
                    return context.ServiceProvider
                        .ProvideInstance<IMapper>()
                        .Map(context.Request.User, entity);
                });
        }
    }

    public class SaveUserWithResponseProfile 
        : RequestProfile<SaveUserWithResponseRequest>
    {
        public SaveUserWithResponseProfile()
        {
            ForEntity<User>().SelectBy("Name");
        }
    }

    public class DefaultSaveWithoutResponseRequestProfile
        : RequestProfile<SaveRequest<User, UserGetDto>>
    {
        public DefaultSaveWithoutResponseRequestProfile()
        {
            ForEntity<User>().SelectBy(r => e => r.Item.Id == e.Id);
        }
    }

    public class DefaultSaveWithResponseRequestProfile
        : RequestProfile<SaveRequest<User, UserGetDto, UserGetDto>>
    {
        public DefaultSaveWithResponseRequestProfile()
        {
            ForEntity<User>().SelectBy(request => entity => request.Item.Id == entity.Id);
        }
    }

    public class SaveCompositeKeyEntityRequest : ISaveRequest<CompositeKeyEntity>
    {
        public int IntPart { get; set; }

        public Guid GuidPart { get; set; }

        public string Name { get; set; }
    }

    public class SaveCompositeKeyEntityProfile : RequestProfile<SaveCompositeKeyEntityRequest>
    {
        public SaveCompositeKeyEntityProfile()
        {
            ForEntity<CompositeKeyEntity>()
                .UseKeys(new[] { "IntPart", "GuidPart" })
                //.UseKeys(x => new { x.IntPart, x.GuidPart }, x => new { x.IntPart, x.GuidPart })
                .CreateEntityWith(context => new CompositeKeyEntity
                {
                    IntPart = context.Request.IntPart,
                    GuidPart = context.Request.GuidPart,
                    Name = context.Request.Name
                })
                .UpdateEntityWith((context, entity) =>
                {
                    entity.Name = context.Request.Name;
                    return entity;
                });
        }
    }
}