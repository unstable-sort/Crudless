using NUnit.Framework;
using System.Threading.Tasks;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Requests;
using UnstableSort.Crudless.Tests.Fakes;

namespace UnstableSort.Crudless.Tests.RequestTests
{
    [TestFixture]
    public class PagedGetAllRequestTests : BaseUnitTest
    {
        private async Task SeedEntities()
        {
            Context.AddRange(
                new User { Name = "BUser" },
                new User { Name = "AUser" },
                new User { Name = "CUser" },
                new User { Name = "FUser" },
                new User { Name = "DUser" },
                new User { Name = "EUser" }
            );

            await Context.SaveChangesAsync();
        }

        [Test]
        public async Task Handle_GetAllUsersPagedRequest_NoPageSize_ReturnsAllEntities()
        {
            await SeedEntities();

            var request = new GetAllUsersPaged
            {
                PageSize = 0
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(1, response.Result.PageNumber);
            Assert.AreEqual(6, response.Result.PageSize);
            Assert.AreEqual(1, response.Result.PageCount);
            Assert.AreEqual(6, response.Result.TotalItemCount);
            Assert.AreEqual(6, response.Result.Items.Count);
            Assert.AreEqual("FUser", response.Result.Items[0].Name);
            Assert.AreEqual("EUser", response.Result.Items[1].Name);
            Assert.AreEqual("DUser", response.Result.Items[2].Name);
            Assert.AreEqual("CUser", response.Result.Items[3].Name);
            Assert.AreEqual("BUser", response.Result.Items[4].Name);
            Assert.AreEqual("AUser", response.Result.Items[5].Name);
        }

        [Test]
        public async Task Handle_GetAllUsersPagedRequest_DefaultPage_ReturnsPagedEntities()
        {
            await SeedEntities();

            var request = new GetAllUsersPaged
            {
                PageSize = 5
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(1, response.Result.PageNumber);
            Assert.AreEqual(5, response.Result.PageSize);
            Assert.AreEqual(2, response.Result.PageCount);
            Assert.AreEqual(6, response.Result.TotalItemCount);
            Assert.AreEqual(5, response.Result.Items.Count);
            Assert.AreEqual("FUser", response.Result.Items[0].Name);
            Assert.AreEqual("EUser", response.Result.Items[1].Name);
            Assert.AreEqual("DUser", response.Result.Items[2].Name);
            Assert.AreEqual("CUser", response.Result.Items[3].Name);
            Assert.AreEqual("BUser", response.Result.Items[4].Name);
        }

        [Test]
        public async Task Handle_GetAllUsersPagedRequest_WithPageNumber_ReturnsPagedEntities()
        {
            await SeedEntities();

            var request = new GetAllUsersPaged
            {
                PageSize = 2,
                PageNumber = 2
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(2, response.Result.PageNumber);
            Assert.AreEqual(2, response.Result.PageSize);
            Assert.AreEqual(3, response.Result.PageCount);
            Assert.AreEqual(6, response.Result.TotalItemCount);
            Assert.AreEqual(2, response.Result.Items.Count);
            Assert.AreEqual("DUser", response.Result.Items[0].Name);
            Assert.AreEqual("CUser", response.Result.Items[1].Name);
        }

        [Test]
        public async Task Handle_DefaultPagedGetAllRequest_ReturnsPagedEntities()
        {
            await SeedEntities();

            var request = new PagedGetAllRequest<User, UserGetDto>
            {
                PageSize = 2,
                PageNumber = 2
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(2, response.Result.PageNumber);
            Assert.AreEqual(2, response.Result.PageSize);
            Assert.AreEqual(3, response.Result.PageCount);
            Assert.AreEqual(6, response.Result.TotalItemCount);
            Assert.AreEqual(2, response.Result.Items.Count);
            Assert.AreEqual("DUser", response.Result.Items[0].Name);
            Assert.AreEqual("CUser", response.Result.Items[1].Name);
        }

        [Test]
        public async Task Handle_GetAllUsersPagedRequest_WithNoResults_ReturnsEmptyList()
        {
            Context.AddRange(
                new User { Name = "BUser", IsDeleted = true },
                new User { Name = "AUser", IsDeleted = true },
                new User { Name = "CUser", IsDeleted = true },
                new User { Name = "FUser", IsDeleted = true },
                new User { Name = "DUser", IsDeleted = true },
                new User { Name = "EUser", IsDeleted = true }
            );

            await Context.SaveChangesAsync();

            var request = new GetAllFilteredUsersPaged
            {
                PageSize = 2,
                PageNumber = 2,
                DeletedFilter = false
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(0, response.Result.TotalItemCount);
            Assert.AreEqual(1, response.Result.PageNumber);
            Assert.AreEqual(2, response.Result.PageSize);
            Assert.AreEqual(1, response.Result.PageCount);
            Assert.AreEqual(0, response.Result.Items.Count);
        }

        [Test]
        public async Task Handle_GetAllUsersPagedRequest_WithFilter_ReturnsFilteredList()
        {
            Context.AddRange(
                new User { Name = "BUser", IsDeleted = true },
                new User { Name = "AUser", IsDeleted = false },
                new User { Name = "CUser", IsDeleted = true },
                new User { Name = "FUser", IsDeleted = false },
                new User { Name = "DUser", IsDeleted = false },
                new User { Name = "EUser", IsDeleted = true }
            );

            await Context.SaveChangesAsync();

            var request = new GetAllFilteredUsersPaged
            {
                PageSize = 2,
                PageNumber = 1,
                DeletedFilter = false
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(3, response.Result.TotalItemCount);
            Assert.AreEqual(1, response.Result.PageNumber);
            Assert.AreEqual(2, response.Result.PageSize);
            Assert.AreEqual(2, response.Result.PageCount);
            Assert.AreEqual(2, response.Result.Items.Count);
            Assert.AreEqual("AUser", response.Result.Items[0].Name);
            Assert.AreEqual("DUser", response.Result.Items[1].Name);
        }
    }
    
    public class GetAllUsersPaged : IPagedGetAllRequest<User, UserGetDto>, ICrudlessRequest<User, UserGetDto>
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }
    }

    public class GetAllUsersPagedProfile 
        : RequestProfile<IPagedGetAllRequest>
    {
        public GetAllUsersPagedProfile()
        {
            ForEntity<User>().SortWith(builder => builder.SortBy(x => x.Name).Descending());
        }
    }
    
    public class GetAllFilteredUsersPaged
        : IPagedGetAllRequest<User, UserGetDto>
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public bool? DeletedFilter { get; set; }
    }

    public class GetAllFilteredUsersPagedProfile 
        : RequestProfile<GetAllFilteredUsersPaged>
    {
        public GetAllFilteredUsersPagedProfile()
        {
            ConfigureErrors(config => config.FailedToFindInGetAllIsError = false);

            ForEntity<User>()
                .FilterOn(r => r.DeletedFilter, e => e.IsDeleted)
                .SortWith(builder => builder.SortBy("Name"));
        }
    }
}
