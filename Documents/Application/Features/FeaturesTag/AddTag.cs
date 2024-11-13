using Application.Dtos.Tag;
using Application.Exceptions;
using Application.Respository;
using Domain.DocumentManagement.tags;
using Domain.DocumentManagement;
using Domain.Documents;
using FluentValidation;
using MapsterMapper;
using MediatR;
using System.Text.RegularExpressions;
using RapidFuzz.Net;
using Domain.Logs;




namespace Application.Features.FeaturesTag
{
    public class AddTag
    {
        public sealed record Command : IRequest<Tag>
        {
            public readonly TagDto tagDto;

            public Command(TagDto tag)
            {
                tagDto = tag;
            }
        }
        public sealed class AddValidator : AbstractValidator<Command>
        {
            public AddValidator(ITagRepository _repository)
            {
                RuleFor(p => p.tagDto.Name).NotEmpty().WithMessage("Title can't be empty.");
                RuleFor(p => p.tagDto.Matching_algorithm).Must(i => Enum.IsDefined(typeof(Matching_Algorithms), i))
                                              .WithMessage("You must choose a matching algorithm. Invalid matching algorithm.");
                RuleFor(p => p.tagDto.Color).NotEmpty().WithMessage("Color can't be empty.")
                                             .Matches("^#(?:[0-9a-fA-F]{3}){1,2}$").WithMessage("Color must be in hexadecimal format.");

            }
        }
        public sealed class Handler : IRequestHandler<Command, Tag>
        {
            private readonly IDocumentRepository _documentrepository;
            private readonly ITagRepository _repository;
            private readonly IMapper _mapper;
            private readonly ILogRepository _logRepository;

            public Handler(ITagRepository repository, IMapper mapper, IDocumentRepository documentrepository, ILogRepository logRepository)
            {
                _documentrepository = documentrepository;
                _repository = repository;
                _mapper = mapper;
                _logRepository = logRepository;
            }


            public async Task<Tag> Handle(Command request, CancellationToken cancellationToken)
            {
                Logs new_tag = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"new Tag added {request.tagDto.Name}");
                await _logRepository.AddAsync(new_tag);


                List<DocumentTags> document_tag_List = new List<DocumentTags>();

                Tag tag = null;

