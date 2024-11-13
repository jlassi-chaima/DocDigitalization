using Application.Dtos.DocumentType;
using Application.Parameters;
using Application.Respository;
using DD.Core.Pagination;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Features.FeaturesDocumentType
{
    public class ListDocumentTypeDropDown
    {
        public sealed record Query : IRequest<PagedList<DocumentTypeDetailsDTO>>
        {
            public DocumentTypeParameters documenttypeparameters { get; set; }
            public string? Owner { get; set; }
            public Query(string? owner, DocumentTypeParameters doctypeparam)
            {
                documenttypeparameters = doctypeparam;
                Owner = owner;

            }
        }
        public sealed class Handler : IRequestHandler<Query, PagedList<DocumentTypeDetailsDTO>>
        {
            private readonly IDocumentTypeRepository _repository;

            public Handler(IDocumentTypeRepository repository)
            {
                _repository = repository;
            }

            public async Task<PagedList<DocumentTypeDetailsDTO>> Handle(Query request, CancellationToken cancellationToken)
            {
              
                return await _repository.GetPagedDocumentTypeAsync<DocumentTypeDetailsDTO>(request.documenttypeparameters, request.Owner,null, cancellationToken);
            }
        }
    }
}
