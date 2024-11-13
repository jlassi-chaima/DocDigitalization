
using Application.Dtos.Documents;
using Application.Helper;
using Aspose.Pdf.Operators;
using Domain.Documents;
using Domain.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Nest;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Net.Http.Headers;
using File = System.IO.File;

namespace Infrastructure.Adapters
{
    public class GraphApiAdapter : IGraphApiPort
    {

        private readonly string _siteUrl;
        private readonly SharePointOptions _sharePointOptions;
        private readonly GraphServiceClient _graphClient;
        private readonly ILogger<GraphApiAdapter> _logger;
        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _authority;
        private readonly string _scope;

        public GraphApiAdapter(
            IConfiguration configuration,
            GraphServiceClient graphClient,
            IOptions<SharePointOptions> sharePointOptions,
            ILogger<GraphApiAdapter> logger
        )
        {
            _siteUrl = configuration.GetSection("SharepointConfiguration:URL").Value!;
            _graphClient = graphClient;
            _sharePointOptions = sharePointOptions.Value;
            _logger = logger;
            _tenantId = configuration.GetSection("AzureAd:TenantId").Value!;
            _clientId = configuration.GetSection("AzureAd:ClientId").Value!;
            _clientSecret = configuration.GetSection("AzureAd:ClientSecret").Value!;
            _authority = string.Format(
            configuration.GetSection("AzureAd:Authority").Value!,
            _tenantId);
            _scope = configuration.GetSection("AzureAd:Scope").Value!;
        }

        public async Task AddDocumentToList(Document doc)
        {
            try
            {


                string token = await GetAccessToken();
                // get Site Id (Intranet)
                var siteId = await GetSiteIdAsync(
                    _sharePointOptions.Hostname,
                    _sharePointOptions.SitePath,
                    token
                );
                //Get list of sit (docPaperless)
                ListResponseDto listResponse = await getListWebUrl(siteId, _sharePointOptions.ListName, token);

                string listId = listResponse.id;
                string webUrl = listResponse.webUrl;
              
                // Add data to column
                var listItem = new ListItem
                {
                    Fields = new FieldValueSet
                    {
                        AdditionalData = new Dictionary<string, dynamic>
                    {

                        { "DocumentType", doc?.Document_Type?.Name ?? "Unknown" },
                        { "Correspondent", doc?.Correspondent?.Name ?? "Unknown"},
                        { "StoragePath", doc?.StoragePath?.Name ?? "Unknown"},
                       { "Tags", string.Join(Environment.NewLine, doc.Tags?.Select(t => t.Tag?.Name) ?? Enumerable.Empty<string>()) },
                        { "Date", DateTime.Now },
                    }
                    }
                };

                var drive = await _graphClient.Sites[siteId].Lists[listId].Drive.Request().GetAsync();

                //byte[] fileBytes = FileProccess.Base64ToByteArray(doc.Base64Data);
                //using (var stream = FileProccess.ByteArrayToMemoryStream(fileBytes))
                //{
                using (Stream stream = File.OpenRead(doc.FileData))
                {
                    //    await document.File.CopyToAsync(stream);
                    //    stream.Position = 0; // Reset the stream position to the beginning
                    //var streamContent = new StreamContent(stream);
                    //streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
                    // create place for file
                    stream.Position = 0;

                    
                    string contentType=FileProccess.GetContentTypeByExtension(doc.MimeType);
                    string fileName = doc.Title + Path.GetExtension(doc.FileData);
                    var driveItem = await _graphClient
                        .Drives[drive.Id]
                        .Root.ItemWithPath(fileName)
                        .Content.Request()
                        .PutAsync<DriveItem>(stream);
                    // uplaod 



                    var uploadedDriveItem = await _graphClient
                        .Drives[drive.Id]
                        .Items[driveItem.Id]
                        .Request()
                        .Expand("listItem")
                        .GetAsync();

                    if (uploadedDriveItem.ListItem != null)
                    {
                        var listItemId = uploadedDriveItem.ListItem.Id;

                        await _graphClient
                            .Sites[siteId]
                            .Lists[listId]
                            .Items[listItemId]
                            .Fields
                            .Request()
                            .UpdateAsync(listItem.Fields);
                    }
                    else
                    {
                        throw new Exception("Unable to retrieve the ListItem associated with the uploaded file.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private async Task<string> GetSiteIdAsync(
            string hostname,
        string sitePath,
            string accesToken
        )
        {
            try
            {
                string key = "Authorization";
                string value = $"Bearer {accesToken}";
                HeaderOption header = new HeaderOption(key, value);
                var request = _graphClient.Sites.GetByPath(sitePath, hostname).Request();

                request.Headers.Add(header);
                var site = await request.GetAsync();
                return site.Id;
            }
            
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
    
              }
}   

        public async Task<ListResponseDto> getListWebUrl(
            string siteId,
            string listName,
            string accessToken
        )
        {
            try
            {
                using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json")
                );
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken
                );

                var lists = await _graphClient
                    .Sites[siteId]
                    .Lists.Request()
                    .Filter($"displayName eq '{listName}'")
                    .GetAsync();

                var list = lists.CurrentPage.FirstOrDefault();

                return new ListResponseDto { id = list.Id, webUrl = list.WebUrl };
            }
            }

            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);

            }
        }

        public async Task<string> GetAccessToken()
        {
            try
            {
                string tokenEndpoint = $"{_authority}/oauth2/v2.0/token";

            using (HttpClient client = new HttpClient())
            {
                var postData = new FormUrlEncodedContent(
                    new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "client_credentials"),
                        new KeyValuePair<string, string>("client_id", _clientId),
                        new KeyValuePair<string, string>("client_secret", _clientSecret),
                        new KeyValuePair<string, string>("scope", _scope)
                    }
                );

                HttpResponseMessage response = await client.PostAsync(tokenEndpoint, postData);
                string json = await response.Content.ReadAsStringAsync();
                var token = JObject.Parse(json)["access_token"].ToString();
                return token;
                }
           }

            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);

            }
        }

        private async Task<string> GetSharePointLists(string accessToken)
        {
            try
            {
                string requestUri = $"{_siteUrl}/_api/web/lists";

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json")
                    );
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        accessToken
                    );

                    HttpResponseMessage response = await client.GetAsync(requestUri);
                    string json = await response.Content.ReadAsStringAsync();
                    var lists = JObject.Parse(json)["d"]["results"].ToString();

                    return lists;
                }
          }

        catch (Exception ex)
        {
            Log.Error(ex.Message);
            throw new Exception(ex.Message);

            }
         }
        
    }
}
