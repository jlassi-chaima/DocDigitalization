using Application.Dtos.Documents;
using Application.Features.ArchiveSerialNumbersFeature;
using Application.Respository;
using Application.Services;
using DD.Core.Pagination;
using Domain.Ports;
using MediatR;
using Newtonsoft.Json;
using Serilog;
using System.Globalization;
using System.Text.RegularExpressions;

public sealed class GetDocumentByTagCorrespondentDocumentType
{
    public sealed record Query : IRequest<PagedList<DocumentDetailsDTO>>
    {
        public DocumentSearchParameters DocumentSearchParameters;
        public Query(DocumentSearchParameters documentSearchParameters)
        {
            DocumentSearchParameters = documentSearchParameters;
        }

        public sealed class Handler : IRequestHandler<Query, PagedList<DocumentDetailsDTO>>
        {
            private readonly IDocumentRepository _repository;
           
            private readonly IUserGroupService _userGroupService;

            public Handler(IDocumentRepository repository,IUserGroupService userGroupService)
            {
                _repository = repository;
                _userGroupService = userGroupService;
            }

            public async Task<PagedList<DocumentDetailsDTO>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {

                    DateTime? createdFrom = null;
                    DateTime? createdTo = null;

                    if (!string.IsNullOrEmpty(request.DocumentSearchParameters.Created))
                    {
                        // Example: created:[-1 week to now]
                        TryParseDate(ref createdTo, ref createdFrom, request.DocumentSearchParameters.Created);
                    }
                    Guid groupId = Guid.Empty;
                    List<string> GroupsList = new List<string>();
                    if (!string.IsNullOrEmpty(request.DocumentSearchParameters.Owner))
                    {
                        groupId = await _userGroupService.GetGroupForUser(request.DocumentSearchParameters.Owner); 
                        // ToDO:remove when use one group
                        GroupsList.Add(groupId.ToString());
                            //await _userGroupService.GetGroupId(request.DocumentSearchParameters.Owner);
                    }

                    var DocumentList = await _repository.GetDocumentByTagCorrespondentDocumentType<DocumentDetailsDTO>(
                     new DocumentSearchDto()
                     {
                         DocumentParameters = request.DocumentSearchParameters.DocumentParameters,
                         TagID = ParseGuids(request.DocumentSearchParameters.TagID),
                         CorrespondentID = ParseGuids(request.DocumentSearchParameters.CorrespondentID),
                         DocumentTypeID = ParseGuids(request.DocumentSearchParameters.DocumentTypeID),
                         StoragePathID = ParseGuids(request.DocumentSearchParameters.StoragePathID),
                         TitleContains = request.DocumentSearchParameters.TitleIcontains,
                         CreatedFrom = createdFrom,
                         CreatedTo = createdTo,
                         Owner = request.DocumentSearchParameters.Owner,
                         OwnerIdNone = request.DocumentSearchParameters.OwnerIdNone,
                         OwnerIsNull = request.DocumentSearchParameters.OwnerIsNull,
                         Search = request.DocumentSearchParameters.Search,
                         Ordering = request.DocumentSearchParameters.Ordering,
                         ArchiveSerialNumber = request.DocumentSearchParameters.ArchiveSerialNumber,
                         ArchiveSerialNumberIsNull = request.DocumentSearchParameters.ArchiveSerialNumberIsNull,
                         ArchiveSerialNumberGT = request.DocumentSearchParameters.ArchiveSerialNumberGT,
                         ArchiveSerialNumberLT = request.DocumentSearchParameters.ArchiveSerialNumberLT,
                         TitleContent = request.DocumentSearchParameters.TitleContent,

                     },
                     groupId,
                     cancellationToken
                 );

                    return DocumentList;
                }
                catch (Exception ex)
                {
                    Log.Error($"Error Message : {ex.Message}");
                    throw new Exception(ex.Message, ex);
                }
            }
          
