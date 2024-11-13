using Application.Dtos.BulkEdit;
using Application.Respository;
using Application.Services;
using Domain.DocumentManagement.tags;
using Domain.Documents;
using MediatR;
using System.Text.Json;


namespace Application.Features.FeaturesDocument
{
    public class BulkEditDocument
    {
        public sealed record Command : IRequest
        {
            public readonly BulkEdit BulkEditDTO;
            public Command(BulkEdit bulkedit)
            {
                BulkEditDTO = bulkedit;
            }
        }
        public sealed class Handler : IRequestHandler<Command>
        {
            private readonly IDocumentRepository _repository;
            private readonly ITagRepository _tagrepository;
            private readonly ICorrespondentRepository _correspondentRepository;
            private readonly IDocumentTypeRepository _typeRepository;
            private readonly IStoragePathRepository _storageRepository;
            private readonly ArchiveStoragePath archiveStorage;
            public Handler(IDocumentRepository repository, ITagRepository tagrepository, ICorrespondentRepository correspondentRepository, IStoragePathRepository storageRepository, IDocumentTypeRepository typeRepository, ArchiveStoragePath archiveStorage)
            {
                _repository = repository;
                _tagrepository = tagrepository;
                _correspondentRepository = correspondentRepository;
                _storageRepository = storageRepository;
                _typeRepository = typeRepository;
                this.archiveStorage = archiveStorage;
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var ids = request.BulkEditDTO.Documents;
                foreach (var id in ids)
                {
                    var document = _repository.FindByIdAsync(id).Result;
                    if(document != null)
                    {
                        switch (request.BulkEditDTO.Method.ToLower())
                        {
                            case "modify_tags":                               
                                    await ModifyTagsAsync(document, request.BulkEditDTO.Parameters);
                                break;
                            case "set_correspondent":
                                await SetCorrespondentAsync(document, request.BulkEditDTO.Parameters);
                                break;
                            case "set_document_type":
                                await SetDocumentTypeAsync(document, request.BulkEditDTO.Parameters);
                                break;
                            case "set_storage_path":
                                await SetStoragePathAsync(document, request.BulkEditDTO.Parameters);
                                break;
                            case "delete":
                                await DeleteAsync(document);
                                break;
                            case "set_permissions":
                                await SetPermissionsAsync(document, request.BulkEditDTO.Parameters);
                                break;

                        }
                        
                    }
                   
                }
            }
            private async Task ModifyTagsAsync(Document document, dynamic parameters)
            {
                if (parameters is JsonElement jsonElement)
                {
                    // Check if add_tags property exists and handle as array of Guids
                    if (jsonElement.TryGetProperty("add_tags", out var addTagsElement) && addTagsElement.ValueKind == JsonValueKind.Array)
                    {
                        var addTags = addTagsElement.EnumerateArray().Select(e => e.GetGuid()).ToList();
                        var tagsToAdd = await _tagrepository.FindByListIdsAsync(addTags);

                        foreach (var tag in tagsToAdd)
                        {
                            if (document.Tags == null)
                            {
                                document.Tags = new List<DocumentTags>();
                            }

                            if (!document.Tags.Any(t => t.TagId == tag.Id))
                            {
                                document.Tags.Add(new DocumentTags
                                {
                                    Document = document,
                                    DocumentId = document.Id,
                                    Tag = tag,
                                    TagId = tag.Id
                                });
                            }
                        }
                        await _repository.UpdateAsync(document);
                    }

                    // Check if remove_tags property exists and handle as array of Guids
                    if (jsonElement.TryGetProperty("remove_tags", out var removeTagsElement) && removeTagsElement.ValueKind == JsonValueKind.Array)
                    {
                        var removeTags = removeTagsElement.EnumerateArray().Select(e => e.GetGuid()).ToList();
                        document.Tags = document.Tags?.Where(t => !removeTags.Contains(t.TagId)).ToList();
                        await _repository.UpdateAsync(document);
                    }
                }
            }

