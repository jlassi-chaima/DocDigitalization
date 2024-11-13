using Application.Respository;
using Domain.DocumentManagement.DocumentTypes;
using Domain.DocumentManagement.tags;
using MediatR;

namespace Application.Features.FeaturesDocumentType
{
    public class DetailsDocumentType
    {
        public sealed record Query : IRequest<DocumentType>
        {
            public readonly Guid Id;
            public Query(Guid id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Query, DocumentType>
        {
            private readonly IDocumentTypeRepository _repository;

            public Handler(IDocumentTypeRepository repository)
            {
                _repository = repository;
            }

            public async Task<DocumentType> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _repository.FindByIdAsync(request.Id, cancellationToken);
            }
        }
    }
}
