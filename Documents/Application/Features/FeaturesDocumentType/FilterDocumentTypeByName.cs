using Application.Dtos.DocumentType;
using Application.Parameters;
using Application.Respository;
using DD.Core.Pagination;
using Domain.DocumentManagement.DocumentTypes;
using MediatR;


namespace Application.Features.FeaturesDocumentType
{
    public class FilterDocumentTypeByName
    {
        public sealed record Query : IRequest<PagedList<DocumentTypeDetailsDTO>>
        {
            public DocumentTypeParameters documenttypeparameters;
            public readonly string Name;
            public readonly string Owner;
            public Query(string namefilter, DocumentTypeParameters documentparams, string owner)
            {
                Name = namefilter;
                documenttypeparameters = documentparams;
                Owner = owner;
            }
        }
        public class Handler : IRequestHandler<Query, PagedList<DocumentTypeDetailsDTO>>
        {
            private readonly IDocumentTypeRepository _documenttypeRepository;

            public Handler(IDocumentTypeRepository documenttypeRepository)
            {
                _documenttypeRepository = documenttypeRepository;
            }

            public async Task<PagedList<DocumentTypeDetailsDTO>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Use the repository to fetch tags by name
                return await _documenttypeRepository.GetDocumentTypesByNameAsync(request.Name,request.documenttypeparameters, request.Owner);
            }
        }
    }
}
