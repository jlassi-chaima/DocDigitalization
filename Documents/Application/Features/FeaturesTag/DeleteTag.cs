using Application.Respository;
using Domain.DocumentManagement.tags;
using Domain.Logs;
using MediatR;


namespace Application.Features.FeaturesTag
{
    public class DeleteTag
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
            private readonly ITagRepository _repository;
            private readonly ILogRepository _logRepository;
            public Handler(ITagRepository repository, ILogRepository logRepository)
            {
                _repository = repository;
                _logRepository = logRepository;
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                Tag tag_to_delete = await _repository.FindByIdAsync(request.Id, cancellationToken);
                Logs new_tag = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Tag deleted {tag_to_delete.Name}");
                await _logRepository.AddAsync(new_tag);
                await _repository.DeleteByIdAsync(request.Id, cancellationToken);
                
            }
        }
    }
}
