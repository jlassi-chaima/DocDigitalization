using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Core.Serializers;
using Newtonsoft.Json.Converters;

namespace Infrastructure.Serializers
{
    public class NewtonSoftService : ISerializerService
    {
        public T Deserialize<T>(string text)
        {
            //var options = new JsonSerializerOptions();
            //options.Converters.Add(new GuidConverter());
            return JsonConvert.DeserializeObject<T>(text);
        }

        public string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,

                Converters = new List<JsonConverter>
            {
                new StringEnumConverter(new CamelCaseNamingStrategy())
            }
            });
        }

        public string Serialize<T>(T obj, Type type)
        {
            return JsonConvert.SerializeObject(obj, type, new());
        }

    }


}
