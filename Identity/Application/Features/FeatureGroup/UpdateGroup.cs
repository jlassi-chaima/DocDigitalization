using Application.Dtos.GroupDTO;
using Application.Repository;
using Domain.User;
using MapsterMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.FeatureGroup
{
    public class UpdateGroup
    {
        public sealed record Command : IRequest<Groups>
        {
            public readonly Guid groupId;
            public readonly GroupDto Grouptoupdate;

            public Command(GroupDto grouptoupdate, Guid id)
            {
                Grouptoupdate = grouptoupdate;
                groupId = id;
            }
        }

        public sealed class Handler : IRequestHandler<Command, Groups>
        {
            private readonly IGroupRepository _repository;
            private readonly IMapper _mapper;
            private readonly IUserRepository _userRepository;
            public Handler(IGroupRepository repository, IMapper mapper, IUserRepository userRepository)
            {
                _repository = repository;
                _mapper = mapper;
                _userRepository = userRepository;
            }

            public async Task<Groups> Handle(Command request, CancellationToken cancellationToken)
            {
                Groups thegrouptoupdate = _repository.FindByIdAsync(request.groupId, cancellationToken).GetAwaiter().GetResult();

                foreach (UserGroups usergroup in thegrouptoupdate.Users)
                {
                    ApplicationUser theusertoupdate = _userRepository.FindByIdAsyncString(usergroup.UserID, cancellationToken).GetAwaiter().GetResult();
                    IList<string> user_old_Permissions = theusertoupdate.Permissions;

                    // Collect permissions to be removed in a separate list
                    List<string> permissionsToRemove = new List<string>();
                    foreach (string permission in user_old_Permissions)
                    {
                        if (thegrouptoupdate.Permissions.Contains(permission))
                        {
                            permissionsToRemove.Add(permission);
                        }
                    }

                    // Remove the collected permissions
                    foreach (string permission in permissionsToRemove)
                    {
                        theusertoupdate.Permissions.Remove(permission);
                    }

                    // Add the new permissions to the user based on the group modifications
                    IList<string> user_new_Permissions = request.Grouptoupdate.Permissions;
                    foreach (string permission in user_new_Permissions)
                    {
                        if (!theusertoupdate.Permissions.Contains(permission))
                        {
                            theusertoupdate.Permissions.Add(permission);
                        }
                    }

                    await _userRepository.UpdateAsync(theusertoupdate);
                }

                thegrouptoupdate.Permissions = request.Grouptoupdate.Permissions;
                thegrouptoupdate.Name = request.Grouptoupdate.Name;
              //  _mapper.Map(request.Grouptoupdate, thegrouptoupdate);
                await _repository.UpdateAsync(thegrouptoupdate);
                return thegrouptoupdate;

            }
        }
    }
}
