////using Application.Consumers.RestAPIDocuments.Dtos;
////using Application.Consumers.RestAPIDocuments.Endpoints;
////using Application.Dtos.MailRule;
////using Application.Repository;
////using Domain.MailAccounts;
////using Domain.MailRules;
////using Domain.MailRules.Enum;
////using MailKit;
////using MailKit.Net.Imap;
////using MailKit.Search;
////using MapsterMapper;
////using MediatR;
////using MimeKit;
////using System.Net.Http.Json;
////using Microsoft.Extensions.Configuration;
////using Application.Features.Helper.ProcessorHelper;
////using System.Threading;
////using MailKit.Net.Imap;
////using Tesseract;
////using System.Text.RegularExpressions;
////using System.Net.Http;
////using FluentValidation;
////using System.Data.SqlTypes;
////using static System.Net.WebRequestMethods;


////namespace Application.Features.FeaturesMailRule
////{
////    public class AddMailRule
////    {
////        public sealed record Command : IRequest<MailRule>
////        {
////            public readonly MailRuleDto MailRule;

////            public Command(MailRuleDto mailrule)
////            {
////                MailRule = mailrule;
////            }
////        }
////        public sealed class AddValidator : AbstractValidator<Command>
////        {
////            public AddValidator(IMailRuleRepository _repository)
////            {
////                RuleFor(mr => mr.MailRule.Name).NotEmpty().MustAsync(async (name, ct) => !await _repository.ExistsAsync(mr => mr.Name == name, ct))
////                                 .WithMessage("Name must be unique.");

////                RuleFor(mr => mr.MailRule.Order).NotEmpty().MustAsync(async (order, ct) => !await _repository.ExistsAsync(mr => mr.Order == order, ct))
////                                 .WithMessage("Order must be unique.");

////            }
////        }
////        public sealed class Handler : IRequestHandler<Command, MailRule>
////        {
////            private readonly IMailRuleRepository _mailRuleRepository;
////            private readonly ImapClient _client;
////            private readonly Mapper _mapper;
////            private readonly IMailAccountRepository _mailAccountRepository;
////            private readonly IConfiguration _configuration;

////            public Handler(ImapClient client, Mapper mapper, IMailRuleRepository mailRuleRepository, IMailAccountRepository mailAccountRepository, IConfiguration configuration)
////            {
////                _client = client;
////                _mapper = mapper;
////                _mailRuleRepository = mailRuleRepository;
////                _mailAccountRepository = mailAccountRepository;
////                _configuration = configuration;
////            }
////            public async Task<MailRule> Handle(Command request, CancellationToken cancellationToken)
////            {

////                // Check Filter "" 
////                string Filter_From;
////                if (request.MailRule.Filter_from == "")
////                {
////                    Filter_From = null;
////                }
////                else
////                {
////                    Filter_From = request.MailRule.Filter_from;
////                }
////                //check Filter To
////                string Filter_To;
////                if (request.MailRule.Filter_to == "")
////                {
////                    Filter_To = null;
////                }
////                else
////                {
////                    Filter_To = request.MailRule.Filter_to;
////                }
////                // chechk Filter Body
////                string Filter_Body;
////                if (request.MailRule.Filter_body == "")
////                {
////                    Filter_Body = null;
////                }
////                else
////                {
////                    Filter_Body = request.MailRule.Filter_body;
////                }
////                //check Filter Subject
////                string Filter_Subject;
////                if (request.MailRule.Filter_subject == "")
////                {
////                    Filter_Subject = null;
////                }
////                else
////                {
////                    Filter_Subject = request.MailRule.Filter_subject;
////                }


////                Guid? corr_id = null;

////                // Create a new MailRule instance
////                var mailRule = new MailRule
////                {
////                    Name = request.MailRule.Name,
////                    Folder = request.MailRule.Folder,
////                    Maximum_age = request.MailRule.Maximum_age,
////                    Attachment_type = request.MailRule.Attachment_type,
////                    Consumption_scope = request.MailRule.Consumption_scope,
////                    Order = request.MailRule.Order,
////                    Filter_from = Filter_From,
////                    Filter_to = Filter_To,
////                    Filter_subject = Filter_Subject,
////                    Filter_body = Filter_Body,
////                    Filter_attachment_filename = request.MailRule.Filter_attachment_filename,
////                    Action = request.MailRule.Action,
////                    Assign_title_from = request.MailRule.Assign_title_from,
////                    Action_parameter = request.MailRule.Action_parameter,
////                    Assign_tags = request.MailRule.Assign_tags,
////                    Assign_document_type = request.MailRule.Assign_document_type,
////                    Assign_correspondent_from = request.MailRule.Assign_correspondent_from,
////                    Assign_correspondent = request.MailRule.Assign_correspondent,
////                    Assign_owner_from_rule = request.MailRule.Assign_owner_from_rule
////                };

////                //getting the tags ids
////                List<TagsDto> tags = TagsList.CallRestApi().Result;
////                if (request.MailRule.Assign_tags != null)
////                {
////                    if (request.MailRule.Assign_tags.Count > 0)
////                    {
////                        foreach (var tagId in request.MailRule.Assign_tags)
////                        {
////                            bool exists = tags.Any(tag => tag.Id == tagId);
////                            if (exists)
////                            {
////                                mailRule.Assign_tags = request.MailRule.Assign_tags;
////                            }
////                            else
////                            {
////                                mailRule.Assign_tags = null;
////                            }
////                        }
////                    }
////                    else
////                    {
////                        mailRule.Assign_tags = null;
////                    }
////                }


////                //getting the documenttype id
////                List<DocumentType> documentTypes = DocumentTypeList.CallRestApi().Result;
////                if (request.MailRule.Assign_document_type != null)
////                {
////                    bool exists = documentTypes.Any(doctype => doctype.Id == request.MailRule.Assign_document_type);
////                    if (exists)
////                    {
////                        mailRule.Assign_document_type = request.MailRule.Assign_document_type;
////                    }
////                    else
////                    {
////                        mailRule.Assign_document_type = null;
////                    }

////                }

////                //getting the mail account 
////                MailAccount selected_mailaccount = await _mailAccountRepository.FindByIdAsync(request.MailRule.Account, cancellationToken);

////                mailRule.Account = selected_mailaccount;
////                //getting the corrspondent id
////                corr_id = await RetriveMail(selected_mailaccount, _client, mailRule);
////                if (corr_id == Guid.Empty)
////                {
////                    mailRule.Assign_correspondent = null;
////                }
////                else
////                {
////                    mailRule.Assign_correspondent = corr_id;
////                }


////                // Add the MailRule to the database
////                await _mailRuleRepository.AddAsync(mailRule, cancellationToken);
////                return mailRule;
////            }

////            private async Task<Guid?> RetriveMail(MailAccount mailAccount, ImapClient imapClient, MailRule mailrule)
////            {
////                Guid? corr_id = null;
////                if (mailAccount.IMAP_Security == Imap_Security.SSL || mailAccount.IMAP_Security == Imap_Security.STARTTLS)
////                    imapClient.Connect(mailAccount.IMAP_Server, mailAccount.IMAP_Port, true);
////                var passwrddecrypt = CryptoHelper.DecodeFrom64(mailAccount.Password);
////                imapClient.Authenticate(mailAccount.Username, passwrddecrypt);
////                Console.WriteLine("Connected to the mail server successfully.");
////                //var inbox = _client.Inbox;

////                var inbox = _client.GetFolder(mailrule.Folder);
////                //var Folder = imapClient.GetFolder(imapClient.PersonalNamespaces[0]);


////                inbox.Open(FolderAccess.ReadWrite);
////                // function to BuildSerchQuery Filter and Maximum Age
////                var combinedQuery = BuildSearchQuery(mailrule);
////                var uids = inbox.Search(combinedQuery);
////                // create Directory
////                string tessDataPath = _configuration["TesseractPath:tessdatafile"];
////                string baseDestinationPath = GetBaseDestinationPath();
////                Directory.CreateDirectory(baseDestinationPath);
////                // function send Documents to Service Document
////                corr_id = await SendDocumentsToService(uids.ToList(), inbox, mailrule, imapClient, baseDestinationPath, tessDataPath, mailAccount);
////                return corr_id;
////            }
////            private SearchQuery BuildSearchQuery(MailRule mailRule)
////            {
////                var query = SearchQuery.All;

