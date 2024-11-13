

using Application.Dtos.ArchivenSerialNumber;
using Application.Dtos.CustomField;
using Application.Parameters;
using Application.Respository;
using DD.Core.Pagination;
using Domain.DocumentManagement.CustomFields;
using Domain.Documents;
using MediatR;
using Serilog;

namespace Application.Features.FeaturesCustomField
{
    public class ArchiveSerialNumberPagedList
    {
        public sealed record Query : IRequest<PagedList<ArchiveSerialNumbers>>
        {
            public ArchiveSerialNumberParameters ArchiveSerialNumberParameters { get; set; }
            public string Owner { get; set; }
            public string? NameIcontains { get; set; }

            public Query(ArchiveSerialNumberParameters archiveSerialNumberParameters,string owner,string name_icontains)
            {
                ArchiveSerialNumberParameters = archiveSerialNumberParameters;
                Owner = owner;
                NameIcontains = name_icontains;

            }
        }
        public sealed class Handler : IRequestHandler<Query, PagedList<ArchiveSerialNumbers>>
        {
            private readonly IArchiveSerialNumberRepository _archiveSerialNumberRepository;

            public Handler(IArchiveSerialNumberRepository archiveSerialNumberRepository )
            {
                _archiveSerialNumberRepository = archiveSerialNumberRepository;
            }

            public async Task<PagedList<ArchiveSerialNumbers>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {

                    return await _archiveSerialNumberRepository.GetArchiveSerialNumberNameAsync<ArchiveSerialNumbers>(request.ArchiveSerialNumberParameters, request.Owner,request.NameIcontains);
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
