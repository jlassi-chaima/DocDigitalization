using Application.Dtos.Documents;
using Application.Features.FeaturesDocument.Documents;
using Application.Respository;
using Domain.Documents;
using MediatR;
using Serilog;


namespace Application.Features.FeatureDashboard
{
    public class ChartsDetails
    {
        public sealed record Query : IRequest<Dictionary<string, int>>
        {
            public readonly string Id;
            public Query(string owner)
            {
                Id = owner;
            }
        }

        public sealed class Handler : IRequestHandler<Query, Dictionary<string, int>>
        {
            private readonly IDocumentRepository _documentRepository;
            private readonly IUploadDocumentUseCase _uploadDocumentUseCase;

            public Handler(IDocumentRepository documentRepository, IUploadDocumentUseCase uploadDocumentUseCase)
            {
                _documentRepository = documentRepository;
                _uploadDocumentUseCase= uploadDocumentUseCase;

            }

            public async Task<Dictionary<string, int>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {

                   Guid groupId=await _uploadDocumentUseCase.GetGroupForUser(request.Id);
                    List<Document> documents = await _documentRepository.GetAllDocumentByGroup(groupId);


                    var documentsPerMonth = new Dictionary<string, int>();


                    foreach (var document in documents)
                    {
                        string monthKey = document.CreatedOn.ToString("yyyy-MM");
                        if (!documentsPerMonth.ContainsKey(monthKey))
                        {
                            documentsPerMonth[monthKey] = 0;
                        }
                        documentsPerMonth[monthKey]++;
                    }


                    var orderedDocumentsPerMonth = documentsPerMonth
                        .OrderBy(entry => DateTime.ParseExact(entry.Key, "yyyy-MM", null))
                        .ToList();


                    var orderedDocumentsPerMonthDict = orderedDocumentsPerMonth.ToDictionary(entry => entry.Key, entry => entry.Value);

                    return orderedDocumentsPerMonthDict;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    throw new Exception("An error has occurred, please try again later");
                }
            }
        }
    }
}