////                // Filter by "From" address if provided
////                if (!string.IsNullOrEmpty(mailRule.Filter_from))
////                {
////                    query = query.And(SearchQuery.FromContains(mailRule.Filter_from));
////                }
////                // Filter by "To" address if provided
////                if (!string.IsNullOrEmpty(mailRule.Filter_to))
////                {
////                    query = query.And(SearchQuery.ToContains(mailRule.Filter_to));
////                }
////                // Filter by "Subject" if provided
////                if (!string.IsNullOrEmpty(mailRule.Filter_subject))
////                {
////                    query = query.And(SearchQuery.SubjectContains(mailRule.Filter_subject));
////                }
////                // Filter by "Body" if provided
////                if (!string.IsNullOrEmpty(mailRule.Filter_body))
////                {
////                    query = query.And(SearchQuery.BodyContains(mailRule.Filter_body));
////                    Console.WriteLine(query.ToString());
////                }


////                // Assuming Maximum_age is represented in days
////                var maximumAgeDays = mailRule.Maximum_age;
////                var deliveredAfterDate = DateTime.Today.AddDays(-maximumAgeDays);
////                Console.WriteLine(deliveredAfterDate.ToString());
////                // Add filter for "DeliveredAfter"
////                query = query.And(SearchQuery.DeliveredAfter(deliveredAfterDate));
////                return query;
////            }
////            // get path originals
////            public string GetBaseDestinationPath()
////            {

////                return _configuration["OriginalsSettings:OutputFolder"];
////            }
////            private async Task<Guid?> SendDocumentsToService(List<UniqueId> uids, IMailFolder inbox, MailRule mail, ImapClient imapClient, string directorypath, string tessDataPath, MailAccount mailAccount)
////            {
////                var httpClient = new HttpClient();
////                List<Document> documents = new List<Document>();
////                Guid? corr_id = null;
////                var messages = new List<MimeMessage>();
////                foreach (var uid in uids)
////                {

////                    var message = inbox.GetMessage(uid);
////                    messages.Add(message);
////                    string content = "";
////                    string title = "";
////                    //in the mail is not not read you can read it
////                    var summary = inbox.Fetch(new[] { uid }, MessageSummaryItems.Flags).FirstOrDefault();
////                    bool isRead = summary.Flags.HasValue && summary.Flags.Value.HasFlag(MessageFlags.Seen);
////                    bool isFlagged = summary.Flags.HasValue && summary.Flags.Value.HasFlag(MessageFlags.Flagged);
////                    Console.WriteLine($"Message UID: {uid}, IsRead: {isRead}");

////                    if (!isRead && mail.Action == MailAction.MarkRead)
////                    {
////                        corr_id = await attachement_types_and_consumption_scopes(message, directorypath, mail, title, documents, uid, content, tessDataPath);
////                        inbox.AddFlags(uid, MessageFlags.Seen, true);

////                    }
////                    else if (mail.Action == MailAction.Delete)
////                    {
////                        corr_id = await attachement_types_and_consumption_scopes(message, directorypath, mail, title, documents, uid, content, tessDataPath);
////                        inbox.AddFlags(uid, MessageFlags.Deleted, true);
////                    }
////                    else if (!isFlagged && mail.Action == MailAction.Flag)
////                    {
////                        corr_id = await attachement_types_and_consumption_scopes(message, directorypath, mail, title, documents, uid, content, tessDataPath);
////                        inbox.AddFlags(uid, MessageFlags.Flagged, true);
////                    }
////                    else if (mail.Action == MailAction.Move)
////                    {
////                        corr_id = await attachement_types_and_consumption_scopes(message, directorypath, mail, title, documents, uid, content, tessDataPath);
////                        // Assuming mail.Action_parameter is the folder name
////                        string destinationFolderName = mail.Action_parameter;
////                        // Get the root folder (usually INBOX)
////                        var Folder = imapClient.GetFolder(imapClient.PersonalNamespaces[0]);

////                        // Retrieve a list of all folders available on the server
////                        var listFolder = Folder.GetSubfolders();
////                        bool destinationFolderExists = listFolder.Any(folder => folder.FullName == destinationFolderName);
////                        if (destinationFolderName != null && !destinationFolderExists)
////                        {
////                            // Get or create the destination folder
////                            var rootFolder = imapClient.GetFolder("");
////                            IMailFolder newFolder = rootFolder.Create(destinationFolderName, true);
////                            inbox.MoveTo(uid, newFolder);
////                        }
////                        else if (destinationFolderExists)
////                        {
////                            var destinationFolder = imapClient.GetFolder(destinationFolderName);
////                            inbox.MoveTo(uid, destinationFolder);
////                        }
////                    }
////                    else if (mail.Action == MailAction.Tag)
////                    {
////                        corr_id = await attachement_types_and_consumption_scopes(message, directorypath, mail, title, documents, uid, content, tessDataPath);
////                        if (mailAccount.IMAP_Server == "imap.gmail.com")
////                        {
////                            // Assuming mail.Action_parameter is the label name
////                            var labelName = mail.Action_parameter;

////                            // Check if the label already exists
////                            var existingFolder = inbox.GetSubfolders(false)
////                                                       .FirstOrDefault(folder => folder.Name == labelName);

////                            if (existingFolder == null)
////                            {
////                                // Create a new folder with the label name
////                                existingFolder = inbox.Create(labelName, true);
////                                // Add the Keyword flag to the new folder
////                                inbox.AddLabels(uid, new List<string> { labelName }, true);
////                            }
////                            else
////                            {

////                                // Add the keyword to the processed mail
////                                inbox.AddLabels(uid, new List<string> { labelName }, true);
////                            }
////                        }

////                    }

////                }
////                // Disconnected from the mail server
////                imapClient.Disconnect(true);
////                Console.WriteLine("Disconnected from the mail server.");
////                // send documents to DocumentService
////                foreach (Document doc in documents)
////                {
////                    var response = await httpClient.PostAsJsonAsync("http://localhost:5046/document", doc);
////                }

////                return corr_id;
////            }


////            public async Task<Guid?> attachement_types_and_consumption_scopes(MimeMessage message, string directorypath, MailRule mail, string title, List<Document> documents, UniqueId uid, string content, string tessDataPath)
////            {
////                var httpClient = new HttpClient();
////                Guid? corr_id = Guid.Empty;

////                string pattern = @"^(?:""(?<name>[^""]*)"")?\s*<(?<email>[^<>]+)>|^(?<email>[^<>]+)$";

////                Match match = Regex.Match(message.From.ToString(), pattern);

////                // Extract the name and email
////                string name = match.Groups["name"].Value.Trim();
////                string email = match.Groups["email"].Value.Trim();

////                List<string> list_email = new List<string>();
////                List<string> list_name = new List<string>();

////                if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(email))
////                {
////                    // Both name and email are empty, you may handle this case here if needed.
////                }
////                else if (string.IsNullOrEmpty(name))
////                {
////                    // Name is empty, only add email
////                    list_email.Add(email);
////                }
////                else
////                {
////                    // Add both name and email
////                    list_email.Add(email);
////                    list_name.Add(name);
////                }

////                if (mail.Assign_correspondent_from == MailMetadataCorrespondentOption.FromEmail)
////                {
////                    var correspondent = new CorrespondentCreate
////                    {
////                        Name = email,
////                        Slug = email,
////                        Match = list_email,
////                        Matching_algorithm = Matching_Algorithms.MATCH_ANY,
////                        Is_insensitive = false,
////                        Owner = "user",
////                        Document_count = 0,
////                        Last_correspondence = DateTime.UtcNow
////                    };
////                    var response = await httpClient.PostAsJsonAsync("http://localhost:5046/correspondents/post_correspondent", correspondent);

////                    string corresp_content = await response.Content.ReadAsStringAsync();

