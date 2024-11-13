using Application.Dtos.StoragePath;
using Application.Respository;
using Domain.DocumentManagement;
using Domain.Documents;
using RapidFuzz.Net;
using System.Text.RegularExpressions;

namespace Application.Features.AssignDocumentMangement
{
    public class AssignStoragePathToDocument
    {
        private readonly IStoragePathRepository _repository;

        public AssignStoragePathToDocument(IStoragePathRepository repository)
        {
            _repository = repository;
        }
        public async Task<StoragePathDto> AssignStoragePath(Document document, string idowner)
        {
            List<StoragePathDto> stroagepaths = await _repository.GetListStoragePathByOwner(idowner);
            StoragePathDto result = new StoragePathDto();
            foreach (StoragePathDto storagepath in stroagepaths)
            {
                if (storagepath.Matching_algorithm == Matching_Algorithms.MATCH_ANY)
                {
                    if (storagepath.Is_insensitive == true)
                    {
                     
                        if (storagepath.Match != null)
                        {
                            var lowerCaseMatch = storagepath.Match.Select(x => x.ToLower()).ToList();
                            bool allConditionsMet = lowerCaseMatch.Any(x => document.Content.ToLower().Contains(x));
                            Console.WriteLine(allConditionsMet);
                            if (allConditionsMet)
                            {
                                return storagepath;
                            }
                        }
                    }
                    else
                    {
                        if (storagepath.Match != null)
                        {
                            
                            bool allConditionsMet = storagepath.Match.Any(x => document.Content.ToLower().Contains(x));
                            Console.WriteLine(allConditionsMet);
                            if (allConditionsMet)
                            {
                                return storagepath;
                            }
                        }
                    }
                }
                if (storagepath.Matching_algorithm == Matching_Algorithms.MATCH_ALL)
                {
                    if (storagepath.Is_insensitive == true)
                    {
                        if (storagepath.Match != null)
                        {
                            var lowerCaseMatch = storagepath.Match.Select(x => x.ToLower()).ToList();
                            bool allConditionsMet = lowerCaseMatch.All(x => document.Content.ToLower().Contains(x));
                            Console.WriteLine(allConditionsMet);
                            if (allConditionsMet)
                            {
                                return storagepath;
                            }
                        }
                    }
                    else
                    {
                        if (storagepath.Match != null)
                        {

                            bool allConditionsMet = storagepath.Match.All(x => document.Content.ToLower().Contains(x));
                            Console.WriteLine(allConditionsMet);
                            if (allConditionsMet)
                            {
                                return storagepath;
                            }
                        }
                    }
                }
                if (storagepath.Matching_algorithm == Matching_Algorithms.MATCH_LITERAL)
                {
                    if (storagepath.Is_insensitive == true)
                    {
                        if (storagepath.Match != null)
                        {
                            var lowerCaseMatch = storagepath.Match.Select(x => x.ToLower()).ToList();
                            if (lowerCaseMatch.Count() == 1 && document.Content.ToLower().Contains(lowerCaseMatch[0]))
                            {
                                return storagepath;
                            }
                        }
                    }
                    else
                    {
                        if (storagepath.Match != null)
                        {
                            if (storagepath.Match.Count() == 1 && document.Content.ToLower().Contains(storagepath.Match[0]))
                            {
                                return storagepath;
                            }
                        }
                    }
                }
                if (storagepath.Matching_algorithm == Matching_Algorithms.MATCH_REGEX)
                {
                    if (storagepath.Is_insensitive == true)
                    {
                        if (storagepath.Match != null)
                        {
                            var lowerCaseMatch = storagepath.Match.Select(x => x.ToLower()).ToList();
                            foreach (string regexPattern in lowerCaseMatch)
                            {
                                if (Regex.IsMatch(document.Content, regexPattern))
                                {
                                    return storagepath;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (storagepath.Match != null)
                        {
                            foreach (string regexPattern in storagepath.Match)
                            {
                                if (Regex.IsMatch(document.Content, regexPattern))
                                {
                                    return storagepath;
                                }
                            }
                        }
                    }
                }
                int threshold = 90;
                if (storagepath.Matching_algorithm == Matching_Algorithms.MATCH_FUZZY)
                {
                    if (storagepath.Is_insensitive == true)
                    {
                        if (storagepath.Match != null)
                        {
                            var lowerCaseMatch = storagepath.Match.Select(x => x.ToLower()).ToList();
                            foreach (string matchWord in lowerCaseMatch)
                            {

                                if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= threshold)
                                {
                                    return storagepath;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (storagepath.Match != null)
                        {

                            foreach (string matchWord in storagepath.Match)
                            {

                                if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= threshold)
                                {
                                    return storagepath;
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
