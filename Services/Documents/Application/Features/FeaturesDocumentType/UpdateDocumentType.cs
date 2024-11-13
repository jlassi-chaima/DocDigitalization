using Application.Dtos.DocumentType;
using Application.Respository;
using Domain.DocumentManagement;
using Domain.DocumentManagement.DocumentTypes;
using Domain.Documents;
using Domain.Logs;
using MapsterMapper;
using MediatR;
using RapidFuzz.Net;
using System.Text.RegularExpressions;

namespace Application.Features.FeaturesDocumentType
{
    public class UpdateDocumentType
    {
        public sealed record Command : IRequest<DocumentType>
        {
            public readonly Guid DocumentTypeId;
            public readonly DocumentTypeDto DocumentTypeupdate;

            public Command(DocumentTypeDto documentTypeupdate, Guid id)
            {
                DocumentTypeupdate = documentTypeupdate;
                DocumentTypeId = id;
            }
        }
        public sealed class Handler : IRequestHandler<Command, DocumentType>
        {
            private readonly IDocumentTypeRepository _repository;
            private readonly IDocumentRepository _documentrepository;
            private readonly IMapper _mapper;
            private readonly ILogRepository _logRepository;

            public Handler(IDocumentTypeRepository repository, IMapper mapper, IDocumentRepository documentrepository, ILogRepository logRepository)
            {
                _repository = repository;
                _mapper = mapper;
                _documentrepository = documentrepository;
                _logRepository = logRepository;
            }