////                    Console.WriteLine("the full content of the created correspondent : " + content);
////                    // Deserialize the JSON content
////                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Correspondent>(corresp_content);

////                    // Access the ID
////                    corr_id = responseObject.Id;
////                    Console.WriteLine("correspendent id : " + corr_id);

////                }
////                else if (mail.Assign_correspondent_from == MailMetadataCorrespondentOption.FromName)
////                {
////                    CorrespondentCreate correspondent = new CorrespondentCreate();
////                    if (name != "")
////                    {
////                        correspondent = new CorrespondentCreate
////                        {
////                            Name = name,
////                            Slug = name,
////                            Match = list_name,
////                            Matching_algorithm = Matching_Algorithms.MATCH_ANY,
////                            Is_insensitive = false,
////                            Owner = "user",
////                            Document_count = 0,
////                            Last_correspondence = DateTime.UtcNow
////                        };
////                    }
////                    else
////                    {
////                        correspondent = new CorrespondentCreate
////                        {
////                            Name = email,
////                            Slug = email,
////                            Match = list_email,
////                            Matching_algorithm = Matching_Algorithms.MATCH_ANY,
////                            Is_insensitive = false,
////                            Owner = "user",
////                            Document_count = 0,
////                            Last_correspondence = DateTime.UtcNow
////                        };
////                    }
////                    var response = await httpClient.PostAsJsonAsync("http://localhost:5046/correspondents/post_correspondent", correspondent);
////                    string corresp_content = await response.Content.ReadAsStringAsync();
////                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Correspondent>(corresp_content);
////                    corr_id = responseObject.Id;

////                }
////                else if (mail.Assign_correspondent_from == MailMetadataCorrespondentOption.FromCustom)
////                {
////                    // check if Assign_Correspondent exists
////                    List<Correspondent> correspondents = CorrespondentList.CallRestApi().Result;
////                    if (mail.Assign_correspondent != null)
////                    {
////                        bool exists = correspondents.Any(correspondent => correspondent.Id == mail.Assign_correspondent);
////                        if (exists)
////                        {
////                            corr_id = mail.Assign_correspondent.Value;
////                        }
////                        Console.WriteLine(exists);

////                    }
////                }
////                else if (mail.Assign_correspondent_from == MailMetadataCorrespondentOption.FromNothing)
////                {
////                    corr_id = null;
////                }

////                mail.Assign_correspondent = corr_id;

////                if (mail.Attachment_type == MailFilterAttachmentType.Attachments && mail.Consumption_scope == MailRuleConsumptionScope.Attachments) // Process attachments only if attachment_type is 1 and consumption_scope is 1
////                {
////                    ProcessorOnlyAttachementHelper.save_pdf_and_images_for_online_attachement(message, directorypath, mail, title, documents, tessDataPath, corr_id ?? Guid.Empty, httpClient);
////                }
////                else if (mail.Attachment_type == MailFilterAttachmentType.Attachments && mail.Consumption_scope == MailRuleConsumptionScope.Email_Only)// Process email body only if consumption_scope is 2
////                {
////                    ProcessorOnlyAttachementHelper.save_eml_files_for_online_attachement(message, directorypath, mail, title, documents, uid, corr_id ?? Guid.Empty);// Check if the email meets the condition: no attachments but with inline attachments
////                }
////                else if (mail.Attachment_type == MailFilterAttachmentType.Attachments && mail.Consumption_scope == MailRuleConsumptionScope.Everything)
////                {
////                    ProcessorOnlyAttachementHelper.save_pdf_and_images_for_online_attachement(message, directorypath, mail, title, documents, tessDataPath, corr_id ?? Guid.Empty, httpClient);
////                    ProcessorOnlyAttachementHelper.save_eml_files_for_online_attachement(message, directorypath, mail, title, documents, uid, corr_id ?? Guid.Empty);
////                }
////                else if (mail.Attachment_type == MailFilterAttachmentType.Everything && mail.Consumption_scope == MailRuleConsumptionScope.Attachments)// Attachment Type : Everything*************ConsumptionScope : Attchaments
////                {
////                    ProcessorInlineAttachementHelper.save_pdf_and_images_for_online_attachement_including_inline(message, directorypath, mail, title, documents, content, tessDataPath, corr_id ?? Guid.Empty);
////                }
////                else if (mail.Attachment_type == MailFilterAttachmentType.Everything && mail.Consumption_scope == MailRuleConsumptionScope.Email_Only)// Attachment Type : Everything*************ConsumptionScope : Email_Only
////                {
////                    ProcessorInlineAttachementHelper.save_eml_files_for_online_attachement_including_inline(message, directorypath, mail, title, documents, uid, corr_id ?? Guid.Empty);
////                }
////                else if (mail.Attachment_type == MailFilterAttachmentType.Everything && mail.Consumption_scope == MailRuleConsumptionScope.Everything)// Attachment Type : Everything*************ConsumptionScope : Everything
////                {
////                    ProcessorInlineAttachementHelper.save_pdf_and_images_for_online_attachement_including_inline(message, directorypath, mail, title, documents, content, tessDataPath, corr_id ?? Guid.Empty);
////                    ProcessorInlineAttachementHelper.save_eml_files_for_online_attachement_including_inline(message, directorypath, mail, title, documents, uid, corr_id ?? Guid.Empty);
////                }
////                Thread.Sleep(3000);

////                return corr_id;
////            }
////        }
////    }
////}
//using Application.Consumers.RestAPIDocuments.Dtos;
//using Application.Consumers.RestAPIDocuments.Endpoints;
//using Application.Dtos.MailRule;
//using Application.Repository;
//using Domain.MailAccounts;
//using Domain.MailRules;
//using Domain.MailRules.Enum;
//using MailKit;
//using MailKit.Net.Imap;
//using MailKit.Search;
//using MapsterMapper;
//using MediatR;
//using MimeKit;
//using System.Net.Http.Json;
//using Microsoft.Extensions.Configuration;
//using Application.Features.Helper.ProcessorHelper;
//using System.Threading;
//using MailKit.Net.Imap;
//using Tesseract;
//using System.Text.RegularExpressions;
//using System.Net.Http;
//using FluentValidation;
//using System.Data.SqlTypes;
//using static System.Net.WebRequestMethods;


//namespace Application.Features.FeaturesMailRule
//{
//    public class AddMailRule
//    {
//        public sealed record Command : IRequest<MailRule>
//        {
//            public readonly MailRuleDto MailRule;

//            public Command(MailRuleDto mailrule)
//            {
//                MailRule = mailrule;
//            }
//        }
//        public sealed class AddValidator : AbstractValidator<Command>
//        {
//            public AddValidator(IMailRuleRepository _repository)
//            {
//                RuleFor(mr => mr.MailRule.Name).NotEmpty().MustAsync(async (name, ct) => !await _repository.ExistsAsync(mr => mr.Name == name, ct))
//                                 .WithMessage("Name must be unique.");

//                RuleFor(mr => mr.MailRule.Order).NotEmpty().MustAsync(async (order, ct) => !await _repository.ExistsAsync(mr => mr.Order == order, ct))
//                                 .WithMessage("Order must be unique.");

//            }
//        }
//        public sealed class Handler : IRequestHandler<Command, MailRule>
//        {
//            private readonly IMailRuleRepository _mailRuleRepository;
//            private readonly ImapClient _client;
//            private readonly Mapper _mapper;
//            private readonly IMailAccountRepository _mailAccountRepository;
//            private readonly IConfiguration _configuration;

//            public Handler(ImapClient client, Mapper mapper, IMailRuleRepository mailRuleRepository, IMailAccountRepository mailAccountRepository, IConfiguration configuration)
//            {
//                _client = client;
//                _mapper = mapper;
//                _mailRuleRepository = mailRuleRepository;
//                _mailAccountRepository = mailAccountRepository;
//                _configuration = configuration;
//            }
//            public async Task<MailRule> Handle(Command request, CancellationToken cancellationToken)
//            {

