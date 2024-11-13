using Application.Respository;
using Application.Services;
using Domain.DocumentManagement.StoragePath;
using Domain.Documents;
using Domain.Logs;
using MediatR;


namespace Application.Features.FeaturesStoragePath
{
    public class DeleteStoragePath
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
            private readonly IStoragePathRepository _repository;
            private readonly IDocumentRepository _documentrepository;
            private readonly ArchiveStoragePath _archiveStoragePath;
            private readonly ILogRepository _logRepository;
            public Handler(IStoragePathRepository repository, IDocumentRepository documentrepository, ArchiveStoragePath archiveStoragePath, ILogRepository logRepository)
            {
                _repository = repository;
                _documentrepository = documentrepository;
                _archiveStoragePath = archiveStoragePath;
                _logRepository = logRepository;
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                StoragePath storagepathtodelete = _repository.FindByIdAsync(request.Id, cancellationToken).GetAwaiter().GetResult();

                Logs new_storagepath = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"StoragePath deleted {storagepathtodelete.Name}");
                await _logRepository.AddAsync(new_storagepath);

                List<Document> all_documents = (List<Document>)await _documentrepository.GetAllAsync();
                foreach (Document document in all_documents)
                {
                    if (document.StoragePathId == storagepathtodelete.Id)
                    {
                        _archiveStoragePath.deleteArchiveStoragePath(storagepathtodelete, document);
                    }
                }


                await _repository.DeleteByIdAsync(request.Id, cancellationToken);
            }
        }
    }
}