            public async Task<DocumentType> Handle(Command request, CancellationToken cancellationToken)
            {
                Logs new_document_type = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Document Type updated {request.DocumentTypeupdate.Name}");
                await _logRepository.AddAsync(new_document_type);


                DocumentType olddoc = new DocumentType();
                DocumentType documenttypetoupdate = _repository.FindByIdAsync(request.DocumentTypeId, cancellationToken).GetAwaiter().GetResult();


                olddoc.Matching_algorithm = documenttypetoupdate.Matching_algorithm;
                olddoc.Match = documenttypetoupdate.Match;
                if (documenttypetoupdate.UsersView != null || documenttypetoupdate.GroupsView != null ||
               documenttypetoupdate.UsersChange != null || documenttypetoupdate.GroupsChange != null)
                {
                    documenttypetoupdate.UsersView?.Clear();
                    documenttypetoupdate.GroupsView?.Clear();
                    documenttypetoupdate.UsersChange?.Clear();
                    documenttypetoupdate.GroupsChange?.Clear();
                }
                _mapper.Map(request.DocumentTypeupdate, documenttypetoupdate);
                documenttypetoupdate.Slug = request.DocumentTypeupdate.Name;
                if (request.DocumentTypeupdate.Set_permissions?.Change?.Groups != null)
                {

                    foreach (var group in request.DocumentTypeupdate.Set_permissions.Change.Groups)
                    {
                        if(documenttypetoupdate.GroupsChange ==null)
                        {
                            documenttypetoupdate.GroupsChange = new List<string>();
                        }

                        if (!documenttypetoupdate.GroupsChange.Contains(group))
                        {
                            documenttypetoupdate.GroupsChange.Add(group);
                        }
                    }
                }
                if (request.DocumentTypeupdate.Set_permissions?.Change?.Users != null)
                {
                    foreach (var user in request.DocumentTypeupdate.Set_permissions.Change.Users)
                    {
                        if (documenttypetoupdate.UsersChange == null)
                        {
                            documenttypetoupdate.UsersChange = new List<string>();
                        }
                        if (!documenttypetoupdate.UsersChange.Contains(user))
                        {
                            documenttypetoupdate.UsersChange.Add(user);
                        }
                    }
                }
                if (request.DocumentTypeupdate.Set_permissions?.View?.Users != null)
                {
                    foreach (var user in request.DocumentTypeupdate.Set_permissions.View.Users)
                    {
                        if (documenttypetoupdate.UsersView == null)
                        {
                            documenttypetoupdate.UsersView = new List<string>();
                        }
                        if (!documenttypetoupdate.UsersView.Contains(user))
                        {
                            documenttypetoupdate.UsersView.Add(user);
                        }
                    }
                }
                if (request.DocumentTypeupdate.Set_permissions?.View?.Groups != null)
                {
                    foreach (var user in request.DocumentTypeupdate.Set_permissions.View.Groups)
                    {
                        if (documenttypetoupdate.GroupsView == null)
                        {
                            documenttypetoupdate.GroupsView = new List<string>();
                        }
                        if (!documenttypetoupdate.GroupsView.Contains(user))
                        {
                            documenttypetoupdate.GroupsView.Add(user);
                        }
                    }
                }
                await _repository.UpdateAsync(documenttypetoupdate);

                List<Document> all_documents = await _documentrepository.GetAllDocumentByOwner(documenttypetoupdate.Owner);
                if (documenttypetoupdate.Matching_algorithm == Matching_Algorithms.MATCH_NONE)
                {
                    foreach (Document document in all_documents)
                    {
                        if (document.DocumentTypeId == documenttypetoupdate.Id)
                        {
                            document.Document_Type = null;
                            document.DocumentTypeId = null;
                            await _documentrepository.UpdateAsync(document);

                        }
                    }
                }
                else if (documenttypetoupdate.Matching_algorithm == Matching_Algorithms.MATCH_ANY)
                {
                    if (olddoc.Matching_algorithm != Matching_Algorithms.MATCH_NONE)
                    {
                        foreach (Document document in all_documents)
                        {


                            if (document.DocumentTypeId == documenttypetoupdate.Id)
                            {
                                document.Document_Type = null;
                                document.DocumentTypeId = null;
                                await _documentrepository.UpdateAsync(document);
                            }


                        }
                    }
                    if (documenttypetoupdate.Match != null)
                    {
                        foreach (Document document in all_documents)
                        {
                            try
                            {
                                if (document.DocumentTypeId == null)
                                {
                                    if (documenttypetoupdate.Is_insensitive == true)
                                    {
                                        var lowerCaseMatch = documenttypetoupdate.Match.Select(x => x.ToLower()).ToList();
                                        bool allConditionsMet = lowerCaseMatch.Any(x => document.Content.Contains(x));

                                        Console.WriteLine(allConditionsMet);

                                        if (allConditionsMet)
                                        {
                                            document.DocumentTypeId = documenttypetoupdate.Id;
                                            document.Document_Type = documenttypetoupdate;
                                            await _documentrepository.UpdateAsync(document);
                                        }
                                    }
                                    else
                                    {
                                        bool allConditionsMet = documenttypetoupdate.Match.Any(x => document.Content?.Contains(x) ?? false);


                                        Console.WriteLine(allConditionsMet);

                                        if (allConditionsMet)
                                        {
                                            document.DocumentTypeId = documenttypetoupdate.Id;
                                            document.Document_Type = documenttypetoupdate;
                                            await _documentrepository.UpdateAsync(document);
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
                else if (documenttypetoupdate.Matching_algorithm == Matching_Algorithms.MATCH_ALL)
                {
                    if (olddoc.Matching_algorithm != Matching_Algorithms.MATCH_NONE)
                    {
                        foreach (Document document in all_documents)
                        {



                            if (document.DocumentTypeId == documenttypetoupdate.Id)
                            {
                                document.Document_Type = null;
                                document.DocumentTypeId = null;
                                await _documentrepository.UpdateAsync(document);
                            }


                        }
                    }
                    if (documenttypetoupdate.Match != null)
                    {
                        foreach (Document document in all_documents)
                        {
                            try
                            {
                                if (document.DocumentTypeId == null)
                                {
                                    if (documenttypetoupdate.Is_insensitive == true)
                                    {
                                        var lowerCaseMatch = documenttypetoupdate.Match.Select(x => x.ToLower()).ToList();
                                        bool allConditionsMet = lowerCaseMatch.Any(x => document.Content.Contains(x));

                                        Console.WriteLine(allConditionsMet);

                                        if (allConditionsMet)
                                        {
                                            document.DocumentTypeId = documenttypetoupdate.Id;
                                            document.Document_Type = documenttypetoupdate;
                                            await _documentrepository.UpdateAsync(document);
                                        }
                                    }
                                    else
                                    {
                                        bool allConditionsMet = documenttypetoupdate.Match.All(x => document.Content?.Contains(x) ?? false);


                                        Console.WriteLine(allConditionsMet);

                                        if (allConditionsMet)
                                        {
                                            document.DocumentTypeId = documenttypetoupdate.Id;
                                            document.Document_Type = documenttypetoupdate;
                                            await _documentrepository.UpdateAsync(document);
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
                else if (documenttypetoupdate.Matching_algorithm == Matching_Algorithms.MATCH_LITERAL)
                {

                    //in case it's not none because none has no effect on the document but other algos do (there is no storagepathids to delete)
                    if (olddoc.Matching_algorithm != Matching_Algorithms.MATCH_NONE)
                    {
                        foreach (Document document in all_documents)
                        {


                            if (document.DocumentTypeId == documenttypetoupdate.Id)
                            {
                                document.Document_Type = null;
                                document.DocumentTypeId = null;
                                await _documentrepository.UpdateAsync(document);
                            }


                        }
                    }
                    if (documenttypetoupdate.Match != null)
                    {
                        foreach (Document document in all_documents)
                        {
                            try
                            {
                                if (document.DocumentTypeId == null)
                                {
                                    if (document != null && documenttypetoupdate != null)
                                    {
                                        if (documenttypetoupdate.Is_insensitive == true)
                                        {

                                            if (documenttypetoupdate.Match.Count() == 1 && document.Content.Contains(documenttypetoupdate.Match[0].ToLower()))
                                            {
                                                document.DocumentTypeId = documenttypetoupdate.Id;
                                                document.Document_Type = documenttypetoupdate;
                                                await _documentrepository.UpdateAsync(document);
                                            }
                                        }
                                        else
                                        {
                                            if (documenttypetoupdate.Match.Count() == 1 && document.Content.Contains(documenttypetoupdate.Match[0]))
                                            {
                                                document.DocumentTypeId = documenttypetoupdate.Id;
                                                document.Document_Type = documenttypetoupdate;
                                                await _documentrepository.UpdateAsync(document);
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
                else if (documenttypetoupdate.Matching_algorithm == Matching_Algorithms.MATCH_REGEX)
                {

                    //in case it's not none because none has no effect on the document but other algos do (there is no storagepathids to delete)
                    if (olddoc.Matching_algorithm != Matching_Algorithms.MATCH_NONE)
                    {
                        foreach (Document document in all_documents)
                        {


                            if (document.DocumentTypeId == documenttypetoupdate.Id)
                            {
                                document.Document_Type = null;
                                document.DocumentTypeId = null;
                                await _documentrepository.UpdateAsync(document);
                            }


                        }
                    }
                    if (documenttypetoupdate.Match != null)
                    {
                        foreach (Document document in all_documents)
                        {
                            try
                            {
                                if (document.DocumentTypeId == null)
                                {
                                    if (documenttypetoupdate.Is_insensitive == true)
                                    {
                                        foreach (string regexPattern in documenttypetoupdate.Match.Select(x => x.ToLower()).ToList())
                                        {
                                            if (Regex.IsMatch(document.Content, regexPattern))
                                            {
                                                document.DocumentTypeId = documenttypetoupdate.Id;
                                                document.Document_Type = documenttypetoupdate;
                                                await _documentrepository.UpdateAsync(document);
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (string regexPattern in documenttypetoupdate.Match)
                                        {
                                            if (Regex.IsMatch(document.Content, regexPattern))
                                            {
                                                document.DocumentTypeId = documenttypetoupdate.Id;
                                                document.Document_Type = documenttypetoupdate;
                                                await _documentrepository.UpdateAsync(document);
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
                else if (documenttypetoupdate.Matching_algorithm == Matching_Algorithms.MATCH_FUZZY)
                {

                    //in case it's not none because none has no effect on the document but other algos do (there is no storagepathids to delete)
                    if (olddoc.Matching_algorithm != Matching_Algorithms.MATCH_NONE)
                    {
                        foreach (Document document in all_documents)
                        {


                            if (document.DocumentTypeId == documenttypetoupdate.Id)
                            {
                                document.Document_Type = null;
                                document.DocumentTypeId = null;
                                await _documentrepository.UpdateAsync(document);
                            }


                        }
                    }

                    int threshold = 90;
                    if (documenttypetoupdate.Match != null)
                    {
                        foreach (Document document in all_documents)
                        {
                            try
                            {
                                if (document.DocumentTypeId == null)
                                {
                                    foreach (string matchWord in documenttypetoupdate.Match.Select(x => x.ToLower()).ToList())
                                    {
                                        if (documenttypetoupdate.Is_insensitive == true)
                                        {
                                            Console.WriteLine("test " + FuzzyMatcher.PartialRatio(matchWord, document.Content));
                                            if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= threshold)
                                            {
                                                document.DocumentTypeId = documenttypetoupdate.Id;
                                                document.Document_Type = documenttypetoupdate;
                                                await _documentrepository.UpdateAsync(document);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("test " + FuzzyMatcher.PartialRatio(matchWord, document.Content));
                                            if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= threshold)
                                            {
                                                document.DocumentTypeId = documenttypetoupdate.Id;
                                                document.Document_Type = documenttypetoupdate;
                                                await _documentrepository.UpdateAsync(document);
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
                return documenttypetoupdate;

            }
        }
    }
}