using Application.Exceptions;
using Application.Repository;
using MediatR;

namespace Application.Features.FeatureUser
{
    public class VerifyUserExist
    {
        public sealed record Query : IRequest<bool>
        {
            public readonly string Id;
            public Query(string id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Query, bool>
        {
            private readonly IUserRepository _repository;

            public Handler(IUserRepository repository)
            {
                _repository = repository;
            }

            public async Task<bool> Handle(Query request, CancellationToken cancellationToken)
            {
                bool userDetails = await _repository.ExistsByIdAsync(request.Id, cancellationToken);
                if (userDetails == null)
                {
                    throw new UserException($"User with ID {request.Id} not found.");
                }
                return userDetails;
            }
        }
    }
}
