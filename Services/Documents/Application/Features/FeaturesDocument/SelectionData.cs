using Application.Dtos.SelectionData;
using Application.Parameters;
using Application.Respository;
using MediatR;


namespace Application.Features.FeaturesDocument
{
    public class SelectionData
    {
        public sealed record Query : IRequest<SelectionDataDTO>
        {
            public readonly SelectionDataDocuments? Ids;
            public readonly string? Owner;
            public Query(SelectionDataDocuments? ids,string owner)
            {
                Ids = ids;
                Owner = owner;
               
            }
        }
        public sealed class Handler : IRequestHandler<Query, SelectionDataDTO>
        {
            private readonly ITagRepository _tagRepository;
            private readonly IStoragePathRepository _storagePathRepository;
            private readonly ICorrespondentRepository _correspondentRepository;
            private readonly IDocumentTypeRepository _documentTypeRepository;

            public Handler(ITagRepository tagRepository, IStoragePathRepository storagePathRepository, ICorrespondentRepository correspondentRepository, IDocumentTypeRepository documentTypeRepository)
            {
                _tagRepository = tagRepository;
                _storagePathRepository = storagePathRepository;
                _correspondentRepository = correspondentRepository;
                _documentTypeRepository = documentTypeRepository;
            }

            public async Task<SelectionDataDTO> Handle(Query request, CancellationToken cancellationToken)
            {
                var correspondents = await _correspondentRepository.SelectionDataCorrespondent(request.Ids, request.Owner);
                var tags = await _tagRepository.SelectionDataTag(request.Ids, request.Owner);
                var documentTypes = await _documentTypeRepository.SelectionDataDocumentTypes(request.Ids);
                var storagePaths = await _storagePathRepository.SelectionDataStoragePath(request.Ids);

                var result = new SelectionDataDTO
                {
                    Selected_correspondents = correspondents,
                    Selected_tags = tags,
                    Selected_Document_types = documentTypes,
                    Selected_storage_paths = storagePaths
                };

                return result;
            }
        }
    }
}
