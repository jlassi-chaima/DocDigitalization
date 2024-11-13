
using Application.Consumers.RestAPIDocuments.Dtos;
using Application.Services;
using Domain.Correspondents;
using Domain.MailRules.Enum;
using Domain.Ports;
using Newtonsoft.Json;
using Serilog;
using Domain.MailRules;
using MassTransit.Util;
namespace Infrastructure.Services;
public class CorrespondentsService(ICorrespondentPort correspondentPort) : ICorrespondentService
{

    public async Task<Correspondent> CreateCorrespondent(CreateCorrespondent correspondent)
    {
        try
        {
            var res = await correspondentPort.AddCorrespondent(correspondent);
            var responseContent = await res.Content.ReadAsStringAsync();
            if (res.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Log.Error($"Error Message : {responseContent}");
                throw new HttpRequestException("An error has occured, please try again later");
            }
            var JSONObj = JsonConvert.DeserializeObject<Correspondent>(responseContent)!;
            return JSONObj;
        }

        catch (HttpRequestException ex)
        {
            Log.Error(ex.ToString());
            throw new HttpRequestException(ex.Message);
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
            throw new Exception($"Exception: {ex.Message}");
        }

    }
    public async Task<List<CreateCorrespondent>> GetCorrespondents(string idOwner)
    {
        try
        {
            var res = await correspondentPort.GetCorrespondents(idOwner);
            var responseContent = await res.Content.ReadAsStringAsync();
            if (res.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Log.Error($"Error Message : {responseContent}");
                throw new HttpRequestException("An error has occured, please try again later");
            }
            //To do change if not work Correspondent
            var JSONObj = JsonConvert.DeserializeObject<List<CreateCorrespondent>>(responseContent)!;

            return JSONObj;
        }

        catch (HttpRequestException ex)
        {
            Log.Error(ex.ToString());
            throw new HttpRequestException(ex.Message);
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
            throw new Exception($"Exception: {ex.Message}");
        }

    }

    public async Task<CreateCorrespondent> GenerateCorrespondentFromMetadata(string idOwner, MailMetadataCorrespondentOption? mailCorrespondentOption, string name, string email, MailRule mail, List<string> listName, List<string> listEmail)
    {
        try
        {
            if (mailCorrespondentOption == null)
            {
                throw new ArgumentException("Invalid MailMetadataCorrespondentOption");
            }
            switch (mailCorrespondentOption)
            {
                case MailMetadataCorrespondentOption.FromEmail:

                    return new CreateCorrespondent
                    {
                        Id=Guid.NewGuid(),
                        Name = email,
                        Slug = email,
                        Match = listEmail,
                        Matching_algorithm = Domain.Correspondents.Matching_Algorithms.MATCH_ANY,
                        Is_insensitive = false,
                        Owner = mail.Owner,
                        Document_count = 0,
                        Last_correspondence = DateTime.UtcNow
                    };


                case MailMetadataCorrespondentOption.FromName:
                    return new CreateCorrespondent
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Slug = name,
                        Match = listName,
                        Matching_algorithm = Domain.Correspondents.Matching_Algorithms.MATCH_ANY,
                        Is_insensitive = false,
                        Owner = mail.Owner,
                        Document_count = 0,
                        Last_correspondence = DateTime.UtcNow
                    };

                case MailMetadataCorrespondentOption.FromCustom:
                    List<CreateCorrespondent> correspondents = await GetCorrespondents(idOwner);
                    // ToDo To old code
                    return correspondents.First(correspondent => correspondent.Id == mail.Assign_correspondent);



                case MailMetadataCorrespondentOption.FromNothing:
                    return null;

                default:
                    Log.Error("we can not Found MailMetadataCorrespondentOption");
                    throw new Exception("we can not Found MailMetadataCorrespondentOption");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error in SetCorrespondentFromAssignCorrespondentFrom: {ex.Message}");
            throw;
        }
    }

}



