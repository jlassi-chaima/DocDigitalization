using Application.Dtos.DocumentType;
using Application.Respository;
using Domain.DocumentManagement.DocumentTypes;
using Domain.DocumentManagement;
using Domain.Documents;
using FluentValidation;
using MapsterMapper;
using MediatR;
using System.Text.RegularExpressions;
using Application.Exceptions;
using RapidFuzz.Net;
using Domain.Logs;

namespace Application.Features.FeaturesDocumentType
{
    public class AddDocumentType
    {
        public sealed record Command : IRequest<DocumentType>
        {
            public readonly DocumentTypeDto DocumentType;

            public Command(DocumentTypeDto document)
            {
                DocumentType = document;
            }
        }
        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator(IDocumentTypeRepository _repository)
            {

                RuleFor(p => p.DocumentType.Name).NotEmpty().WithMessage("Title can't be empty.");
                RuleFor(p => p.DocumentType.Matching_algorithm).Must(i => Enum.IsDefined(typeof(Matching_Algorithms), i))
                                              .WithMessage("You must choose a matching algorithm. Invalid matching algorithm.");

                //RuleFor(p=>p.DocumentType.ExtractedData).NotEmpty().WithMessage()
            }
        }
        public sealed class Handler : IRequestHandler<Command, DocumentType>
        {
            private readonly IDocumentRepository _documentrepository;
            private readonly ICustomFieldRepository _customfieldrepository;
            private readonly IDocumentTypeRepository _repository;
            private readonly IMapper _mapper;
            private readonly ILogRepository _logRepository;


            public Handler(IDocumentTypeRepository repository, IMapper mapper, IDocumentRepository documentrepository, ICustomFieldRepository customfieldrepository, ILogRepository logRepository)
            {
                _documentrepository = documentrepository;
                _repository = repository;
                _mapper = mapper;
                _customfieldrepository = customfieldrepository;
                _logRepository = logRepository;
            }

            public async Task<DocumentType> Handle(Command request, CancellationToken cancellationToken)
            {
                Logs new_document_type = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"new Document Type added {request.DocumentType.Name}");
                await _logRepository.AddAsync(new_document_type);


