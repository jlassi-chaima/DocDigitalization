using Application.Dtos.StoragePath;
using Application.Exceptions;
using Application.Respository;
using Domain.DocumentManagement;
using Domain.DocumentManagement.StoragePath;
using Domain.Documents;
using FluentValidation;
using MapsterMapper;
using MediatR;
using System.Text.RegularExpressions;
using RapidFuzz.Net;
using Application.Services;
using Domain.Logs;


namespace Application.Features.FeaturesStoragePath
{
    public class UpdateStoragePath
    {
        public sealed record Command : IRequest<StoragePath>
        {
            public readonly Guid storagepathId;
            public readonly UpdateStoragePathDto storagepathDto;
            public Command(UpdateStoragePathDto storagepathDtodto, Guid Id)
            {
                storagepathDto = storagepathDtodto;
                storagepathId = Id;
            }

            public sealed class Validator : AbstractValidator<Command>
            {
                public Validator(IStoragePathRepository _repository)
                {
                    RuleFor(p => p.storagepathId).NotEmpty();
                }
            }

            public sealed class Handler : IRequestHandler<Command, StoragePath>
            {
                private readonly IStoragePathRepository _repository;
                private readonly IMapper _mapper;
                private readonly IDocumentRepository _documentrepository;
                private readonly ArchiveStoragePath _archiveStoragePath;
                private readonly ILogRepository _logRepository;
                public Handler(IStoragePathRepository repository, IMapper mapper, IDocumentRepository documentrepository, ArchiveStoragePath archiveStoragePath, ILogRepository logRepository)
                {
                    _repository = repository;
                    _mapper = mapper;
                    _documentrepository = documentrepository;
                    _archiveStoragePath = archiveStoragePath;
                    _logRepository = logRepository;
                }
                public async Task<StoragePath> Handle(Command request, CancellationToken cancellationToken)
                {
                    Logs new_storagepath = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"StoragePath updated {request.storagepathDto.Name}");
                    await _logRepository.AddAsync(new_storagepath);


                    StoragePath old_storagepath = new StoragePath();

                    StoragePath storagepathtoupdate = _repository.FindByIdAsync(request.storagepathId, cancellationToken).GetAwaiter().GetResult();

                    old_storagepath.Matching_algorithm = storagepathtoupdate.Matching_algorithm;
                    old_storagepath.Path = storagepathtoupdate.Path;

                    if (storagepathtoupdate == null)
                    {
                        throw new StoragePathException($"StoragePath with ID {request.storagepathId} not found.");
                    }
                    _mapper.Map(request.storagepathDto, storagepathtoupdate);
                    storagepathtoupdate.Slug = request.storagepathDto.Name;

                    if (storagepathtoupdate.UsersView != null || storagepathtoupdate.GroupsView != null ||
            storagepathtoupdate.UsersChange != null || storagepathtoupdate.GroupsChange != null)
                    {
                        storagepathtoupdate.UsersView?.Clear();
                        storagepathtoupdate.GroupsView?.Clear();
                        storagepathtoupdate.UsersChange?.Clear();
                        storagepathtoupdate.GroupsChange?.Clear();
                    }
                    if (request.storagepathDto.Set_permissions?.View?.Users != null)
                    {
                        if (storagepathtoupdate.UsersView == null)
                        {
                            storagepathtoupdate.UsersView = new List<string>();
                        }
                        foreach (var user in request.storagepathDto.Set_permissions.View.Users)
                        {
                            if (!storagepathtoupdate.UsersView.Contains(user))
                            {
                                storagepathtoupdate.UsersView.Add(user);
                            }

                        }
                    }
                    if (request.storagepathDto.Set_permissions?.View?.Groups != null)
                    {
                        if (storagepathtoupdate.GroupsView == null)
                        {
                            storagepathtoupdate.GroupsView = new List<string>();
                        }
                        foreach (var group in request.storagepathDto.Set_permissions.View.Groups)
                        {
                            if (!storagepathtoupdate.GroupsView.Contains(group))
                            {
                                storagepathtoupdate.GroupsView.Add(group);
                            }

                        }
                    }
                    if (request.storagepathDto.Set_permissions?.Change?.Users != null)
                    {
                        if (storagepathtoupdate.UsersChange == null)
                        {
                            storagepathtoupdate.UsersChange = new List<string>();
                        }
                        foreach (var user in request.storagepathDto.Set_permissions.Change.Users)
                        {
                            if (!storagepathtoupdate.UsersChange.Contains(user))
                            {
                                storagepathtoupdate.UsersChange.Add(user);
                            }

                        }
                    }
                    if (request.storagepathDto.Set_permissions?.Change?.Groups != null)
                    {
                        if (storagepathtoupdate.GroupsChange == null)
                        {
                            storagepathtoupdate.GroupsChange = new List<string>();
                        }

                        foreach (var group in request.storagepathDto.Set_permissions.Change.Groups)
                        {
                            if (!storagepathtoupdate.GroupsChange.Contains(group))
                            {
                                storagepathtoupdate.GroupsChange.Add(group);
                            }
                        }
                    }
                    await _repository.UpdateAsync(storagepathtoupdate);

