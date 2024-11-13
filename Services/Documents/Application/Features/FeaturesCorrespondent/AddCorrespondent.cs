using Application.Dtos.Correspondent;
using Application.Exceptions;
using Application.Respository;
using Domain.DocumentManagement.Correspondent;
using Domain.DocumentManagement;
using Domain.Documents;
using FluentValidation;
using MediatR;
using System.Text.RegularExpressions;
using RapidFuzz.Net;
using Domain.Logs;


namespace Application.Features.FeaturesCorrespondent
{
    public class AddCorrespondent
    {
        public sealed record Command : IRequest<Correspondent>
        {
            public readonly CorrespondentDto correspondent;

            public Command(CorrespondentDto command)
            {
                correspondent = command;
            }
        }
        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator(ICorrespondentRepository _repository)
            {
                RuleFor(c => c.correspondent.Name).NotEmpty().WithMessage("This field may not be null.");
                RuleFor(c => c.correspondent.Matching_algorithm).Must(i => Enum.IsDefined(typeof(Matching_Algorithms), i)).WithMessage("This field may not be null.");
            }
        }
        public sealed class Handler : IRequestHandler<Command, Correspondent>
        {
            private readonly IDocumentRepository _documentrepository;
            private readonly ICorrespondentRepository _repository;
            private readonly ILogRepository _logRepository;
            public Handler(ICorrespondentRepository repository,  IDocumentRepository documentrepository, ILogRepository logRepository)
            {
                _repository = repository;
         
                _documentrepository = documentrepository;
                _logRepository = logRepository;
            }
            public async Task<Correspondent> Handle(Command request, CancellationToken cancellationToken)
            {

                Logs new_correspondent = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"new Correspondent added {request.correspondent.Name}");
                await _logRepository.AddAsync(new_correspondent);
                Correspondent checkCorrespondent = _repository.FindByUsernameAsync(request.correspondent.Name).Result;
                if (checkCorrespondent != null)
                {
                    return checkCorrespondent;
                }
                else
                {


                    List<Document> document_correspondent_List = new List<Document>();
                    Correspondent correspondent = null;
                    // match is empty 
                    if (request.correspondent.Matching_algorithm == Matching_Algorithms.MATCH_NONE || request.correspondent.Match.Count == 0)
                    {
                        var correspondentToAdd = Correspondent.Create(
                                                                     request.correspondent.Name,
                                                                     request.correspondent.Name,
                                                                     request.correspondent.Owner,
                                                                     request.correspondent.Is_insensitive,
                                                                     request.correspondent.Matching_algorithm,
                                                                     request.correspondent.Set_permissions?.View?.Users ?? new List<string>(),
                                                                     request.correspondent.Set_permissions?.View?.Groups ?? new List<string>(),
                                                                     request.correspondent.Set_permissions?.Change?.Users ?? new List<string>(),
                                                                     request.correspondent.Set_permissions?.Change?.Groups ?? new List<string>());
                        request.correspondent.Match = null;
                        correspondentToAdd.Documents = null;
                        correspondent = correspondentToAdd;
                        await _repository.AddAsync(correspondentToAdd);

                    }

                    else if (request.correspondent.Matching_algorithm == Matching_Algorithms.MATCH_ANY)
                    {
                        List<Document> documents = await _documentrepository.GetAllDocumentByOwner(request.correspondent.Owner);

                        var correspondentToAdd = Correspondent.Create(
                                                                      request.correspondent.Name,
                                                                      request.correspondent.Name,
                                                                      request.correspondent.Owner,
                                                                      request.correspondent.Is_insensitive,
                                                                      request.correspondent.Matching_algorithm,
                                                                      request.correspondent.Set_permissions?.View?.Users,
                                                                      request.correspondent.Set_permissions?.View?.Groups,
                                                                      request.correspondent.Set_permissions?.Change?.Users,
                                                                      request.correspondent.Set_permissions?.Change?.Groups);


                        foreach (Document document in documents)
                        {
                            if (document.CorrespondentId == null)
                            {
                                if (request.correspondent.Is_insensitive == true)
                                {
                                    var lowerCaseMatch = request.correspondent.Match.Select(x => x.ToLower()).ToList();
                                    bool allConditionsMet = lowerCaseMatch.Any(x => document.Content.ToLower().Contains(x));
                                    Console.WriteLine(allConditionsMet);
                                    if (allConditionsMet)
                                    {
                                        document_correspondent_List.Add(document);
                                        correspondentToAdd.Match = request.correspondent.Match;

                                    }

                                }
                                else
                                {
                                    bool allConditionsMet = request.correspondent.Match.Any(x => document.Content.ToLower().Contains(x));
                                    Console.WriteLine(allConditionsMet);
                                    if (allConditionsMet)
                                    {

                                        document_correspondent_List.Add(document);
                                        correspondentToAdd.Match = request.correspondent.Match;

                                    }
                                }

                            }
                        }
                        if (document_correspondent_List.Any())
                        {

                            correspondentToAdd.Last_correspondence = DateTime.UtcNow;
                            correspondentToAdd.Documents = document_correspondent_List;
                            await _repository.AddAsync(correspondentToAdd);
                        }
                        else
                        {
                            correspondentToAdd.Documents = null;
                            correspondentToAdd.Match = request.correspondent.Match;
                            await _repository.AddAsync(correspondentToAdd);
                        }

                        correspondent = correspondentToAdd;


                    }
                    else if (request.correspondent.Matching_algorithm == Matching_Algorithms.MATCH_AUTO)
                    {

                    }
                    else if (request.correspondent.Matching_algorithm == Matching_Algorithms.MATCH_ALL)
                    {
                        List<Document> documents = await _documentrepository.GetAllDocumentByOwner(request.correspondent.Owner);
                        var correspondentToAdd = Correspondent.Create(
                                                                   request.correspondent.Name,
                                                                   request.correspondent.Name,
                                                                   request.correspondent.Owner,
                                                                   request.correspondent.Is_insensitive,
                                                                   request.correspondent.Matching_algorithm,
                                                                   request.correspondent.Set_permissions?.View?.Users ?? new List<string>(),
                                                                   request.correspondent.Set_permissions?.View?.Groups ?? new List<string>(),
                                                                   request.correspondent.Set_permissions?.Change?.Users ?? new List<string>(),
                                                                   request.correspondent.Set_permissions?.Change?.Groups ?? new List<string>());

                        foreach (Document document in documents)
                        {

                            if (document.CorrespondentId == null)
                            {
                                if (request.correspondent.Is_insensitive == true)
                                {
                                    var lowerCaseMatch = request.correspondent.Match.Select(x => x.ToLower()).ToList();
                                    bool allConditionsMet = lowerCaseMatch.Any(x => document.Content.ToLower().Contains(x));

                                    Console.WriteLine(allConditionsMet);
                                    if (allConditionsMet)
                                    {

                                        document_correspondent_List.Add(document);
                                        correspondentToAdd.Match = request.correspondent.Match;
                                    }
                                }
                                else
                                {
                                    bool allConditionsMet = request.correspondent.Match.All(x => document.Content.ToLower().Contains(x));
                                    Console.WriteLine(allConditionsMet);
                                    if (allConditionsMet)
                                    {

                                        document_correspondent_List.Add(document);
                                        correspondentToAdd.Match = request.correspondent.Match;
                                    }
                                }
                            }
                        }
                        if (document_correspondent_List.Any())
                        {
                            correspondentToAdd.Last_correspondence = DateTime.UtcNow;
                            correspondentToAdd.Documents = document_correspondent_List;
                            await _repository.AddAsync(correspondentToAdd);
                        }
                        else
                        {
                            correspondentToAdd.Documents = null;
                            correspondentToAdd.Match = request.correspondent.Match;
                            await _repository.AddAsync(correspondentToAdd);
                        }
                        correspondent = correspondentToAdd;

                    }
                    else if (request.correspondent.Matching_algorithm == Matching_Algorithms.MATCH_LITERAL)
                    {
                        List<Document> documents = await _documentrepository.GetAllDocumentByOwner(request.correspondent.Owner);
                        var correspondentToAdd = Correspondent.Create(
                                                                request.correspondent.Name,
                                                                request.correspondent.Name,
                                                                request.correspondent.Owner,
                                                                request.correspondent.Is_insensitive,
                                                                request.correspondent.Matching_algorithm,
                                                                request.correspondent.Set_permissions?.View?.Users ?? new List<string>(),
                                                                request.correspondent.Set_permissions?.View?.Groups ?? new List<string>(),
                                                                request.correspondent.Set_permissions?.Change?.Users ?? new List<string>(),
                                                                request.correspondent.Set_permissions?.Change?.Groups ?? new List<string>());

                        foreach (Document document in documents)
                        {
                            if (document.CorrespondentId == null)
                            {
                                if (request.correspondent.Is_insensitive == true)
                                {
                                    if (request.correspondent.Match.Count() == 1 && document.Content.Contains(request.correspondent.Match[0].ToLower()))
                                    {
                                        document_correspondent_List.Add(document);
                                        correspondentToAdd.Match = request.correspondent.Match;

                                    }
                                }
                                else
                                {
                                    if (request.correspondent.Match.Count() == 1 && document.Content.Contains(request.correspondent.Match[0]))
                                    {
                                        document_correspondent_List.Add(document);
                                        correspondentToAdd.Match = request.correspondent.Match;

                                    }
                                }
                            }
                        }
                        if (document_correspondent_List.Any())
                        {

                            correspondentToAdd.Last_correspondence = DateTime.UtcNow;
                            correspondentToAdd.Documents = document_correspondent_List;
                            await _repository.AddAsync(correspondentToAdd);
                        }
                        else
                        {
                            correspondentToAdd.Documents = null;
                            correspondentToAdd.Match = request.correspondent.Match;
                            await _repository.AddAsync(correspondentToAdd);
                        }
                        correspondent = correspondentToAdd;

                    }
                    else if (request.correspondent.Matching_algorithm == Matching_Algorithms.MATCH_REGEX)
                    {
                        // Bring all the documents
                        List<Document> all_documents = await _documentrepository.GetAllDocumentByOwner(request.correspondent.Owner);


                        var correspondentToAdd = Correspondent.Create(
                                                                  request.correspondent.Name,
                                                                  request.correspondent.Name,
                                                                  request.correspondent.Owner,
                                                                  request.correspondent.Is_insensitive,
                                                                  request.correspondent.Matching_algorithm,
                                                                  request.correspondent.Set_permissions?.View?.Users ?? new List<string>(),
                                                                  request.correspondent.Set_permissions?.View?.Groups ?? new List<string>(),
                                                                  request.correspondent.Set_permissions?.Change?.Users ?? new List<string>(),
                                                                  request.correspondent.Set_permissions?.Change?.Groups ?? new List<string>());

                        List<Document> matchedDocuments = new List<Document>();

                        foreach (Document document in all_documents)
                        {
                            if (document.CorrespondentId == null)
                            {
                                try
                                {
                                    if (request.correspondent.Is_insensitive == true)
                                    {

                                        foreach (string regexPattern in request.correspondent.Match.Select(x => x.ToLower()).ToList())
                                        {
                                            if (Regex.IsMatch(document.Content, regexPattern))
                                            {
                                                matchedDocuments.Add(document);
                                                break; // No need to continue checking for this document
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (string regexPattern in request.correspondent.Match)
                                        {
                                            if (Regex.IsMatch(document.Content, regexPattern))
                                            {
                                                matchedDocuments.Add(document);
                                                break; // No need to continue checking for this document
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {

                                    Console.WriteLine($"Exception: {ex.Message}");
                                }


                            }
                        }
                        if (matchedDocuments.Any())
                        {
                            correspondentToAdd.Documents = matchedDocuments;
                            correspondentToAdd.Match = request.correspondent.Match;
                            await _repository.AddAsync(correspondentToAdd);
                        }
                        else
                        {
                            correspondentToAdd.Documents = null;
                            correspondentToAdd.Match = request.correspondent.Match;
                            await _repository.AddAsync(correspondentToAdd);
                        }
                        correspondent = correspondentToAdd;


                    }
                    else if (request.correspondent.Matching_algorithm == Matching_Algorithms.MATCH_FUZZY)
                    {

                        List<Document> all_documents = await _documentrepository.GetAllDocumentByOwner(request.correspondent.Owner);


                        // Create the StoragePath and StoragePathDocument objects
                        var correspondentToAdd = Correspondent.Create(
                                                                 request.correspondent.Name,
                                                                 request.correspondent.Name,
                                                                 request.correspondent.Owner,
                                                                 request.correspondent.Is_insensitive,
                                                                 request.correspondent.Matching_algorithm,
                                                                 request.correspondent.Set_permissions?.View?.Users ?? new List<string>(),
                                                                request.correspondent.Set_permissions?.View?.Groups ?? new List<string>(),
                                                                request.correspondent.Set_permissions?.Change?.Users ?? new List<string>(),
                                                                request.correspondent.Set_permissions?.Change?.Groups ?? new List<string>());

                        int threshold = 90;


                        foreach (Document document in all_documents)
                        {
                            if (document.CorrespondentId == null)
                            {
                                try
                                {
                                    if (request.correspondent.Is_insensitive == true)
                                    {
                                        foreach (string matchWord in request.correspondent.Match.Select(x => x.ToLower()).ToList())
                                        {
                                            Console.WriteLine(FuzzyMatcher.PartialRatio(matchWord, document.Content.ToLower()));
                                            if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= threshold)
                                            {
                                                document_correspondent_List.Add(document);
                                                // No need to continue checking for this document
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (string matchWord in request.correspondent.Match)
                                        {
                                            Console.WriteLine(FuzzyMatcher.PartialRatio(matchWord, document.Content.ToLower()));
                                            if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= threshold)
                                            {
                                                document_correspondent_List.Add(document);
                                                // No need to continue checking for this document
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

                        Console.WriteLine(document_correspondent_List.Count());
                        // Add the matched documents to the StoragePath
                        if (document_correspondent_List.Any())
                        {

                            correspondentToAdd.Documents = document_correspondent_List;
                            correspondentToAdd.Match = request.correspondent.Match;
                            await _repository.AddAsync(correspondentToAdd);
                        }
                        else
                        {
                            correspondentToAdd.Documents = null;
                            correspondentToAdd.Match = request.correspondent.Match;
                            await _repository.AddAsync(correspondentToAdd);
                        }
                        correspondent = correspondentToAdd;
                    }


                    else
                    {
                        throw new CorrespondentException("You must choose any algorithm for matching");
                    }

                    return correspondent ;
                }

            }
        }
    }
}
