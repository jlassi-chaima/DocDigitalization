using Aspose.Pdf.Operators;
using Domain.Ports;
using Microsoft.Extensions.Configuration;
using Serilog;


namespace Infrastructure.Adapters
{
    public class UserGroupAdapter: IUserGroupPort
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _firstGroupForUser;
        private readonly string _groups;
        private readonly string _groupsId;




        public UserGroupAdapter(IHttpClientFactory httpClientFactory, IConfiguration configuration) {
            _httpClientFactory=httpClientFactory;
            _firstGroupForUser = configuration.GetSection("UserGroupApi:FirstGRoupForUser").Value!;
            _groups = configuration.GetSection("UserGroupApi:GroupsList").Value!;
            _groupsId = configuration.GetSection("UserGroupApi:GroupsId").Value!;




        }
        public async Task<HttpResponseMessage> GetFirstGroupForUser(string owner)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("UserClient");
              

               
                Log.Information($"Request URL: {client.BaseAddress}{_firstGroupForUser}{owner}");

                var response = await client.GetAsync($"{_firstGroupForUser}?id={owner}");
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
        public async Task<HttpResponseMessage> GetGroups()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("GroupClient");



                Log.Information($"Request URL: {client.BaseAddress}{_groups}");

                var response = await client.GetAsync($"{_groups}");
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
        public async Task<HttpResponseMessage> GetGroupsId(string owner)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("UserClient");



                Log.Information($"Request URL: {client.BaseAddress}{_groupsId}");

                var response = await client.GetAsync($"{_groupsId}?id={owner}");
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
