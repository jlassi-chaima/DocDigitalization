using Application.Dtos.ShareFolder;
using Application.PaginationParams;
using Application.Repository;
using DD.Core.Pagination;
using Domain.Ports;
using MediatR;
using Newtonsoft.Json;
using Serilog;


namespace Application.Features.FeatureShareFolder
{
    public class ListShareFolder
    {
        public sealed record Query : IRequest<PagedList<ShareFolderPagedList>>
        {
            public ShareFolderParameters Sharefolderparameters { get; set; }
            public string Owner { get; set; }
            public Query(ShareFolderParameters sharefolderparam, string owner)
            {
                Sharefolderparameters = sharefolderparam;
                Owner = owner;

            }

        }
        public sealed class Handler : IRequestHandler<Query, PagedList<ShareFolderPagedList>>
        {
            private readonly IShareFolderRepository _repository;
            private readonly IUserGroupPort _userGroupPort;

            public Handler(IShareFolderRepository repository, IUserGroupPort userGroupPort)
            {
                _repository = repository;
                _userGroupPort=userGroupPort;
            }

            public async Task<PagedList<ShareFolderPagedList>> Handle(Query request, CancellationToken cancellationToken)
            {
                //return (List<ShareFolder>)await _repository.GetAllAsync();
                try
                {
                   
                    Guid groupId=await GetGroupForUser(request.Owner);
                 return await _repository.GetPagedFileShareAsync(request.Sharefolderparameters,request.Owner, groupId);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    throw new Exception(ex.Message);
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
