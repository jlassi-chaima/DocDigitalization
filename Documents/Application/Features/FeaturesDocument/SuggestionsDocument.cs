using Application.Dtos.Correspondent;
using Application.Dtos.Documents;
using Application.Dtos.DocumentType;
using Application.Dtos.StoragePath;
using Application.Dtos.Suggestions;
using Application.Dtos.Tag;
using Application.Exceptions;
using Application.Respository;
using Domain.DocumentManagement;
using MediatR;
using System.Text.RegularExpressions;

namespace Application.Features.FeaturesDocument
{
    public class SuggestionsDocument
    {
        public sealed record Query : IRequest<SuggestionsDto>
        {
            public readonly Guid Id;
            public Query(Guid id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Query, SuggestionsDto>
        {
            private readonly IDocumentRepository _repository;
            private readonly ITagRepository _tagRepository;
            private readonly IStoragePathRepository _storagePathRepository;
            private readonly ICorrespondentRepository _correspondentRepository;
            private readonly IDocumentTypeRepository _documentTypeRepository;
            public Handler(IDocumentRepository repository, ITagRepository tagRepository, IStoragePathRepository storagePathRepository, ICorrespondentRepository correspondentRepository, IDocumentTypeRepository documentTypeRepository)
            {
                _repository = repository;
                _tagRepository = tagRepository;
                _storagePathRepository = storagePathRepository;
                _correspondentRepository = correspondentRepository;
                _documentTypeRepository = documentTypeRepository;
            }

            public async Task<SuggestionsDto> Handle(Query request, CancellationToken cancellationToken)
            {
                SuggestionsDto suggestions = new SuggestionsDto();
                suggestions.Tags = [];
                suggestions.Correspondents = [];
                suggestions.DocumentTypes = [];
                suggestions.StoragePaths = [];
                suggestions.Dates = [];


                DocumentSuggestionsDto documentdetails = await _repository.FindByIdDetailsSuggestionsAsync(request.Id, cancellationToken);
                if (documentdetails == null)
                {
                    throw new DocumentsException($"Document with ID {request.Id} not found.");
                }
                //TAGS
                List<TagDtoDetails> ListTags = await _tagRepository.GetTagDetailsAsync(cancellationToken);
                foreach (TagDtoDetails tag in ListTags)
                {
                    if (tag.Match != null)
                    {

                        if (tag.matching_algorithm == Matching_Algorithms.MATCH_ANY)
                        {
                            if (tag.Is_insensitive == true)
                            {
                                var lowerCaseMatch = tag.Match.Select(x => x.ToLower()).ToList();
                                bool allConditionsMet = lowerCaseMatch.Any(x => documentdetails.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {
                                    suggestions.Tags.Add(tag.Id);
                                }
                            }

                            else
                            {
                                bool allConditionsMet = tag.Match.Any(x => documentdetails.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {
                                    suggestions.Tags.Add(tag.Id);
                                }
                            }
                        }
                        if (tag.matching_algorithm == Matching_Algorithms.MATCH_ALL)
                        {
                            if (tag.Is_insensitive == true)
                            {
                                var lowerCaseMatch = tag.Match.Select(x => x.ToLower()).ToList();
                                bool allConditionsMet = lowerCaseMatch.All(x => documentdetails.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {
                                    suggestions.Tags.Add(tag.Id);
                                }
                            }
                            else
                            {
                                bool allConditionsMet = tag.Match.All(x => documentdetails.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {
                                    suggestions.Tags.Add(tag.Id);
                                }
                            }
                        }
                        if (tag.matching_algorithm == Matching_Algorithms.MATCH_LITERAL)
                        {
                            if (tag.Is_insensitive == true)
                            {
                                var lowerCaseMatch = tag.Match.Select(x => x.ToLower()).ToList();
                                if (lowerCaseMatch.Count() == 1 && documentdetails.Content.Contains(lowerCaseMatch[0]))
                                {
                                    suggestions.Tags.Add(tag.Id);
                                }
                            }
                            else
                            {
                                if (tag.Match.Count() == 1 && documentdetails.Content.Contains(tag.Match[0]))
                                {
                                    suggestions.Tags.Add(tag.Id);
                                }
                            }
                        }
                        if (tag.matching_algorithm == Matching_Algorithms.MATCH_REGEX)
                        {
                            if (tag.Is_insensitive == true)
                            {
                                var lowerCaseMatch = tag.Match.Select(x => x.ToLower()).ToList();

                                foreach (string regexPattern in lowerCaseMatch)
                                {
                                    if (Regex.IsMatch(documentdetails.Content, regexPattern))
                                    {
                                        suggestions.Tags.Add(tag.Id);
                                    }
                                }
                            }
                            else
                            {
                                foreach (string regexPattern in tag.Match)
                                {
                                    if (Regex.IsMatch(documentdetails.Content, regexPattern))
                                    {
                                        suggestions.Tags.Add(tag.Id);
                                    }
                                }
                            }
                        }
                    }
                }
                if (documentdetails.Tags != null)
                {
                    foreach (DocumentTagsDTO tag in documentdetails.Tags)
                    {
                        suggestions.Tags.Remove(tag.TagId);
                    }
                }
                //CORRESONDENTS
                List<CorrespondentListDTO> ListCorrespondents = await _correspondentRepository.GetCorrespondentDetailsAsync<CorrespondentListDTO>(cancellationToken);
                foreach (CorrespondentListDTO correspondent in ListCorrespondents)
                {
                    if (correspondent.Match != null)
                    {

                        if (correspondent.Matching_algorithm == Matching_Algorithms.MATCH_ANY)
                        {
                            if (correspondent.Is_insensitive == true)
                            {
                                var lowerCaseMatch = correspondent.Match.Select(x => x.ToLower()).ToList();
                                bool allConditionsMet = lowerCaseMatch.Any(x => documentdetails.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {
                                    suggestions.Correspondents.Add(correspondent.Id);
                                }
                            }

                            else
                            {
                                bool allConditionsMet = correspondent.Match.Any(x => documentdetails.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {
                                    suggestions.Correspondents.Add(correspondent.Id);
                                }
                            }
                        }
                        if (correspondent.Matching_algorithm == Matching_Algorithms.MATCH_ALL)
                        {
                            if (correspondent.Is_insensitive == true)
                            {
                                var lowerCaseMatch = correspondent.Match.Select(x => x.ToLower()).ToList();
                                bool allConditionsMet = lowerCaseMatch.All(x => documentdetails.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {
                                    suggestions.Correspondents.Add(correspondent.Id);
                                }
                            }
                            else
                            {
                                bool allConditionsMet = correspondent.Match.All(x => documentdetails.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {
                                    suggestions.Correspondents.Add(correspondent.Id);
                                }
                            }
                        }
                        if (correspondent.Matching_algorithm == Matching_Algorithms.MATCH_LITERAL)
                        {
                            if (correspondent.Is_insensitive == true)
                            {
                                var lowerCaseMatch = correspondent.Match.Select(x => x.ToLower()).ToList();
                                if (lowerCaseMatch.Count() == 1 && documentdetails.Content.Contains(lowerCaseMatch[0]))
                                {
                                    suggestions.Correspondents.Add(correspondent.Id);
                                }
                            }
                            else
                            {
                                if (correspondent.Match.Count() == 1 && documentdetails.Content.Contains(correspondent.Match[0]))
                                {
                                    suggestions.Correspondents.Add(correspondent.Id);
                                }
                            }
                        }
                        if (correspondent.Matching_algorithm == Matching_Algorithms.MATCH_REGEX)
                        {
                            if (correspondent.Is_insensitive == true)
                            {
                                var lowerCaseMatch = correspondent.Match.Select(x => x.ToLower()).ToList();

                                foreach (string regexPattern in lowerCaseMatch)
                                {
                                    if (Regex.IsMatch(documentdetails.Content, regexPattern))
                                    {
                                        suggestions.Correspondents.Add(correspondent.Id);
                                    }
                                }
                            }
                            else
                            {
                                foreach (string regexPattern in correspondent.Match)
                                {
                                    if (Regex.IsMatch(documentdetails.Content, regexPattern))
                                    {
                                        suggestions.Correspondents.Add(correspondent.Id);
                                    }
                                }
                            }
                        }
                    }
                }
                if (documentdetails != null)
                {
                    if (documentdetails.CorrespondentId != null)
                    {
                        suggestions.Correspondents.Remove(documentdetails.CorrespondentId);
                    }
                }


                //STORAGE PATHS
                List<ListStoragePathDto> ListStoragePaths = await _storagePathRepository.GetStoragePathDetailsAsync<ListStoragePathDto>(cancellationToken);
                foreach (ListStoragePathDto storagepath in ListStoragePaths)
                {
                    if (storagepath.Match != null)
                    {

                        if (storagepath.Matching_algorithm == Matching_Algorithms.MATCH_ANY)
                        {
                            if (storagepath.Is_insensitive == true)
                            {
                                var lowerCaseMatch = storagepath.Match.Select(x => x.ToLower()).ToList();
                                bool allConditionsMet = lowerCaseMatch.Any(x => documentdetails.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {
                                    suggestions.StoragePaths.Add(storagepath.Id);
                                }
                            }

                            else
                            {
                                bool allConditionsMet = storagepath.Match.Any(x => documentdetails.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {
                                    suggestions.StoragePaths.Add(storagepath.Id);
                                }
                            }
                        }
                        if (storagepath.Matching_algorithm == Matching_Algorithms.MATCH_ALL)
                        {
                            if (storagepath.Is_insensitive == true)
                            {
                                var lowerCaseMatch = storagepath.Match.Select(x => x.ToLower()).ToList();
                                bool allConditionsMet = lowerCaseMatch.All(x => documentdetails.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {
                                    suggestions.StoragePaths.Add(storagepath.Id);
                                }
                            }
                            else
                            {
                                bool allConditionsMet = storagepath.Match.All(x => documentdetails.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {
                                    suggestions.StoragePaths.Add(storagepath.Id);
                                }
                            }
                        }
                        if (storagepath.Matching_algorithm == Matching_Algorithms.MATCH_LITERAL)
                        {
                            if (storagepath.Is_insensitive == true)
                            {
                                var lowerCaseMatch = storagepath.Match.Select(x => x.ToLower()).ToList();
                                if (lowerCaseMatch.Count() == 1 && documentdetails.Content.Contains(lowerCaseMatch[0]))
                                {
                                    suggestions.StoragePaths.Add(storagepath.Id);
                                }
                            }
                            else
                            {
                                if (storagepath.Match.Count() == 1 && documentdetails.Content.Contains(storagepath.Match[0]))
                                {
                                    suggestions.StoragePaths.Add(storagepath.Id);
                                }
                            }
                        }
                        if (storagepath.Matching_algorithm == Matching_Algorithms.MATCH_REGEX)
                        {
                            if (storagepath.Is_insensitive == true)
                            {
                                var lowerCaseMatch = storagepath.Match.Select(x => x.ToLower()).ToList();

                                foreach (string regexPattern in lowerCaseMatch)
                                {
                                    if (Regex.IsMatch(documentdetails.Content, regexPattern))
                                    {
                                        suggestions.StoragePaths.Add(storagepath.Id);
                                    }
                                }
                            }
                            else
                            {
                                foreach (string regexPattern in storagepath.Match)
                                {
                                    if (Regex.IsMatch(documentdetails.Content, regexPattern))
                                    {
                                        suggestions.StoragePaths.Add(storagepath.Id);
                                    }
                                }
                            }
                        }
                    }
                }
                if (documentdetails != null)
                {
                    if (documentdetails.StoragePathId != null)
                    {
                        suggestions.StoragePaths.Remove(documentdetails.StoragePathId);
                    }
                }


                //DOCUMENT TYPES
                List<DocumentTypeDetailsDTO> ListDocumentTypes = await _documentTypeRepository.GetDocumentTypesDetailsAsync(cancellationToken);
                foreach (DocumentTypeDetailsDTO documenttype in ListDocumentTypes)
                {
                    if (documenttype.Match != null)
                    {

                        if (documenttype.Matching_algorithm == Matching_Algorithms.MATCH_ANY)
                        {
                            if (documenttype.Is_insensitive == true)
                            {
                                var lowerCaseMatch = documenttype.Match.Select(x => x.ToLower()).ToList();
                                bool allConditionsMet = lowerCaseMatch.Any(x => documentdetails.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {
                                    suggestions.DocumentTypes.Add(documenttype.Id);
                                }
                            }

                            else
                            {
                                bool allConditionsMet = documenttype.Match.Any(x => documentdetails.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {
                                    suggestions.DocumentTypes.Add(documenttype.Id);
                                }
                            }
                        }
                        if (documenttype.Matching_algorithm == Matching_Algorithms.MATCH_ALL)
                        {
                            if (documenttype.Is_insensitive == true)
                            {
                                var lowerCaseMatch = documenttype.Match.Select(x => x.ToLower()).ToList();
                                bool allConditionsMet = lowerCaseMatch.All(x => documentdetails.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {
                                    suggestions.DocumentTypes.Add(documenttype.Id);
                                }
                            }
                            else
                            {
                                bool allConditionsMet = documenttype.Match.All(x => documentdetails.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {
                                    suggestions.DocumentTypes.Add(documenttype.Id);
                                }
                            }
                        }
                        if (documenttype.Matching_algorithm == Matching_Algorithms.MATCH_LITERAL)
                        {
                            if (documenttype.Is_insensitive == true)
                            {
                                var lowerCaseMatch = documenttype.Match.Select(x => x.ToLower()).ToList();
                                if (lowerCaseMatch.Count() == 1 && documentdetails.Content.Contains(lowerCaseMatch[0]))
                                {
                                    suggestions.DocumentTypes.Add(documenttype.Id);
                                }
                            }
                            else
                            {
                                if (documenttype.Match.Count() == 1 && documentdetails.Content.Contains(documenttype.Match[0]))
                                {
                                    suggestions.DocumentTypes.Add(documenttype.Id);
                                }
                            }
                        }
                        if (documenttype.Matching_algorithm == Matching_Algorithms.MATCH_REGEX)
                        {
                            if (documenttype.Is_insensitive == true)
                            {
                                var lowerCaseMatch = documenttype.Match.Select(x => x.ToLower()).ToList();

                                foreach (string regexPattern in lowerCaseMatch)
                                {
                                    if (Regex.IsMatch(documentdetails.Content, regexPattern))
                                    {
                                        suggestions.DocumentTypes.Add(documenttype.Id);
                                    }
                                }
                            }
                            else
                            {
                                foreach (string regexPattern in documenttype.Match)
                                {
                                    if (Regex.IsMatch(documentdetails.Content, regexPattern))
                                    {
                                        suggestions.DocumentTypes.Add(documenttype.Id);
                                    }
                                }
                            }
                        }
                    }
                }
                if (documentdetails != null)
                {
                    if (documentdetails.DocumentTypeId != null)
                    {
                        suggestions.DocumentTypes.Remove(documentdetails.DocumentTypeId);
                    }
                }



                return suggestions;
            }
        }

    }
}
