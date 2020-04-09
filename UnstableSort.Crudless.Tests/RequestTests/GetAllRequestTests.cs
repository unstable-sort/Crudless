using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Requests;
using UnstableSort.Crudless.Tests.Fakes;

namespace UnstableSort.Crudless.Tests.RequestTests
{
    [TestFixture]
    public class GetAllRequestTests : BaseUnitTest
    {
        private async Task SeedSortEntities()
        {
            Context.AddRange(
                new User { Name = "BUser", IsDeleted = true },
                new User { Name = "AUser", IsDeleted = false },
                new User { Name = "CUser", IsDeleted = false },
                new User { Name = "FUser", IsDeleted = true },
                new User { Name = "DUser", IsDeleted = true },
                new User { Name = "EUser", IsDeleted = false }
            );

            await Context.SaveChangesAsync();
        }

        [Test]
        public async Task Handle_GetAllSimpleSortedUsersRequest_ReturnsAllEntitiesSorted()
        {
            await SeedSortEntities();

            var request = new GetAllSimpleSortedUsers();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(6, response.Result.Items.Count);
            Assert.AreEqual("FUser", response.Result.Items[0].Name);
            Assert.AreEqual("EUser", response.Result.Items[1].Name);
            Assert.AreEqual("DUser", response.Result.Items[2].Name);
            Assert.AreEqual("CUser", response.Result.Items[3].Name);
            Assert.AreEqual("BUser", response.Result.Items[4].Name);
            Assert.AreEqual("AUser", response.Result.Items[5].Name);
        }

        [Test]
        public async Task Handle_GetAllCustomSortedUsersRequest_ReturnsAllEntitiesSorted()
        {
            await SeedSortEntities();

            var request = new GetAllCustomSortedUsers();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(6, response.Result.Items.Count);
            Assert.AreEqual("FUser", response.Result.Items[0].Name);
            Assert.AreEqual("EUser", response.Result.Items[1].Name);
            Assert.AreEqual("DUser", response.Result.Items[2].Name);
            Assert.AreEqual("CUser", response.Result.Items[3].Name);
            Assert.AreEqual("BUser", response.Result.Items[4].Name);
            Assert.AreEqual("AUser", response.Result.Items[5].Name);
        }
        
        [Test]
        public async Task Handle_GetAllBasicSortedUsersRequest_Ungrouped_ReturnsAllEntitiesSorted()
        {
            await SeedSortEntities();

            var request = new GetAllBasicSortedUsers { GroupDeleted = false };

            var response = await Mediator.HandleAsync(request);
            
            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(6, response.Result.Items.Count);
            Assert.AreEqual("AUser", response.Result.Items[0].Name);
            Assert.AreEqual("BUser", response.Result.Items[1].Name);
            Assert.AreEqual("CUser", response.Result.Items[2].Name);
            Assert.AreEqual("DUser", response.Result.Items[3].Name);
            Assert.AreEqual("EUser", response.Result.Items[4].Name);
            Assert.AreEqual("FUser", response.Result.Items[5].Name);
        }

        [Test]
        public async Task Handle_GetAllBasicSortedUsersRequest_Grouped_ReturnsAllEntitiesSorted()
        {
            await SeedSortEntities();

            var request = new GetAllBasicSortedUsers { GroupDeleted = true };

            var response = await Mediator.HandleAsync(request);
            
            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(6, response.Result.Items.Count);
            Assert.AreEqual("EUser", response.Result.Items[0].Name);
            Assert.AreEqual("CUser", response.Result.Items[1].Name);
            Assert.AreEqual("AUser", response.Result.Items[2].Name);
            Assert.AreEqual("FUser", response.Result.Items[3].Name);
            Assert.AreEqual("DUser", response.Result.Items[4].Name);
            Assert.AreEqual("BUser", response.Result.Items[5].Name);
        }
        
