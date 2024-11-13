
using Application.Consumers.RestAPIDocuments.Dtos;
using Domain.Correspondents;
using Domain.MailRules;
using Domain.MailRules.Enum;



namespace Application.Services
{
    public interface ICorrespondentService
    {
        Task<Correspondent> CreateCorrespondent(CreateCorrespondent correspondent);
        Task<List<CreateCorrespondent>> GetCorrespondents(string idOwner);
        Task<CreateCorrespondent> GenerateCorrespondentFromMetadata(string idOwner, MailMetadataCorrespondentOption? mailCorrespondentOption, string name, string email, MailRule mail, List<string> listName, List<string> listEmail);
    }
}
