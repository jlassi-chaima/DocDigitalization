using Application.Dtos.ArchivenSerialNumber;
using Application.Respository;
using Application.Services;
using Core.Exceptions;
using Domain.DocumentManagement.CustomFields;
using Domain.Documents;
using Domain.Logs;
using FluentValidation;
using MediatR;
using Serilog;


namespace Application.Features.ArchiveSerialNumbersFeature
{
    public class DeleteArchiveSerialNumber
    {
        public sealed record Command : IRequest
        {
            public readonly Guid Id;
            public Command(Guid id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Command>
        {
            private readonly IArchiveSerialNumberRepository _archiveSerialNumberRepository;
    
            private readonly ILogService _logService;

            public Handler(IArchiveSerialNumberRepository archiveSerialNumberRepository, ILogService logService)
            {
                _archiveSerialNumberRepository = archiveSerialNumberRepository;
                _logService = logService;
            }
            public sealed class GuidValidator : AbstractValidator<Guid>
            {
                public GuidValidator()
                {

                    RuleFor(p => p).NotEmpty().WithMessage("Id is required");

                }
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    ArchiveSerialNumbers archiveToDelete = await _archiveSerialNumberRepository.FindByIdAsync(request.Id, cancellationToken);
                    if(archiveToDelete == null)
                    {
                        Log.Error("archive serial number not Found.");
                        await _logService.AddLogs(LogLevel.ERROR, LogName.EasyDoc, "archive serial number not Found.");
                        throw new NotFoundException("archive serial number not Found.");
                    }
                    await _logService.AddLogs(LogLevel.INFO, LogName.EasyDoc, $"archive serial number deleted {archiveToDelete.Prefix}-{archiveToDelete.GroupName}-{archiveToDelete.Year}");
                 
                    await _archiveSerialNumberRepository.DeleteAsync(archiveToDelete, cancellationToken);
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
