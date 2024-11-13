

using Application.Dtos.MailAccount;
using Application.PaginationParams;
using Application.Repository;
using DD.Core.Pagination;
using Domain.Ports;
using MediatR;
using Newtonsoft.Json;
using Serilog;

namespace Application.Features.FeaturesMailAccount
{
    public class ListMailAccount
    {
        public sealed record Query : IRequest<PagedList<MailAccountPagedList>>
        {
            public MailAccountParameters Mailaccountparameters { get; set; }
            public string Owner { get; set; }

            public Query(MailAccountParameters mailaccountparam, string owner)
            {
                Mailaccountparameters = mailaccountparam;
                Owner = owner;


            }
        }
        public sealed class Handler : IRequestHandler<Query, PagedList<MailAccountPagedList>>
        {
            private readonly IMailAccountRepository _repository;
            private readonly IUserGroupPort _userGroupPort;


            public Handler(IMailAccountRepository repository, IUserGroupPort userGroupPort)
            {
                _repository = repository;
                _userGroupPort = userGroupPort;

            }

            public async Task<PagedList<MailAccountPagedList>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    Guid groupId = await GetGroupForUser(request.Owner);

                    return await _repository.GetAllByPagedAsync(request.Mailaccountparameters, groupId);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    throw new Exception($"Exception: {ex.Message}");
                }
            }
            private async Task<Guid> GetGroupForUser(string idOwner)
            {
                try
                {
                    var res = await _userGroupPort.GetFirstGRoupForUser(idOwner);
                    var responseContent = await res.Content.ReadAsStringAsync();
                    if (res.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        Log.Error($"Error Message : {responseContent}");
                        throw new HttpRequestException("An error has occured, please try again later");
                    }
                    var JSONObj = JsonConvert.DeserializeObject<Guid>(responseContent)!;
                    return JSONObj;
                }

                catch (HttpRequestException ex)
                {
                    Log.Error(ex.ToString());
                    throw new HttpRequestException(ex.Message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    throw new Exception($"Exception: {ex.Message}");
                }

            }
        }
    }
}