                    List<Document> all_documents = (List<Document>)await _documentrepository.GetAllAsync();

                    if (storagepathtoupdate.Matching_algorithm == Matching_Algorithms.MATCH_NONE)
                    {


                        foreach (Document document in all_documents)
                        {
                            if (document.StoragePathId == storagepathtoupdate.Id)
                            {
                                document.StoragePathId = null;
                                document.StoragePath = null;
                                await _documentrepository.UpdateAsync(document);
                                _archiveStoragePath.deleteArchiveStoragePath(old_storagepath, document);

                            }

                        }
                    }

                    else if (storagepathtoupdate.Matching_algorithm == Matching_Algorithms.MATCH_ANY)
                    {

                        //in case it's not none because none has no effect on the document but other algos do (there is no storagepathids to delete)
                        if (old_storagepath.Matching_algorithm != Matching_Algorithms.MATCH_NONE)
                        {
                            foreach (Document document in all_documents)
                            {


                                if (document.StoragePathId == storagepathtoupdate.Id)
                                {
                                    document.StoragePathId = null;
                                    document.StoragePath = null;
                                    await _documentrepository.UpdateAsync(document);
                                    _archiveStoragePath.deleteArchiveStoragePath(old_storagepath, document);
                                }


                            }
                        }
                        if (storagepathtoupdate.Match != null && storagepathtoupdate.Match.Count > 0)
                        {
                            foreach (Document document in all_documents)
                            {
                                try
                                {
                                    if (document.StoragePathId == null)
                                    {
                                        if (storagepathtoupdate.Is_insensitive == true)
                                        {
                                            var lowerCaseMatch = storagepathtoupdate.Match.Select(x => x.ToLower()).ToList();
                                            bool allConditionsMet = lowerCaseMatch.All(x => document.Content.ToLower().Contains(x));

                                            Console.WriteLine(allConditionsMet);

                                            if (allConditionsMet)
                                            {

                                                document.StoragePathId = storagepathtoupdate.Id;
                                                await _documentrepository.UpdateAsync(document);
                                                _archiveStoragePath.updateArchiveStoragePath(storagepathtoupdate, document);

                                            }
                                        }
                                        else
                                        {
                                            bool allConditionsMet = storagepathtoupdate.Match.Any(x => document.Content?.ToLower().Contains(x) ?? false);

                                            Console.WriteLine(allConditionsMet);

                                            if (allConditionsMet)
                                            {

                                                document.StoragePathId = storagepathtoupdate.Id;
                                                await _documentrepository.UpdateAsync(document);
                                                _archiveStoragePath.updateArchiveStoragePath(storagepathtoupdate, document);

                                            }

                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Log the exception details
                                    Console.WriteLine($"Exception: {ex.Message}");
                                }

                            }
                        }
                    }

                    else if (storagepathtoupdate.Matching_algorithm == Matching_Algorithms.MATCH_ALL)
                    {

                        //in case it's not none because none has no effect on the document but other algos do (there is no storagepathids to delete)
                        if (old_storagepath.Matching_algorithm != Matching_Algorithms.MATCH_NONE)
                        {
                            foreach (Document document in all_documents)
                            {


                                if (document.StoragePathId == storagepathtoupdate.Id)
                                {
                                    document.StoragePathId = null;
                                    document.StoragePath = null;
                                    await _documentrepository.UpdateAsync(document);
                                    _archiveStoragePath.deleteArchiveStoragePath(old_storagepath, document);
                                }


                            }
                        }
                        if (storagepathtoupdate.Match != null && storagepathtoupdate.Match.Count > 0)
                        {
                            foreach (Document document in all_documents)
                            {
                                try
                                {
                                    if (document.StoragePathId == null)
                                    {
                                        if (storagepathtoupdate.Is_insensitive == true)
                                        {
                                            var lowerCaseMatch = storagepathtoupdate.Match.Select(x => x.ToLower()).ToList();
                                            bool allConditionsMet = lowerCaseMatch.All(x => document.Content.ToLower().Contains(x));
                                            Console.WriteLine(allConditionsMet);

                                            if (allConditionsMet)
                                            {
                                                document.StoragePathId = storagepathtoupdate.Id;
                                                await _documentrepository.UpdateAsync(document);
                                                _archiveStoragePath.updateArchiveStoragePath(storagepathtoupdate, document);
                                            }
                                        }
                                        else
                                        {

                                            bool allConditionsMet = storagepathtoupdate.Match.All(x => document.Content?.ToLower().Contains(x) ?? false);

                                            Console.WriteLine(allConditionsMet);

                                            if (allConditionsMet)
                                            {
                                                document.StoragePathId = storagepathtoupdate.Id;
                                                await _documentrepository.UpdateAsync(document);
                                                _archiveStoragePath.updateArchiveStoragePath(storagepathtoupdate, document);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Log the exception details
                                    Console.WriteLine($"Exception: {ex.Message}");
                                }

                            }
                        }
                    }

                    else if (storagepathtoupdate.Matching_algorithm == Matching_Algorithms.MATCH_LITERAL)
                    {

                        //in case it's not none because none has no effect on the document but other algos do (there is no storagepathids to delete)
                        if (old_storagepath.Matching_algorithm != Matching_Algorithms.MATCH_NONE)
                        {
                            foreach (Document document in all_documents)
                            {


                                if (document.StoragePathId == storagepathtoupdate.Id)
                                {
                                    document.StoragePathId = null;
                                    document.StoragePath = null;
                                    await _documentrepository.UpdateAsync(document);
                                    _archiveStoragePath.deleteArchiveStoragePath(old_storagepath, document);
                                }


                            }
                        }
                        if (storagepathtoupdate.Match != null && storagepathtoupdate.Match.Count > 0)
                        {
                            foreach (Document document in all_documents)
                            {
                                try
                                {
                                    if (document.StoragePathId == null)
                                    {
                                        if (storagepathtoupdate.Is_insensitive == true)
                                        {
                                            if (storagepathtoupdate.Match.Count() == 1 && document.Content.ToLower().Contains(storagepathtoupdate.Match[0].ToLower()))
                                            {
                                                document.StoragePathId = storagepathtoupdate.Id;
                                                await _documentrepository.UpdateAsync(document);
                                                _archiveStoragePath.updateArchiveStoragePath(storagepathtoupdate, document);
                                            }
                                        }
                                        else
                                        {

                                            if (storagepathtoupdate.Match.Count() == 1 && document.Content.ToLower().Contains(storagepathtoupdate.Match[0]))
                                            {
                                                document.StoragePathId = storagepathtoupdate.Id;
                                                await _documentrepository.UpdateAsync(document);
                                                _archiveStoragePath.updateArchiveStoragePath(storagepathtoupdate, document);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Log the exception details
                                    Console.WriteLine($"Exception: {ex.Message}");
                                }

                            }
                        }
                    }

                    else if (storagepathtoupdate.Matching_algorithm == Matching_Algorithms.MATCH_FUZZY)
                    {

                        //in case it's not none because none has no effect on the document but other algos do (there is no storagepathids to delete)
                        if (old_storagepath.Matching_algorithm != Matching_Algorithms.MATCH_NONE)
                        {
                            foreach (Document document in all_documents)
                            {


                                if (document.StoragePathId == storagepathtoupdate.Id)
                                {
                                    document.StoragePathId = null;
                                    document.StoragePath = null;
                                    await _documentrepository.UpdateAsync(document);
                                    _archiveStoragePath.deleteArchiveStoragePath(old_storagepath, document);
                                }


                            }
                        }

                        int threshold = 90;
                        if (storagepathtoupdate.Match != null && storagepathtoupdate.Match.Count > 0)
                        {
                            foreach (Document document in all_documents)
                            {
                                try
                                {
                                    if (document.StoragePathId == null)
                                    {
                                        if (storagepathtoupdate.Is_insensitive == true)
                                        {
                                            foreach (string matchWord in storagepathtoupdate.Match.Select(x => x.ToLower()).ToList())
                                            {
                                                Console.WriteLine(FuzzyMatcher.PartialRatio(matchWord, document.Content.ToLower()));
                                                if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= threshold)
                                                {
                                                    document.StoragePathId = storagepathtoupdate.Id;
                                                    await _documentrepository.UpdateAsync(document);
                                                    _archiveStoragePath.updateArchiveStoragePath(storagepathtoupdate, document);
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (string matchWord in storagepathtoupdate.Match)
                                            {
                                                Console.WriteLine(FuzzyMatcher.PartialRatio(matchWord, document.Content.ToLower()));
                                                if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= threshold)
                                                {
                                                    document.StoragePathId = storagepathtoupdate.Id;
                                                    await _documentrepository.UpdateAsync(document);
                                                    _archiveStoragePath.updateArchiveStoragePath(storagepathtoupdate, document);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Log the exception details
                                    Console.WriteLine($"Exception: {ex.Message}");
                                }

                            }
                        }
                    }

                    else if (storagepathtoupdate.Matching_algorithm == Matching_Algorithms.MATCH_REGEX)
                    {

                        //in case it's not none because none has no effect on the document but other algos do (there is no storagepathids to delete)
                        if (old_storagepath.Matching_algorithm != Matching_Algorithms.MATCH_NONE)
                        {
                            foreach (Document document in all_documents)
                            {


                                if (document.StoragePathId == storagepathtoupdate.Id)
                                {
                                    document.StoragePathId = null;
                                    document.StoragePath = null;
                                    await _documentrepository.UpdateAsync(document);
                                    _archiveStoragePath.deleteArchiveStoragePath(old_storagepath, document);
                                }


                            }
                        }
                        if (storagepathtoupdate.Match != null && storagepathtoupdate.Match.Count > 0)
                        {
                            foreach (Document document in all_documents)
                            {
                                try
                                {
                                    if (document.StoragePathId == null)
                                    {
                                        if (storagepathtoupdate.Is_insensitive == true)
                                        {
                                            foreach (string regexPattern in storagepathtoupdate.Match.Select(x => x.ToLower()).ToList())
                                            {
                                                if (Regex.IsMatch(document.Content, regexPattern))
                                                {
                                                    document.StoragePathId = storagepathtoupdate.Id;
                                                    await _documentrepository.UpdateAsync(document);
                                                    _archiveStoragePath.updateArchiveStoragePath(storagepathtoupdate, document);
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (string regexPattern in storagepathtoupdate.Match)
                                            {
                                                if (Regex.IsMatch(document.Content, regexPattern))
                                                {
                                                    document.StoragePathId = storagepathtoupdate.Id;
                                                    await _documentrepository.UpdateAsync(document);
                                                    _archiveStoragePath.updateArchiveStoragePath(storagepathtoupdate, document);
                                                    break;
                                                }
                                            }

                                        }
                                    }
                                }

                                catch (Exception ex)
                                {
                                    // Log the exception details
                                    Console.WriteLine($"Exception: {ex.Message}");
                                }
                            }
                        }
                    }


                    return storagepathtoupdate;


                }
            }
        }
    }
}