

using Application.Features.FeaturesDocument.Documents;
using Application.Respository;
using Domain.Documents;
using Domain.Ports;
using MediatR;
using Newtonsoft.Json;
using Serilog;

namespace Application.Features.FeatureDashboard
{
    public class DocumetsGroupsStatistics
    {
        public sealed record Query : IRequest<Dictionary<string, int>>
        {

            public Query()
            {
             
            }
        }

        public sealed class Handler : IRequestHandler<Query, Dictionary<string, int>>
        {
            private readonly IDocumentRepository _documentRepository;
            private readonly IUserGroupPort _userGroupPort;
            


            public Handler(IDocumentRepository documentRepository,IUserGroupPort userGroupPort)
            {
                _documentRepository = documentRepository;
                _userGroupPort = userGroupPort;


            }

            public async Task<Dictionary<string, int>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {


                    List<Document> documents = await _documentRepository.GetAllDocuments();


                    var groups = await GetGroups();
                    var groupDictionary = groups.ToDictionary(g => g.Id.ToString(), g => g.Name); 

                 
                    var documentCountsByGroupName = new Dictionary<string, int>();

                    if (documents != null && documents.Count > 0)
                    {
                       
                        var documentCountsByGroupId = documents
                            .GroupBy(doc => doc.GroupId.ToString()) 
                            .ToDictionary(group => group.Key, group => group.Count());

                       
                        foreach (var kvp in documentCountsByGroupId)
                        {
                           
                            string groupName = groupDictionary.ContainsKey(kvp.Key) ? groupDictionary[kvp.Key] : "Unknown Group";
                            documentCountsByGroupName[groupName] = kvp.Value; // Set the count for the group name
                        }
                    }

                    return documentCountsByGroupName;




                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    throw new Exception("An error has occurred, please try again later");
                }
            }
            public async Task<List<Groups>> GetGroups()
            {
                try
                {
                    var res = await _userGroupPort.GetGroups();
                    var responseContent = await res.Content.ReadAsStringAsync();
                    if (res.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        Log.Error($"Error Message : {responseContent}");
                        throw new HttpRequestException("An error has occured, please try again later");
                    }
                    var JSONObj = JsonConvert.DeserializeObject<List<Groups>>(responseContent)!;
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
        }
    }
}
