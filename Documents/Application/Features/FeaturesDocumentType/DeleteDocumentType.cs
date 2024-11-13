using Application.Respository;
using Domain.DocumentManagement.DocumentTypes;
using Domain.Logs;
using MediatR;

namespace Application.Features.FeaturesDocumentType
{
    public class DeleteDocumentType
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
            private readonly IDocumentTypeRepository _repository;
            private readonly ILogRepository _logRepository;
            public Handler(IDocumentTypeRepository repository, ILogRepository logRepository)
            {
                _repository = repository;
                _logRepository = logRepository;
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                DocumentType document_type_to_delete = await _repository.FindByIdAsync(request.Id, cancellationToken);

                Logs new_document_type = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Document Type deleted {document_type_to_delete.Name}");
                await _logRepository.AddAsync(new_document_type);
                await _repository.DeleteByIdAsync(request.Id, cancellationToken);
            }
        }
    }
}
