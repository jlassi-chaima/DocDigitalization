

using Application.Dtos.ArchivenSerialNumber;
using Application.Dtos.View;
using Application.Exceptions;
using Application.Respository;
using Application.Services;
using Domain.DocumentManagement.Views;
using Domain.Documents;
using Domain.Logs;
using FluentValidation;
using MediatR;
using Serilog;

namespace Application.Features.FeaturesViews
{
    public class AddView
    {
            public sealed record Command : IRequest<View>
            {
                public ViewDto ViewDto;

                public Command(ViewDto viewDto)
                {
                ViewDto = viewDto;
                }
            }
            public sealed class Handler : IRequestHandler<Command, View>
            {
                private readonly IViewRepository _viewRepository;

                private readonly ILogService _logService;
                public Handler(IViewRepository viewRepository
                    , ILogService logService)
                {
                _viewRepository = viewRepository;
                    _logService = logService;
                }
               
                public sealed class ViewValidator : AbstractValidator<ViewDto>
                {
                    public ViewValidator()
                    {

                        RuleFor(p => p.Name).NotEmpty().WithMessage("Prefix is required");
                        RuleFor(p => p.StartDate).NotEmpty().WithMessage("StartDate is required");
                        RuleFor(p => p.EndDate).NotEmpty().WithMessage("EndDate is required");
                        RuleFor(p => p.Owner).NotEmpty().WithMessage("Ouner is required");

                    }
                }
                public async Task<View> Handle(Command request, CancellationToken cancellationToken)
                {
                    try
                    {

                        View exist = await _viewRepository.FindByNameAsync(request.ViewDto.Name,request.ViewDto.Owner)!;
                        if (exist != null)
                        {
                            Log.Information($"this view : {exist.Name} already exist");
                            await _logService.AddLogs(LogLevel.ERROR, LogName.EasyDoc, $"this view: {exist.Name} already exist");

                            throw new ViewException($"this view: {exist.Name} already exist");
                        }

                        View view = View.Create(request.ViewDto.Name,
                            request.ViewDto.TagId, request.ViewDto.Owner);
                        await _viewRepository.AddAsync(view);
                        await _logService.AddLogs(LogLevel.INFO, LogName.EasyDoc, $"new view added {request.ViewDto.Name}");
                        return view;
                    }
                    catch (ViewException ex)
                    {
                        Log.Error(ex.Message, ex);
                        throw new ViewException(ex.Message);
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