                if (request.tagDto.Matching_algorithm == Matching_Algorithms.MATCH_NONE || request.tagDto.Match.Count == 0)
                {
                    var tagToAdd = Tag.Create(
                                          request.tagDto.Name,
                                          request.tagDto.Name,
                                          request.tagDto.Color,
                                          request.tagDto.Owner,
                                          request.tagDto.Is_insensitive,
                                          request.tagDto.Matching_algorithm,
                                          request.tagDto.Set_permissions.View.Users,
                                          request.tagDto.Set_permissions.View.Groups,
                                          request.tagDto.Set_permissions.Change.Users,
                                          request.tagDto.Set_permissions.Change.Groups
                                             );
                    request.tagDto.Match = null;
                    tagToAdd.Documents = null;
                    Logs matchdocument = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"new Tag added {request.tagDto.Name}");
                    await _logRepository.AddAsync(matchdocument);
                    await _repository.AddAsync(tagToAdd);

                }
                else if (request.tagDto.Matching_algorithm == Matching_Algorithms.MATCH_ANY)
                {
                    List<Document> documents = (List<Document>)await _documentrepository.GetAllAsync();
                    var tagToAdd = Tag.Create(
                                request.tagDto.Name,
                                request.tagDto.Name,
                                request.tagDto.Color,
                                request.tagDto.Owner,
                                request.tagDto.Is_insensitive,
                                request.tagDto.Matching_algorithm,
                                request.tagDto.Set_permissions.View.Users,
                                request.tagDto.Set_permissions.View.Groups,
                                request.tagDto.Set_permissions.Change.Users,
                                request.tagDto.Set_permissions.Change.Groups

                          );
                    Logs addtag = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"new Tag added {request.tagDto.Name}");
                    await _logRepository.AddAsync(addtag);
                    foreach (Document document in documents)
                    {
                        if (request.tagDto.Is_insensitive == true)
                        {
                            var lowerCaseMatch = request.tagDto.Match.Select(x => x.ToLower()).ToList();
                            bool allConditionsMet = lowerCaseMatch.Any(x => document.Content.ToLower().Contains(x));
                            Console.WriteLine(allConditionsMet);
                            if (allConditionsMet)
                            {

                                DocumentTags documentTag = new DocumentTags
                                {
                                    Document = document,
                                    DocumentId = document.Id,
                                    Tag = tagToAdd,
                                    TagId = tagToAdd.Id
                                };
                                Logs matchdocument = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Document match tag {request.tagDto.Name}");
                                await _logRepository.AddAsync(matchdocument);
                                document_tag_List.Add(documentTag);
                                tagToAdd.Match = request.tagDto.Match;

                            }
                            else
                            {
                                Logs matchdocument = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Document did not match tag {request.tagDto.Name}");
                                await _logRepository.AddAsync(matchdocument);
                            }
                        }
                        else
                        {
                            bool allConditionsMet = request.tagDto.Match.Any(x => document.Content.ToLower().Contains(x));
                            Console.WriteLine(allConditionsMet);
                            if (allConditionsMet)
                            {

                                DocumentTags documentTag = new DocumentTags
                                {
                                    Document = document,
                                    DocumentId = document.Id,
                                    Tag = tagToAdd,
                                    TagId = tagToAdd.Id
                                };
                                Logs matchdocument = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Document match tag {request.tagDto.Name}");
                                await _logRepository.AddAsync(matchdocument);
                                document_tag_List.Add(documentTag);
                                tagToAdd.Match = request.tagDto.Match;

                            }
                            else
                            {
                                Logs matchdocument = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Document did not match tag {request.tagDto.Name}");
                                await _logRepository.AddAsync(matchdocument);
                            }
                        }

                    }
                    if (document_tag_List.Any())
                    {
                        tagToAdd.Documents = document_tag_List;
                        await _repository.AddAsync(tagToAdd);
                    }
                    else
                    {
                        tagToAdd.Documents = null;
                        tagToAdd.Match = request.tagDto.Match;
                        await _repository.AddAsync(tagToAdd);
                    }


                }
                else if (request.tagDto.Matching_algorithm == Matching_Algorithms.MATCH_AUTO)
                {

                }
                else if (request.tagDto.Matching_algorithm == Matching_Algorithms.MATCH_ALL)
                {
                    List<Document> documents = (List<Document>)await _documentrepository.GetAllAsync();
                    var tagToAdd = Tag.Create(
                                request.tagDto.Name,
                                request.tagDto.Name,
                                request.tagDto.Color,
                                request.tagDto.Owner,
                                request.tagDto.Is_insensitive,
                                request.tagDto.Matching_algorithm,
                                request.tagDto.Set_permissions.View.Users,
                                request.tagDto.Set_permissions.View.Groups,
                                request.tagDto.Set_permissions.Change.Users,
                                request.tagDto.Set_permissions.Change.Groups
                           );
                    Logs addtag = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"new Tag added {request.tagDto.Name}");
                    await _logRepository.AddAsync(addtag);
                    foreach (Document document in documents)
                    {
                        if (request.tagDto.Is_insensitive == true)
                        {
                            var lowerCaseMatch = request.tagDto.Match.Select(x => x.ToLower()).ToList();
                            bool allConditionsMet = lowerCaseMatch.All(x => document.Content.Contains(x));
                            Console.WriteLine(allConditionsMet);
                            if (allConditionsMet)
                            {

                                DocumentTags documentTag = new DocumentTags
                                {
                                    Document = document,
                                    DocumentId = document.Id,
                                    Tag = tagToAdd,
                                    TagId = tagToAdd.Id
                                };
                                Logs matchdocument = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Document match tag {request.tagDto.Name}");
                                await _logRepository.AddAsync(matchdocument);
                                document_tag_List.Add(documentTag);
                                tagToAdd.Match = request.tagDto.Match;

                            }
                        }
                        else
                        {
                            bool allConditionsMet = request.tagDto.Match.All(x => document.Content.Contains(x));
                            Console.WriteLine(allConditionsMet);
                            if (allConditionsMet)
                            {

                                DocumentTags documentTag = new DocumentTags
                                {
                                    Document = document,
                                    DocumentId = document.Id,
                                    Tag = tagToAdd,
                                    TagId = tagToAdd.Id
                                };
                                Logs matchdocument = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Document match tag {request.tagDto.Name}");
                                await _logRepository.AddAsync(matchdocument);
                                document_tag_List.Add(documentTag);
                                tagToAdd.Match = request.tagDto.Match;

                            }
                        }
                    }
                    if (document_tag_List.Any())
                    {
                        tagToAdd.Documents = document_tag_List;
                        await _repository.AddAsync(tagToAdd);
                    }
                    else
                    {
                        tagToAdd.Documents = null;
                        tagToAdd.Match = request.tagDto.Match;
                        await _repository.AddAsync(tagToAdd);
                    }
                }
                else if (request.tagDto.Matching_algorithm == Matching_Algorithms.MATCH_LITERAL)
                {
                    List<Document> documents = (List<Document>)await _documentrepository.GetAllAsync();
                    var tagToAdd = Tag.Create(
                                  request.tagDto.Name,
                                  request.tagDto.Name,
                                  request.tagDto.Color,
                                  request.tagDto.Owner,
                                  request.tagDto.Is_insensitive,
                                  request.tagDto.Matching_algorithm,
                                  request.tagDto.Set_permissions.View.Users,
                                  request.tagDto.Set_permissions.View.Groups,
                                  request.tagDto.Set_permissions.Change.Users,
                                  request.tagDto.Set_permissions.Change.Groups

                            );
                    Logs addtag = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"new Tag added {request.tagDto.Name}");
                    await _logRepository.AddAsync(addtag);
                    foreach (Document document in documents)
                    {
                        if (request.tagDto.Is_insensitive == true)
                        {
                            var lowerCaseMatch = request.tagDto.Match.Select(x => x.ToLower()).ToList();
                            if (lowerCaseMatch.Count() == 1 && document.Content.Contains(lowerCaseMatch[0]))
                            {

                                DocumentTags documentTag = new DocumentTags
                                {
                                    Document = document,
                                    DocumentId = document.Id,
                                    Tag = tagToAdd,
                                    TagId = tagToAdd.Id
                                };
                                Logs matchdocument = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Document match tag {request.tagDto.Name}");
                                await _logRepository.AddAsync(matchdocument);
                                document_tag_List.Add(documentTag);
                                tagToAdd.Match = request.tagDto.Match;

                            }
                        }
                        else
                        {
                            if (request.tagDto.Match.Count() == 1 && document.Content.Contains(request.tagDto.Match[0]))
                            {

                                DocumentTags documentTag = new DocumentTags
                                {
                                    Document = document,
                                    DocumentId = document.Id,
                                    Tag = tagToAdd,
                                    TagId = tagToAdd.Id
                                };
                                Logs matchdocument = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Document match tag {request.tagDto.Name}");
                                await _logRepository.AddAsync(matchdocument);
                                document_tag_List.Add(documentTag);
                                tagToAdd.Match = request.tagDto.Match;

                            }
                        }
                    }
                    if (document_tag_List.Any())
                    {
                        tagToAdd.Documents = document_tag_List;
                        await _repository.AddAsync(tagToAdd);
                    }
                    else
                    {
                        tagToAdd.Documents = null;
                        tagToAdd.Match = request.tagDto.Match;
                        await _repository.AddAsync(tagToAdd);
                    }


                }
                else if (request.tagDto.Matching_algorithm == Matching_Algorithms.MATCH_REGEX)
                {

                    List<Document> all_documents = (List<Document>)await _documentrepository.GetAllAsync();

                    var tagToAdd = Tag.Create(
                                   request.tagDto.Name,
                                   request.tagDto.Name,
                                   request.tagDto.Color,
                                   request.tagDto.Owner,
                                   request.tagDto.Is_insensitive,
                                   request.tagDto.Matching_algorithm,
                                   request.tagDto.Set_permissions.View.Users,
                                   request.tagDto.Set_permissions.View.Groups,
                                   request.tagDto.Set_permissions.Change.Users,
                                   request.tagDto.Set_permissions.Change.Groups
                    );
                    Logs addtag = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"new Tag added {request.tagDto.Name}");
                    await _logRepository.AddAsync(addtag);

                    foreach (Document document in all_documents)
                    {
                        try
                        {
                            if (request.tagDto.Is_insensitive == true)
                            {
                                var lowerCaseMatch = request.tagDto.Match.Select(x => x.ToLower()).ToList();

                                foreach (string regexPattern in lowerCaseMatch)
                                {
                                    if (Regex.IsMatch(document.Content, regexPattern))
                                    {
                                        DocumentTags matcheddocument = new DocumentTags
                                        {
                                            Document = document,
                                            DocumentId = document.Id,
                                            Tag = tagToAdd,
                                            TagId = tagToAdd.Id
                                        };
                                        Logs matchdocument = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Document match tag {request.tagDto.Name}");
                                        await _logRepository.AddAsync(matchdocument);
                                        document_tag_List.Add(matcheddocument);
                                        break; // No need to continue checking for this document
                                    }
                                }
                            }
                            else
                            {
                                foreach (string regexPattern in request.tagDto.Match)
                                {
                                    if (Regex.IsMatch(document.Content, regexPattern))
                                    {
                                        DocumentTags matcheddocument = new DocumentTags
                                        {
                                            Document = document,
                                            DocumentId = document.Id,
                                            Tag = tagToAdd,
                                            TagId = tagToAdd.Id
                                        };
                                        Logs matchdocument = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Document match tag {request.tagDto.Name}");
                                        await _logRepository.AddAsync(matchdocument);
                                        document_tag_List.Add(matcheddocument);
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
                    if (document_tag_List.Any())
                    {
                        tagToAdd.Documents = document_tag_List;
                        await _repository.AddAsync(tagToAdd);
                    }
                    else
                    {
                        tagToAdd.Documents = null;
                        tagToAdd.Match = request.tagDto.Match;
                        await _repository.AddAsync(tagToAdd);
                    }
                }
                else if (request.tagDto.Matching_algorithm == Matching_Algorithms.MATCH_FUZZY)
                {


                    List<Document> all_documents = (List<Document>)await _documentrepository.GetAllAsync();


                    var tagToAdd = Tag.Create(
                                    request.tagDto.Name,
                                    request.tagDto.Name,
                                    request.tagDto.Color,
                                    request.tagDto.Owner,
                                    request.tagDto.Is_insensitive,
                                    request.tagDto.Matching_algorithm,
                                    request.tagDto.Set_permissions.View.Users,
                                    request.tagDto.Set_permissions.View.Groups,
                                    request.tagDto.Set_permissions.Change.Users,
                                    request.tagDto.Set_permissions.Change.Groups

                              );

                    int threshold = 90;
                    Logs addtag = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"new Tag added {request.tagDto.Name}");
                    await _logRepository.AddAsync(addtag);
                    foreach (Document document in all_documents)
                    {
                        try
                        {
                            if (request.tagDto.Is_insensitive == true)
                            {
                                var lowerCaseMatch = request.tagDto.Match.Select(x => x.ToLower()).ToList();
                                foreach (string matchWord in lowerCaseMatch)
                                {

                                    if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= threshold)
                                    {
                                        DocumentTags matcheddocument = new DocumentTags
                                        {
                                            Document = document,
                                            DocumentId = document.Id,
                                            Tag = tagToAdd,
                                            TagId = tagToAdd.Id
                                        };
                                        Logs matchdocument = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Document match tag {request.tagDto.Name}");
                                        await _logRepository.AddAsync(matchdocument);
                                        document_tag_List.Add(matcheddocument);
                                        // No need to continue checking for this document
                                    }
                                }
                            }
                            else
                            {
                                foreach (string matchWord in request.tagDto.Match)
                                {

                                    if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= threshold)
                                    {
                                        DocumentTags matcheddocument = new DocumentTags
                                        {
                                            Document = document,
                                            DocumentId = document.Id,
                                            Tag = tagToAdd,
                                            TagId = tagToAdd.Id
                                        };

                                        document_tag_List.Add(matcheddocument);
                                        Logs matchdocument = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Document match tag {request.tagDto.Name}");
                                        await _logRepository.AddAsync(matchdocument);
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
                    if (document_tag_List.Any())
                    {
                        tagToAdd.Documents = document_tag_List;
                        tagToAdd.Match = request.tagDto.Match;
                        await _repository.AddAsync(tagToAdd);
                    }
                    else
                    {
                        tagToAdd.Documents = null;
                        tagToAdd.Match = request.tagDto.Match;
                        await _repository.AddAsync(tagToAdd);
                    }
                }

                else
                {
                    throw new TagException("You must choose any algorithm for matching");
                }

                return _mapper.Map<Tag>(tag);
            }


        }
    }
}
