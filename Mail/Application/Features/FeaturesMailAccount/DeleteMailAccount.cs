using Application.Repository;
using MediatR;


namespace Application.Features.FeaturesMailAccount
{
    public class DeleteMailAccount
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
            private readonly IMailAccountRepository _repository;

            public Handler(IMailAccountRepository repository)
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