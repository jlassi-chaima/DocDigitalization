using Application.Repository;
using MediatR;


namespace Application.Features.FeatureUser
{
    public class DeleteUser
    {
        public sealed record Command : IRequest
        {
            public readonly string Id;
            public Command(string id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Command>
        {
            private readonly IUserRepository _repository;

            public Handler(IUserRepository repository)
            {
                _repository = repository;

            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {

                await _repository.DeleteByIdAsyncString(request.Id, cancellationToken);
            }
        }
    }
}