            private List<Guid> ParseGuids(string? guidsString)
            {
                var guidList = new List<Guid>();
                if (!string.IsNullOrEmpty(guidsString))
                {
                    var guidStrings = guidsString.Split(',');
                    foreach (var guidString in guidStrings)
                    {
                        if (Guid.TryParse(guidString, out var guid))
                        {
                            guidList.Add(guid);
                        }
                    }
                }
                return guidList;
            }
            private void TryParseDate(ref DateTime? createdTo, ref DateTime? createdFrom, string created)
            {
                var match = Regex.Match(created, @"created:\[(-?\d+)\s+(\w+)\s+to\s+(now|\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2})\]", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var value = int.Parse(match.Groups[1].Value);
                    var unit = match.Groups[2].Value.ToLower();
                    var toPart = match.Groups[3].Value;

                    switch (unit)
                    {
                        case "day":
                            createdFrom = DateTime.UtcNow.AddDays(value).Date;
                            break;
                        case "week":
                            createdFrom = DateTime.UtcNow.AddDays(7 * value).Date;
                            break;
                        case "month":
                            createdFrom = DateTime.UtcNow.AddMonths(value).Date;
                            break;
                        case "year":
                            createdFrom = DateTime.UtcNow.AddYears(value).Date;
                            break;
                        default:
                            // Handle other units if needed
                            break;
                    }

                    if (toPart.Equals("now", StringComparison.OrdinalIgnoreCase))
                    {
                        createdTo = DateTime.UtcNow;
                    }
                    else if (DateTime.TryParseExact(toPart, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
                    {
                        createdTo = toDate.ToUniversalTime();
                    }
                }
            }
            private bool TryParseOffset(string input, out TimeSpan offset)
            {
                offset = TimeSpan.Zero;
                var regex = new Regex(@"-(\d+)\s*(week|month|year|day)s?", RegexOptions.IgnoreCase);
                var match = regex.Match(input);

                if (!match.Success)
                {
                    return false;
                }

                var value = int.Parse(match.Groups[1].Value);
                var unit = match.Groups[2].Value.ToLower();

                switch (unit)
                {
                    case "day":
                        offset = TimeSpan.FromDays(-value);
                        break;
                    case "week":
                        offset = TimeSpan.FromDays(-7 * value);
                        break;
                    case "month":
                        offset = TimeSpan.FromDays(-30 * value); // Approximation
                        break;
                    case "year":
                        offset = TimeSpan.FromDays(-365 * value); // Approximation
                        break;
                    default:
                        return false;
                }

                return true;
            }
            
            //public readonly string? TagID;
            //public readonly string? CorrespondentID;
            //public readonly string? DocumentTypeID;
            //public readonly string? StoragePathID;
            //public readonly string? Title__icontains;
            //public readonly string? Created;
            //public readonly string? Owner;
            //public readonly string? OwnerId;
            //public readonly string? OwnerIdNone;
            //public readonly string? Search;
            //public readonly int? OwnerIsNull;
            //public readonly string? Ordering;
            //public readonly int? ArchiveSerialNumber;
            //public readonly int? ArchiveSerialNumberIsNull;
            //public readonly int? ArchiveSerialNumberGT;
            //public readonly int? ArchiveSerialNumberLT;
            //public readonly string? TitleContent;

            //  public DocumentParameters Documentparameters { get; set; }
            //foreach (var document in DocumentList.Results)
            //{
            //    if (document.Owner != request.Owner && document.Owner != null)
            //    {
            //        var apiUrlgetname = "http://localhost:5183/user/get_user_name";
            //        // Append the result as a query parameter to the URL
            //        apiUrlgetname += "?id=" + Uri.EscapeDataString(document?.Owner);
            //        using (var httpClient = new HttpClient())
            //        {

            //            var response = await httpClient.GetAsync(apiUrlgetname);

            //            // Ensure the request completed successfully
            //            response.EnsureSuccessStatusCode();

            //            // Read the response content as a string
            //            var responseBody = await response.Content.ReadAsStringAsync();


            //            document.Owner = responseBody;
            //        }
            //    }
            //}


            //List<Guid> guidListTag = ParseGuids(request.TagID);
            //List<Guid> guidListCorrespondent = ParseGuids(request.CorrespondentID);
            //List<Guid> guidListDocumentType = ParseGuids(request.DocumentTypeID);
            //List<Guid> guidListStoragePath = ParseGuids(request.StoragePathID);
        }
    }
}
