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
    public class UpdateTag
    {
        public sealed record Command : IRequest<Tag>
        {
            public readonly Guid tagId;
            public readonly TagDto tag;

            public Command(TagDto tagdto, Guid id)
            {
                tag = tagdto;
                tagId = id;
            }
        }
        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator(ITagRepository _repository)
            {
                RuleFor(p => p.tagId).NotEmpty();
                RuleFor(p => p.tag.Color).NotEmpty().WithMessage("Color can't be empty.")
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
                _repository = repository;
                _mapper = mapper;
                _documentrepository = documentrepository;
                _logRepository = logRepository;
            }
            public async Task<Tag> Handle(Command request, CancellationToken cancellationToken)
            {
                Logs new_tag = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Tag updated {request.tag.Name}");
                await _logRepository.AddAsync(new_tag);

                Tag oldtag = new Tag();
                Tag Tagtoupdate = _repository.FindByIdAsync(request.tagId, cancellationToken).GetAwaiter().GetResult();
                oldtag.Matching_algorithm = Tagtoupdate.Matching_algorithm;
                oldtag.Documents = Tagtoupdate.Documents;
                if (Tagtoupdate == null)
                {
                    throw new TagException($"Tag with ID {request.tagId} not found.");
                }

                _mapper.Map(request.tag, Tagtoupdate);
                if (Tagtoupdate.UsersView != null || Tagtoupdate.GroupsView != null ||
            Tagtoupdate.UsersChange != null || Tagtoupdate.GroupsChange != null)
                {
                    Tagtoupdate.UsersView?.Clear();
                    Tagtoupdate.GroupsView?.Clear();
                    Tagtoupdate.UsersChange?.Clear();
                    Tagtoupdate.GroupsChange?.Clear();
                }
                

            Tagtoupdate.Slug = request.tag.Name;
                
                    if (request.tag.Set_permissions?.View?.Users != null)
                    {
                        if (Tagtoupdate.UsersView == null)
                        {
                        Tagtoupdate.UsersView = new List<string>();
                        }
                        foreach (var user in request.tag.Set_permissions.View.Users)
                        {
                         if(!Tagtoupdate.UsersView.Contains(user))
                            {
                                Tagtoupdate.UsersView.Add(user);
                            }
                       
                        }
                    }
                    if (request.tag.Set_permissions?.View?.Groups != null)
                    {
                        if (Tagtoupdate.GroupsView == null)
                        {
                        Tagtoupdate.GroupsView = new List<string>();
                        }
                        foreach (var group in request.tag.Set_permissions.View.Groups)
                        {
                        if (!Tagtoupdate.GroupsView.Contains(group))
                        {
                            Tagtoupdate.GroupsView.Add(group);
                        }
                        
                        }
                    }
                    if (request.tag.Set_permissions?.Change?.Users != null)
                    {
                        if (Tagtoupdate.UsersChange == null)
                        {
                        Tagtoupdate.UsersChange = new List<string>();
                        }
                        foreach (var user in request.tag.Set_permissions.Change.Users)
                        {
                        if (!Tagtoupdate.UsersChange.Contains(user))
                        {
                            Tagtoupdate.UsersChange.Add(user);
                        }
                       
                        }
                    }
                    if (request.tag.Set_permissions?.Change?.Groups != null)
                    {
                        if (Tagtoupdate.GroupsChange == null)
                        {
                        Tagtoupdate.GroupsChange = new List<string>();
                        }

                        foreach (var group in request.tag.Set_permissions.Change.Groups)
                        {
                        if (!Tagtoupdate.GroupsChange.Contains(group))
                        {
                            Tagtoupdate.GroupsChange.Add(group);
                        }
                    }
                    }
                
                await _repository.UpdateAsync(Tagtoupdate);

                List<Document> all_documents = (List<Document>)await _documentrepository.GetAllAsync();
                List<DocumentTags> documents_tags_list = new List<DocumentTags>();
                if (Tagtoupdate.Matching_algorithm == Matching_Algorithms.MATCH_NONE && Tagtoupdate.Match.Count() == 0)
                {
                    foreach (Document document in all_documents)
                    {


                        var tagsToRemove = oldtag.Documents.ToList(); // Create a copy of the list to avoid modifying the original while iterating.
                        foreach (var item in tagsToRemove)
                        {
                            if (document.Tags.Contains(item))
                            {
                                document.Tags.Remove(item);
                                await _documentrepository.UpdateAsync(document);
                            }
                        }
                    }


                }


                if (Tagtoupdate.Matching_algorithm == Matching_Algorithms.MATCH_ANY && Tagtoupdate.Match != null)
                {

                    foreach (Document document in all_documents)
                    {



                        var tagsToRemove = oldtag.Documents.ToList();
                        foreach (var item in tagsToRemove)
                        {
                            if (document.Tags.Contains(item))
                            {
                                document.Tags.Remove(item);
                                await _documentrepository.UpdateAsync(document);
                            }
                        }
                    }



                    foreach (Document document in all_documents)
                    {
                        if (Tagtoupdate.Is_insensitive == true)
                        {
                            var lowerCaseMatch = Tagtoupdate.Match.Select(x => x.ToLower()).ToList();
                            bool allConditionsMet = Tagtoupdate.Match.Any(x => document.Content.Contains(x));
                            Console.WriteLine(allConditionsMet);
                            if (allConditionsMet)
                            {

                                DocumentTags documentTag = new DocumentTags
                                {
                                    Document = document,
                                    DocumentId = document.Id,
                                    Tag = Tagtoupdate,
                                    TagId = Tagtoupdate.Id
                                };

                                documents_tags_list.Add(documentTag);


                            }
                        }
                        else
                        {
                            bool allConditionsMet = Tagtoupdate.Match.Any(x => document.Content.Contains(x));
                            Console.WriteLine(allConditionsMet);
                            if (allConditionsMet)
                            {

                                DocumentTags documentTag = new DocumentTags
                                {
                                    Document = document,
                                    DocumentId = document.Id,
                                    Tag = Tagtoupdate,
                                    TagId = Tagtoupdate.Id
                                };

                                documents_tags_list.Add(documentTag);


                            }
                        }

                    }
                    if (documents_tags_list.Any())
                    {
                        Tagtoupdate.Documents = documents_tags_list;
                        await _repository.UpdateAsync(Tagtoupdate);
                    }
                    else
                    {
                        Tagtoupdate.Documents = null;

                        await _repository.AddAsync(Tagtoupdate);
                    }

                }
                if (Tagtoupdate.Matching_algorithm == Matching_Algorithms.MATCH_ALL && Tagtoupdate.Match != null)
                {

                    foreach (Document document in all_documents)
                    {
                        if (document.Tags != null && oldtag.Documents != null)
                        {

                            var tagsToRemove = oldtag.Documents.ToList();
                            foreach (var item in tagsToRemove)
                            {
                                if (document.Tags.Contains(item))
                                {
                                    document.Tags.Remove(item);
                                    await _documentrepository.UpdateAsync(document);
                                }
                            }
                        }
                        else
                            continue;

                    }
                    foreach (Document document in all_documents)
                    {
                        if (Tagtoupdate.Is_insensitive == true)
                        {
                            var lowerCaseMatch = Tagtoupdate.Match.Select(x => x.ToLower()).ToList();
                            bool allConditionsMet = lowerCaseMatch.All(x => document.Content.Contains(x));
                            Console.WriteLine(allConditionsMet);
                            if (allConditionsMet)
                            {

                                DocumentTags documentTag = new DocumentTags
                                {
                                    Document = document,
                                    DocumentId = document.Id,
                                    Tag = Tagtoupdate,
                                    TagId = Tagtoupdate.Id
                                };

                                documents_tags_list.Add(documentTag);


                            }
                        }
                        else
                        {
                            bool allConditionsMet = Tagtoupdate.Match.All(x => document.Content.Contains(x));
                            Console.WriteLine(allConditionsMet);
                            if (allConditionsMet)
                            {

                                DocumentTags documentTag = new DocumentTags
                                {
                                    Document = document,
                                    DocumentId = document.Id,
                                    Tag = Tagtoupdate,
                                    TagId = Tagtoupdate.Id
                                };

                                documents_tags_list.Add(documentTag);


                            }
                        }

                    }
                    if (documents_tags_list.Any())
                    {
                        Tagtoupdate.Documents = documents_tags_list;
                        await _repository.UpdateAsync(Tagtoupdate);
                    }
                    else
                    {
                        Tagtoupdate.Documents = null;

                        await _repository.AddAsync(Tagtoupdate);
                    }

                }
                if (Tagtoupdate.Matching_algorithm == Matching_Algorithms.MATCH_LITERAL && Tagtoupdate.Match != null)
                {

                    foreach (Document document in all_documents)
                    {


                        var tagsToRemove = oldtag.Documents.ToList();
                        foreach (var item in tagsToRemove)
                        {
                            if (document.Tags.Contains(item))
                            {
                                document.Tags.Remove(item);
                                await _documentrepository.UpdateAsync(document);
                            }
                        }
                    }



                    foreach (Document document in all_documents)
                    {
                        if (Tagtoupdate.Is_insensitive == true)
                        {

                            if (Tagtoupdate.Match.Count() == 1 && document.Content.Contains(Tagtoupdate.Match[0].ToLower()))
                            {
                                DocumentTags documentTag = new DocumentTags
                                {
                                    Document = document,
                                    DocumentId = document.Id,
                                    Tag = Tagtoupdate,
                                    TagId = Tagtoupdate.Id
                                };
                                documents_tags_list.Add(documentTag);


                            }
                        }
                        else
                        {
                            if (Tagtoupdate.Match.Count() == 1 && document.Content.Contains(Tagtoupdate.Match[0]))
                            {
                                DocumentTags documentTag = new DocumentTags
                                {
                                    Document = document,
                                    DocumentId = document.Id,
                                    Tag = Tagtoupdate,
                                    TagId = Tagtoupdate.Id
                                };
                                documents_tags_list.Add(documentTag);


                            }
                        }
                    }
                    if (documents_tags_list.Any())
                    {
                        Tagtoupdate.Documents = documents_tags_list;
                        await _repository.UpdateAsync(Tagtoupdate);
                    }
                    else
                    {
                        Tagtoupdate.Documents = null;

                        await _repository.AddAsync(Tagtoupdate);
                    }

                }
                if (Tagtoupdate.Matching_algorithm == Matching_Algorithms.MATCH_REGEX && Tagtoupdate.Match != null)
                {

                    foreach (Document document in all_documents)
                    {


                        var tagsToRemove = oldtag.Documents.ToList();
                        foreach (var item in tagsToRemove)
                        {
                            if (document.Tags.Contains(item))
                            {
                                document.Tags.Remove(item);
                                await _documentrepository.UpdateAsync(document);
                            }
                        }
                    }



                    foreach (Document document in all_documents)
                    {
                        try
                        {
                            if (Tagtoupdate.Is_insensitive == true)
                            {

                                foreach (string regexPattern in Tagtoupdate.Match.Select(x => x.ToLower()).ToList())
                                {
                                    if (Regex.IsMatch(document.Content, regexPattern))
                                    {
                                        DocumentTags documentTag = new DocumentTags
                                        {
                                            Document = document,
                                            DocumentId = document.Id,
                                            Tag = Tagtoupdate,
                                            TagId = Tagtoupdate.Id
                                        };
                                        documents_tags_list.Add(documentTag);
                                        break; // No need to continue checking for this document
                                    }
                                }
                            }
                            else
                            {
                                foreach (string regexPattern in Tagtoupdate.Match)
                                {
                                    if (Regex.IsMatch(document.Content, regexPattern))
                                    {
                                        DocumentTags documentTag = new DocumentTags
                                        {
                                            Document = document,
                                            DocumentId = document.Id,
                                            Tag = Tagtoupdate,
                                            TagId = Tagtoupdate.Id
                                        };
                                        documents_tags_list.Add(documentTag);
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
                    if (documents_tags_list.Any())
                    {
                        Tagtoupdate.Documents = documents_tags_list;
                        await _repository.UpdateAsync(Tagtoupdate);
                    }
                    else
                    {
                        Tagtoupdate.Documents = null;

                        await _repository.AddAsync(Tagtoupdate);
                    }


                }
                else if (Tagtoupdate.Matching_algorithm == Matching_Algorithms.MATCH_FUZZY && Tagtoupdate.Match != null)
                {
                    foreach (Document document in all_documents)
                    {


                        var tagsToRemove = oldtag.Documents.ToList();
                        foreach (var item in tagsToRemove)
                        {
                            if (document.Tags.Contains(item))
                            {
                                document.Tags.Remove(item);
                                await _documentrepository.UpdateAsync(document);
                            }
                        }


                    }


                    int threshold = 90;

                    foreach (Document document in all_documents)
                    {
                        try
                        {
                            if (Tagtoupdate.Is_insensitive == true)
                            {

                                foreach (string matchWord in Tagtoupdate.Match.Select(x => x.ToLower()).ToList())
                                {
                                    if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= 90)
                                    {
                                        DocumentTags documentTag = new DocumentTags
                                        {
                                            Document = document,
                                            DocumentId = document.Id,
                                            Tag = Tagtoupdate,
                                            TagId = Tagtoupdate.Id
                                        };
                                        documents_tags_list.Add(documentTag);
                                        // No need to continue checking for this document
                                    }
                                }
                            }
                            else
                            {
                                foreach (string matchWord in Tagtoupdate.Match)
                                {
                                    if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= 90)
                                    {
                                        DocumentTags documentTag = new DocumentTags
                                        {
                                            Document = document,
                                            DocumentId = document.Id,
                                            Tag = Tagtoupdate,
                                            TagId = Tagtoupdate.Id
                                        };
                                        documents_tags_list.Add(documentTag);
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
                    if (documents_tags_list.Any())
                    {
                        Tagtoupdate.Documents = documents_tags_list;
                        await _repository.UpdateAsync(Tagtoupdate);
                    }
                    else
                    {
                        Tagtoupdate.Documents = null;

                        await _repository.AddAsync(Tagtoupdate);
                    }
                }


                return Tagtoupdate;


            }
        }
    }
}