//                // Check Filter "" 
//                string Filter_From;
//                if (request.MailRule.Filter_from == "")
//                {
//                    Filter_From = null;
//                }
//                else
//                {
//                    Filter_From = request.MailRule.Filter_from;
//                }
//                //check Filter To
//                string Filter_To;
//                if (request.MailRule.Filter_to == "")
//                {
//                    Filter_To = null;
//                }
//                else
//                {
//                    Filter_To = request.MailRule.Filter_to;
//                }
//                // chechk Filter Body
//                string Filter_Body;
//                if (request.MailRule.Filter_body == "")
//                {
//                    Filter_Body = null;
//                }
//                else
//                {
//                    Filter_Body = request.MailRule.Filter_body;
//                }
//                //check Filter Subject
//                string Filter_Subject;
//                if (request.MailRule.Filter_subject == "")
//                {
//                    Filter_Subject = null;
//                }
//                else
//                {
//                    Filter_Subject = request.MailRule.Filter_subject;
//                }


//                Guid? corr_id = null;

//                // Create a new MailRule instance
//                var mailRule = new MailRule
//                {
//                    Name = request.MailRule.Name,
//                    Folder = request.MailRule.Folder,
//                    Maximum_age = request.MailRule.Maximum_age,
//                    Attachment_type = request.MailRule.Attachment_type,
//                    Consumption_scope = request.MailRule.Consumption_scope,
//                    Order = request.MailRule.Order,
//                    Filter_from = Filter_From,
//                    Filter_to = Filter_To,
//                    Filter_subject = Filter_Subject,
//                    Filter_body = Filter_Body,
//                    Filter_attachment_filename = request.MailRule.Filter_attachment_filename,
//                    Action = request.MailRule.Action,
//                    Assign_title_from = request.MailRule.Assign_title_from,
//                    Action_parameter = request.MailRule.Action_parameter,
//                    Assign_tags = request.MailRule.Assign_tags,
//                    Assign_document_type = request.MailRule.Assign_document_type,
//                    Assign_correspondent_from = request.MailRule.Assign_correspondent_from,
//                    Assign_correspondent = request.MailRule.Assign_correspondent,
//                    Assign_owner_from_rule = request.MailRule.Assign_owner_from_rule,
//                    Owner = request.MailRule.Owner
//                };

//                //getting the tags ids
//                List<TagsDto> tags = TagsList.CallRestApi().Result;
//                if (request.MailRule.Assign_tags != null)
//                {
//                    if (request.MailRule.Assign_tags.Count > 0)
//                    {
//                        foreach (var tagId in request.MailRule.Assign_tags)
//                        {
//                            bool exists = tags.Any(tag => tag.Id == tagId);
//                            if (exists)
//                            {
//                                mailRule.Assign_tags = request.MailRule.Assign_tags;
//                            }
//                            else
//                            {
//                                mailRule.Assign_tags = null;
//                            }
//                        }
//                    }
//                    else
//                    {
//                        mailRule.Assign_tags = null;
//                    }
//                }


//                //getting the documenttype id
//                List<DocumentType> documentTypes = DocumentTypeList.CallRestApi().Result;
//                if (request.MailRule.Assign_document_type != null)
//                {
//                    bool exists = documentTypes.Any(doctype => doctype.Id == request.MailRule.Assign_document_type);
//                    if (exists)
//                    {
//                        mailRule.Assign_document_type = request.MailRule.Assign_document_type;
//                    }
//                    else
//                    {
//                        mailRule.Assign_document_type = null;
//                    }

//                }

//                //getting the mail account 
//                MailAccount selected_mailaccount = await _mailAccountRepository.FindByIdAsync(request.MailRule.Account, cancellationToken);

//                mailRule.Account = selected_mailaccount;
//                //getting the corrspondent id
//                corr_id = await RetriveMail(selected_mailaccount, _client, mailRule);
//                if (corr_id == Guid.Empty)
//                {
//                    mailRule.Assign_correspondent = null;
//                }
//                else
//                {
//                    mailRule.Assign_correspondent = corr_id;
//                }


//                // Add the MailRule to the database
//                await _mailRuleRepository.AddAsync(mailRule, cancellationToken);
//                return mailRule;
//            }

//            private async Task<Guid?> RetriveMail(MailAccount mailAccount, ImapClient imapClient, MailRule mailrule)
//            {
//                Guid? corr_id = null;
//                if (mailAccount.IMAP_Security == Imap_Security.SSL || mailAccount.IMAP_Security == Imap_Security.STARTTLS)
//                    imapClient.Connect(mailAccount.IMAP_Server, mailAccount.IMAP_Port, true);
//                var passwrddecrypt = CryptoHelper.DecodeFrom64(mailAccount.Password);
//                //imapClient.Authenticate("hamrounirahma001@gmail.com", "xkfy yqzf uiyr zgcn");
//                imapClient.Authenticate(mailAccount.Username, passwrddecrypt);
//                Console.WriteLine("Connected to the mail server successfully.");
//                //var inbox = _client.Inbox;

//                var inbox = _client.GetFolder(mailrule.Folder);
//                //var Folder = imapClient.GetFolder(imapClient.PersonalNamespaces[0]);


//                inbox.Open(FolderAccess.ReadWrite);
//                // function to BuildSerchQuery Filter and Maximum Age
//                var combinedQuery = BuildSearchQuery(mailrule);
//                var uids = inbox.Search(combinedQuery);
//                // create Directory
//                string tessDataPath = _configuration["TesseractPath:tessdatafile"];
//                string baseDestinationPath = GetBaseDestinationPath();
//                Directory.CreateDirectory(baseDestinationPath);
//                // function send Documents to Service Document
//                corr_id = await SendDocumentsToService(uids.ToList(), inbox, mailrule, imapClient, baseDestinationPath, tessDataPath, mailAccount);
//                return corr_id;
//            }
//            private SearchQuery BuildSearchQuery(MailRule mailRule)
//            {
//                var query = SearchQuery.All;

//                // Filter by "From" address if provided
//                if (!string.IsNullOrEmpty(mailRule.Filter_from))
//                {
//                    query = query.And(SearchQuery.FromContains(mailRule.Filter_from));
//                }
//                // Filter by "To" address if provided
//                if (!string.IsNullOrEmpty(mailRule.Filter_to))
//                {
//                    query = query.And(SearchQuery.ToContains(mailRule.Filter_to));
//                }
//                // Filter by "Subject" if provided
//                if (!string.IsNullOrEmpty(mailRule.Filter_subject))
//                {
//                    query = query.And(SearchQuery.SubjectContains(mailRule.Filter_subject));
//                }
//                // Filter by "Body" if provided
//                if (!string.IsNullOrEmpty(mailRule.Filter_body))
//                {
//                    query = query.And(SearchQuery.BodyContains(mailRule.Filter_body));
//                    Console.WriteLine(query.ToString());
//                }


//                // Assuming Maximum_age is represented in days
//                var maximumAgeDays = mailRule.Maximum_age;
//                var deliveredAfterDate = DateTime.Today.AddDays(-maximumAgeDays);
//                Console.WriteLine(deliveredAfterDate.ToString());
//                // Add filter for "DeliveredAfter"
//                query = query.And(SearchQuery.DeliveredAfter(deliveredAfterDate));
//                return query;
//            }
//            // get path originals
//            public string GetBaseDestinationPath()
//            {

//                return _configuration["OriginalsSettings:OutputFolder"];
//            }
//            private async Task<Guid?> SendDocumentsToService(List<UniqueId> uids, IMailFolder inbox, MailRule mail, ImapClient imapClient, string directorypath, string tessDataPath, MailAccount mailAccount)
//            {
//                var httpClient = new HttpClient();
//                List<Document> documents = new List<Document>();
//                Guid? corr_id = null;
//                var messages = new List<MimeMessage>();
//                foreach (var uid in uids)
//                {

