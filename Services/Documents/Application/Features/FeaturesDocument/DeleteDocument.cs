using Application.Respository;
using MediatR;

namespace Application.Features.FeaturesDocument
{
    public class DeleteDocument
    {
        public sealed record Command : IRequest
        {
            public readonly Guid Id;
            public Command(Guid id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Command>
        {
            private readonly IDocumentRepository _repository;
        
            public Handler(IDocumentRepository repository)
            {
                _repository = repository;
               
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                await _repository.DeleteByIdAsync(request.Id, cancellationToken);
            }
        }
    }

}
