using Application.Dtos.Documents;
using Application.Parameters;
using Application.Respository;
using DD.Core.Pagination;
using MediatR;
using System.Text.Json;


namespace Application.Features.FeaturesDocument
{
    public class ListDocument
    {
        public sealed record Query : IRequest<PagedList<DocumentDetailsDTO>>
        {
            public DocumentParameters documentparameters { get; set; }
            public Query(DocumentParameters docparam)
            {
                documentparameters = docparam;

            }
        }
        public sealed class Handler : IRequestHandler<Query, PagedList<DocumentDetailsDTO>>
        {
            private readonly IDocumentRepository _repository;

            public Handler(IDocumentRepository repository)
            {
                _repository = repository;
            }

            public async Task<PagedList<DocumentDetailsDTO>> Handle(Query request, CancellationToken cancellationToken)
            {
               
                return await _repository.GetPagedDocumentAsync<DocumentDetailsDTO>(request.documentparameters,  cancellationToken);
            }
        }
    }
}
