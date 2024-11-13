using Application.Respository;
using Domain.DocumentManagement.DocumentNote;
using MediatR;


namespace Application.Features.FeaturesNote
{
    public class ListNote
    {
        public sealed record Query : IRequest<List<DocumentNote>>
        {
            public readonly Guid Id;
            public Query(Guid id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Query, List<DocumentNote>>
        {
            private readonly IDocumentNoteRepository _repository;
                
            public Handler(IDocumentNoteRepository repository)
            {
                _repository = repository;
            }

            public async Task<List<DocumentNote>> Handle(Query request, CancellationToken cancellationToken)
            {
                return (List<DocumentNote>)await _repository.GetAllByDocIdAsync(request.Id, cancellationToken);
            }
        }
    }
}
