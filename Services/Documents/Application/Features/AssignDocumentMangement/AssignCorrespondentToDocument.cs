using Application.Dtos.Correspondent;
using Application.Respository;
using Domain.DocumentManagement;
using Domain.Documents;
using RapidFuzz.Net;
using System.Text.RegularExpressions;

namespace Application.Features.AssignDocumentMangement
{
    public class AssignCorrespondentToDocument
    {
        private readonly ICorrespondentRepository _correspondentrepository;

        public AssignCorrespondentToDocument(ICorrespondentRepository correspondentrepository)
        {
            _correspondentrepository = correspondentrepository;
        }
        public async Task<CorrespondentListDTO> AssignCorrespondent(Document document, string idowner)
        {
            //List<CorrespondentListDTO> correpondents = await _correspondentrepository.GetCorrespondentDetailsAsync<CorrespondentListDTO>();
            List<CorrespondentListDTO> correpondents = await _correspondentrepository.GetListStoragePathByOwner(idowner);

            foreach (CorrespondentListDTO correspondent in correpondents)
            {
                if (correspondent.Matching_algorithm == Matching_Algorithms.MATCH_ANY)
                {
                    if (correspondent.Is_insensitive == true)
                    {
                        if (correspondent.Match != null)
                        {
                            var lowerCaseMatch = correspondent.Match.Select(x => x.ToLower()).ToList();
                            bool allConditionsMet = lowerCaseMatch.Any(x => document.Content.ToLower().Contains(x));
                            Console.WriteLine(allConditionsMet);
                            if (allConditionsMet)
                            {
                                return correspondent;
                            }

                        }
                    }
                    else
                    {
                        if (correspondent.Match != null)
                        {
                            bool allConditionsMet = correspondent.Match.Any(x => document.Content.ToLower().Contains(x));
                            Console.WriteLine(allConditionsMet);
                            if (allConditionsMet)
                            {
                                return correspondent;
                            }
                        }
                    }
                }
                if (correspondent.Matching_algorithm == Matching_Algorithms.MATCH_ALL)
                {
                    if (correspondent.Is_insensitive == true)
                    {
                        if (correspondent.Match != null)
                        {
                            var lowerCaseMatch = correspondent.Match.Select(x => x.ToLower()).ToList();
                            bool allConditionsMet = lowerCaseMatch.Any(x => document.Content.ToLower().Contains(x));
                            if (allConditionsMet)
                            {
                                return correspondent;
                            }
                        }
                    }
                    else
                    {
                        if (correspondent.Match != null)
                        {
                            bool allConditionsMet = correspondent.Match.All(x => document.Content.ToLower().Contains(x));
                            Console.WriteLine(allConditionsMet);
                            if (allConditionsMet)
                            {
                                return correspondent;
                            }
                        }
                    }

                }
                if (correspondent.Matching_algorithm == Matching_Algorithms.MATCH_LITERAL)
                {
                    if (correspondent.Is_insensitive == true)
                    {
                        if (correspondent.Match != null)
                        {
                            var lowerCaseMatch = correspondent.Match.Select(x => x.ToLower()).ToList();

                            if (lowerCaseMatch.Count() == 1 && document.Content.ToLower().Contains(lowerCaseMatch[0]))
                            {
                                return correspondent;
                            }
                        }
                    }
                    else
                    {
                        if (correspondent.Match != null)
                        {
                            if (correspondent.Match.Count() == 1 && document.Content.ToLower().Contains(correspondent.Match[0]))
                            {
                                return correspondent;
                            }
                        }
                    }
                }
                else if (correspondent.Matching_algorithm == Matching_Algorithms.MATCH_REGEX)
                {
                    if (correspondent.Is_insensitive == true)
                    {
                        if (correspondent.Match != null)
                        {
                            var lowerCaseMatch = correspondent.Match.Select(x => x.ToLower()).ToList();
                            foreach (string regexPattern in correspondent.Match)
                            {
                                if (Regex.IsMatch(document.Content, regexPattern))
                                {
                                    return correspondent;
                                }
                            }
                        }
                    }
                    else
                    {

                        if (correspondent.Match != null)
                        {
                            foreach (string regexPattern in correspondent.Match)
                            {
                                if (Regex.IsMatch(document.Content, regexPattern))
                                {
                                    return correspondent;
                                }
                            }
                        }
                    }
                }
                else if (correspondent.Matching_algorithm == Matching_Algorithms.MATCH_FUZZY)
                {
                    if (correspondent.Is_insensitive == true)
                    {
                        if (correspondent.Match != null)
                        {
                            foreach (string matchWord in correspondent.Match)
                            {

                                if (FuzzyMatcher.PartialRatio(matchWord, document.Content) > 90)
                                {
                                    return correspondent;

                                }
                            }
                        }
                    }
                    else
                    {
                        if (correspondent.Match != null)
                        {
                            foreach (string matchWord in correspondent.Match)
                            {
                                if (FuzzyMatcher.PartialRatio(matchWord, document.Content) > 90)
                                {
                                    return correspondent;

                                }
                            }
                        }
                    }
                }

            }
            return null;
        }
    }
}
