using Application.Repository;
using Domain.User;
using MediatR;
using Serilog;

namespace Application.Features.FeatureUser
{
    public class GetFirstGroupForUser
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
            private readonly IUserRepository _userRepository;

            public Handler(IUserRepository userRepository)
            {
                _userRepository = userRepository;
            }

            public async Task<string> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {

                    UserGroups userdetails = await _userRepository.GetFirstGroupForUser(request.Id, cancellationToken);

                    return userdetails?.GroupID.ToString()??"";
                }

                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}