        [Test]
        public async Task Handle_GetAllSwitchSortedUsersRequest_Case_ReturnsAllEntitiesSorted()
        {
            await SeedSortEntities();

            var request = new GetAllSwitchSortedUsers { Case = UsersSortColumn.Name };

            var response = await Mediator.HandleAsync(request);
            
            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(6, response.Result.Items.Count);
            Assert.AreEqual("FUser", response.Result.Items[0].Name);
            Assert.AreEqual("EUser", response.Result.Items[1].Name);
            Assert.AreEqual("DUser", response.Result.Items[2].Name);
            Assert.AreEqual("CUser", response.Result.Items[3].Name);
            Assert.AreEqual("BUser", response.Result.Items[4].Name);
            Assert.AreEqual("AUser", response.Result.Items[5].Name);
        }

        [Test]
        public async Task Handle_GetAllSwitchSortedUsersRequest_Default_ReturnsAllEntitiesSorted()
        {
            await SeedSortEntities();

            var request = new GetAllSwitchSortedUsers { Case = "BadCase" };

            var response = await Mediator.HandleAsync(request);
            
            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(6, response.Result.Items.Count);
            Assert.AreEqual("EUser", response.Result.Items[0].Name);
            Assert.AreEqual("CUser", response.Result.Items[1].Name);
            Assert.AreEqual("AUser", response.Result.Items[2].Name);
            Assert.AreEqual("FUser", response.Result.Items[3].Name);
            Assert.AreEqual("DUser", response.Result.Items[4].Name);
            Assert.AreEqual("BUser", response.Result.Items[5].Name);
        }

        [Test]
        public async Task Handle_GetAllSwitchSortedUsersWithoutDefaultRequest_WithBadCase_ReturnsAllEntitiesSortedByFirstCase()
        {
            await SeedSortEntities();

            var request = new GetAllSwitchSortedUsersWithoutDefault { Case = "BadCase" };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(6, response.Result.Items.Count);
            Assert.AreEqual("AUser", response.Result.Items[0].Name);
            Assert.AreEqual("BUser", response.Result.Items[1].Name);
            Assert.AreEqual("CUser", response.Result.Items[2].Name);
            Assert.AreEqual("DUser", response.Result.Items[3].Name);
            Assert.AreEqual("EUser", response.Result.Items[4].Name);
            Assert.AreEqual("FUser", response.Result.Items[5].Name);
        }

