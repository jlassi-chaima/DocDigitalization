using Application.Exceptions;
using Application.Respository;
using Core.Exceptions;
using Domain.DocumentManagement.CustomFields;
using Domain.Documents;
using MediatR;
using Serilog;


namespace Application.Features.ArchiveSerialNumbersFeature
{
    public class DetailsArchiveSerialNumber
    {

        public sealed record Query : IRequest<ArchiveSerialNumbers>
        {
            public readonly Guid Id;
            public Query(Guid id)
            {
                Id = id;
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
                    ArchiveSerialNumbers archiveSerialNumbers = await _archiveSerialNumberRepository.FindByIdAsync(request.Id, cancellationToken);
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
