using Application.RestApiMail.Dto;
using System.Text.Json;


namespace Application.RestApiMail.EndPoints
{
    public class MailRulesList
    {

        public async static Task<List<MailRule>> CallRestApi()
        {
            string apiUrl = "http://localhost:5297/mailrule/list_mailrule";
            List<MailRule> mailrules = new List<MailRule>();
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

                        // Create a new TagsDto object and add it to the list
                        MailRule mailrule = new MailRule
                        {

                            Name = result.GetProperty("name").GetString(),

                        };

                        mailrules.Add(mailrule);
                    }
                    return mailrules;
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                }

            }

            // Return null if there's an error
            return null;
        }
    }
}
