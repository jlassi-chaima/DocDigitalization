using Application.Dtos.UserDTO;
using Application.Features.FeatureUser;
using Application.Parameters;
using Core.Database;
using DD.Core.Pagination;
using Domain.User;

namespace Application.Repository
{
    public interface IUserRepository : IRepository <ApplicationUser, Guid>
    {
        Task<PagedList<ListUserDto>> GetPagedtagAsync<ListUserDto>(UserParameters userparameters, CancellationToken cancellationToken = default);
        Task DeleteByIdAsyncString(string id, CancellationToken cancellationToken = default);
        Task<ApplicationUser?> FindByIdAsyncString(string id, CancellationToken cancellationToken = default);
        Task<ApplicationUser?> FindUserByMailAsyncString(string mail, CancellationToken cancellationToken = default);
        Task<List<ApplicationUser>> GetAllUsers(CancellationToken cancellationToken = default);
        Task<string?> GetUserRoleByID(string id, CancellationToken cancellationToken = default);
      
         Task<bool> ExistsByIdAsync(string userId, CancellationToken cancellationToken = default);

        Task<List<string>> GetListGroupsByID(string id, CancellationToken cancellationToken = default);
        Task<UserGroups> GetFirstGroupForUser(string userId, CancellationToken cancellationToken = default);

    }
}
