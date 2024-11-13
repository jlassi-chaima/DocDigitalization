using Application.Dtos.DocumentType;
using Application.Helper;
using Application.Respository;
using Domain.DocumentManagement;
using Domain.DocumentManagement.CustomFields;
using Domain.Documents;
using Newtonsoft.Json.Linq;
using RapidFuzz.Net;
using System.Text.RegularExpressions;

namespace Application.Features.AssignDocumentMangement
{
    public class AssignDocumentTypeToDocument
    {
        private readonly IDocumentTypeRepository _documentTypeRepository;
        private readonly ICustomFieldRepository _customFieldRepository;

        public AssignDocumentTypeToDocument(IDocumentTypeRepository documentTypeRepository, ICustomFieldRepository customFieldRepository)
        {
            _documentTypeRepository = documentTypeRepository;
            _customFieldRepository = customFieldRepository;
        }
        public async Task<DocumentTypeDetailsDTO> AssignDocumenttype(Document document, string idowner, string classification_result, JObject uniqueSubdirJson)
        {
            //List<DocumentTypeDetailsDTO> documentTypes = await _documentTypeRepository.GetDocumentTypesDetailsAsync();
            List<DocumentTypeDetailsDTO> documentTypes = await _documentTypeRepository.GetListDocumentTypeByOwner(idowner);


            List<DocumentTypeDetailsDTO> documentTypesMatchAlgoAuto = await _documentTypeRepository.GetDocumentTypesDetailsWithMatchingAlgoAutoAsync();

            string responseObject = null;
            bool generated_response_ML = false;

            //bring the ML Classification result 
            //using (var httpClient = new HttpClient())
            //{
            //    var apiUrl = "http://localhost:5000/classify";
            //    // Append the result as a query parameter to the URL
            //    apiUrl += "?result=" + Uri.EscapeDataString(document.Content);


            //    try
            //    {
            //        // Send the POST request to the Flask endpoint with the result as a query parameter
            //        var response = await httpClient.PostAsync(apiUrl, null);

            //        // Ensure the request completed successfully
            //        response.EnsureSuccessStatusCode();

            //        // Read the response content as a string
            //        var responseBody = await response.Content.ReadAsStringAsync();

            //        // Deserialize the JSON string into an object
            //         responseObject = JsonConvert.DeserializeObject<string>(responseBody);

            //        // Handle the response as needed
            //        Console.WriteLine("Response from other service: " + responseObject);
            //        if (response.IsSuccessStatusCode)
            //        {
            //            Console.WriteLine("Result sent successfully.");
            //        }
            //        else
            //        {
            //            Console.WriteLine("Failed to send result. Status code: " + response.StatusCode);
            //        }
            //    }
            //    catch (HttpRequestException ex)
            //    {
            //        Console.WriteLine("Failed to send result. Error: " + ex.Message);
            //    }
            //}
            TypeField type = TypeField.UNKNOWN;

            if (documentTypesMatchAlgoAuto != null)
            {
                foreach (DocumentTypeDetailsDTO documentType in documentTypesMatchAlgoAuto)
                {
                    if (classification_result != null)
                    {
                        if (classification_result.Contains(documentType.Name))
                        {
                            generated_response_ML = true;
                            document.DocumentTypeId = documentType.Id;

                            // List Cutsom Fields
                            List<CustomField> DocTypeCustomFiels = (List<CustomField>)await _customFieldRepository.GetAllAsync();
                            // init Document Custom Field
                            List<DocumentCustomField> list = new List<DocumentCustomField>();
                            // Iterate over each key-value pair in the JObject
                            foreach (var kvp in uniqueSubdirJson)
                            {
                                string key = kvp.Key;
                                string value = kvp.Value.ToString();
                                Console.WriteLine($"{key}: {value}");

                                bool keyExists = DocTypeCustomFiels.Any(customfield => key.Equals(customfield.Name));

                                if (keyExists)
                                {
                                    var customfield = DocTypeCustomFiels.First(cf => key.Equals(cf.Name));
                                    DocumentCustomField documentCustomField = new DocumentCustomField
                                    {
                                        DocumentId = document.Id,
                                        Document = document,
                                        CustomFieldId = customfield.Id,
                                        CustomField = customfield,
                                        Value = value
                                    };
                                    list.Add(documentCustomField);
                                }
                                else
                                {


                                    if (FileHelper.IsString(value))
                                        type = TypeField.STRING;
                                    else if (FileHelper.IsUrl(value))
                                        type = TypeField.URL;
                                    else if (FileHelper.IsDate(value))
                                        type = TypeField.DATE;
                                    else if (FileHelper.IsBoolean(value))
                                        type = TypeField.BOOLEAN;
                                    else if (FileHelper.IsInteger(value))
                                        type = TypeField.INTEGER;
                                    else if (FileHelper.IsFloat(value))
                                        type = TypeField.FLOAT;
                                    else if (FileHelper.IsMonetary(value))
                                        type = TypeField.MONETARY;
                                    else
                                        throw new ArgumentException("Unsupported value type");

                                    var customToAdd = CustomField.Create(key, type);
                                    await _customFieldRepository.AddAsync(customToAdd);

                                    DocumentCustomField documentCustomField = new DocumentCustomField
                                    {
                                        DocumentId = document.Id,
                                        Document = document,
                                        CustomFieldId = customToAdd.Id,
                                        CustomField = customToAdd,
                                        Value = value
                                    };
                                    list.Add(documentCustomField);
                                }
                            }
                            document.DocumentsCustomFields = list;

                            //// Convert each question to a string surrounded by double quotes
                            //List<string> quotedQuestions = questions.Select(q => $"\"{q}\"").ToList();

                            //// Combine quoted questions into a single string, separated by commas
                            //string questionsString = "[" + string.Join(",", quotedQuestions) + "]";
                            //string questionsString = string.Join(",", questions);

                            //Console.WriteLine(questionsString);
                            //bring the ML Data Extraction result 

                            return documentType;
                        }
                    }
                    else
                    {
                        break;
                    }
                }



            }


            if (generated_response_ML == false)
            {
                foreach (DocumentTypeDetailsDTO documentType in documentTypes)
                {
                    if (documentType.Matching_algorithm == Matching_Algorithms.MATCH_ANY)
                    {
                        if (documentType.Is_insensitive == true)
                        {
                            if (documentType.Match != null)
                            {
                                var lowerCaseMatch = documentType.Match.Select(x => x.ToLower()).ToList();
                                bool allConditionsMet = lowerCaseMatch.Any(x => document.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {
                                    return documentType;
                                }
                            }
                        }
                        else
                        {
                            if (documentType.Match != null)
                            {
                                bool allConditionsMet = documentType.Match.Any(x => document.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {
                                    return documentType;
                                }
                            }
                        }
                    }
                    if (documentType.Matching_algorithm == Matching_Algorithms.MATCH_ALL)
                    {
                        if (documentType.Is_insensitive == true)
                        {
                            if (documentType.Match != null)
                            {
                                var lowerCaseMatch = documentType.Match.Select(x => x.ToLower()).ToList();
                                bool allConditionsMet = lowerCaseMatch.All(x => document.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {
                                    return documentType;
                                }
                            }
                        }
                        else
                        {
                            if (documentType.Match != null)
                            {
                                bool allConditionsMet = documentType.Match.All(x => document.Content.Contains(x));
                                Console.WriteLine(allConditionsMet);
                                if (allConditionsMet)
                                {
                                    return documentType;
                                }
                            }
                        }
                    }
                    if (documentType.Matching_algorithm == Matching_Algorithms.MATCH_LITERAL)
                    {
                        if (documentType.Is_insensitive == true)
                        {
                            if (documentType.Match != null)
                            {
                                var lowerCaseMatch = documentType.Match.Select(x => x.ToLower()).ToList();
                                if (lowerCaseMatch.Count() == 1 && document.Content.Contains(lowerCaseMatch[0]))
                                {
                                    return documentType;
                                }
                            }
                        }
                        else
                        {
                            if (documentType.Match != null)
                            {
                                if (documentType.Match.Count() == 1 && document.Content.Contains(documentType.Match[0]))
                                {
                                    return documentType;
                                }
                            }
                        }
                    }
                    if (documentType.Matching_algorithm == Matching_Algorithms.MATCH_REGEX)
                    {
                        if (documentType.Is_insensitive == true)
                        {
                            if (documentType.Match != null)
                            {
                                var lowerCaseMatch = documentType.Match.Select(x => x.ToLower()).ToList();
                                foreach (string regexPattern in lowerCaseMatch)
                                {
                                    if (Regex.IsMatch(document.Content, regexPattern))
                                    {
                                        return documentType;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (documentType.Match != null)
                            {
                                if (documentType.Match.Count() == 1 && document.Content.Contains(documentType.Match[0]))
                                {
                                    return documentType;
                                }
                            }
                        }
                    }
                    int threshold = 90;
                    if (documentType.Matching_algorithm == Matching_Algorithms.MATCH_FUZZY)
                    {
                        if (documentType.Is_insensitive == true)
                        {
                            if (documentType.Match != null)
                            {
                                var lowerCaseMatch = documentType.Match.Select(x => x.ToLower()).ToList();
                                foreach (string matchWord in lowerCaseMatch)
                                {
                                    if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= threshold)
                                    {
                                        return documentType;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (documentType.Match != null)
                            {
                                foreach (string matchWord in documentType.Match)
                                {
                                    if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= threshold)
                                    {
                                        return documentType;
                                    }
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
