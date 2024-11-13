using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Text;

namespace Application.Helpers
{
    public static class CasingHelper
    {
        public static StringContent FormatRequestCasing(object request)
        {
            try
            {
                var jsonSettings = new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    }
                };

                var jsonContent = JsonConvert.SerializeObject(request, jsonSettings);
                return new StringContent(jsonContent, Encoding.UTF8, "application/json");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