//                    var message = inbox.GetMessage(uid);
//                    messages.Add(message);
//                    string content = "";
//                    string title = "";
//                    //in the mail is not not read you can read it
//                    var summary = inbox.Fetch(new[] { uid }, MessageSummaryItems.Flags).FirstOrDefault();
//                    bool isRead = summary.Flags.HasValue && summary.Flags.Value.HasFlag(MessageFlags.Seen);
//                    bool isFlagged = summary.Flags.HasValue && summary.Flags.Value.HasFlag(MessageFlags.Flagged);
//                    Console.WriteLine($"Message UID: {uid}, IsRead: {isRead}");

//                    if (!isRead && mail.Action == MailAction.MarkRead)
//                    {
//                        corr_id = await attachement_types_and_consumption_scopes(message, directorypath, mail, title, documents, uid, content, tessDataPath);
//                        inbox.AddFlags(uid, MessageFlags.Seen, true);

//                    }
//                    else if (mail.Action == MailAction.Delete)
//                    {
//                        corr_id = await attachement_types_and_consumption_scopes(message, directorypath, mail, title, documents, uid, content, tessDataPath);
//                        inbox.AddFlags(uid, MessageFlags.Deleted, true);
//                    }
//                    else if (!isFlagged && mail.Action == MailAction.Flag)
//                    {
//                        corr_id = await attachement_types_and_consumption_scopes(message, directorypath, mail, title, documents, uid, content, tessDataPath);
//                        inbox.AddFlags(uid, MessageFlags.Flagged, true);
//                    }
//                    else if (mail.Action == MailAction.Move)
//                    {
//                        corr_id = await attachement_types_and_consumption_scopes(message, directorypath, mail, title, documents, uid, content, tessDataPath);
//                        // Assuming mail.Action_parameter is the folder name
//                        string destinationFolderName = mail.Action_parameter;
//                        // Get the root folder (usually INBOX)
//                        var Folder = imapClient.GetFolder(imapClient.PersonalNamespaces[0]);

//                        // Retrieve a list of all folders available on the server
//                        var listFolder = Folder.GetSubfolders();
//                        bool destinationFolderExists = listFolder.Any(folder => folder.FullName == destinationFolderName);
//                        if (destinationFolderName != null && !destinationFolderExists)
//                        {
//                            // Get or create the destination folder
//                            var rootFolder = imapClient.GetFolder("");
//                            IMailFolder newFolder = rootFolder.Create(destinationFolderName, true);
//                            inbox.MoveTo(uid, newFolder);
//                        }
//                        else if (destinationFolderExists)
//                        {
//                            var destinationFolder = imapClient.GetFolder(destinationFolderName);
//                            inbox.MoveTo(uid, destinationFolder);
//                        }
//                    }
//                    else if (mail.Action == MailAction.Tag)
//                    {
//                        corr_id = await attachement_types_and_consumption_scopes(message, directorypath, mail, title, documents, uid, content, tessDataPath);
//                        if (mailAccount.IMAP_Server == "imap.gmail.com")
//                        {
//                            // Assuming mail.Action_parameter is the label name
//                            var labelName = mail.Action_parameter;

//                            // Check if the label already exists
//                            var existingFolder = inbox.GetSubfolders(false)
//                                                       .FirstOrDefault(folder => folder.Name == labelName);

//                            if (existingFolder == null)
//                            {
//                                // Create a new folder with the label name
//                                existingFolder = inbox.Create(labelName, true);
//                                // Add the Keyword flag to the new folder
//                                inbox.AddLabels(uid, new List<string> { labelName }, true);
//                            }
//                            else
//                            {

//                                // Add the keyword to the processed mail
//                                inbox.AddLabels(uid, new List<string> { labelName }, true);
//                            }
//                        }

//                    }

//                }
//                // Disconnected from the mail server
//                imapClient.Disconnect(true);
//                Console.WriteLine("Disconnected from the mail server.");
//                // send documents to DocumentService
//                foreach (Document doc in documents)
//                {
//                    var response = await httpClient.PostAsJsonAsync("http://localhost:5046/document", doc);
//                }

//                return corr_id;
//            }


//            public async Task<Guid?> attachement_types_and_consumption_scopes(MimeMessage message, string directorypath, MailRule mail, string title, List<Document> documents, UniqueId uid, string content, string tessDataPath)
//            {
//                var httpClient = new HttpClient();
//                Guid? corr_id = Guid.Empty;

//                string pattern = @"^(?:""(?<name>[^""]*)"")?\s*<(?<email>[^<>]+)>|^(?<email>[^<>]+)$";

//                Match match = Regex.Match(message.From.ToString(), pattern);

//                // Extract the name and email
//                string name = match.Groups["name"].Value.Trim();
//                string email = match.Groups["email"].Value.Trim();

//                List<string> list_email = new List<string>();
//                List<string> list_name = new List<string>();

//                if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(email))
//                {
//                    // Both name and email are empty, you may handle this case here if needed.
//                }
//                else if (string.IsNullOrEmpty(name))
//                {
//                    // Name is empty, only add email
//                    list_email.Add(email);
//                }
//                else
//                {
//                    // Add both name and email
//                    list_email.Add(email);
//                    list_name.Add(name);
//                }

//                if (mail.Assign_correspondent_from == MailMetadataCorrespondentOption.FromEmail)
//                {
//                    var correspondent = new CorrespondentCreate
//                    {
//                        Name = email,
//                        Slug = email,
//                        Match = list_email,
//                        Matching_algorithm = Matching_Algorithms.MATCH_ANY,
//                        Is_insensitive = false,
//                        Owner = "user",
//                        Document_count = 0,
//                        Last_correspondence = DateTime.UtcNow
//                    };
//                    var response = await httpClient.PostAsJsonAsync("http://localhost:5046/correspondents/post_correspondent", correspondent);

//                    string corresp_content = await response.Content.ReadAsStringAsync();

//                    Console.WriteLine("the full content of the created correspondent : " + content);
//                    // Deserialize the JSON content
//                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Correspondent>(corresp_content);

//                    // Access the ID
//                    corr_id = responseObject.Id;
//                    Console.WriteLine("correspendent id : " + corr_id);

//                }
//                else if (mail.Assign_correspondent_from == MailMetadataCorrespondentOption.FromName)
//                {
//                    CorrespondentCreate correspondent = new CorrespondentCreate();
//                    if (name != "")
//                    {
//                        correspondent = new CorrespondentCreate
//                        {
//                            Name = name,
//                            Slug = name,
//                            Match = list_name,
//                            Matching_algorithm = Matching_Algorithms.MATCH_ANY,
//                            Is_insensitive = false,
//                            Owner = "user",
//                            Document_count = 0,
//                            Last_correspondence = DateTime.UtcNow
//                        };
//                    }
//                    else
//                    {
//                        correspondent = new CorrespondentCreate
//                        {
//                            Name = email,
//                            Slug = email,
//                            Match = list_email,
//                            Matching_algorithm = Matching_Algorithms.MATCH_ANY,
//                            Is_insensitive = false,
//                            Owner = "user",
//                            Document_count = 0,
//                            Last_correspondence = DateTime.UtcNow
//                        };
//                    }
//                    var response = await httpClient.PostAsJsonAsync("http://localhost:5046/correspondents/post_correspondent", correspondent);
//                    string corresp_content = await response.Content.ReadAsStringAsync();
//                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Correspondent>(corresp_content);
//                    corr_id = responseObject.Id;

//                }
//                else if (mail.Assign_correspondent_from == MailMetadataCorrespondentOption.FromCustom)
//                {
//                    // check if Assign_Correspondent exists
//                    List<Correspondent> correspondents = CorrespondentList.CallRestApi().Result;
//                    if (mail.Assign_correspondent != null)
//                    {
//                        bool exists = correspondents.Any(correspondent => correspondent.Id == mail.Assign_correspondent);
//                        if (exists)
//                        {
//                            corr_id = mail.Assign_correspondent.Value;
//                        }
//                        Console.WriteLine(exists);

//                    }
//                }
//                else if (mail.Assign_correspondent_from == MailMetadataCorrespondentOption.FromNothing)
//                {
//                    corr_id = null;
//                }

//                mail.Assign_correspondent = corr_id;