                List<Document> documentwithTypeList = new List<Document>();
                DocumentType documentType = null;
                if (request.DocumentType.Matching_algorithm == Matching_Algorithms.MATCH_NONE || request.DocumentType.Match?.Count == 0 )
                {

                    var documentTypeToAdd = DocumentType.Create(
                        request.DocumentType.Name,
                        request.DocumentType.Name,
                        request.DocumentType.Owner,
                        request.DocumentType.Is_insensitive,
                        request.DocumentType.Matching_algorithm,
                        request.DocumentType.Set_permissions?.View?.Users,
                        request.DocumentType.Set_permissions?.View?.Groups,
                        request.DocumentType.Set_permissions?.Change?.Users,
                        request.DocumentType.Set_permissions?.Change?.Groups

                    );

                    documentTypeToAdd.Match = null;
                    documentTypeToAdd.Documents = null;
                    documentType = documentTypeToAdd;
                    await _repository.AddAsync(documentTypeToAdd);

                }
                else if (request.DocumentType.Matching_algorithm == Matching_Algorithms.MATCH_AUTO)
                {

                    var documentTypeToAdd = DocumentType.Create(
                      request.DocumentType.Name,
                      request.DocumentType.Name,
                      request.DocumentType.Owner,
                      request.DocumentType.Is_insensitive,
                      request.DocumentType.Matching_algorithm,
                      request.DocumentType.Set_permissions?.View?.Users,
                        request.DocumentType.Set_permissions?.View?.Groups,
                        request.DocumentType.Set_permissions?.Change?.Users,
                        request.DocumentType.Set_permissions?.Change?.Groups

                  );
                    if (request.DocumentType.ExtractedData != null)
                    {
                        List<Guid> cutomfieldslist = request.DocumentType.ExtractedData;
                        bool allExist = true;

                        foreach (Guid customfield in cutomfieldslist)
                        {
                            if (!await _customfieldrepository.ExistsByIdAsync(customfield))
                            {
                                allExist = false;
                                break;
                            }
                        }
                        if (allExist)
                        {
                            documentTypeToAdd.Match = null;
                            documentTypeToAdd.Documents = null;
                            documentTypeToAdd.ExtractedData = cutomfieldslist;

                            await _repository.AddAsync(documentTypeToAdd);
                        }
                        else
                        {
                            Console.WriteLine("Error");
                        }

                    }
                    else
                    {
                        documentTypeToAdd.Match = null;
                        documentTypeToAdd.Documents = null;
                        documentTypeToAdd.ExtractedData = null;

                        await _repository.AddAsync(documentTypeToAdd);
                    }

                }
                else if (request.DocumentType.Matching_algorithm == Matching_Algorithms.MATCH_ANY)
                {
                    List<Document> documents = await _documentrepository.GetAllDocumentByOwner(request.DocumentType.Owner);
                    var documentTypeToAdd = DocumentType.Create(
                                      request.DocumentType.Name,
                                      request.DocumentType.Name,
                                      request.DocumentType.Owner,
                                      request.DocumentType.Is_insensitive,
                                      request.DocumentType.Matching_algorithm,
                                    request.DocumentType.Set_permissions?.View?.Users,
                        request.DocumentType.Set_permissions?.View?.Groups,
                        request.DocumentType.Set_permissions?.Change?.Users,
                        request.DocumentType.Set_permissions?.Change?.Groups
                              );
                    foreach (Document document in documents)
                    {
                        if (document.DocumentTypeId == null)
                        {
                            if (request.DocumentType.Is_insensitive == true)
                            {
                                var lowerCaseMatch = request.DocumentType.Match.Select(x => x.ToLower()).ToList();
                                bool allConditionsMet = lowerCaseMatch.Any(x => document.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {

                                    documentwithTypeList.Add(document);
                                    documentTypeToAdd.Match = request.DocumentType.Match;

                                }
                            }
                            else
                            {
                                bool allConditionsMet = request.DocumentType.Match.Any(x => document.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {

                                    documentwithTypeList.Add(document);
                                    documentTypeToAdd.Match = request.DocumentType.Match;

                                }
                            }
                        }

                    }
                    if (documentwithTypeList.Any())
                    {
                        documentTypeToAdd.Documents = documentwithTypeList;
                        await _repository.AddAsync(documentTypeToAdd);
                    }
                    else
                    {
                        documentTypeToAdd.Documents = null;
                        documentTypeToAdd.Match = request.DocumentType.Match;
                        await _repository.AddAsync(documentTypeToAdd);
                    }
                    documentType = documentTypeToAdd;
                }
                else if (request.DocumentType.Matching_algorithm == Matching_Algorithms.MATCH_ALL)
                {
                    List<Document> documents = await _documentrepository.GetAllDocumentByOwner(request.DocumentType.Owner);
                    var documentTypeToAdd = DocumentType.Create(
                                      request.DocumentType.Name,
                                      request.DocumentType.Name,
                                      request.DocumentType.Owner,
                                      request.DocumentType.Is_insensitive,
                                      request.DocumentType.Matching_algorithm,
                               request.DocumentType.Set_permissions?.View?.Users,
                        request.DocumentType.Set_permissions?.View?.Groups,
                        request.DocumentType.Set_permissions?.Change?.Users,
                        request.DocumentType.Set_permissions?.Change?.Groups
                              );
                    foreach (Document document in documents)
                    {
                        if (document.DocumentTypeId == null)
                        {
                            if (request.DocumentType.Is_insensitive == true)
                            {
                                var lowerCaseMatch = request.DocumentType.Match.Select(x => x.ToLower()).ToList();
                                bool allConditionsMet = lowerCaseMatch.All(x => document.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {


                                    documentwithTypeList.Add(document);
                                    documentTypeToAdd.Match = request.DocumentType.Match;


                                }
                            }
                            else
                            {
                                bool allConditionsMet = request.DocumentType.Match.All(x => document.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {


                                    documentwithTypeList.Add(document);
                                    documentTypeToAdd.Match = request.DocumentType.Match;
                                    //documentTypeToAdd.DocumentwithTypes = documentwithTypeList;
                                    //documentType= documentTypeToAdd;

                                }
                            }
                        }

                    }
                    if (documentwithTypeList.Any())
                    {
                        documentTypeToAdd.Documents = documentwithTypeList;
                        await _repository.AddAsync(documentTypeToAdd);
                    }
                    else
                    {
                        documentTypeToAdd.Documents = null;
                        documentTypeToAdd.Match = request.DocumentType.Match;
                        await _repository.AddAsync(documentTypeToAdd);
                    }
                    documentType = documentTypeToAdd;

                }
                else if (request.DocumentType.Matching_algorithm == Matching_Algorithms.MATCH_LITERAL)
                {
                    List<Document> documents = await _documentrepository.GetAllDocumentByOwner(request.DocumentType.Owner);
                    var documentTypeToAdd = DocumentType.Create(
                                     request.DocumentType.Name,
                                     request.DocumentType.Name,
                                     request.DocumentType.Owner,
                                     request.DocumentType.Is_insensitive,
                                     request.DocumentType.Matching_algorithm,
                                     request.DocumentType.Set_permissions?.View?.Users,
                        request.DocumentType.Set_permissions?.View?.Groups,
                        request.DocumentType.Set_permissions?.Change?.Users,
                        request.DocumentType.Set_permissions?.Change?.Groups
                             );
                    foreach (Document document in documents)
                    {
                        if (document.DocumentTypeId == null)
                        {
                            if (request.DocumentType.Is_insensitive == true)
                            {
                                var lowerCaseMatch = request.DocumentType.Match.Select(x => x.ToLower()).ToList();
                                if (lowerCaseMatch.Count() == 1 && document.Content.Contains(lowerCaseMatch[0]))
                                {


                                    documentwithTypeList.Add(document);
                                    documentTypeToAdd.Match = request.DocumentType.Match;
                                }
                            }
                            else
                            {
                                if (request.DocumentType.Match.Count() == 1 && document.Content.Contains(request.DocumentType.Match[0]))
                                {


                                    documentwithTypeList.Add(document);
                                    documentTypeToAdd.Match = request.DocumentType.Match;
                                }
                            }
                        }
                    }
                    if (documentwithTypeList.Any())
                    {
                        documentTypeToAdd.Documents = documentwithTypeList;
                        await _repository.AddAsync(documentTypeToAdd);
                    }
                    else
                    {
                        documentTypeToAdd.Documents = null;
                        documentTypeToAdd.Match = request.DocumentType.Match;
                        await _repository.AddAsync(documentTypeToAdd);
                    }
                    documentType = documentTypeToAdd;
                }
                else if (request.DocumentType.Matching_algorithm == Matching_Algorithms.MATCH_REGEX)
                {

                    List<Document> all_documents = await _documentrepository.GetAllDocumentByOwner(request.DocumentType.Owner);

                    var documentTypeToAdd = DocumentType.Create(
                                      request.DocumentType.Name,
                                      request.DocumentType.Name,
                                      request.DocumentType.Owner,
                                      request.DocumentType.Is_insensitive,
                                      request.DocumentType.Matching_algorithm,
                                      request.DocumentType.Set_permissions?.View?.Users,
                        request.DocumentType.Set_permissions?.View?.Groups,
                        request.DocumentType.Set_permissions?.Change?.Users,
                        request.DocumentType.Set_permissions?.Change?.Groups
                              );



                    foreach (Document document in all_documents)
                    {
                        if (document.DocumentTypeId == null)
                        {
                            try
                            {
                                if (request.DocumentType.Is_insensitive == true)
                                {
                                    var lowerCaseMatch = request.DocumentType.Match.Select(x => x.ToLower()).ToList();
                                    foreach (string regexPattern in lowerCaseMatch)
                                    {
                                        if (Regex.IsMatch(document.Content, regexPattern))
                                        {
                                            documentwithTypeList.Add(document);
                                            break; // No need to continue checking for this document
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (string regexPattern in request.DocumentType.Match)
                                    {
                                        if (Regex.IsMatch(document.Content, regexPattern))
                                        {
                                            documentwithTypeList.Add(document);
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
                    }
                    if (documentwithTypeList.Any())
                    {
                        documentTypeToAdd.Documents = documentwithTypeList;
                        documentTypeToAdd.Match = request.DocumentType.Match;
                        await _repository.AddAsync(documentTypeToAdd);
                    }
                    else
                    {
                        documentTypeToAdd.Documents = null;
                        documentTypeToAdd.Match = request.DocumentType.Match;
                        await _repository.AddAsync(documentTypeToAdd);
                    }
                    documentType = documentTypeToAdd;
                }
                else if (request.DocumentType.Matching_algorithm == Matching_Algorithms.MATCH_FUZZY)
                {


                    List<Document> all_documents = await _documentrepository.GetAllDocumentByOwner(request.DocumentType.Owner);


                    var documentTypeToAdd = DocumentType.Create(
                                         request.DocumentType.Name,
                                         request.DocumentType.Name,
                                         request.DocumentType.Owner,
                                         request.DocumentType.Is_insensitive,
                                         request.DocumentType.Matching_algorithm,
                                         request.DocumentType.Set_permissions?.View?.Users,
                        request.DocumentType.Set_permissions?.View?.Groups,
                        request.DocumentType.Set_permissions?.Change?.Users,
                        request.DocumentType.Set_permissions?.Change?.Groups
                                 );

                    int threshold = 90;

                    foreach (Document document in all_documents)
                    {
                        if (document.DocumentTypeId == null)
                        {
                            try
                            {
                                if (request.DocumentType.Is_insensitive == true)
                                {
                                    var lowerCaseMatch = request.DocumentType.Match.Select(x => x.ToLower()).ToList();
                                    foreach (string matchWord in lowerCaseMatch)
                                    {

                                        if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= threshold)
                                        {
                                            documentwithTypeList.Add(document);
                                            // No need to continue checking for this document
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (string matchWord in request.DocumentType.Match)
                                    {

                                        if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= threshold)
                                        {
                                            documentwithTypeList.Add(document);
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
                    if (documentwithTypeList.Any())
                    {
                        documentTypeToAdd.Documents = documentwithTypeList;
                        documentTypeToAdd.Match = request.DocumentType.Match;
                        await _repository.AddAsync(documentTypeToAdd);
                    }
                    else
                    {
                        documentTypeToAdd.Documents = null;
                        documentTypeToAdd.Match = request.DocumentType.Match;
                        await _repository.AddAsync(documentTypeToAdd);
                    }
                }
                else
                {
                    throw new DocumentTypeException("You must choose any algorithm for matching");
                }

                return _mapper.Map<DocumentType>(documentType);
            }


        }
    }
}