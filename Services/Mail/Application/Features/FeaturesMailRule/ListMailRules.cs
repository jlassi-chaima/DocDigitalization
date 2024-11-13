using Application.Dtos.MailRule;
using Application.PaginationParams;
using Application.Repository;
using DD.Core.Pagination;
using Domain.Ports;
using MediatR;
using Newtonsoft.Json;
using Serilog;


namespace Application.Features.FeaturesMailRule
{
    public class ListMailRules
    {
        public sealed record Query : IRequest<PagedList<MailRulePagedList>>
        {
            public MailRuleParameters Mailruleparameters { get; set; }
            public string Owner { get; set; }

            public Query(MailRuleParameters mailruelparam, string owner)
            {
                Mailruleparameters = mailruelparam;
                Owner = owner;

            }
        }
        public sealed class Handler : IRequestHandler<Query, PagedList<MailRulePagedList>>
        {
            private readonly IMailRuleRepository _repository;
            private readonly IUserGroupPort _userGroupPort;


            public Handler(IMailRuleRepository repository, IUserGroupPort userGroupPort)
            {
                _repository = repository;
                _userGroupPort = userGroupPort;


            }

            public async Task<PagedList<MailRulePagedList>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    Guid groupId = await GetGroupForUser(request.Owner);
                    return await _repository.GetAllByPagedAsync(request.Mailruleparameters, groupId);
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