//                if (mail.Attachment_type == MailFilterAttachmentType.Attachments && mail.Consumption_scope == MailRuleConsumptionScope.Attachments) // Process attachments only if attachment_type is 1 and consumption_scope is 1
//                {
//                    ProcessorOnlyAttachementHelper.save_pdf_and_images_for_online_attachement(message, directorypath, mail, title, documents, tessDataPath, corr_id ?? Guid.Empty, httpClient);
//                }
//                else if (mail.Attachment_type == MailFilterAttachmentType.Attachments && mail.Consumption_scope == MailRuleConsumptionScope.Email_Only)// Process email body only if consumption_scope is 2
//                {
//                    ProcessorOnlyAttachementHelper.save_eml_files_for_online_attachement(message, directorypath, mail, title, documents, uid, corr_id ?? Guid.Empty, httpClient);// Check if the email meets the condition: no attachments but with inline attachments
//                }
//                else if (mail.Attachment_type == MailFilterAttachmentType.Attachments && mail.Consumption_scope == MailRuleConsumptionScope.Everything)
//                {
//                    ProcessorOnlyAttachementHelper.save_pdf_and_images_for_online_attachement(message, directorypath, mail, title, documents, tessDataPath, corr_id ?? Guid.Empty, httpClient);
//                    ProcessorOnlyAttachementHelper.save_eml_files_for_online_attachement(message, directorypath, mail, title, documents, uid, corr_id ?? Guid.Empty, httpClient);
//                }
//                else if (mail.Attachment_type == MailFilterAttachmentType.Everything && mail.Consumption_scope == MailRuleConsumptionScope.Attachments)// Attachment Type : Everything*************ConsumptionScope : Attchaments
//                {
//                    ProcessorInlineAttachementHelper.save_pdf_and_images_for_online_attachement_including_inline(message, directorypath, mail, title, documents, content, tessDataPath, corr_id ?? Guid.Empty, httpClient);
//                }
//                else if (mail.Attachment_type == MailFilterAttachmentType.Everything && mail.Consumption_scope == MailRuleConsumptionScope.Email_Only)// Attachment Type : Everything*************ConsumptionScope : Email_Only
//                {
//                    ProcessorInlineAttachementHelper.save_eml_files_for_online_attachement_including_inline(message, directorypath, mail, title, documents, uid, corr_id ?? Guid.Empty, httpClient);
//                }
//                else if (mail.Attachment_type == MailFilterAttachmentType.Everything && mail.Consumption_scope == MailRuleConsumptionScope.Everything)// Attachment Type : Everything*************ConsumptionScope : Everything
//                {
//                    ProcessorInlineAttachementHelper.save_pdf_and_images_for_online_attachement_including_inline(message, directorypath, mail, title, documents, content, tessDataPath, corr_id ?? Guid.Empty, httpClient);
//                    ProcessorInlineAttachementHelper.save_eml_files_for_online_attachement_including_inline(message, directorypath, mail, title, documents, uid, corr_id ?? Guid.Empty, httpClient);
//                }
//                Thread.Sleep(3000);

//                return corr_id;
//            }
//        }
//    }
//}
using Application.Consumers.RestAPIDocuments.Dtos;
using Application.Consumers.RestAPIDocuments.Endpoints;
using Application.Dtos.MailRule;
using Application.Repository;
using Domain.MailAccounts;
using Domain.MailRules;
using Domain.MailRules.Enum;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MapsterMapper;
using MediatR;
using MimeKit;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Application.Features.Helper.ProcessorHelper;
using System.Threading;
using MailKit.Net.Imap;
using Tesseract;
using System.Text.RegularExpressions;
using System.Net.Http;
using FluentValidation;
using System.Data.SqlTypes;
using static System.Net.WebRequestMethods;
using Microsoft.Office.Interop.Word;
using Document = Application.Consumers.RestAPIDocuments.Dtos.Document;
using Application.Exceptions;
using Newtonsoft.Json;
using Serilog;
using Domain.Ports;


namespace Application.Features.FeaturesMailRule
{
    public class AddMailRule
    {
        public sealed record Command : IRequest<MailRule>
        {
            public readonly MailRuleDto MailRule;

            public Command(MailRuleDto mailrule)
            {
                MailRule = mailrule;
            }
        }
        public sealed class AddValidator : AbstractValidator<Command>
        {
            public AddValidator(IMailRuleRepository _repository)
            {
                RuleFor(mr => mr.MailRule.Name).NotEmpty().MustAsync(async (name, ct) => !await _repository.ExistsAsync(mr => mr.Name == name, ct))
                                 .WithMessage("Name must be unique.");

                RuleFor(mr => mr.MailRule.Order).NotEmpty().MustAsync(async (order, ct) => !await _repository.ExistsAsync(mr => mr.Order == order, ct))
                                 .WithMessage("Order must be unique.");

            }
        }
        public sealed class Handler : IRequestHandler<Command, MailRule>
        {
            private readonly IMailRuleRepository _mailRuleRepository;
            private readonly ImapClient _client;
            private readonly Mapper _mapper;
            private readonly IMailAccountRepository _mailAccountRepository;
            private readonly IConfiguration _configuration;
            private readonly IUserGroupPort _userGroupPort;

            public Handler(ImapClient client, Mapper mapper, 
                IMailRuleRepository mailRuleRepository,
                IMailAccountRepository mailAccountRepository,
                IConfiguration configuration
                , IUserGroupPort userGroupPort)
            {
                _client = client;
                _mapper = mapper;
                _mailRuleRepository = mailRuleRepository;
                _mailAccountRepository = mailAccountRepository;
                _configuration = configuration;
                _userGroupPort = userGroupPort;
            }
            public async Task<MailRule> Handle(Command request, CancellationToken cancellationToken)
            {

                // Check Filter "" 
                string Filter_From;
                if (request.MailRule.Filter_from == "")
                {
                    Filter_From = null;
                }
                else
                {
                    Filter_From = request.MailRule.Filter_from;
                }
                //check Filter To
                string Filter_To;
                if (request.MailRule.Filter_to == "")
                {
                    Filter_To = null;
                }
                else
                {
                    Filter_To = request.MailRule.Filter_to;
                }
                // chechk Filter Body
                string Filter_Body;
                if (request.MailRule.Filter_body == "")
                {
                    Filter_Body = null;
                }
                else
                {
                    Filter_Body = request.MailRule.Filter_body;
                }
                //check Filter Subject
                string Filter_Subject;
                if (request.MailRule.Filter_subject == "")
                {
                    Filter_Subject = null;
                }
                else
                {
                    Filter_Subject = request.MailRule.Filter_subject;
                }


                Guid? corr_id = null;
               // Guid groupId = await GetGroupForUser(request.MailRule.Owner??string.Empty);
                
                // Create a new MailRule instance
                var mailRule = new MailRule
                {
                    Name = request.MailRule.Name,
                    Folder = request.MailRule.Folder,
                    Maximum_age = request.MailRule.Maximum_age,
                    Attachment_type = request.MailRule.Attachment_type,
                    Consumption_scope = request.MailRule.Consumption_scope,
                    Order = request.MailRule.Order,
                    Filter_from = Filter_From,
                    Filter_to = Filter_To,
                    Filter_subject = Filter_Subject,
                    Filter_body = Filter_Body,
                    Filter_attachment_filename = request.MailRule.Filter_attachment_filename,
                    Action = request.MailRule.Action,
                    Assign_title_from = request.MailRule.Assign_title_from,
                    Action_parameter = request.MailRule.Action_parameter,
                    Assign_tags = request.MailRule.Assign_tags,
                    Assign_document_type = request.MailRule.Assign_document_type,
                    Assign_correspondent_from = request.MailRule.Assign_correspondent_from,
                    Assign_correspondent = request.MailRule.Assign_correspondent,
                    Assign_owner_from_rule = request.MailRule.Assign_owner_from_rule,
                    Owner = request.MailRule.Owner,
                    //GroupId=groupId

                };

                //getting the tags ids
                List<TagsDto> tags = TagsList.CallRestApi(request.MailRule.Owner).Result;
                if (request.MailRule.Assign_tags != null)
                {
                    if (request.MailRule.Assign_tags.Count > 0)
                    {
                        foreach (var tagId in request.MailRule.Assign_tags)
                        {
                            bool exists = tags.Any(tag => tag.Id == tagId);
                            if (exists)
                            {
                                mailRule.Assign_tags = request.MailRule.Assign_tags;
                            }
                            else
                            {
                                mailRule.Assign_tags = null;
                            }
                        }
                    }
                    else
                    {
                        mailRule.Assign_tags = null;
                    }
                }








                //getting the mail account 
                MailAccount selected_mailaccount = await _mailAccountRepository.FindByIdAsync(request.MailRule.Account, cancellationToken);

                mailRule.Account = selected_mailaccount;
                //getting the corrspondent id
                corr_id = await RetriveMail(selected_mailaccount, _client, mailRule);



                if (request.MailRule.Assign_correspondent_from == MailMetadataCorrespondentOption.FromCustom && corr_id == null)
                {
                    // check if Assign_Correspondent exists
                    List<Correspondent> correspondents = CorrespondentList.CallRestApi(request.MailRule.Owner).Result;
                    if (request.MailRule.Assign_correspondent != null)
                    {
                        bool exists = correspondents.Any(correspondent => correspondent.Id == request.MailRule.Assign_correspondent);
                        if (exists)
                        {
                            corr_id = request.MailRule.Assign_correspondent.Value;
                        }
                        Console.WriteLine(exists);

                    }
                }




                if (corr_id == Guid.Empty)
                {
                    mailRule.Assign_correspondent = null;
                }
                else
                {
                    mailRule.Assign_correspondent = corr_id;
                }

                
                // Add the MailRule to the database
                await _mailRuleRepository.AddAsync(mailRule, cancellationToken);
                return mailRule;
            }

