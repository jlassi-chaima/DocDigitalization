

using Application.Repository;
using MediatR;

namespace Application.Features.FeatureShareFolder
{
    public class DeleteShareFolder
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
            private readonly IShareFolderRepository _repository;

            public Handler(IShareFolderRepository repository)
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

