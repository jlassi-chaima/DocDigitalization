using Application.Repository;
using MediatR;


namespace Application.Features.FeatureUser
{
    public class GetUserRoleByID
    {
        public sealed record Query : IRequest<string>
        {
            public readonly string Id;
            public Query(string id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Query, string>
        {
            private readonly IUserRepository _repository;

            public Handler(IUserRepository repository)
            {
                _repository = repository;
            }

            public async Task<string> Handle(Query request, CancellationToken cancellationToken)
            {
                string userdetails = await _repository.GetUserRoleByID(request.Id, cancellationToken);

                return userdetails;
            }
        }
    }
}