            private async Task SetCorrespondentAsync(Document document, dynamic parameters)
            {
                if (parameters is JsonElement jsonElement && jsonElement.TryGetProperty("correspondent", out var correspondentElement))
                {
                    var correspondentId = correspondentElement.GetGuid();
                    var correspondentToAdd = await _correspondentRepository.FindByIdAsync(correspondentId);

                    if (correspondentToAdd != null)
                    {
                        document.CorrespondentId = correspondentToAdd.Id;
                    }
                    await _repository.UpdateAsync(document);
                }
            }
            private async Task SetDocumentTypeAsync(Document document, dynamic parameters)
            {
                if (parameters is JsonElement jsonElement && jsonElement.TryGetProperty("document_type", out var document_typeElement))
                {
                    var document_typeId = document_typeElement.GetGuid();
                    var document_typeToAdd = await _typeRepository.FindByIdAsync(document_typeId);

                    if (document_typeToAdd != null)
                    {
                        document.DocumentTypeId = document_typeToAdd.Id;
                    }
                    await _repository.UpdateAsync(document);
                }
            }
            private async Task SetStoragePathAsync(Document document, dynamic parameters)
            {
                if (parameters is JsonElement jsonElement && jsonElement.TryGetProperty("storage_path", out var storage_pathElement))
                {
                    var storagePathId = storage_pathElement.GetGuid();
                    var StoragePathToAdd = await _storageRepository.FindByIdAsync(storagePathId);

                    if (StoragePathToAdd != null)
                    {
                        document.StoragePathId = StoragePathToAdd.Id;
                     //   ArchiveStoragePath.
                    }
                    await _repository.UpdateAsync(document);
                }
            }
            private async Task DeleteAsync (Document document)
            {
                await _repository.DeleteAsync(document);
            }
            private async Task SetPermissionsAsync(Document document, dynamic parameters)
            {
                if (parameters is JsonElement jsonElement && jsonElement.TryGetProperty("set_permissions", out var setPermissionsElement))
                {
                    if (setPermissionsElement.TryGetProperty("view", out var viewPermissions))
                    {
                        if (viewPermissions.TryGetProperty("users", out var viewUsersElement) && viewUsersElement.ValueKind == JsonValueKind.Array)
                        {
                            var viewUsers = viewUsersElement.EnumerateArray().Select(e => e.GetString()).ToList();
                            if (document.UsersView == null)
                            {
                                document.UsersView = new List<string>();
                            }
                            foreach(var user in viewUsers)
                            {
                                document.UsersView.Add(user);
                            }
                           
                        }

                        if (viewPermissions.TryGetProperty("groups", out var viewGroupsElement) && viewGroupsElement.ValueKind == JsonValueKind.Array)
                        {
                            var viewGroups = viewGroupsElement.EnumerateArray().Select(e => e.GetString()).ToList();
                            if (document.GroupsView == null)
                            {
                                document.GroupsView = new List<string>();
                            }
                            foreach (var groups in viewGroups)
                            {
                                document.GroupsView.Add(groups);
                            }
                          
                        }
                    }

                    if (setPermissionsElement.TryGetProperty("change", out var changePermissions))
                    {
                        if (changePermissions.TryGetProperty("users", out var changeUsersElement) && changeUsersElement.ValueKind == JsonValueKind.Array)
                        {
                            var changeUsers = changeUsersElement.EnumerateArray().Select(e => e.GetString()).ToList();
                            if (document.UsersChange == null)
                            {
                                document.UsersChange = new List<string>();
                            }
                            foreach (var user in changeUsers)
                            {
                                document.UsersChange.Add(user);
                            }

                        }

                        if (changePermissions.TryGetProperty("groups", out var changeGroupsElement) && changeGroupsElement.ValueKind == JsonValueKind.Array)
                        {
                            var changeGroups = changeGroupsElement.EnumerateArray().Select(e => e.GetString()).ToList();
                            if (document.GroupsChange == null)
                            {
                                document.GroupsChange = new List<string>();
                            }
                            foreach (var user in changeGroups)
                            {
                                document.GroupsChange.Add(user);
                            }
                           
                        }
                    }
                    await _repository.UpdateAsync(document);
                }
            }


        }

    }
}
