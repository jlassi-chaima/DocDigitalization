using Application.Respository;
using Domain.Logs;
using Domain.Templates;
using MediatR;


namespace Application.Features.FeaturesTemplates
{
    public class DeleteTemplate
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
            private readonly ITemplateRepository _repository;
            private readonly ILogRepository _logRepository;
            public Handler(ITemplateRepository repository, ILogRepository logRepository)
            {
                _repository = repository;
                _logRepository = logRepository;
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                Template template_to_delete = await _repository.FindByIdAsync(request.Id, cancellationToken);

                Logs new_note = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Template deleted {template_to_delete.Name}");
                await _logRepository.AddAsync(new_note);

                await _repository.DeleteByIdAsync(request.Id, cancellationToken);
            }
        }
    }
}
