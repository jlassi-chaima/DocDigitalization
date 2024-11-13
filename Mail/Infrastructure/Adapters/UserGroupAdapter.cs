using Domain.Ports;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Infrastructure.Adapters
{
    public class UserGroupAdapter: IUserGroupPort
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _firstGroupForUser;
        private readonly string _baseUrl;


        public UserGroupAdapter(IHttpClientFactory httpClientFactory, IConfiguration configuration) {
            _httpClientFactory=httpClientFactory;
            _firstGroupForUser = configuration.GetSection("UserGroupApi:FirstGRoupForUser").Value!;
            _baseUrl = configuration.GetSection("UserGroupApi:BaseUrl").Value!;


        }
        public async Task<HttpResponseMessage> GetFirstGRoupForUser(string idOwner)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("UserGroupClient");
              

               
                Log.Information($"Request URL: {client.BaseAddress}{_firstGroupForUser}{idOwner}");

                var response = await client.GetAsync($"{_firstGroupForUser}?id={idOwner}");
                if (!response.IsSuccessStatusCode)
                {
                    Log.Error($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    throw new HttpRequestException($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
                return response;
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex.ToString());
                throw new HttpRequestException(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw new Exception($"Exception: {ex.Message}");
            }
        }
    }
}