        [Test]
        public async Task Handle_GetAllTableSortedUsersWithDefaultRequest_WithBadCase_ReturnsAllEntitiesSortedByDefault()
        {
            await SeedSortEntities();

            var request = new GetAllTableSortedUsers
            {
                PrimaryColumn = "BadProperty"
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(6, response.Result.Items.Count);
            Assert.AreEqual("AUser", response.Result.Items[0].Name);
            Assert.AreEqual("BUser", response.Result.Items[1].Name);
            Assert.AreEqual("CUser", response.Result.Items[2].Name);
            Assert.AreEqual("DUser", response.Result.Items[3].Name);
            Assert.AreEqual("EUser", response.Result.Items[4].Name);
            Assert.AreEqual("FUser", response.Result.Items[5].Name);
        }

        [Test]
        public async Task Handle_GetAllTableSortedUsersWithoutDefaultRequest_WithBadCase_ReturnsAllEntitiesSortedByFirstProperty()
        {
            await SeedSortEntities();

            var request = new GetAllTableSortedUsersWithoutDefault
            {
                Column = "BadProperty"
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(6, response.Result.Items.Count);
            Assert.AreEqual("AUser", response.Result.Items[0].Name);
            Assert.AreEqual("BUser", response.Result.Items[1].Name);
            Assert.AreEqual("CUser", response.Result.Items[2].Name);
            Assert.AreEqual("DUser", response.Result.Items[3].Name);
            Assert.AreEqual("EUser", response.Result.Items[4].Name);
            Assert.AreEqual("FUser", response.Result.Items[5].Name);
        }

        [Test]
        public async Task Handle_GetAllTableSortedUsersRequest_ByIsDeletedThenByNameDesc_ReturnsAllEntitiesSorted()
        {
            await SeedSortEntities();

            var request = new GetAllTableSortedUsers
            {
                PrimaryColumn = UsersSortColumn.IsDeleted,
                SecondaryColumn = UsersSortColumn.Name,
                SecondaryDirection = 1
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(6, response.Result.Items.Count);
            Assert.AreEqual("EUser", response.Result.Items[0].Name);
            Assert.AreEqual("CUser", response.Result.Items[1].Name);
            Assert.AreEqual("AUser", response.Result.Items[2].Name);
            Assert.AreEqual("FUser", response.Result.Items[3].Name);
            Assert.AreEqual("DUser", response.Result.Items[4].Name);
            Assert.AreEqual("BUser", response.Result.Items[5].Name);
        }

        [Test]
        public async Task Handle_GetAllTableSortedUsersRequest_ByIsDeletedThenByNameAsc_ReturnsAllEntitiesSorted()
        {
            await SeedSortEntities();
            
            var request = new GetAllTableSortedUsers
            {
                PrimaryColumn = UsersSortColumn.IsDeleted,
                SecondaryColumn = UsersSortColumn.Name,
                SecondaryDirection = 0
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(6, response.Result.Items.Count);
            Assert.AreEqual("AUser", response.Result.Items[0].Name);
            Assert.AreEqual("CUser", response.Result.Items[1].Name);
            Assert.AreEqual("EUser", response.Result.Items[2].Name);
            Assert.AreEqual("BUser", response.Result.Items[3].Name);
            Assert.AreEqual("DUser", response.Result.Items[4].Name);
            Assert.AreEqual("FUser", response.Result.Items[5].Name);
        }

        [Test]
        public async Task Handle_GetAllTableSortedUsersRequest_ByNameThenByIsDeleted_ReturnsAllEntitiesSorted()
        {
            await SeedSortEntities();
            
            var request = new GetAllTableSortedUsers
            {
                PrimaryColumn = UsersSortColumn.Name,
                SecondaryColumn = UsersSortColumn.IsDeleted,
                SecondaryDirection = 1
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(6, response.Result.Items.Count);
            Assert.AreEqual("AUser", response.Result.Items[0].Name);
            Assert.AreEqual("BUser", response.Result.Items[1].Name);
            Assert.AreEqual("CUser", response.Result.Items[2].Name);
            Assert.AreEqual("DUser", response.Result.Items[3].Name);
            Assert.AreEqual("EUser", response.Result.Items[4].Name);
            Assert.AreEqual("FUser", response.Result.Items[5].Name);
        }

        [Test]
        public async Task Handle_GetAllBasicUnconditionalFilteredUsersRequest_ReturnsAllEntitiesFiltered()
        {
            Context.AddRange(
                new User { Name = "BUser", IsDeleted = true },
                new User { Name = "AUser", IsDeleted = false },
                new User { Name = "CUser", IsDeleted = true },
                new User { Name = "DUser", IsDeleted = false }
            );

            await Context.SaveChangesAsync();

            var request = new GetAllBasicUnconditionalFilteredUsers();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(2, response.Result.Items.Count);
            Assert.AreEqual("AUser", response.Result.Items[0].Name);
            Assert.AreEqual("DUser", response.Result.Items[1].Name);
        }

        [Test]
        public async Task Handle_GetAllBasicConditionalFilteredUsersRequestWithFilterOff_ReturnsAllEntitiesUnfiltered()
        {
            Context.AddRange(
                new User { Name = "BUser", IsDeleted = false },
                new User { Name = "AUser", IsDeleted = true },
                new User { Name = "CUser", IsDeleted = true },
                new User { Name = "DUser", IsDeleted = false }
            );

            await Context.SaveChangesAsync();

            var request = new GetAllBasicConditionalFilteredUsers
            {
                DeletedFilter = null
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(4, response.Result.Items.Count);
            Assert.AreEqual("BUser", response.Result.Items[0].Name);
            Assert.AreEqual("AUser", response.Result.Items[1].Name);
            Assert.AreEqual("CUser", response.Result.Items[2].Name);
            Assert.AreEqual("DUser", response.Result.Items[3].Name);
        }

        [Test]
        public async Task Handle_GetAllBasicConditionalFilteredUsersRequestWithFilterOnFalse_ReturnsAllEntitiesFiltered()
        {
            Context.AddRange(
                new User { Name = "BUser", IsDeleted = false },
                new User { Name = "AUser", IsDeleted = false },
                new User { Name = "CUser", IsDeleted = true },
                new User { Name = "DUser", IsDeleted = false }
            );

            await Context.SaveChangesAsync();

            var request = new GetAllBasicConditionalFilteredUsers
            {
                DeletedFilter = false
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(3, response.Result.Items.Count);
            Assert.AreEqual("BUser", response.Result.Items[0].Name);
            Assert.AreEqual("AUser", response.Result.Items[1].Name);
            Assert.AreEqual("DUser", response.Result.Items[2].Name);
        }

        [Test]
        public async Task Handle_GetAllBasicConditionalFilteredUsersRequestWithFilterOnTrue_ReturnsAllEntitiesFiltered()
        {
            Context.AddRange(
                new User { Name = "BUser", IsDeleted = false },
                new User { Name = "AUser", IsDeleted = false },
                new User { Name = "CUser", IsDeleted = true },
                new User { Name = "DUser", IsDeleted = false }
            );

            await Context.SaveChangesAsync();

            var request = new GetAllBasicConditionalFilteredUsers
            {
                DeletedFilter = true
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(1, response.Result.Items.Count);
            Assert.AreEqual("CUser", response.Result.Items[0].Name);
        }

        [Test]
        public async Task Handle_GetUsersWithDefaultWithError_ReturnsDefaultAndError()
        {
            var request = new GetUsersWithDefaultWithErrorRequest();
            var response = await Mediator.HandleAsync(request);

            Assert.IsTrue(response.HasErrors);
            Assert.AreEqual("Failed to find entity.", response.Errors[0].ErrorMessage);
            Assert.IsNull(response.Result);
        }

        [Test]
        public async Task Handle_GetUsersWithDefaultWithoutError_ReturnsDefault()
        {
            var request = new GetUsersWithDefaultWithoutErrorRequest();
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(1, response.Result.Items.Count);
            Assert.AreEqual("DefaultUser", response.Result.Items[0].Name);
        }

        [Test]
        public async Task Handle_GetUsersWithoutDefaultWithError_ReturnsError()
        {
            var request = new GetUsersWithoutDefaultWithErrorRequest();
            var response = await Mediator.HandleAsync(request);

            Assert.IsTrue(response.HasErrors);
            Assert.AreEqual("Failed to find entity.", response.Errors[0].ErrorMessage);
            Assert.IsNull(response.Result);
        }

        [Test]
        public async Task Handle_GetUsersWithoutDefaultWithoutError_ReturnsEmptyList()
        {
            var request = new GetUsersWithoutDefaultWithoutErrorRequest();
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(0, response.Result.Items.Count);
        }

        [Test]
        public async Task Handle_DefaultGetAllRequest_ReturnsAllEntities()
        {
            Context.AddRange(new User { Name = "User1" }, new User { Name = "User2" });
            await Context.SaveChangesAsync();

            var request = new GetAllRequest<User, UserGetDto>();
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(2, response.Result.Items.Count);
        }

        [Test]
        public async Task Handle_UnprojectedGetAllRequest_ReturnsAllEntities()
        {
            Context.AddRange(new User { Name = "User1" }, new User { Name = "User2" });
            await Context.SaveChangesAsync();

            var request = new GetUsersUnprojectedRequest();
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(2, response.Result.Items.Count);
        }

        [Test]
        public async Task Handle_GetUsersWithInlineProfile_ReturnsAllEntities()
        {
            Context.AddRange(
                new User { Name = "IncludedUserD", IsDeleted = false },
                new User { Name = "ExcludedUserE", IsDeleted = false },
                new User { Name = "IncludedUserA", IsDeleted = false },
                new User { Name = "ExcludedUserB", IsDeleted = false },
                new User { Name = "IncludedUserC", IsDeleted = false },
                new User { Name = "ExcludedUserF", IsDeleted = false });

            await Context.SaveChangesAsync();

            var request = new GetAllRequest<User, UserGetInlineDto>();
            request.Configure(profile => profile
                .ForEntity<User>()
                .AddFilter(user => user.Name.StartsWith("Included")));

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(3, response.Result.Items.Count);
            Assert.AreEqual("IncludedUserD", response.Result.Items[0].Name);
            Assert.AreEqual("IncludedUserC", response.Result.Items[1].Name);
            Assert.AreEqual("IncludedUserA", response.Result.Items[2].Name);
        }
    }

    public static class UsersSortColumn
    {
        public const string Name = "Name";
        public const string IsDeleted = "IsDeleted";
    };
    
    public class GetAllSimpleSortedUsers : IGetAllRequest<User, UserGetDto>
    {
    }

    public class GetAllSimpleSortedUsersProfile 
        : RequestProfile<GetAllSimpleSortedUsers>
    {
        public GetAllSimpleSortedUsersProfile()
        {
            ForEntity<User>().SortByDescending(x => x.Name);
        }
    }
    
    public class GetAllCustomSortedUsers : IGetAllRequest<User, UserGetDto>
    {
    }

    public class GetAllCustomSortedUsersProfile 
        : RequestProfile<GetAllCustomSortedUsers>
    {
        public GetAllCustomSortedUsersProfile()
        {
            ForEntity<User>()
                .SortCustom((req, users) => users.OrderByDescending(user => user.Name));
        }
    }
    
    public class GetAllBasicSortedUsers : IGetAllRequest<User, UserGetDto>
    {
        public bool GroupDeleted { get; set; }
    }

    public class GetAllBasicSortedUsersProfile 
        : RequestProfile<GetAllBasicSortedUsers>
    {
        public GetAllBasicSortedUsersProfile()
        {
            ForEntity<User>()
                .SortBy(user => user.IsDeleted, then => then
                    .ThenBy("Name").Descending()
                    .When(r => r.GroupDeleted)
                .SortBy("Name")
                    .Otherwise());
        }
    }
    
    public class GetAllSwitchSortedUsers : IGetAllRequest<User, UserGetDto>
    {
        public string Case { get; set; }
    }

    public class GetAllSwitchSortedUsersProfile 
        : RequestProfile<GetAllSwitchSortedUsers>
    {
        public GetAllSwitchSortedUsersProfile()
        {
            ForEntity<User>()
                .SortAsVariant<string>("Case", options => options
                    .ForCase(UsersSortColumn.Name).SortBy("Name").Descending()
                    .ForDefaultCase().SortBy(user => user.IsDeleted).ThenBy("Name").Descending());
        }
    }

    public class GetAllSwitchSortedUsersWithoutDefault : IGetAllRequest<User, UserGetDto>
    {
        public string Case { get; set; }
    }

    public class GetAllSwitchSortedUsersWithoutDefaultProfile : RequestProfile<GetAllSwitchSortedUsersWithoutDefault>
    {
        public GetAllSwitchSortedUsersWithoutDefaultProfile()
        {
            ForEntity<User>()
                .SortAsVariant(x => x.Case, options => options
                    .ForCase("Name").SortBy("Name"));
        }
    }

    public class GetAllTableSortedUsersWithoutDefault : IGetAllRequest<User, UserGetDto>
    {
        public string Column { get; set; }
    }

    public class GetAllTableSortedUsersWithoutDefaultProfile
        : RequestProfile<GetAllTableSortedUsersWithoutDefault>
    {
        public GetAllTableSortedUsersWithoutDefaultProfile()
        {
            ForEntity<User>().SortAsTable(r => r.Column, o => o.OnAnyProperty());
        }
    }

    public class GetAllTableSortedUsers : IGetAllRequest<User, UserGetDto>
    {
        public string PrimaryColumn { get; set; }

        public string SecondaryColumn { get; set; }

        public int SecondaryDirection { get; set; }
    }
    
    public class GetAllTableSortedUsersProfile
        : RequestProfile<GetAllTableSortedUsers>
    {
        public GetAllTableSortedUsersProfile()
        {
            ForEntity<User>()
                .SortAsTable<string>(options => options
                    .WithControl(r => r.PrimaryColumn, SortDirection.Ascending)
                    .WithControl("SecondaryColumn", "SecondaryDirection")
                    .OnProperty(UsersSortColumn.IsDeleted, user => user.IsDeleted)
                    .OnProperty(UsersSortColumn.Name, "Name", true));
        }
    }
    
    public class GetAllBasicUnconditionalFilteredUsers
        : IGetAllRequest<User, UserGetDto>
    { }

    public class GetAllBasicUnconditionalFilteredUsersProfile 
        : RequestProfile<GetAllBasicUnconditionalFilteredUsers>
    {
        public GetAllBasicUnconditionalFilteredUsersProfile()
        {
            ForEntity<IEntity>()
                .AddFalseFilter(x => x.IsDeleted);
        }
    }
    
    public class GetAllBasicConditionalFilteredUsers
        : IGetAllRequest<User, UserGetDto>
    {
        public bool? DeletedFilter { get; set; }
    }

    public class GetAllBasicConditionalFilteredUsersProfile 
        : RequestProfile<GetAllBasicConditionalFilteredUsers>
    {
        public GetAllBasicConditionalFilteredUsersProfile()
        {
            ForEntity<IEntity>()
                .AddFilter(
                    (r, e) => e.IsDeleted == r.DeletedFilter.Value,
                    r => r.DeletedFilter.HasValue);
        }
    }
    
    public class GetUsersWithDefaultWithErrorRequest 
        : IGetAllRequest<User, UserGetDto>
    { }
    
    public class GetUsersWithDefaultWithoutErrorRequest 
        : IGetAllRequest<User, UserGetDto>
    { }
    
    public class GetUsersWithoutDefaultWithErrorRequest 
        : IGetAllRequest<User, UserGetDto>
    { }
    
    public class GetUsersWithoutDefaultWithoutErrorRequest 
        : IGetAllRequest<User, UserGetDto>
    { }

    public class GetWithDefaultWithErrorProfile 
        : RequestProfile<GetUsersWithDefaultWithErrorRequest>
    {
        public GetWithDefaultWithErrorProfile()
        {
            UseErrorConfiguration(config => config.FailedToFindInGetAllIsError = true);
            ForEntity<User>().UseDefaultValue(new User { Name = "DefaultUser" });
        }
    }

    public class GetWithDefaultWithoutErrorProfile 
        : RequestProfile<GetUsersWithDefaultWithoutErrorRequest>
    {
        public GetWithDefaultWithoutErrorProfile()
        {
            ForEntity<User>().UseDefaultValue(new User { Name = "DefaultUser" });
            UseErrorConfiguration(config => config.FailedToFindInGetAllIsError = false);
        }
    }

    public class GetWithoutDefaultWithErrorProfile 
        : RequestProfile<GetUsersWithoutDefaultWithErrorRequest>
    {
        public GetWithoutDefaultWithErrorProfile()
        {
            UseErrorConfiguration(config => config.FailedToFindInGetAllIsError = true);
        }
    }

    public class GetWithoutDefaultWithoutErrorProfile 
        : RequestProfile<GetUsersWithoutDefaultWithoutErrorRequest>
    {
        public GetWithoutDefaultWithoutErrorProfile()
        {
            UseErrorConfiguration(config => config.FailedToFindInGetAllIsError = false);
        }
    }
    
    public class GetUsersUnprojectedRequest : GetAllRequest<User, UserGetDto> { }

    public class GetUsersUnprojectedProfile : RequestProfile<GetUsersUnprojectedRequest>
    {
        public GetUsersUnprojectedProfile()
        {
            UseOptions(config => config.UseProjection = false);
        }
    }

    public class GetInlineUsersProfile : RequestProfile<IGetAllRequest<User, UserGetInlineDto>>
    {
        public GetInlineUsersProfile()
        {
            ForEntity<User>()
                .SortByDescending("Name");
        }
    }
}
 
 