


using Domain.Correspondents;

namespace Domain.Ports
{
    public interface ICorrespondentPort
    {
        Task<HttpResponseMessage> AddCorrespondent(CreateCorrespondent request);
        Task<HttpResponseMessage> GetCorrespondents(string idOwner);
    }
}
