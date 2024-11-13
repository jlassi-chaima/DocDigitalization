

using Application.Repository;
using Domain.User;
using MediatR;

namespace Application.Features.FeatureUser
{
    public class GetUser
    {
        public sealed record Query : IRequest<List<string>>
        {
            public readonly string Id;
            public Query(string id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Query, List<string>>
        {
            private readonly IUserRepository _repository;

            public Handler(IUserRepository repository)
            {
                _repository = repository;
            }

            public async Task<List<string>> Handle(Query request, CancellationToken cancellationToken)
            {
                ApplicationUser userdetails = await _repository.FindByIdAsyncString(request.Id, cancellationToken);

                return (List<string>)userdetails.Permissions;
            }
        }
    }
}
