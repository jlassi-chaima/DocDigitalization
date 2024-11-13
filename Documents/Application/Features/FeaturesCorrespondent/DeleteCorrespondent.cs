using Application.Respository;
using Domain.DocumentManagement.Correspondent;
using Domain.Logs;
using MediatR;


namespace Application.Features.FeaturesCorrespondent
{
    public class DeleteCorrespondent
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
            private readonly ICorrespondentRepository _repository;
            private readonly ILogRepository _logRepository;

            public Handler(ICorrespondentRepository repository, ILogRepository logRepository)
            {
                _repository = repository;
                _logRepository = logRepository;
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                Correspondent correspondent_to_delete = await _repository.FindByIdAsync(request.Id, cancellationToken);

                Logs new_correspondent = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Correspondent deleted {correspondent_to_delete.Name}");
                await _logRepository.AddAsync(new_correspondent);
                await _repository.DeleteByIdAsync(request.Id, cancellationToken);
            }
        }
    }
}
