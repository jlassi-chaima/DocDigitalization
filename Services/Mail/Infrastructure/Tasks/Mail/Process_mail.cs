using Application.Consumers.RestAPIDocuments.Dtos;
using Application.Consumers.RestAPIDocuments.Endpoints;
using Application.Features;
using Application.Features.Helper.ProcessorHelper;
using Application.Repository;
using Application.Services;
using Domain.Correspondents;
using Domain.MailAccounts;
using Domain.MailRules;
using Domain.MailRules.Enum;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using MimeKit;
using MimeKit.Cryptography;
using Newtonsoft.Json;
using Npgsql;
using Org.BouncyCastle.Crypto;
using Quartz;
using Serilog;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;



namespace Infrastructure.Tasks.Mail
{
    public class Process_mail : IJob
    {
        private readonly IMailAccountRepository _mailAccountRepository;
        private readonly IMailRuleRepository _mailRuleRepository;
        private readonly ImapClient _client;
        private readonly IConfiguration _configuration;
        private readonly ICorrespondentService _correspondentService;
        private static readonly string[] Scopes = new string[] {

              "https://outlook.office.com/.default"
        };
       
        private readonly string _clientId;
        private readonly string _tenantId;
        private readonly string _clientSecret;


        //int total_new_documents = 0;
        public Process_mail(IMailAccountRepository mailAccountRepository,
            IMailRuleRepository mailRuleRepository,
            ImapClient client,
            IConfiguration configuration,
            ICorrespondentService correspondentService)
        {
            _clientId = configuration.GetSection("IMAPConfiguration:ClientId").Value!;
            _tenantId = configuration.GetSection("IMAPConfiguration:TenantId").Value!;
            _clientSecret = configuration.GetSection("IMAPConfiguration:ClientSecretValue").Value!;

            _mailAccountRepository = mailAccountRepository;
            _mailRuleRepository = mailRuleRepository;
            _configuration = configuration;
            _client = client;
            _correspondentService = correspondentService;
        }

