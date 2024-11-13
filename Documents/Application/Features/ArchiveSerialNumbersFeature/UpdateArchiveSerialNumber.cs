using Application.Dtos.ArchivenSerialNumber;
using Application.Dtos.CustomField;
using Application.Respository;
using Application.Services;
using Aspose.Pdf.Operators;
using Core.Exceptions;
using Domain.DocumentManagement.CustomFields;
using Domain.Documents;
using Domain.Logs;
using MapsterMapper;
using MediatR;
using Serilog;

namespace Application.Features.ArchiveSerialNumbersFeature
{
    public class UpdateArchiveSerialNumber
    {
        public sealed record Command : IRequest<ArchiveSerialNumbers>
        {
            public readonly Guid Id;
            public readonly ArchiveSerialNumberDto ArchiveSerialNumberDto;

            public Command(ArchiveSerialNumberDto archiveSerialNumberDto, Guid id)
            {
                ArchiveSerialNumberDto = archiveSerialNumberDto;
                Id = id;
            }
        }
        //public sealed class Validator : AbstractValidator<Command>
        //{
        //    public Validator(ICustomFieldRepository _repository)
        //    {
        //        RuleFor(p => p.customfieldupdate.Data_type).Empty();
        //    }
        //}
        public sealed class Handler : IRequestHandler<Command, ArchiveSerialNumbers>
        {
            private readonly IArchiveSerialNumberRepository _archiveSerialNumberRepository;
            private readonly IMapper _mapper;
            private readonly ILogService _logService;
            public Handler(IArchiveSerialNumberRepository archiveSerialNumberRepository, IMapper mapper, ILogService logService)
            {
                _archiveSerialNumberRepository = archiveSerialNumberRepository;
                _mapper = mapper;
                _logService = logService;
            }
            public async Task<ArchiveSerialNumbers> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {


                   

                    ArchiveSerialNumbers archiveSerialNumberToUpdate = _archiveSerialNumberRepository.FindByIdAsync(request.Id, cancellationToken).GetAwaiter().GetResult();
                    if (archiveSerialNumberToUpdate == null)
                    {
                        await _logService.AddLogs(LogLevel.ERROR, LogName.EasyDoc, $"archive serial number not found.");
                        throw new NotFoundException($"archive serial number not found.");

                    }
                    _mapper.Map(request.ArchiveSerialNumberDto, archiveSerialNumberToUpdate);

                    await _archiveSerialNumberRepository.UpdateAsync(archiveSerialNumberToUpdate);
                    await _logService.AddLogs(LogLevel.INFO, LogName.EasyDoc, $"new archive serial number Updated {request.ArchiveSerialNumberDto.Prefix}-{request.ArchiveSerialNumberDto.GroupName}-{request.ArchiveSerialNumberDto.Year}");


                    return archiveSerialNumberToUpdate;

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
