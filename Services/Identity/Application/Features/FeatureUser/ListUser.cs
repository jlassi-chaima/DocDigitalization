
using Application.Dtos.UserDTO;
using Application.Repository;
using DD.Core.Pagination;
using Domain.User;
using Mapster;
using MediatR;


namespace Application.Features.FeatureUser
{
    public class ListUser
    {
        public sealed record Query : IRequest<List<UserList>>
        {
          

        }
        public sealed class Handler : IRequestHandler<Query, List<UserList>>
        {
            private readonly IUserRepository _userRepository;

            public Handler(IUserRepository userRepository)
            {
                _userRepository = userRepository;
            }

            public async Task<List<UserList>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var users = await _userRepository.GetAllUsers();
                    var listusers= users.Adapt<List<UserList>>();
                    foreach (var user in listusers)
                    {
                        user.PasswordHash = "";
                    }
                    return listusers;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message); 
                }
            }
      }

    }
}
