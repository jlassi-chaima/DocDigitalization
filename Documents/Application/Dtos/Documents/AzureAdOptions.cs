namespace Application.Dtos.Documents
{
    public class AzureAdOptions
    {
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Authority { get; set; }
    }

    public class SharePointOptions
    {
        public string Hostname { get; set; }
        public string SitePath { get; set; }
        public string ListName { get; set; }

    }
}
