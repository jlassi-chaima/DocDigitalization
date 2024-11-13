
using Application.Helpers;
using Domain.Ports;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Infrastructure.Adapters
{
    public class CorrespondentAdapter : ICorrespondentPort
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _createCorrespondents;
        private readonly string _getCorrespondents;


        public CorrespondentAdapter(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _createCorrespondents = configuration.GetSection("CorrespondentsApi:CreateCorrespondents").Value!;
            _getCorrespondents = configuration.GetSection("CorrespondentsApi:GetCorrespondents").Value!;
        }
        public async Task<HttpResponseMessage> AddCorrespondent(Domain.Correspondents.CreateCorrespondent request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CorrespondentsClient");

                Log.Information($"Request URL: {client.BaseAddress}{_createCorrespondents}");

                var response = await client.PostAsync($"{_createCorrespondents}", CasingHelper.FormatRequestCasing(request));
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
        public async Task<HttpResponseMessage> GetCorrespondents(string idOwner)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CorrespondentsClient");

                Log.Information($"Request URL: {client.BaseAddress}{_getCorrespondents}");

                var response = await client.GetAsync($"{_getCorrespondents}?id={idOwner}");
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
