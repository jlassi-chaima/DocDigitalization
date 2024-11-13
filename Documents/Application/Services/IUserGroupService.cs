

namespace Application.Services
{
    public interface IUserGroupService
    {
         Task<Guid> GetGroupForUser(string owner);
        Task<List<string>> GetGroupId(string owner);
    }
}
