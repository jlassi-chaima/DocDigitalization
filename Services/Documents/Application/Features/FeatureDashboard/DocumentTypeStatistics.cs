

using Application.Features.FeaturesDocument.Documents;
using Application.Respository;
using Domain.Documents;
using Domain.Ports;
using MediatR;
using Newtonsoft.Json;
using Serilog;

namespace Application.Features.FeatureDashboard
{
    public class DocumentTypeStatistics
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


            public Handler(IDocumentRepository documentRepository, IUserGroupPort userGroupPort)
            {
                _documentRepository = documentRepository;
                _userGroupPort = userGroupPort;



            }

            public async Task<Dictionary<string, int>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {


                    var groups = await GetGroups();
                    var groupDictionary = groups.ToDictionary(g => g.Id.ToString(), g => g.Name);

                    // Get all documents
                    List<Document> documents = await _documentRepository.GetAllDocuments();
                    Dictionary<string, int> documentCountsByGroupName = new Dictionary<string, int>();

                    if (documents != null && documents.Count > 0)
                    {
                        // Group documents by their associated group and document type
                        foreach (var document in documents)
                        {
                            if (document.Document_Type != null && document.GroupId != null)
                            {
                                // Get the group name from the dictionary
                                if (groupDictionary.TryGetValue(document.GroupId.ToString(), out string groupName))
                                {
                                    // Create a unique key combining group name and document type
                                    string key = $"{groupName}_{document.Document_Type.Name}";

                                    // Increment the count for this group and document type
                                    if (documentCountsByGroupName.ContainsKey(key))
                                    {
                                        documentCountsByGroupName[key]++;
                                    }
                                    else
                                    {
                                        documentCountsByGroupName[key] = 1;
                                    }
                                }
                            }
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




//public sealed class Handler : IRequestHandler<Query, Dictionary<string, int>>
//{
//    private readonly IDocumentRepository _documentRepository;


//    public Handler(IDocumentRepository documentRepository)
//    {
//        _documentRepository = documentRepository;


//    }

//    public async Task<Dictionary<string, int>> Handle(Query request, CancellationToken cancellationToken)
//    {
//        try
//        {


//            List<Document> documents = await _documentRepository.GetAllDocuments();
//            Dictionary<string, int> documentCountsByDocType = new Dictionary<string, int>();
//            if (documents != null)
//            {
//                // Grouping documents by Document_Type.Name and handling null Document_Type
//                documentCountsByDocType = documents
//                    .Where(doc => doc.Document_Type != null) // Filter out documents with null Document_Type
//                    .GroupBy(doc => doc.Document_Type.Name)  // Group by Document_Type.Name
//                    .ToDictionary(group => group.Key, group => group.Count());
//            }
//            return documentCountsByDocType;




//        }
//        catch (Exception ex)
//        {
//            Log.Error(ex.ToString());
//            throw new Exception("An error has occurred, please try again later");
//        }
//    }
//}