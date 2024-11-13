using Application.Dtos.ArchivenSerialNumber;
using Application.Exceptions;
using Application.Respository;
using Application.Services;
using Domain.Documents;
using Domain.Logs;
using FluentValidation;
using MediatR;
using Serilog;


namespace Application.Features.ArchiveSerialNumbersFeature
{
    public class AddArchiveSerialNumber
    {
        public sealed record Command : IRequest<ArchiveSerialNumbers>
        {
            public ArchiveSerialNumberDto ArchiveSerialNumberDto;

            public Command(ArchiveSerialNumberDto archiveSerialNumberDto)
            {
                ArchiveSerialNumberDto = archiveSerialNumberDto;
            }
        }
        public sealed class Handler : IRequestHandler<Command, ArchiveSerialNumbers>
        {
            private readonly IArchiveSerialNumberRepository _archiveSerialNumber;

            private readonly ILogService _logService;
            public Handler(IArchiveSerialNumberRepository archiveSerialNumber
                , ILogService logService)
            {
                _archiveSerialNumber = archiveSerialNumber;
                _logService = logService;
            }
            //public sealed class Validator : AbstractValidator<Command>
            //{
            //    public Validator(IArchiveSerialNumberRepository _archiveSerialNumber)
            //    {
            //        RuleFor(p => p.ArchiveSerialNumberDto.Prefix).NotEmpty().WithMessage("Prefix is required");
            //        RuleFor(p => p.ArchiveSerialNumberDto.GroupName).NotEmpty().WithMessage("Group is required");

            //    }
            //}
            public sealed class ArchiveValidator : AbstractValidator<ArchiveSerialNumberDto>
            {
                public ArchiveValidator()
                {

                    RuleFor(p => p.Prefix).NotEmpty().WithMessage("Prefix is required");
                    RuleFor(p => p.GroupName).NotEmpty().WithMessage("GroupName is required");
                    RuleFor(p => p.GroupId).NotEmpty().WithMessage("GroupId is required");
                    RuleFor(p => p.Owner).NotEmpty().WithMessage("Ouner is required");

                }
            }
            public async Task<ArchiveSerialNumbers> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {

                    ArchiveSerialNumbers exist = await _archiveSerialNumber.GetArchiveNumberByGroupId(request.ArchiveSerialNumberDto.GroupId);
                    if (exist != null)
                    {
                        Log.Information($"this group : {exist.GroupName} already have archive serial number");
                        await _logService.AddLogs(LogLevel.ERROR, LogName.EasyDoc, $"this group : {exist.GroupName} already have archive serial number");

                        throw new ArchiveException($"this group : {exist.GroupName}  already have archive serial number");
                    }

                    ArchiveSerialNumbers archiveSerialNumbers = ArchiveSerialNumbers.Create(request.ArchiveSerialNumberDto.Prefix,
                        request.ArchiveSerialNumberDto.GroupName, request.ArchiveSerialNumberDto.GroupId, request.ArchiveSerialNumberDto.Year, request.ArchiveSerialNumberDto?.Owner);
                    await _archiveSerialNumber.AddAsync(archiveSerialNumbers);
                    await _logService.AddLogs(LogLevel.INFO, LogName.EasyDoc, $"new archive serial number added {request.ArchiveSerialNumberDto.Prefix}-{request.ArchiveSerialNumberDto.GroupName}-{request.ArchiveSerialNumberDto.Year}");
                    return archiveSerialNumbers;
                }
                catch (ArchiveException ex)
                {
                    Log.Error(ex.Message, ex);
                    throw new ArchiveException(ex.Message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                    throw new Exception(ex.Message);
                }
            }

        }
    }


}
