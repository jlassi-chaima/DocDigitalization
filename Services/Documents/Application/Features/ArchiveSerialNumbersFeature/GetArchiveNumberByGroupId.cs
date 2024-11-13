using Application.Respository;
using Core.Exceptions;
using Domain.Documents;
using MediatR;
using Serilog;
using System.Web;

namespace Application.Features.ArchiveSerialNumbersFeature
{
    public class GetArchiveNumberByGroupId
    {

        public sealed record Query : IRequest<ArchiveSerialNumbers>
        {
            public readonly Guid GroupId;
            public readonly string IdOwner;

            public Query(Guid groupId,string idOwner)
            {
                GroupId = groupId;
                IdOwner = idOwner;
            }
        }
        public sealed class Handler : IRequestHandler<Query, ArchiveSerialNumbers>
        {
            private readonly IArchiveSerialNumberRepository _archiveSerialNumberRepository;

            public Handler(IArchiveSerialNumberRepository archiveSerialNumberRepository)
            {
                _archiveSerialNumberRepository = archiveSerialNumberRepository;
            }

            public async Task<ArchiveSerialNumbers> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    ArchiveSerialNumbers archiveSerialNumbers = await _archiveSerialNumberRepository.GetArchiveNumberByGroupId(request.GroupId);
                    if (archiveSerialNumbers == null)
                    {
                        throw new NotFoundException($"archive serial number not found.");
                    }
                    return archiveSerialNumbers;
                }
                catch (NotFoundException ex)
                {
                    Log.Error(ex.Message);
                    throw new NotFoundException(ex.Message);
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

