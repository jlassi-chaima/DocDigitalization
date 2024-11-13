
namespace Domain.Ports
{
    public interface IUserGroupPort
    {
       Task<HttpResponseMessage> GetFirstGroupForUser(string owner);
        Task<HttpResponseMessage> GetGroups();
        Task<HttpResponseMessage> GetGroupsId(string owner);

    }
}
