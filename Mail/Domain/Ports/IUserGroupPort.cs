
namespace Domain.Ports
{
    public interface IUserGroupPort
    {
       Task<HttpResponseMessage> GetFirstGRoupForUser(string owner);

    }
}
