using Domain.DocumentManagement;
using Domain.Documents;
using Domain.Templates;
using RapidFuzz.Net;
using System.Text.RegularExpressions;

namespace Infrastructure.Service.Helper
{
    public class MatchingAlgorithmsWorkflow
    {
        public static bool ExistingDocumentMatchesWorkflow(Document document, Template template) 
        { 
            if (template.Content_matching_algorithm == Matching_Algorithms.MATCH_ANY && template.Content_matching_pattern != null) 
            {
                if (template.Is_Insensitive == true)
                {
                    var lowerCaseMatch = template.Content_matching_pattern.Select(x => x.ToLower()).ToList();
                    bool allConditionsMet = lowerCaseMatch.Any(x => document.Content.Contains(x));
                    Console.WriteLine(allConditionsMet);
                    if (allConditionsMet)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    bool allConditionsMet = template.Content_matching_pattern.Any(x => document.Content.Contains(x));
                    Console.WriteLine(allConditionsMet);
                    if (allConditionsMet)
                    {
                        return true;
                    }
                    else 
                    { 
                        return false; 
                    }
                }

            }
            else if (template.Content_matching_algorithm == Matching_Algorithms.MATCH_ALL && template.Content_matching_pattern != null)
            {
                if (template.Is_Insensitive == true)
                {
                    var lowerCaseMatch = template.Content_matching_pattern.Select(x => x.ToLower()).ToList();
                    bool allConditionsMet = lowerCaseMatch.All(x => document.Content.Contains(x));
                    if (allConditionsMet)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    bool allConditionsMet = template.Content_matching_pattern.All(x => document.Content.Contains(x));
                    if (allConditionsMet)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

            }
            else if (template.Content_matching_algorithm == Matching_Algorithms.MATCH_REGEX && template.Content_matching_pattern != null)
            {
                if (template.Is_Insensitive == true)
                {
                    var lowerCaseMatch = template.Content_matching_pattern.Select(x => x.ToLower()).ToList();

                    foreach (string regexPattern in lowerCaseMatch)
                    {
                        if (Regex.IsMatch(document.Content, regexPattern))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                   
                }
                else
                {
                    foreach (string regexPattern in template.Content_matching_pattern)
                    {
                        if (Regex.IsMatch(document.Content, regexPattern))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

            }
            else if (template.Content_matching_algorithm == Matching_Algorithms.MATCH_FUZZY && template.Content_matching_pattern != null)
            {
                int threshold = 90;
                if (template.Is_Insensitive == true)
                {
                    var lowerCaseMatch = template.Content_matching_pattern.Select(x => x.ToLower()).ToList();
                    

                    foreach (string matchWord in lowerCaseMatch)
                    {

                        if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= threshold)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                }
                else
                {
                    foreach (string matchWord in template.Content_matching_pattern)
                    {

                        if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= threshold)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

            }
            // Add a return statement at the end of the method to handle other cases
            return false;
        }
    }
}

