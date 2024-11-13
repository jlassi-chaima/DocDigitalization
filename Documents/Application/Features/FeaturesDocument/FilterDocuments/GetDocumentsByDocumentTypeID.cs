using Application.Dtos.Documents;
using Application.Parameters;
using Application.Respository;
using DD.Core.Pagination;
using MediatR;


namespace Application.Features.FeaturesDocument.FilterDocuments
{
    public class GetDocumentsByDocumentTypeID
    {
        public sealed record Query : IRequest<PagedList<DocumentDetailsDTO>>
        {
            public readonly string? ID;
            public DocumentParameters documentparameters { get; set; }
            public Query(string? id, DocumentParameters docparam)
            {
                ID = id;
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
                string[] guidStrings = request.ID.Split(',');
                List<Guid> guidList = new List<Guid>();

                // Convert each string to a GUID and add it to the list
                foreach (string guidString in guidStrings)
                {
                    if (Guid.TryParse(guidString, out Guid guid))
                    {
                        guidList.Add(guid);
                    }

                }
                return await _repository.GetDocumentsByDocumentTypeID<DocumentDetailsDTO>(request.documentparameters, guidList, cancellationToken);
            }
        }
    }
}
