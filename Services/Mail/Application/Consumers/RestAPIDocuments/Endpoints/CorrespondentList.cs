
using Application.Consumers.RestAPIDocuments.Dtos;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Application.Consumers.RestAPIDocuments.Endpoints
{

    public class CorrespondentList
    {
        public async static Task<List<Correspondent>> CallRestApi(string owner_id)
        {
            string apiUrl = "http://localhost:5046/correspondents/list_correspondent";
            List<Correspondent> correspondents = new List<Correspondent>();
            // Create query parameters
            var queryParams = new Dictionary<string, string>
            {
                { "owner", owner_id }
            };
            apiUrl += ToQueryString(queryParams);
            using (HttpClient client = new HttpClient())
            {

                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the JSON response into a dynamic object
                    using var jsonDocument = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

                    // Get the root element of the JSON document
                    var root = jsonDocument.RootElement;

                    // Get the "results" array from the root element
                    var results = root.GetProperty("results").EnumerateArray();

                    // Parse each result and map it to a TagDto object
                    foreach (var result in results)
                    {
                        // Deserialize the "match" array into a list of strings
                        var matchArrayProperty = result.GetProperty("match");
                        List<string> matchArray = matchArrayProperty.ValueKind == JsonValueKind.Null ?
                            new List<string>() :
                            matchArrayProperty.EnumerateArray().Select(x => x.GetString()).ToList();

                        // Parse the "matching_algorithm" integer to the Matching_Algorithms enum
                        var matchingAlgorithmValue = result.GetProperty("matching_algorithm").GetInt32();
                        var matchingAlgorithm = (Matching_Algorithms)matchingAlgorithmValue;

                        // Create a new TagsDto object and add it to the list
                        Correspondent correspondent = new Correspondent
                        {
                            Id = Guid.Parse(result.GetProperty("id").GetString()),
                            Name = result.GetProperty("name").GetString(),
                            Match = matchArray,
                            Matching_algorithm = matchingAlgorithm,
                            Is_insensitive = result.GetProperty("is_insensitive").GetBoolean(),
                            Owner = result.GetProperty("owner").GetString(),
                            Document_count = result.GetProperty("document_count").GetInt32()
                        };
                        // Print the properties including the "match" property
                        Console.WriteLine($" Match: {string.Join(",", matchArray)}, Matching Algorithm: {matchingAlgorithm}");
                        correspondents.Add(correspondent);
                    }
                    return correspondents;
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                }

            }

            // Return null if there's an error
            return null;
        }
        private static string ToQueryString(Dictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return "";

            var queryString = new StringBuilder("?");
            foreach (var kvp in parameters)
            {
                queryString.Append($"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}&");
            }

            // Remove the trailing '&'
            return queryString.ToString().TrimEnd('&');
        }
    }
}
