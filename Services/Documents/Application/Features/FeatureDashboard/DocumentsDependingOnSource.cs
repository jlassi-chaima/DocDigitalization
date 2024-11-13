using Application.Features.FeaturesDocument.Documents;
using Application.Respository;
using Domain.Documents;
using Domain.Templates.Enum;
using MediatR;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dashboard
{
    public class DocumentsDependingOnSource
    {
        public sealed record Query : IRequest<Dictionary<DocumentSource, int>>
        {
            public readonly string Id;
            public Query(string owner)
            {
                Id = owner;
            }
        }
        public sealed class Handler : IRequestHandler<Query, Dictionary<DocumentSource, int>>
        {
            private readonly IDocumentRepository _documentRepository;
            private readonly IUploadDocumentUseCase _uploadDocumentUseCase;

            public Handler(IDocumentRepository repository, IUploadDocumentUseCase uploadDocumentUseCase)
            {
                _documentRepository = repository;
                _uploadDocumentUseCase = uploadDocumentUseCase;

            }

            public async Task<Dictionary<DocumentSource, int>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var documentCountsBySource = new Dictionary<DocumentSource, int>();

                    Guid groupId = await _uploadDocumentUseCase.GetGroupForUser(request.Id);
                    List<Document> documents = await _documentRepository.GetAllDocumentByGroup(groupId);

                    //List<Document> documents = await _repository.GetAllDocumentByOwner(request.Id);

                    foreach (var document in documents)
                    {
                        if (!documentCountsBySource.ContainsKey(document.Source))
                        {
                            documentCountsBySource[document.Source] = 0;
                        }
                        documentCountsBySource[document.Source]++;
                    }

                    return documentCountsBySource;
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
