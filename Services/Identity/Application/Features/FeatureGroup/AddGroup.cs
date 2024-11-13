using Application.Dtos.GroupDTO;
using Application.Repository;
using Domain.User;
using FluentValidation;
using MediatR;
using Serilog;


namespace Application.Features.FeatureGroup
{
    public class AddGroup
    {
        public sealed record Command : IRequest<Groups>
        {
            public readonly GroupDto Groupuser;

            public Command(GroupDto groupDto)
            {
                Groupuser = groupDto;
            }
        }
        public sealed class AddValidator : AbstractValidator<Command>
        {
            public AddValidator(IGroupRepository _repository)
            {
                RuleFor(p => p.Groupuser.Name).NotEmpty();


            }
        }
        public sealed class Handler : IRequestHandler<Command, Groups>
        {
            private readonly IGroupRepository _groupRepository;

            public Handler(IGroupRepository groupRepository)
            {
                _groupRepository = groupRepository;
            }

            public async Task<Groups> Handle(Command request, CancellationToken cancellationToken)
            {

                var userGroup = request.Groupuser;

                //Groups groupToAdd = Groups.Create(
                //                                   userGroup.Name,
                //                                   userGroup.Permissions
                //                              );
                //// Hash the password

                //await _groupRepository.AddAsync(groupToAdd, cancellationToken);
                //return groupToAdd;
                //               List<string> permissions = [
                //                   "add_document",
                //                   "view_document",
                //                   "change_document",
                //                   "delete_document",
                //                   "add_tag",
                //                   "view_tag",
                //                   "change_tag",
                //                   "delete_tag",
                //                   "add_correspondent",
                //                   "view_correspondent",
                //                   "change_correspondent",
                //                   "delete_correspondent",
                //                   "add_documenttype",
                //                   "view_documenttype",
                //                   "change_documenttype",
                //                   "delete_documenttype",
                //                   "add_storagepath",
                //                   "view_storagepath",
                //                   "change_storagepath",
                //                   "delete_storagepath",
                //                   "add_savedview",
                //                   "view_savedview",
                //                   "change_savedview",
                //                   "delete_savedview",
                //                   "add_paperlesstask",
                //                   "view_paperlesstask",
                //                   "change_paperlesstask",
                //                   "delete_paperlesstask",
                //                   "add_uisettings",
                //                   "view_uisettings",
                //                   "change_uisettings",
                //                   "delete_uisettings",
                //                   "add_note",
                //                   "view_note",
                //                   "change_note",
                //                   "delete_note",
                //                   "add_mailaccount",
                //                   "view_mailaccount",
                //                   "change_mailaccount",
                //                   "delete_mailaccount",
                //                   "add_mailrule",
                //                   "view_mailrule",
                //                   "change_mailrule",
                //                   "delete_mailrule",
                //                   "add_user",
                //                   "view_user",
                //                   "change_user",
                //                   "delete_user",
                //                   "add_group",
                //                   "view_group",
                //                   "change_group",
                //                   "delete_group",
                //                   "add_logentry",
                //                   "view_logentry",
                //                   "change_logentry",
                //                   "delete_logentry",
                //                   "add_sharelink",
                //                   "view_sharelink",
                //                   "change_sharelink",
                //                   "delete_sharelink",
                //                   "add_consumptiontemplate",
                //                   "view_consumptiontemplate",
                //                   "change_consumptiontemplate",
                //                   "delete_consumptiontemplate",
                //                   "add_customfield",
                //                   "view_customfield",
                //                   "change_customfield",
                //                   "delete_customfield"
                //];


                //if (userGroup.Permissions != null)
                //{
                //    bool allPermissionsExist = userGroup.Permissions.All(x => permissions.Contains(x));

                //    if (allPermissionsExist)
                //    {
                try
                {
                    Groups groupToAdd = Groups.Create(
                                                   userGroup.Name,
                                                   userGroup.Permissions
                                              );


                    await _groupRepository.AddAsync(groupToAdd, cancellationToken);
                    return groupToAdd;
                }
                catch(Exception  ex)
                {
                    Log.Error(ex.Message);
                    throw new Exception(ex.Message);
                }
                       
                //    }

                //}
                //else
                //{
                //    Groups groupToAdd = Groups.Create(
                //                                    userGroup.Name,
                //                                    null
                //                               );
                //    await _groupRepository.AddAsync(groupToAdd, cancellationToken);
                //    return groupToAdd;
                //}
                //return null;



            }

        }
    }
}
