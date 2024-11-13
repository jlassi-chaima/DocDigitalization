using Application.Features.FeaturesDocument.Documents;
using Application.Respository;
using Domain.Documents;
using MediatR;


namespace Application.Features.FeatureDashboard
{
    public class DocumentsCountDeendingOnMimeType
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
            private readonly IDocumentRepository _repository;
            private readonly IUploadDocumentUseCase _uploadDocumentUseCase;
            public Handler(IDocumentRepository repository, IUploadDocumentUseCase uploadDocumentUseCase)
            {
                _repository = repository;
                _uploadDocumentUseCase = uploadDocumentUseCase;

            }

            public async Task<Dictionary<string, int>> Handle(Query request, CancellationToken cancellationToken)
            {
                var documentCountsByMimeType = new Dictionary<string, int>();

                Guid groupId = await _uploadDocumentUseCase.GetGroupForUser(request.Id);
                List<Document> documents = await _repository.GetAllDocumentByGroup(groupId);

                foreach (var document in documents)
                {
                    if (!documentCountsByMimeType.ContainsKey(document.MimeType))
                    {
                        documentCountsByMimeType[document.MimeType] = 0;
                    }
                    documentCountsByMimeType[document.MimeType]++;
                }

                return documentCountsByMimeType;
            }
        }
    }
}
