using Application.Dtos.StoragePath;
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
    public class AddStoragePath
    {
        public sealed record Command : IRequest<StoragePath>
        {
            public readonly UpdateStoragePathDto storagepath;

            public Command(UpdateStoragePathDto storagepathdto)
            {
                storagepath = storagepathdto;
            }
        }
        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator(IStoragePathRepository _repository)
            {
                RuleFor(p => p.storagepath.Name).NotEmpty().WithMessage("Title can't be empty.");
                RuleFor(p => p.storagepath.Matching_algorithm).Must(i => Enum.IsDefined(typeof(Matching_Algorithms), i))
                                              .WithMessage("You must choose a matching algorithm. Invalid matching algorithm.");
            }
            public sealed class Handler : IRequestHandler<Command, StoragePath>
            {

                private readonly IDocumentRepository _documentrepository;
                private readonly IStoragePathRepository _repository;
                private readonly IMapper _mapper;
                private readonly ArchiveStoragePath _archiveStoragePath;
                private readonly ILogRepository _logRepository;
                public Handler(IStoragePathRepository repository, IMapper mapper, IDocumentRepository documentrepository, ArchiveStoragePath archiveStoragePath, ILogRepository logRepository)
                {
                    _documentrepository = documentrepository;
                    _repository = repository;
                    _mapper = mapper;
                    _archiveStoragePath = archiveStoragePath;
                    _logRepository = logRepository;
                }
                public async Task<StoragePath> Handle(Command request, CancellationToken cancellationToken)
                {
                    Logs new_storagepath = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"new StoragePath added {request.storagepath.Name}");
                    await _logRepository.AddAsync(new_storagepath);

                    List<Document> documents = new List<Document>();
                    StoragePath storage = null;

                    if (request.storagepath.Matching_algorithm == Matching_Algorithms.MATCH_NONE || request.storagepath.Match.Count == 0)
                    {
                        var stotagepathToAdd = StoragePath.Create(

                            request.storagepath.Name,
                            request.storagepath.Match = null,
                            request.storagepath.Owner,
                            request.storagepath.Is_insensitive,
                            request.storagepath.Path,
                            request.storagepath.Name,
                            request.storagepath.Document_count,
                            request.storagepath.Matching_algorithm,
                            documents = null,
                            request.storagepath.Set_permissions.View.Users,
                            request.storagepath.Set_permissions.View.Groups,
                            request.storagepath.Set_permissions.Change.Users,
                            request.storagepath.Set_permissions.Change.Groups
                        );
                        stotagepathToAdd.Slug = request.storagepath.Name;
                        storage = stotagepathToAdd;
                        await _repository.AddAsync(stotagepathToAdd);

                    }

                    else if (request.storagepath.Matching_algorithm == Matching_Algorithms.MATCH_ANY)
                    {


                        // Bring all the documents
                        List<Document> all_documents = await _documentrepository.GetAllDocumentByOwner(request.storagepath.Owner);

                        // Create the StoragePath and StoragePathDocument objects
                        var stotagepathToAdd = StoragePath.Create(
                        request.storagepath.Name,
                            request.storagepath.Match,
                            request.storagepath.Owner,
                            request.storagepath.Is_insensitive,
                            request.storagepath.Path,
                            request.storagepath.Name,
                            request.storagepath.Document_count,
                            request.storagepath.Matching_algorithm,
                            documents,
                            request.storagepath.Set_permissions.View.Users,
                            request.storagepath.Set_permissions.View.Groups,
                            request.storagepath.Set_permissions.Change.Users,
                            request.storagepath.Set_permissions.Change.Groups
                        );
                        stotagepathToAdd.Slug = request.storagepath.Name;



                        foreach (Document document in all_documents)
                        {
                            try
                            {
                                if (document.StoragePathId == null)
                                {
                                    if (request.storagepath.Is_insensitive == true)
                                    {
                                        var lowerCaseMatch = request.storagepath.Match.Select(x => x.ToLower()).ToList();
                                        bool allConditionsMet = lowerCaseMatch.Any(x => document.Content.ToLower().Contains(x));
                                        Console.WriteLine(allConditionsMet);
                                        if (allConditionsMet)
                                        {

                                            documents.Add(document);
                                            _archiveStoragePath.addArchiveStoragePath(request.storagepath, document);

                                        }
                                    }
                                    else
                                    {
                                        bool allConditionsMet = request.storagepath.Match.Any(x => document.Content.ToLower().Contains(x));
                                        Console.WriteLine(allConditionsMet);
                                        if (allConditionsMet)
                                        {
                                            if (document.StoragePathId == null)
                                            {
                                                documents.Add(document);

                                                _archiveStoragePath.addArchiveStoragePath(request.storagepath, document);
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
                        // Add the StoragePathDocument entries to the repository
                        if (documents.Any())
                        {
                            stotagepathToAdd.Documents = documents;
                            await _repository.AddAsync(stotagepathToAdd);
                        }
                        else
                        {
                            stotagepathToAdd.Documents = null;
                            await _repository.AddAsync(stotagepathToAdd);
                        }
                        storage = stotagepathToAdd;

                    }
                    else if (request.storagepath.Matching_algorithm == Matching_Algorithms.MATCH_AUTO)
                    {
                        //to check later
                    }
                    else if (request.storagepath.Matching_algorithm == Matching_Algorithms.MATCH_ALL)
                    {


                        // Bring all the documents
                        List<Document> all_documents = await _documentrepository.GetAllDocumentByOwner(request.storagepath.Owner);

                        // Create the StoragePath and StoragePathDocument objects
                        var stotagepathToAdd = StoragePath.Create(
                        request.storagepath.Name,
                            request.storagepath.Match,
                            request.storagepath.Owner,
                            request.storagepath.Is_insensitive,
                            request.storagepath.Path,
                            request.storagepath.Name,
                            request.storagepath.Document_count,
                            request.storagepath.Matching_algorithm,
                            documents,
                            request.storagepath.Set_permissions.View.Users,
                            request.storagepath.Set_permissions.View.Groups,
                            request.storagepath.Set_permissions.Change.Users,
                            request.storagepath.Set_permissions.Change.Groups
                        );
                        stotagepathToAdd.Slug = request.storagepath.Name;



                        foreach (Document document in all_documents)
                        {
                            try
                            {
                                if (document.StoragePathId == null)
                                {
                                    if (request.storagepath.Is_insensitive == true)
                                    {
                                        var lowerCaseMatch = request.storagepath.Match.Select(x => x.ToLower()).ToList();
                                        bool allConditionsMet = lowerCaseMatch.All(x => document.Content.ToLower().Contains(x));
                                        Console.WriteLine(allConditionsMet);
                                        if (allConditionsMet)
                                        {
                                            documents.Add(document);

                                            _archiveStoragePath.addArchiveStoragePath(request.storagepath, document);

                                        }
                                    }
                                    else
                                    {
                                        bool allConditionsMet = request.storagepath.Match.All(x => document.Content.ToLower().Contains(x));
                                        Console.WriteLine(allConditionsMet);
                                        if (allConditionsMet)
                                        {
                                            documents.Add(document);

                                            _archiveStoragePath.addArchiveStoragePath(request.storagepath, document);

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
                        // Add the StoragePathDocument entries to the repository
                        if (documents.Any())
                        {
                            stotagepathToAdd.Documents = documents;
                            await _repository.AddAsync(stotagepathToAdd);
                        }
                        else
                        {
                            stotagepathToAdd.Documents = null;
                            await _repository.AddAsync(stotagepathToAdd);
                        }
                        storage = stotagepathToAdd;


                    }

                    else if (request.storagepath.Matching_algorithm == Matching_Algorithms.MATCH_LITERAL)
                    {


                        // Bring all the documents
                        List<Document> all_documents = await _documentrepository.GetAllDocumentByOwner(request.storagepath.Owner);

                        // Create the StoragePath and StoragePathDocument objects
                        var stotagepathToAdd = StoragePath.Create(
                        request.storagepath.Name,
                            request.storagepath.Match,
                            request.storagepath.Owner,
                            request.storagepath.Is_insensitive,
                            request.storagepath.Path,
                            request.storagepath.Name,
                            request.storagepath.Document_count,
                            request.storagepath.Matching_algorithm,
                            documents,
                            request.storagepath.Set_permissions.View.Users,
                            request.storagepath.Set_permissions.View.Groups,
                            request.storagepath.Set_permissions.Change.Users,
                            request.storagepath.Set_permissions.Change.Groups
                        );
                        stotagepathToAdd.Slug = request.storagepath.Name;



                        foreach (Document document in all_documents)
                        {
                            try
                            {
                                if (document.StoragePathId == null)
                                {

                                    if (request.storagepath.Is_insensitive == true)
                                    {
                                        var lowerCaseMatch = request.storagepath.Match.Select(x => x.ToLower()).ToList();
                                        if (request.storagepath.Match.Count() == 1 && document.Content.ToLower().Contains(request.storagepath.Match[0].ToLower()))
                                        {
                                            documents.Add(document);

                                            _archiveStoragePath.addArchiveStoragePath(request.storagepath, document);
                                        }
                                    }
                                    else
                                    {
                                        if (request.storagepath.Match.Count() == 1 && document.Content.ToLower().Contains(request.storagepath.Match[0]))
                                        {
                                            documents.Add(document);
                                            _archiveStoragePath.addArchiveStoragePath(request.storagepath, document);
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
                        // Add the StoragePathDocument entries to the repository
                        if (documents.Any())
                        {
                            stotagepathToAdd.Documents = documents;
                            await _repository.AddAsync(stotagepathToAdd);
                        }
                        else
                        {
                            stotagepathToAdd.Documents = null;
                            await _repository.AddAsync(stotagepathToAdd);
                        }
                        storage = stotagepathToAdd;


                    }


                    else if (request.storagepath.Matching_algorithm == Matching_Algorithms.MATCH_FUZZY)
                    {

                        // Bring all the documents
                        List<Document> all_documents = await _documentrepository.GetAllDocumentByOwner(request.storagepath.Owner);

                        // Create the StoragePath and StoragePathDocument objects
                        var storagePathToAdd = StoragePath.Create(
                            request.storagepath.Name,
                            request.storagepath.Match,
                            request.storagepath.Owner,
                            request.storagepath.Is_insensitive,
                            request.storagepath.Path,
                            request.storagepath.Name,
                            request.storagepath.Document_count,
                            request.storagepath.Matching_algorithm,
                            documents,
                            request.storagepath.Set_permissions.View.Users,
                            request.storagepath.Set_permissions.View.Groups,
                            request.storagepath.Set_permissions.Change.Users,
                            request.storagepath.Set_permissions.Change.Groups
                        );
                        storagePathToAdd.Slug = request.storagepath.Name;

                        List<Document> matchedDocuments = new List<Document>();
                        int threshold = 90;

                        foreach (Document document in all_documents)
                        {
                            try
                            {
                                if (document.StoragePathId == null)
                                {
                                    if (request.storagepath.Is_insensitive == true)
                                    {
                                        foreach (string matchWord in request.storagepath.Match.Select(x => x.ToLower()).ToList())
                                        {
                                            Console.WriteLine(document.Id + " : " + FuzzyMatcher.PartialRatio(matchWord, document.Content));
                                            if (FuzzyMatcher.PartialRatio(matchWord, document.Content.ToLower()) >= threshold)
                                            {
                                                matchedDocuments.Add(document);
                                                _archiveStoragePath.addArchiveStoragePath(request.storagepath, document);

                                                break; // No need to continue checking for this document
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (string matchWord in request.storagepath.Match)
                                        {
                                            Console.WriteLine(document.Id + " : " + FuzzyMatcher.PartialRatio(matchWord, document.Content));
                                            if (FuzzyMatcher.PartialRatio(matchWord, document.Content.ToLower()) >= threshold)
                                            {
                                                matchedDocuments.Add(document);
                                                _archiveStoragePath.addArchiveStoragePath(request.storagepath, document);

                                                break; // No need to continue checking for this document
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

                        // Add the matched documents to the StoragePath
                        if (matchedDocuments.Any())
                        {
                            storagePathToAdd.Documents = matchedDocuments;
                            await _repository.AddAsync(storagePathToAdd);
                        }
                        else
                        {
                            storagePathToAdd.Documents = null;
                            await _repository.AddAsync(storagePathToAdd);
                        }
                        storage = storagePathToAdd;
                    }



                    else if (request.storagepath.Matching_algorithm == Matching_Algorithms.MATCH_REGEX)
                    {
                        // Bring all the documents
                        List<Document> all_documents = await _documentrepository.GetAllDocumentByOwner(request.storagepath.Owner);

                        // Create the StoragePath and StoragePathDocument objects
                        var storagePathToAdd = StoragePath.Create(
                            request.storagepath.Name,
                            request.storagepath.Match,
                            request.storagepath.Owner,
                            request.storagepath.Is_insensitive,
                            request.storagepath.Path,
                            request.storagepath.Name,
                            request.storagepath.Document_count,
                            request.storagepath.Matching_algorithm,
                            documents,
                            request.storagepath.Set_permissions.View.Users,
                            request.storagepath.Set_permissions.View.Groups,
                            request.storagepath.Set_permissions.Change.Users,
                            request.storagepath.Set_permissions.Change.Groups
                        );
                        storagePathToAdd.Slug = request.storagepath.Name;

                        List<Document> matchedDocuments = new List<Document>();

                        if (request.storagepath.Is_insensitive == true)
                        {
                            foreach (Document document in all_documents)
                            {
                                try
                                {
                                    foreach (string regexPattern in request.storagepath.Match.Select(x => x.ToLower()).ToList())
                                    {
                                        if (document.StoragePathId == null)
                                        {
                                            if (Regex.IsMatch(document.Content, regexPattern))
                                            {
                                                matchedDocuments.Add(document);

                                                _archiveStoragePath.addArchiveStoragePath(request.storagepath, document);


                                                break; // No need to continue checking for this document
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

                            // Add the matched documents to the StoragePath
                            if (matchedDocuments.Any())
                            {
                                storagePathToAdd.Documents = matchedDocuments;
                                await _repository.AddAsync(storagePathToAdd);
                            }
                            else
                            {
                                storagePathToAdd.Documents = null;
                                await _repository.AddAsync(storagePathToAdd);
                            }
                        }
                        else
                        {

                            foreach (Document document in all_documents)
                            {
                                try
                                {
                                    foreach (string regexPattern in request.storagepath.Match)
                                    {
                                        if (document.StoragePathId == null)
                                        {
                                            if (Regex.IsMatch(document.Content, regexPattern))
                                            {
                                                matchedDocuments.Add(document);

                                                _archiveStoragePath.addArchiveStoragePath(request.storagepath, document);

                                                break; // No need to continue checking for this document
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

                            // Add the matched documents to the StoragePath
                            if (matchedDocuments.Any())
                            {
                                storagePathToAdd.Documents = matchedDocuments;
                                await _repository.AddAsync(storagePathToAdd);
                            }
                            else
                            {
                                storagePathToAdd.Documents = null;
                                await _repository.AddAsync(storagePathToAdd);
                            }
                        }
                        storage = storagePathToAdd;
                    }


                    return storage;

                }
            }

        }

    }
}