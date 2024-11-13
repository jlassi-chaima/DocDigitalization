using Application.Dtos.ArchivenSerialNumber;
using Application.Dtos.Correspondent;
using Application.Parameters;
using Application.Respository;
using Application.Services;
using Aspose.Pdf.Operators;
using DD.Core.Pagination;
using FluentValidation;
using MediatR;
using Serilog;
using System.Text.Json;

namespace Application.Features.FeaturesCorrespondent
{
    public class ListCorrespondent
    {
        public sealed record Query : IRequest<PagedList<CorrespondentListDTO>>
        {
            public CorrespondentParameters Correspondentparameters { get; set; }
            public string Owner { get; set; }
            public string NameIcontains { get; set; }
            public Query(string? owner, CorrespondentParameters coressparam, string? name_icontains)
            {
                Correspondentparameters = coressparam;
                Owner = owner;
                NameIcontains = name_icontains;
            }
        }
        
        public sealed class Handler : IRequestHandler<Query, PagedList<CorrespondentListDTO>>
        {
            private readonly ICorrespondentRepository _correspondentRepository;
            private readonly IUserGroupService _userGroupService;


            public Handler(ICorrespondentRepository repository, IUserGroupService userGroupService)
            {
                _correspondentRepository = repository;
                _userGroupService = userGroupService;
            }

            public async Task<PagedList<CorrespondentListDTO>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    List<string> GroupsList=new List<string>();
                    if (!string.IsNullOrEmpty(request.Owner))
                    {
                        GroupsList = await _userGroupService.GetGroupId(request.Owner);

                    }
                    if (!string.IsNullOrEmpty(request.NameIcontains))
                    {
                        return await _correspondentRepository.GetCorrespondentsByNameAsync(request.Correspondentparameters, request.NameIcontains, request.Owner);
                    }
                   
                    return await _correspondentRepository.GetPagedCorrespondentAsync<CorrespondentListDTO>(request.Correspondentparameters, request.Owner, GroupsList, cancellationToken);
                 
           
                }
                catch (Exception ex)
                {
                    Log.Error($"Error Message : {ex.Message}");
                    throw new Exception(ex.Message, ex);
                }
            }
        }
        // get list groupe by id user 
        //var apiUrl = "http://localhost:5183/user/getlistGroups";
        //// Append the result as a query parameter to the URL
        //apiUrl += "?id=" + Uri.EscapeDataString(request.Owner);

        //using (var httpClient = new HttpClient())
        //{

        //    var response = await httpClient.GetAsync(apiUrl);

        //    // Ensure the request completed successfully
        //    response.EnsureSuccessStatusCode();

        //    // Read the response content as a string
        //    var responseBody = await response.Content.ReadAsStringAsync();
        //    GroupsList = JsonSerializer.Deserialize<List<string>>(responseBody, new JsonSerializerOptions
        //    {
        //        PropertyNameCaseInsensitive = true
        //    });

        //}
    }
}