        async Task IJob.Execute(IJobExecutionContext context)
        {
            try
            {
                List<MailAccount> mailAccounts = (List<MailAccount>)await _mailAccountRepository.GetAllAsync();

                foreach (var mail in mailAccounts)
                {

                    
                    using (var client = new ImapClient(new ProtocolLogger("imap_log.txt")))
                    {
                        // Step 1: Connect to the IMAP server
                        await ConnectToMailServer(client, mail);

                        // Step 2: Authenticate the user
                        await AuthenticateUser(client, mail);
                        if (client.IsConnected && client.IsAuthenticated)
                        {
                            Console.WriteLine("Authenticated successfully for: " + mail.Username);
                        }
                        else
                        {
                            Console.WriteLine("Failed to authenticate for: " + mail.Username);
                        }
                        // Step 3: Process unseen emails
                        await ProcessUnseenEmailsAsync(client, mail);

                        // Step 4: Disconnect from the mail server
                        client.Disconnect(true);
                    }
                }

                Console.WriteLine("Finished processing all mail accounts.");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }
        public async Task ConnectWithOauth2()
        {

            var cca = ConfidentialClientApplicationBuilder
                .Create(_clientId)
                .WithClientSecret(_clientSecret)
                .WithTenantId(_tenantId)
                .Build();

            AuthenticationResult result = await cca.AcquireTokenForClient(Scopes).ExecuteAsync();
            string accessToken = result.AccessToken;

            var oauth2 = new SaslMechanismOAuth2("chaima.jlassi@consultim-it.com"
                , result.AccessToken);
            //using (var client = new ImapClient())
            //{
            //    await ConnectToMailServerAsync(client, mail);
            //    await client.AuthenticateAsync(oauth2);
            //    await ProcessUnseenEmailsAsync(client, mail);
            //    await client.DisconnectAsync(true);
            //}
        }

        // Method to connect to the mail server
        private async Task ConnectToMailServer(ImapClient client, MailAccount mail)
        {
            if (mail.IMAP_Security == Imap_Security.SSL)
            {
                await client.ConnectAsync(mail.IMAP_Server, mail.IMAP_Port, SecureSocketOptions.SslOnConnect);
            }
            else if (mail.IMAP_Security == Imap_Security.STARTTLS)
            {
                await client.ConnectAsync(mail.IMAP_Server, mail.IMAP_Port, SecureSocketOptions.StartTls);
            }
            else
            {
                await client.ConnectAsync(mail.IMAP_Server, mail.IMAP_Port, SecureSocketOptions.None);
            }


        }
        private async Task AuthenticateUser(ImapClient client, MailAccount mail)
        {
            try
            {

              
                var passwordDecrypt = CryptoHelper.DecodeFrom64(mail.Password);
                await client.AuthenticateAsync(mail.Username, passwordDecrypt);
                //var authString = $"\0{mail.Username}\0{mail.Password}";
                //var authBytes = Encoding.UTF8.GetBytes(authString);
                //var base64Auth = Convert.ToBase64String(authBytes);
                // client.Authenticate(mail.Username, passwordDecrypt);


            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }
        private async Task ProcessUnseenEmailsAsync(ImapClient client, MailAccount mail)
        {
            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadWrite);

            // List UIDs of Not Seen emails
            var uids = inbox.Search(SearchQuery.NotSeen);
            int totalNewDocuments = uids.Count();

            foreach (var uid in uids)
            {
                var message = inbox.GetMessage(uid);
                await SearchMailRuleAsync(message, uid, inbox, mail);
            }

            Console.WriteLine($"Processed {totalNewDocuments} new emails for account: {mail.Username}");
        }

        public string GetBaseDestinationPath()
        {
            return _configuration["OriginalsSettings:OutputFolder"];
        }
        public async Task SearchMailRuleAsync(MimeMessage message, UniqueId uid, IMailFolder inbox, MailAccount mailAccount)
        {
            List<MailRule> mailRules = await _mailRuleRepository.GetAllByOrderAsync();
            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(15)
            };
            // create Directory
            string directorypath = GetBaseDestinationPath();
            Directory.CreateDirectory(directorypath);
            string title = "";
            string content = "";
            // init list Documents to Send
            List<Document> documents = new List<Document>();

            foreach (MailRule mailRule in mailRules)
            {
                //check if mailrule is applied to certain account
                if (mailRule.Account.Id == mailAccount.Id)
                {
                    //mail content respecting filters inside of a mailrule
                    bool respectsRule = RespectsRule(message, mailRule);

                    if (respectsRule)
                    {
                        Console.WriteLine("The message respects the rule.");
                        if (IsWithinMaximumAge(message, mailRule))
                        {
                            // Move to the next mail rule if the current one didn't match
                            Guid? corr_id = await AttachementTypesAndConsumptionScopes(message, directorypath, mailRule, title, documents, uid, content);

                            Console.WriteLine(inbox.ToString());
                            if (mailRule.Action == MailAction.MarkRead)
                            {
                                try
                                {
                                    inbox.AddFlags(uid, MessageFlags.Seen, true);
                                }
                                catch (ObjectDisposedException ex)
                                {
                                    // Handle the exception, such as logging it or taking appropriate action.
                                    Console.WriteLine($"An error occurred while trying to mark the message as seen: {ex.Message}");
                                }
                            }
                            else if (mailRule.Action == MailAction.Delete)
                            {

                                await inbox.AddFlagsAsync(uid, MessageFlags.Deleted, true);
                            }
                            else if (mailRule.Action == MailAction.Flag)
                            {

                                await inbox.AddFlagsAsync(uid, MessageFlags.Flagged, true);
                            }
                            else if (mailRule.Action == MailAction.Tag)
                            {
                                if (mailAccount.IMAP_Server == "imap.gmail.com")
                                {
                                    // Assuming mail.Action_parameter is the label name
                                    var labelName = mailRule.Action_parameter;

                                    // Check if the label already exists
                                    var existingFolder = inbox.GetSubfolders(false)
                                                                .FirstOrDefault(folder => folder.Name == labelName);

                                    if (existingFolder == null)
                                    {
                                        // Create a new folder with the label name
                                        existingFolder = inbox.Create(labelName, true);
                                        // Add the Keyword flag to the new folder
                                        inbox.AddLabels(uid, new List<string> { labelName }, true);
                                    }
                                    else
                                    {

                                        // Add the keyword to the processed mail
                                        inbox.AddLabels(uid, new List<string> { labelName }, true);
                                    }
                                }

                            }
                            else if (mailRule.Action == MailAction.Move)
                            {

                                var folderName = mailRule.Action_parameter ?? "Documents";

                                // Check if the folder exists
                                var folder = inbox.GetSubfolders(false).FirstOrDefault(f => f.Name == folderName);

                                if (folder == null)
                                {
                                    // Create the folder if it doesn't exist
                                    folder = inbox.Create(folderName, true);
                                }

                                // Move the message to the new folder
                                await inbox.MoveToAsync(uid, folder);
                                Console.WriteLine($"Message moved to folder: {folderName}");
                            }
                        }

                        else
                        {
                            continue;
                        }

                    }
                    else
                    {
                        Console.WriteLine("The message does not respect the rule.");
                        continue;
                    }

                    //if (!RespectsRule(message, mailRule))
                    //    continue; // Move to the next mail rule if the current one didn't match
                }
             
            }


            // send documents to DocumentService
            foreach (Document doc in documents)
            {
                try
                {

                    //var response = await httpClient.PostAsJsonAsync("http://localhost:5046/document", doc);

                    var formData = new MultipartFormDataContent();

                    //Open the file stream from the provided file path
                    using (var fileStream = File.OpenRead(doc.FileData))
                    {
                        var streamContent = new StreamContent(fileStream);
                        string contentType = GetContentTypeByExtension(Path.GetExtension(doc.FileData));
                        // Set the content type (e.g., application/pdf, image/jpeg)
                        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                        // Extract file name from the file path
                        var fileName = Path.GetFileName(doc.FileData);

                        // Add the file content to the MultipartFormDataContent
                        formData.Add(streamContent, "formData", fileName);
                        var jsonDoc = JsonConvert.SerializeObject(doc);
                        var jsonContent = new StringContent(jsonDoc, Encoding.UTF8, "application/json");

                        // Add the document JSON content to the form data
                        formData.Add(jsonContent, "document");
                        using (var client = new HttpClient
                        {
                            Timeout = TimeSpan.FromMinutes(5) // Set timeout to 5 minutes
                        })
                        {

                            client.DefaultRequestHeaders.Add("Type", "Mail");

                            var response = await client.PostAsync($"http://localhost:5046/document/Upload?id={doc.Owner}", formData);
                            var corresp_content = response.Content.ReadAsStringAsync();
                            if (response.IsSuccessStatusCode)
                            {
                                //usedfiles.Add(file.FullName);
                                Console.WriteLine("File uploaded successfully.");
                            }
                            else
                            {
                                Console.WriteLine($"Failed to upload file. Status code: {response.StatusCode}");
                            }
                            //var response = await httpClient.PostAsJsonAsync("http://localhost:5046/document", doc);
                            //string corresp_content = await response.Content.ReadAsStringAsync();
                        }
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }


        }
        private static string GetContentTypeByExtension(string extension)
        {
            switch (extension.ToLower())
            {
                case ".pdf":
                    return "application/pdf";
                case ".png":
                    return "image/png";
                case ".jpg":
                    return "image/jpeg";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".pptx":
                    return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case ".txt":
                    return "text/plain";
                default:
                    return "application/octet-stream"; // Default for unknown types
            }
        }


        private bool RespectsRule(MimeMessage message, MailRule mailRule)
        {
            bool respectsRule = false; // Assume rule is respected initially
            if (mailRule.Filter_body == null && mailRule.Filter_to == null && mailRule.Filter_subject == null && mailRule.Filter_from == null)
            {
                respectsRule = true;
            }
            if (mailRule.Filter_from != null)
            {
                // Filter_from is specified and if the message sender matches
                respectsRule = message.From.Any(sender => sender.ToString().Contains(mailRule.Filter_from));

            }
            if (mailRule.Filter_to != null)
            {
                // Check if Filter_to is specified and if the message recipient matches
                respectsRule = message.From.Any(sender => sender.ToString().Contains(mailRule.Filter_to));
            }
            if (mailRule.Filter_subject != null)
            {
                // Check if Filter_subject is specified and if the message subject matches
                respectsRule = message.Subject.Contains(mailRule.Filter_subject);
            }
            if (mailRule.Filter_body != null)
            {
                // Check if Filter_body is specified and if the message body contains the specified text
                respectsRule = message.TextBody.Contains(mailRule.Filter_body);
            }

            return respectsRule;
        }
        private bool IsWithinMaximumAge(MimeMessage message, MailRule mailRule)
        {
            // Assuming Maximum_age is represented in days
            int maximumAgeDays = (int)mailRule.Maximum_age;
            DateTime deliveredAfterDate = DateTime.Today.AddDays(-maximumAgeDays);

            // Check if the message date is after the deliveredAfterDate
            return message.Date > deliveredAfterDate;
        }
        public async Task<Guid?> AttachementTypesAndConsumptionScopes(MimeMessage message, string directorypath, MailRule mail, string title, List<Document> documents, UniqueId uid, string content)
        {
            try
            {

                    var httpClient = new HttpClient
                    {
                        Timeout = TimeSpan.FromMinutes(15)
                    };
                    Guid? corrId = Guid.Empty;
                    string tessDataPath = _configuration["TesseractPath:tessdatafile"];
                    string baseDestinationPath = GetBaseDestinationPath();

                    Match match = IsMatchName(message.From.ToString());

                    // Extract the name and email
                    string name = match.Groups["name"].Value.Trim();
                    string email = match.Groups["email"].Value.Trim();

                    List<string> listEmail = new List<string>();
                    List<string> listName = new List<string>();

                    if (!string.IsNullOrEmpty(email))
                    {
                        listEmail.Add(email);
                        if (!string.IsNullOrEmpty(name))
                        {
                            listName.Add(name);
                        }
                    }
                    //Coresspondent Form 
                    var createCorrespondent = await _correspondentService.GenerateCorrespondentFromMetadata(mail.Owner, mail.Assign_correspondent_from, name, email, mail, listName, listEmail);
                
                     var corr=await  _correspondentService.CreateCorrespondent(createCorrespondent);
                    mail.Assign_correspondent = corrId = corr?.Id;
                   


                    if (mail.Attachment_type == MailFilterAttachmentType.Attachments && mail.Consumption_scope == MailRuleConsumptionScope.Attachments) // Process attachments only if attachment_type is 1 and consumption_scope is 1
                    {
                        ProcessorOnlyAttachementHelper.save_pdf_and_images_for_online_attachement(message, directorypath, mail, title, documents, tessDataPath, corrId ?? Guid.Empty, httpClient);
                    }
                    else if (mail.Attachment_type == MailFilterAttachmentType.Attachments && mail.Consumption_scope == MailRuleConsumptionScope.Email_Only)// Process email body only if consumption_scope is 2
                    {
                        ProcessorOnlyAttachementHelper.save_eml_files_for_online_attachement(message, directorypath, mail, title, documents, uid, corrId ?? Guid.Empty, httpClient);// Check if the email meets the condition: no attachments but with inline attachments
                    }
                    else if (mail.Attachment_type == MailFilterAttachmentType.Attachments && mail.Consumption_scope == MailRuleConsumptionScope.Everything)
                    {
                        ProcessorOnlyAttachementHelper.save_pdf_and_images_for_online_attachement(message, directorypath, mail, title, documents, tessDataPath, corrId ?? Guid.Empty, httpClient);
                        ProcessorOnlyAttachementHelper.save_eml_files_for_online_attachement(message, directorypath, mail, title, documents, uid, corrId ?? Guid.Empty, httpClient);
                    }
                    else if (mail.Attachment_type == MailFilterAttachmentType.Everything && mail.Consumption_scope == MailRuleConsumptionScope.Attachments)// Attachment Type : Everything*************ConsumptionScope : Attchaments
                    {
                        ProcessorInlineAttachementHelper.save_pdf_and_images_for_online_attachement_including_inline(message, directorypath, mail, title, documents, content, tessDataPath, corrId ?? Guid.Empty, httpClient);
                    }
                    else if (mail.Attachment_type == MailFilterAttachmentType.Everything && mail.Consumption_scope == MailRuleConsumptionScope.Email_Only)// Attachment Type : Everything*************ConsumptionScope : Email_Only
                    {
                        ProcessorInlineAttachementHelper.save_eml_files_for_online_attachement_including_inline(message, directorypath, mail, title, documents, uid, corrId ?? Guid.Empty, httpClient);
                    }
                    else if (mail.Attachment_type == MailFilterAttachmentType.Everything && mail.Consumption_scope == MailRuleConsumptionScope.Everything)// Attachment Type : Everything*************ConsumptionScope : Everything
                    {
                        ProcessorInlineAttachementHelper.save_pdf_and_images_for_online_attachement_including_inline(message, directorypath, mail, title, documents, content, tessDataPath, corrId ?? Guid.Empty, httpClient);
                        ProcessorInlineAttachementHelper.save_eml_files_for_online_attachement_including_inline(message, directorypath, mail, title, documents, uid, corrId ?? Guid.Empty, httpClient);
                    }
                    return corrId;
            }
            catch (ArgumentException ex)
            {
                throw new Exception("Invalid MailMetadataCorrespondentOption");

            }
            catch (Exception ex)
            {
                Log.Error($"Error in SetCorrespondentFromAssignCorrespondentFrom: {ex.Message}");
                throw;
            }

        }
        public Match IsMatchName(string message)
        {
            string pattern = @"^(?:""(?<name>[^""]*)"")?\s*<(?<email>[^<>]+)>|^(?<email>[^<>]+)$";
            return Regex.Match(message, pattern);
        }
    }

}

//try
//{


//    List<MailAccount> mailAccounts = (List<MailAccount>)await _mailAccountRepository.GetAllAsync();

//    foreach (var mail in mailAccounts)
//    {
//        using (var client = new ImapClient(new ProtocolLogger("imap_log.txt")))
//        {
//            if (mail.IMAP_Security == Imap_Security.SSL)
//            {
//                client.Connect(mail.IMAP_Server, mail.IMAP_Port, SecureSocketOptions.SslOnConnect);
//            }
//            else if (mail.IMAP_Security == Imap_Security.STARTTLS)
//            {
//                client.Connect(mail.IMAP_Server, mail.IMAP_Port, SecureSocketOptions.StartTls);
//            }
//            else
//            {
//                client.Connect(mail.IMAP_Server, mail.IMAP_Port, SecureSocketOptions.None);
//            }

//            var passwordDecrypt = CryptoHelper.DecodeFrom64(mail.Password);
//            client.Authenticate(mail.Username, passwordDecrypt);

//            Console.WriteLine("Connected to the mail server successfully.");
//            var inbox = client.Inbox;
//            inbox.Open(FolderAccess.ReadWrite);
//            //List uids Not Seen
//            var uids = inbox.Search(SearchQuery.NotSeen);

//            total_new_documents = uids.Count();

//            foreach (var uid in uids)
//            {
//                var message = inbox.GetMessage(uid);
//                await SearchMailRuleAsync(message, uid, inbox, mail);
//            }


//            // Reset the document count for the next mail account
//            total_new_documents = 0;

//        }
//    }
//    Console.WriteLine("Disconnected from the mail server.");
//}
//catch (Exception ex)
//{
//    Log.Error(ex.Message);
//    throw new Exception(ex.Message);
//}
//        }




// Corespondent
//if (mail.Assign_correspondent_from == MailMetadataCorrespondentOption.FromEmail)
//{

//    Correspondent result = await _correspondentService.CreateCorrespondent(new CreateCorrespondent
//    {
//        Name = email,
//        Slug = email,
//        Match = listEmail,
//        Matching_algorithm = Domain.Correspondents.Matching_Algorithms.MATCH_ANY,
//        Is_insensitive = false,
//        Owner = mail.Owner,
//        Document_count = 0,
//        Last_correspondence = DateTime.UtcNow
//    });



//    corrId = result.Id;
//    //corr_id = responseObject.Id;
//    Console.WriteLine("correspendent id : " + corrId);
//    //var response = await httpClient.PostAsJsonAsync("http://localhost:5046/correspondents/post_correspondent", correspondent);

//    //string corresp_content = await response.Content.ReadAsStringAsync();

//    //Console.WriteLine("the full content of the created correspondent : " + content);
//    //// Deserialize the JSON content
//    //var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Correspondent>(corresp_content);

//    // Access the ID

//}
//else if (mail.Assign_correspondent_from == MailMetadataCorrespondentOption.FromName)
//{
//    CorrespondentCreate correspondent = new CorrespondentCreate();
//    if (name != "")
//    {
//        correspondent = new CorrespondentCreate
//        {
//            Name = name,
//            Slug = name,
//            Match = listName,
//            Matching_algorithm = Application.Consumers.RestAPIDocuments.Dtos.Matching_Algorithms.MATCH_ANY,
//            Is_insensitive = false,
//            Owner = mail.Owner,
//            Document_count = 0,
//            Last_correspondence = DateTime.UtcNow
//        };
//    }
//    else
//    {
//        correspondent = new CorrespondentCreate
//        {
//            Name = email,
//            Slug = email,
//            Match = listEmail,
//            Matching_algorithm = Application.Consumers.RestAPIDocuments.Dtos.Matching_Algorithms.MATCH_ANY,
//            Is_insensitive = false,
//            Owner = mail.Owner,
//            Document_count = 0,
//            Last_correspondence = DateTime.UtcNow
//        };
//    }
//    var response = await httpClient.PostAsJsonAsync("http://localhost:5046/correspondents/post_correspondent", correspondent);
//    string corresp_content = await response.Content.ReadAsStringAsync();
//    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Correspondent>(corresp_content);
//    corrId = responseObject.Id;

//}
//else if (mail.Assign_correspondent_from == MailMetadataCorrespondentOption.FromCustom)
//{
//    // check if Assign_Correspondent exists
//    List<Correspondent> correspondents = CorrespondentList.CallRestApi(mail.Owner).Result;
//    if (mail.Assign_correspondent != null)
//    {
//        bool exists = correspondents.Any(correspondent => correspondent.Id == mail.Assign_correspondent);
//        if (exists)
//        {
//            corrId = mail.Assign_correspondent.Value;
//        }
//        Console.WriteLine(exists);

//    }
//}
//else if (mail.Assign_correspondent_from == MailMetadataCorrespondentOption.FromNothing)
//{
//    corrId = null;
//}