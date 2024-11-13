using Application.Respository;
using Domain.DocumentManagement.tags;
using Domain.DocumentManagement;
using Domain.Documents;
using System.Text.RegularExpressions;
using RapidFuzz.Net;



namespace Application.Features.AssignDocumentMangement
{
    public class AssignTagToDocument
    {
        private readonly ITagRepository _tagrepository;
       

        public AssignTagToDocument(ITagRepository tagRepository)
        {
            _tagrepository = tagRepository;
        }

        public async Task<List<DocumentTags>> AssignTag(Document document, string idowner)
        {
            List<Tag> tags = (List<Tag>)await _tagrepository.GetListTagsByOwner(idowner);
            List<DocumentTags> result = new List<DocumentTags>();
            foreach (Tag tag in tags)
            {
                if (tag.Matching_algorithm == Matching_Algorithms.MATCH_ANY)
                {
                    if (tag.Is_insensitive == true)
                    {
                        if (tag.Match != null)
                        {
                            var lowerCaseMatch = tag.Match.Select(x => x.ToLower()).ToList();
                            bool allConditionsMet = lowerCaseMatch.Any(x => document.Content.ToLower().Contains(x));
                            if (allConditionsMet)
                            {

                                DocumentTags documentTag = new DocumentTags
                                {
                                    Document = document,
                                    DocumentId = document.Id,
                                    Tag = tag,
                                    TagId = tag.Id
                                };

                                result.Add(documentTag);
                            }
                        }
                    }
                    else
                    {
                        if (tag.Match != null)
                        {
                            bool allConditionsMet = tag.Match.Any(x => document.Content.Contains(x));
                            if (allConditionsMet)
                            {

                                DocumentTags documentTag = new DocumentTags
                                {
                                    Document = document,
                                    DocumentId = document.Id,
                                    Tag = tag,
                                    TagId = tag.Id
                                };

                                result.Add(documentTag);
                            }
                        }
                    }

                }
                else if (tag.Matching_algorithm == Matching_Algorithms.MATCH_ALL)
                {
                    if (tag.Is_insensitive == true)
                    {
                        if (tag.Match != null)
                        {
                            var lowerCaseMatch = tag.Match.Select(x => x.ToLower()).ToList();
                            bool allConditionsMet = lowerCaseMatch.All(x => document.Content.Contains(x));
                            Console.WriteLine(allConditionsMet);
                            if (allConditionsMet)
                            {

                                DocumentTags documentTag = new DocumentTags
                                {
                                    Document = document,
                                    DocumentId = document.Id,
                                    Tag = tag,
                                    TagId = tag.Id
                                };

                                result.Add(documentTag);

                            }
                        }
                    }
                    else
                    {
                        if (tag.Match != null)
                        {
                            bool allConditionsMet = tag.Match.All(x => document.Content.Contains(x));
                            Console.WriteLine(allConditionsMet);
                            if (allConditionsMet)
                            {

                                DocumentTags documentTag = new DocumentTags
                                {
                                    Document = document,
                                    DocumentId = document.Id,
                                    Tag = tag,
                                    TagId = tag.Id
                                };

                                result.Add(documentTag);

                            }
                        }
                    }
                }
                else if (tag.Matching_algorithm == Matching_Algorithms.MATCH_LITERAL)
                {
                    if (tag.Is_insensitive == true)
                    {
                        if (tag.Match != null)
                        {
                            var lowerCaseMatch = tag.Match.Select(x => x.ToLower()).ToList();
                            if (lowerCaseMatch.Count() == 1 && document.Content.Contains(lowerCaseMatch[0]))
                            {
                                DocumentTags documentTag = new DocumentTags
                                {
                                    Document = document,
                                    DocumentId = document.Id,
                                    Tag = tag,
                                    TagId = tag.Id
                                };
                                result.Add(documentTag);

                            }
                        }
                    }
                    else
                    {
                        if (tag.Match != null)
                        {
                            if (tag.Match.Count() == 1 && document.Content.Contains(tag.Match[0]))
                            {
                                DocumentTags documentTag = new DocumentTags
                                {
                                    Document = document,
                                    DocumentId = document.Id,
                                    Tag = tag,
                                    TagId = tag.Id
                                };
                                result.Add(documentTag);

                            }
                        }
                    }
                }
                else if (tag.Matching_algorithm == Matching_Algorithms.MATCH_REGEX)
                {
                    if (tag.Is_insensitive == true)
                    {
                        if (tag.Match != null)
                        {
                            var lowerCaseMatch = tag.Match.Select(x => x.ToLower()).ToList();

                            foreach (string regexPattern in lowerCaseMatch)
                            {
                                if (Regex.IsMatch(document.Content, regexPattern))
                                {
                                    DocumentTags documentTag = new DocumentTags
                                    {
                                        Document = document,
                                        DocumentId = document.Id,
                                        Tag = tag,
                                        TagId = tag.Id
                                    };

                                    result.Add(documentTag);

                                }
                            }
                        }
                    }
                    else
                    {
                        if (tag.Match != null)
                        {
                            foreach (string regexPattern in tag.Match)
                            {
                                if (Regex.IsMatch(document.Content, regexPattern))
                                {
                                    DocumentTags matcheddocument = new DocumentTags
                                    {
                                        Document = document,
                                        DocumentId = document.Id,
                                        Tag = tag,
                                        TagId = tag.Id
                                    };

                                    result.Add(matcheddocument);
                                }
                            }
                        }
                    }
                }
                else if (tag.Matching_algorithm == Matching_Algorithms.MATCH_FUZZY)
                {
                    int threshold = 90;
                    if (tag.Is_insensitive == true)
                    {
                        if (tag.Match != null)
                        {
                            var lowerCaseMatch = tag.Match.Select(x => x.ToLower()).ToList();
                            foreach (string matchWord in lowerCaseMatch)
                            {

                                if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= threshold)
                                {
                                    DocumentTags matcheddocument = new DocumentTags
                                    {
                                        Document = document,
                                        DocumentId = document.Id,
                                        Tag = tag,
                                        TagId = tag.Id
                                    };

                                    result.Add(matcheddocument);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (tag.Match != null)
                        {
                            foreach (string matchWord in tag.Match)
                            {

                                if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= threshold)
                                {
                                    DocumentTags matcheddocument = new DocumentTags
                                    {
                                        Document = document,
                                        DocumentId = document.Id,
                                        Tag = tag,
                                        TagId = tag.Id
                                    };

                                    result.Add(matcheddocument);
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