            private async Task<Guid?> RetriveMail(MailAccount mailAccount, ImapClient imapClient, MailRule mailrule)
            {
                try
                {               
                    Guid? corr_id = null;

                    if (mailAccount.IMAP_Security == Imap_Security.SSL || mailAccount.IMAP_Security == Imap_Security.STARTTLS)
                        imapClient.Connect(mailAccount.IMAP_Server, mailAccount.IMAP_Port, true);
                    var passwrddecrypt = CryptoHelper.DecodeFrom64(mailAccount.Password);
                    //imapClient.Authenticate("hamrounirahma001@gmail.com", "xkfy yqzf uiyr zgcn");
                    imapClient.Authenticate(mailAccount.Username, passwrddecrypt);
                    Console.WriteLine("Connected to the mail server successfully.");
                    //var inbox = _client.Inbox;

                    var inbox = _client.GetFolder(mailrule.Folder);
                    //var Folder = imapClient.GetFolder(imapClient.PersonalNamespaces[0]);


                    inbox.Open(FolderAccess.ReadWrite);
                    // function to BuildSerchQuery Filter and Maximum Age
                    var combinedQuery = BuildSearchQuery(mailrule);
                    var uids = inbox.Search(combinedQuery);
                    // create Directory
                    string tessDataPath = _configuration["TesseractPath:tessdatafile"];
                    string baseDestinationPath = GetBaseDestinationPath();
                    Directory.CreateDirectory(baseDestinationPath);
                    // function send Documents to Service Document
                    corr_id = await SendDocumentsToService(uids.ToList(), inbox, mailrule, imapClient, baseDestinationPath, tessDataPath, mailAccount);
                    return corr_id;
                }
                catch (MailRuleException ex)
                {
                    throw new System.Exception(ex.Message, ex);
                }
            }
            private SearchQuery BuildSearchQuery(MailRule mailRule)
            {
                var query = SearchQuery.All;

                // Filter by "From" address if provided
                if (!string.IsNullOrEmpty(mailRule.Filter_from))
                {
                    query = query.And(SearchQuery.FromContains(mailRule.Filter_from));
                }
                // Filter by "To" address if provided
                if (!string.IsNullOrEmpty(mailRule.Filter_to))
                {
                    query = query.And(SearchQuery.ToContains(mailRule.Filter_to));
                }
                // Filter by "Subject" if provided
                if (!string.IsNullOrEmpty(mailRule.Filter_subject))
                {
                    query = query.And(SearchQuery.SubjectContains(mailRule.Filter_subject));
                }
                // Filter by "Body" if provided
                if (!string.IsNullOrEmpty(mailRule.Filter_body))
                {
                    query = query.And(SearchQuery.BodyContains(mailRule.Filter_body));
                    Console.WriteLine(query.ToString());
                }


                // Assuming Maximum_age is represented in days
                var maximumAgeDays = mailRule.Maximum_age;
                var deliveredAfterDate = DateTime.Today.AddDays(-maximumAgeDays);
                Console.WriteLine(deliveredAfterDate.ToString());
                // Add filter for "DeliveredAfter"
                query = query.And(SearchQuery.DeliveredAfter(deliveredAfterDate));
                return query;
            }
            // get path originals
            public string GetBaseDestinationPath()
            {

                return _configuration["OriginalsSettings:OutputFolder"];
            }
            private async Task<Guid?> SendDocumentsToService(List<UniqueId> uids, IMailFolder inbox, MailRule mail, ImapClient imapClient, string directorypath, string tessDataPath, MailAccount mailAccount)
            {
                var httpClient = new HttpClient();
                List<Document> documents = new List<Document>();
                Guid? corr_id = null;
                var messages = new List<MimeMessage>();
                foreach (var uid in uids)
                {

                    var message = inbox.GetMessage(uid);
                    messages.Add(message);
                    string content = "";
                    string title = "";
                    //in the mail is not not read you can read it
                    var summary = inbox.Fetch(new[] { uid }, MessageSummaryItems.Flags).FirstOrDefault();
                    bool isRead = summary.Flags.HasValue && summary.Flags.Value.HasFlag(MessageFlags.Seen);
                    bool isFlagged = summary.Flags.HasValue && summary.Flags.Value.HasFlag(MessageFlags.Flagged);
                    Console.WriteLine($"Message UID: {uid}, IsRead: {isRead}");

                    if (!isRead && mail.Action == MailAction.MarkRead)
                    {
                        corr_id = await attachement_types_and_consumption_scopes(message, directorypath, mail, title, documents, uid, content, tessDataPath);
                        inbox.AddFlags(uid, MessageFlags.Seen, true);

                    }
                    else if (mail.Action == MailAction.Delete)
                    {
                        corr_id = await attachement_types_and_consumption_scopes(message, directorypath, mail, title, documents, uid, content, tessDataPath);
                        inbox.AddFlags(uid, MessageFlags.Deleted, true);
                    }
                    else if (!isFlagged && mail.Action == MailAction.Flag)
                    {
                        corr_id = await attachement_types_and_consumption_scopes(message, directorypath, mail, title, documents, uid, content, tessDataPath);
                        inbox.AddFlags(uid, MessageFlags.Flagged, true);
                    }
                    else if (mail.Action == MailAction.Move)
                    {
                        corr_id = await attachement_types_and_consumption_scopes(message, directorypath, mail, title, documents, uid, content, tessDataPath);
                        // Assuming mail.Action_parameter is the folder name
                        string destinationFolderName = mail.Action_parameter;
                        // Get the root folder (usually INBOX)
                        var Folder = imapClient.GetFolder(imapClient.PersonalNamespaces[0]);

                        // Retrieve a list of all folders available on the server
                        var listFolder = Folder.GetSubfolders();
                        bool destinationFolderExists = listFolder.Any(folder => folder.FullName == destinationFolderName);
                        if (destinationFolderName != null && !destinationFolderExists)
                        {
                            // Get or create the destination folder
                            var rootFolder = imapClient.GetFolder("");
                            IMailFolder newFolder = rootFolder.Create(destinationFolderName, true);
                            inbox.MoveTo(uid, newFolder);
                        }
                        else if (destinationFolderExists)
                        {
                            var destinationFolder = imapClient.GetFolder(destinationFolderName);
                            inbox.MoveTo(uid, destinationFolder);
                        }
                    }
                    else if (mail.Action == MailAction.Tag)
                    {
                        corr_id = await attachement_types_and_consumption_scopes(message, directorypath, mail, title, documents, uid, content, tessDataPath);
                        if (mailAccount.IMAP_Server == "imap.gmail.com")
                        {
                            // Assuming mail.Action_parameter is the label name
                            var labelName = mail.Action_parameter;

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

                }
                // Disconnected from the mail server
                imapClient.Disconnect(true);
                Console.WriteLine("Disconnected from the mail server.");
                // send documents to DocumentService
                foreach (Document doc in documents)
                {
                    var response = await httpClient.PostAsJsonAsync("http://localhost:5046/document", doc);
                }

                return corr_id;
            }


            public async Task<Guid?> attachement_types_and_consumption_scopes(MimeMessage message, string directorypath, MailRule mail, string title, List<Document> documents, UniqueId uid, string content, string tessDataPath)
            {
                var httpClient = new HttpClient();
                Guid? corr_id = Guid.Empty;

                string pattern = @"^(?:""(?<name>[^""]*)"")?\s*<(?<email>[^<>]+)>|^(?<email>[^<>]+)$";

                Match match = Regex.Match(message.From.ToString(), pattern);

                // Extract the name and email
                string name = match.Groups["name"].Value.Trim();
                string email = match.Groups["email"].Value.Trim();

                List<string> list_email = new List<string>();
                List<string> list_name = new List<string>();

                if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(email))
                {
                    // Both name and email are empty, you may handle this case here if needed.
                }
                else if (string.IsNullOrEmpty(name))
                {
                    // Name is empty, only add email
                    list_email.Add(email);
                }
                else
                {
                    // Add both name and email
                    list_email.Add(email);
                    list_name.Add(name);
                }

                if (mail.Assign_correspondent_from == MailMetadataCorrespondentOption.FromEmail)
                {
                    var correspondent = new CorrespondentCreate
                    {
                        Name = email,
                        Slug = email,
                        Match = list_email,
                        Matching_algorithm = Matching_Algorithms.MATCH_ANY,
                        Is_insensitive = false,
                        Owner = mail.Owner,
                        Document_count = 0,
                        Last_correspondence = DateTime.UtcNow
                    };
                    var response = await httpClient.PostAsJsonAsync("http://localhost:5046/correspondents/post_correspondent", correspondent);

                    string corresp_content = await response.Content.ReadAsStringAsync();

                    Console.WriteLine("the full content of the created correspondent : " + content);
                    // Deserialize the JSON content
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Correspondent>(corresp_content);

                    // Access the ID
                    corr_id = responseObject.Id;
                    Console.WriteLine("correspendent id : " + corr_id);

                }
                else if (mail.Assign_correspondent_from == MailMetadataCorrespondentOption.FromName)
                {
                    CorrespondentCreate correspondent = new CorrespondentCreate();
                    if (name != "")
                    {
                        correspondent = new CorrespondentCreate
                        {
                            Name = name,
                            Slug = name,
                            Match = list_name,
                            Matching_algorithm = Matching_Algorithms.MATCH_ANY,
                            Is_insensitive = false,
                            Owner = mail.Owner,
                            Document_count = 0,
                            Last_correspondence = DateTime.UtcNow
                        };
                    }
                    else
                    {
                        correspondent = new CorrespondentCreate
                        {
                            Name = email,
                            Slug = email,
                            Match = list_email,
                            Matching_algorithm = Matching_Algorithms.MATCH_ANY,
                            Is_insensitive = false,
                            Owner = mail.Owner,
                            Document_count = 0,
                            Last_correspondence = DateTime.UtcNow
                        };
                    }
                    var response = await httpClient.PostAsJsonAsync("http://localhost:5046/correspondents/post_correspondent", correspondent);
                    string corresp_content = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Correspondent>(corresp_content);
                    corr_id = responseObject.Id;

                }
                else if (mail.Assign_correspondent_from == MailMetadataCorrespondentOption.FromCustom)
                {
                    // check if Assign_Correspondent exists
                    List<Correspondent> correspondents = CorrespondentList.CallRestApi(mail.Owner).Result;
                    if (mail.Assign_correspondent != null)
                    {
                        bool exists = correspondents.Any(correspondent => correspondent.Id == mail.Assign_correspondent);
                        if (exists)
                        {
                            corr_id = mail.Assign_correspondent.Value;
                        }
                        Console.WriteLine(exists);

                    }
                }
                else if (mail.Assign_correspondent_from == MailMetadataCorrespondentOption.FromNothing)
                {
                    corr_id = null;
                }

                mail.Assign_correspondent = corr_id;

                if (mail.Attachment_type == MailFilterAttachmentType.Attachments && mail.Consumption_scope == MailRuleConsumptionScope.Attachments) // Process attachments only if attachment_type is 1 and consumption_scope is 1
                {
                    ProcessorOnlyAttachementHelper.save_pdf_and_images_for_online_attachement(message, directorypath, mail, title, documents, tessDataPath, corr_id ?? Guid.Empty, httpClient);
                }
                else if (mail.Attachment_type == MailFilterAttachmentType.Attachments && mail.Consumption_scope == MailRuleConsumptionScope.Email_Only)// Process email body only if consumption_scope is 2
                {
                    ProcessorOnlyAttachementHelper.save_eml_files_for_online_attachement(message, directorypath, mail, title, documents, uid, corr_id ?? Guid.Empty, httpClient);// Check if the email meets the condition: no attachments but with inline attachments
                }
                else if (mail.Attachment_type == MailFilterAttachmentType.Attachments && mail.Consumption_scope == MailRuleConsumptionScope.Everything)
                {
                    ProcessorOnlyAttachementHelper.save_pdf_and_images_for_online_attachement(message, directorypath, mail, title, documents, tessDataPath, corr_id ?? Guid.Empty, httpClient);
                    ProcessorOnlyAttachementHelper.save_eml_files_for_online_attachement(message, directorypath, mail, title, documents, uid, corr_id ?? Guid.Empty, httpClient);
                }
                else if (mail.Attachment_type == MailFilterAttachmentType.Everything && mail.Consumption_scope == MailRuleConsumptionScope.Attachments)// Attachment Type : Everything*************ConsumptionScope : Attchaments
                {
                    ProcessorInlineAttachementHelper.save_pdf_and_images_for_online_attachement_including_inline(message, directorypath, mail, title, documents, content, tessDataPath, corr_id ?? Guid.Empty, httpClient);
                }
                else if (mail.Attachment_type == MailFilterAttachmentType.Everything && mail.Consumption_scope == MailRuleConsumptionScope.Email_Only)// Attachment Type : Everything*************ConsumptionScope : Email_Only
                {
                    ProcessorInlineAttachementHelper.save_eml_files_for_online_attachement_including_inline(message, directorypath, mail, title, documents, uid, corr_id ?? Guid.Empty, httpClient);
                }
                else if (mail.Attachment_type == MailFilterAttachmentType.Everything && mail.Consumption_scope == MailRuleConsumptionScope.Everything)// Attachment Type : Everything*************ConsumptionScope : Everything
                {
                    ProcessorInlineAttachementHelper.save_pdf_and_images_for_online_attachement_including_inline(message, directorypath, mail, title, documents, content, tessDataPath, corr_id ?? Guid.Empty, httpClient);
                    ProcessorInlineAttachementHelper.save_eml_files_for_online_attachement_including_inline(message, directorypath, mail, title, documents, uid, corr_id ?? Guid.Empty, httpClient);
                }
                Thread.Sleep(3000);

                return corr_id;
            }
            private async Task<Guid> GetGroupForUser(string idOwner)
            {
                try
                {
                    var res = await _userGroupPort.GetFirstGRoupForUser(idOwner);
                    var responseContent = await res.Content.ReadAsStringAsync();
                    if (res.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        Log.Error($"Error Message : {responseContent}");
                        throw new HttpRequestException("An error has occured, please try again later");
                    }
                    var JSONObj = JsonConvert.DeserializeObject<Guid>(responseContent)!;
                    return JSONObj;
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
}