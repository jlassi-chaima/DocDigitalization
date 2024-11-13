using Application.Respository;
using Domain.Documents;
using MapsterMapper;
using MediatR;
using Application.Services;
using Application.Dtos.Documents;
using Application.Features.AssignDocumentMangement;
using Domain.DocumentManagement.tags;
using Domain.DocumentManagement.DocumentTypes;
using Domain.Ports;
using Serilog;
using Application.Features.FeaturesDocument.Documents;
using Application.Helper;
using Domain.DocumentManagement;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using PdfSharp.Pdf;
using Newtonsoft.Json;



namespace Application.Features.FeaturesDocument;

public static class AddDocument
{
    public sealed record Command : IRequest<List<Document>>
    {
        public IFormFile FormData { get; init; }
        public string Document { get; init; }
        public string Type { get; init; }


        public Command(IFormFile formData,string document,string type)
        {
            FormData = formData;
            Document=document;
            Type= type;
        }
    }
    public sealed class Handler : IRequestHandler<Command,List< Document>>
    {

        private readonly IAzurePort _azurePort;
        private readonly IUploadDocumentUseCase _uploadDocumentUseCase;
        private readonly IDocumentTypeRepository _documentTypeRepository;


        public Handler(IDocumentRepository repository,
            IMapper mapper,
            ExtractASNservice extractASNservice,
            AssignTagToDocument assignTag,
            AssignDocumentTypeToDocument assignDocumentTypeToDocument,
            AssignCorrespondentToDocument assignCorrespondentToDocument,
            ITagRepository tagRepository,
            IDocumentTypeRepository documenttype,
            ICorrespondentRepository correspondent,
            ITemplateRepository templateRepository,
            IFileTasksRepository tasksRepository,
            ILogRepository logRepository,
            IAzurePort azurePort,
            IUploadDocumentUseCase uploadDocumentUseCase,
            IDocumentTypeRepository documentTypeRepository)
        {
   
            _azurePort = azurePort;
            _uploadDocumentUseCase= uploadDocumentUseCase;
            _documentTypeRepository= documentTypeRepository;

        }
        public async Task<List<Document>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                DocumentAPI documentAPI = new DocumentAPI();
                List <Document> docs= new List < Document >();
                if (!string.IsNullOrEmpty(request.Document))
                {
                    documentAPI = JsonConvert.DeserializeObject<DocumentAPI>(request.Document);

                    List<ASNInfo> pages = FileProccess.ExtractAndProcessASN(request.FormData);
                    byte[] pdfBytes;
                    using (var memoryStream = new MemoryStream())
                    {
                        request.FormData.CopyTo(memoryStream);
                        pdfBytes = memoryStream.ToArray();  // Store the file content as a byte array
                    }
                    PdfDocument pdfDocument = FileProccess.OpenPdfDocumentAsync(request.FormData);
                    if (pages.Count > 0)
                    {
                        for (int i = 0; i < pages.Count; i++)
                        {
                            int startPage = pages[i].PageNumber;
                            int endPage = (i < pages.Count - 1) ? pages[i + 1].PageNumber : pdfDocument.Pages.Count;
                            using (MemoryStream pageStream = FileProccess.ExtractPageAsStream(pdfDocument,startPage, endPage))
                            {

                                // Clone the stream for classification (to avoid reusing a closed stream)
                                using (MemoryStream clonedStream = FileProccess.CloneStream(pageStream))
                                {
                                    string docType = await _azurePort.ClassifyPdfDocumentType(clonedStream);
                                    pageStream.Position = 0;

                                    // Extract document details
                                    var dataExtract = await _azurePort.ExtractPdfDoc(pageStream, docType);

                                    // Check if the document type exists in the repository
                                    var docTypeExist = await _documentTypeRepository.FindByName(docType) ?? await AddDocumentType(docType, documentAPI.Owner, dataExtract.Content);
                                    Document documentToAdd = documentAPI.Adapt<Document>();
                                    if (documentAPI.Tags != null && documentAPI.Tags.Any())
                                    {
                                        documentToAdd.Tags = documentAPI.Tags
                                                            .Select(tagId => new DocumentTags { TagId = tagId })
                                                            .ToList();
                                    }
                                    else
                                    {
                                        documentToAdd.Tags = null; 
                                    }
                                    // Save the page as a document and process it
                                    Document doc = await _uploadDocumentUseCase.SavePageAsDocument(request.FormData, pdfDocument, dataExtract, docTypeExist, request.Type, documentAPI.Owner, pages[i], i, startPage, endPage, documentToAdd, pages.Count);
                                    docs.Add(doc);
                                    // Add or update custom fields for the document
                                    if (doc != null)
                                    {
                                        await _uploadDocumentUseCase.AddOrUpdateCustomFields(doc, dataExtract);

                                    }
                                    else
                                    {
                                        Log.Error("Can not add null Document");
                                        throw new Exception("Can not add null Document");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Document documentToAdd = documentAPI.Adapt<Document>();
                        if (documentAPI.Tags != null && documentAPI.Tags.Any())
                        {
                            documentToAdd.Tags = documentAPI.Tags
                                                .Select(tagId => new DocumentTags { TagId = tagId })
                                                .ToList();
                        }
                        else
                        {
                            documentToAdd.Tags = null;
                        }
                        //Document doc= await _uploadDocumentUseCase.ProcessEntireDocument(request.FormData, request.Type, documentAPI.Owner, documentToAdd,archive);
                        //docs.Add(doc);
                    }
                }

                return docs;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        //public async Task<Document> Handle(Command request, CancellationToken cancellationToken)
        //{
        //    Logs init = Logs.Create(LogLevel.INFO, LogName.Mail, "Consuming " + request.Document.Title);
        //    Logs mimeType = Logs.Create(LogLevel.DEBUG, LogName.Mail, "Detecting " + request.Document.MimeType);
        //    await _logRepository.
        //          AddAsync(init);
        //    await _logRepository.AddAsync(mimeType);
        //    Console.WriteLine(" documents Details v" + request.Document.Title + " " + request.Document.Content + " " + request.Document.Owner + " " + " " + request.Document.DocumentTypeId + " " + request.Document.CorrespondentId);
        //    request.Document.Content = Regex.Replace(request.Document.Content, @"[\x00-\x1F\x7F]", " ");
        //    string lang = FileHelper.DetectLanguage(request.Document.Content);
        //    var document = Document.Create(request.Document.Title, "", request.Document.Content, request.Document.FileData, request.Document.Owner, request.Document.MimeType, "");
        //    document.Lang = lang;
        //    document.Source = request.Document.Source;
        //    document.Mailrule = request.Document.Mailrule;
        //    string thumbnail_url = FileHelper.CreateThumbnailOfADocumentAsync(document, _logRepository).Result;
        //    document.ThumbnailUrl = thumbnail_url;







        //    string classification_result = null;
        //    string uniqueSubdir = null;
        //    string uniqueSubdirJsonString = null;
        //    JObject uniqueSubdirJson = null;


        //    if (document.MimeType != ".eml")
        //    {
        //        // Bring the ML Classification result 
        //        using (var httpClient = new HttpClient
        //        {
        //            Timeout = TimeSpan.FromMinutes(5) // Set timeout to 5 minutes
        //        })
        //        {
        //            var apiUrl = "http://localhost:5003/classify_extract";
        //            // Append the result as a query parameter to the URL
        //            apiUrl += "?result=" + Uri.EscapeDataString(request.Document.Content);

        //            try
        //            {
        //                // Send the POST request to the Flask endpoint with the result as a query parameter
        //                var response = await httpClient.PostAsync(apiUrl, null);

        //                // Ensure the request completed successfully
        //                response.EnsureSuccessStatusCode();

        //                // Read the response content as a string
        //                var responseBody = await response.Content.ReadAsStringAsync();

        //                //// Parse the JSON string into a JObject
        //                //JObject jsonResponse = JObject.Parse(responseBody);

        //                JArray jsonResponse = JArray.Parse(responseBody);

        //                classification_result = (string)jsonResponse[0];
        //                uniqueSubdirJsonString = (string)jsonResponse[1];

        //                uniqueSubdirJson = JObject.Parse(uniqueSubdirJsonString);


        //                Console.WriteLine("Classification Result: " + classification_result);
        //                Console.WriteLine("Unique Subdir JSON: " + uniqueSubdirJson.ToString());

        //                //// Access the text and path fields
        //                //classification_result = jsonResponse["generated_texts"].ToString();
        //                //uniqueSubdir = jsonResponse["unique_subdir"].ToString();

        //                // Handle the response as needed
        //                Console.WriteLine("Generated Texts:");

        //                Console.WriteLine("Unique Subdirectory: " + uniqueSubdir);

        //                if (response.IsSuccessStatusCode)
        //                {
        //                    Console.WriteLine("Result sent successfully.");
        //                }
        //                else
        //                {
        //                    Console.WriteLine("Failed to send result. Status code: " + response.StatusCode);
        //                }
        //            }
        //            catch (HttpRequestException ex)
        //            {
        //                Console.WriteLine("Failed to send result. Error: " + ex.Message);
        //            }
        //        }
        //    }


        //    //uniqueSubdirJson = JObject.Parse(uniqueSubdirJsonString);




        //    List<Template> templates = await _templateRepository.GetAllByOrderAndOwnerAsync(document.Owner);

        //    if (templates.Count > 0)
        //    {

        //        foreach (Template template in templates)
        //        {
        //            if (template.Type == GetListByType.Started && template.Is_Enabled == true)
        //            {
        //                bool filter_result = FileHelper.Consumption_started_filter(document, template, classification_result);
        //                //check if we're in the case of consumption started 
        //                if (filter_result)
        //                {
        //                    //getting tags of template in case it contains that
        //                    List<DocumentTags> template_tags = new List<DocumentTags>();
        //                    if (template.AssignTags != null)
        //                    {

        //                        foreach (Guid id in template.AssignTags)
        //                        {
        //                            Tag tagtoadd = _tagRepository.FindByIdAsync(id).GetAwaiter().GetResult();

        //                            DocumentTags documentTag = new DocumentTags
        //                            {
        //                                Document = document,
        //                                DocumentId = document.Id,
        //                                Tag = tagtoadd,
        //                                TagId = tagtoadd.Id
        //                            };
        //                            template_tags.Add(documentTag);

        //                        }

        //                    }

        //                    if (request.Document.Tags != null && request.Document.Tags.Count > 0 && template_tags != null && template_tags.Count > 0)
        //                    {
        //                        List<DocumentTags> documentTags = new List<DocumentTags>();
        //                        foreach (var tagId in request.Document.Tags)
        //                        {
        //                            Tag tag = await _tagRepository.FindByIdAsync(tagId, cancellationToken);
        //                            if (tag != null)
        //                            {
        //                                documentTags.Add(new DocumentTags
        //                                {
        //                                    DocumentId = document.Id,
        //                                    Document = document,
        //                                    TagId = tag.Id,
        //                                    Tag = tag
        //                                });
        //                            }
        //                        }
        //                        //concatinate template and mailrule tags to be added for the document
        //                        List<DocumentTags> document_tags_to_process = documentTags.Concat(template_tags)
        //                                                                                  .ToList();

        //                        document.Tags = document_tags_to_process;
        //                    }
        //                    else if (request.Document.Tags != null && request.Document.Tags.Count > 0)
        //                    {
        //                        List<DocumentTags> documentTags = new List<DocumentTags>();
        //                        foreach (var tagId in request.Document.Tags)
        //                        {
        //                            Tag tag = await _tagRepository.FindByIdAsync(tagId, cancellationToken);
        //                            if (tag != null)
        //                            {
        //                                documentTags.Add(new DocumentTags
        //                                {
        //                                    DocumentId = document.Id,
        //                                    Document = document,
        //                                    TagId = tag.Id,
        //                                    Tag = tag
        //                                });
        //                            }
        //                        }
        //                        document.Tags = documentTags;

        //                    }
        //                    else if (template_tags != null && template_tags.Count > 0)
        //                    {

        //                        document.Tags = template_tags;
        //                    }
        //                    else
        //                    {
        //                        List<Tag> tags = (List<Tag>)await _tagRepository.GetAllAsync();
        //                        if (tags.Count == 0)
        //                        {
        //                            document.Tags = null;
        //                        }
        //                        else
        //                        {
        //                            document.Tags = await _assignTag.AssignTag(document, request.Document.Owner);
        //                        }
        //                    }


        //                    if (request.Document.DocumentTypeId == null)
        //                    {
        //                        List<DocumentTypeDetailsDTO> documenttypes = await _documenttype.GetListDocumentTypeByOwner(request.Document.Owner);
        //                        if (documenttypes.Count == 0)
        //                        {
        //                            document.DocumentTypeId = null;
        //                        }
        //                        else
        //                        {
        //                            DocumentTypeDetailsDTO documentType = await _assignDocumentTypeToDocument.AssignDocumenttype(document, request.Document.Owner, classification_result, uniqueSubdirJson);
        //                            document.DocumentTypeId = documentType.Id;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (template.AssignDocumentType != null)
        //                        {
        //                            document.DocumentTypeId = template.AssignDocumentType;
        //                        }
        //                        else
        //                        {
        //                            document.DocumentTypeId = request.Document.DocumentTypeId;
        //                        }
        //                    }


        //                    if (request.Document.CorrespondentId == null)
        //                    {
        //                        List<Correspondent> correspondents = (List<Correspondent>)await _correspondent.GetAllAsync();
        //                        if (correspondents.Count == 0)
        //                        {
        //                            document.CorrespondentId = null;
        //                        }
        //                        else
        //                        {

        //                            CorrespondentListDTO correspondent = await _assignCorrespondentToDocument.AssignCorrespondent(document, request.Document.Owner);
        //                            correspondent.Last_correspondence = DateTime.UtcNow;
        //                            Correspondent c = _mapper.Map<Correspondent>(correspondent);
        //                            await _correspondent.UpdateAsync(c, cancellationToken);
        //                            document.CorrespondentId = correspondent.Id;

        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (template.AssignCorrespondent != null)
        //                        {
        //                            document.CorrespondentId = template.AssignCorrespondent;
        //                        }
        //                        else
        //                        {
        //                            document.CorrespondentId = request.Document.CorrespondentId;
        //                        }
        //                    }
        //                    break;
        //                }
        //            }
        //        }

        //        if (document.CorrespondentId == null && document.Tags == null && document.StoragePathId == null && document.DocumentTypeId == null)
        //        {
        //            // Check and assign tags 
        //            List<DocumentTags> documentTags = await _assignTag.AssignTag(document, request.Document.Owner);
        //            document.Tags = documentTags != null ? documentTags : null;

        //            // Check and assign Correspondent
        //            CorrespondentListDTO correspondent = await _assignCorrespondentToDocument.AssignCorrespondent(document, request.Document.Owner);
        //            document.CorrespondentId = correspondent?.Id;

        //            // Check and assign Document Type
        //            DocumentTypeDetailsDTO documentType = await _assignDocumentTypeToDocument.AssignDocumenttype(document, request.Document.Owner, classification_result, uniqueSubdirJson);
        //            document.DocumentTypeId = documentType?.Id;

        //        }
        //    }


        //    else //in case there are not templates available
        //    {
        //        if (request.Document.Tags != null && request.Document.Tags.Count > 0)
        //        {
        //            List<DocumentTags> documentTags = new List<DocumentTags>();
        //            foreach (var tagId in request.Document.Tags)
        //            {
        //                Tag tag = await _tagRepository.FindByIdAsync(tagId, cancellationToken);
        //                if (tag != null)
        //                {
        //                    documentTags.Add(new DocumentTags
        //                    {
        //                        DocumentId = document.Id,
        //                        Document = document,
        //                        TagId = tag.Id,
        //                        Tag = tag
        //                    });
        //                }
        //            }
        //            document.Tags = documentTags;
        //        }
        //        else
        //        {
        //            document.Tags = await _assignTag.AssignTag(document, request.Document.Owner);
        //        }


        //        if (request.Document.DocumentTypeId == null)
        //        {
        //            //List<DocumentTypeDetailsDTO> documentTypes = await _documentTypeRepository.GetDocumentTypesDetailsAsync();
        //            List<DocumentTypeDetailsDTO> documenttypes = await _documenttype.GetListDocumentTypeByOwner(request.Document.Owner);

        //            //  List<DocumentType> documenttypes = (List<DocumentType>)await _documenttype.GetAllAsync();
        //            if (documenttypes.Count == 0)
        //            {
        //                document.DocumentTypeId = null;
        //            }
        //            else
        //            {
        //                DocumentTypeDetailsDTO documentType = await _assignDocumentTypeToDocument.AssignDocumenttype(document, request.Document.Owner, classification_result, uniqueSubdirJson);
        //                if (documentType != null)
        //                {
        //                    document.DocumentTypeId = documentType.Id;
        //                }

        //            }
        //        }
        //        else
        //        {
        //            document.DocumentTypeId = request.Document.DocumentTypeId;
        //        }


        //        if (request.Document.CorrespondentId == null)
        //        {
        //            List<Correspondent> correspondents = (List<Correspondent>)await _correspondent.GetAllAsync();
        //            if (correspondents.Count == 0)
        //            {
        //                document.CorrespondentId = null;
        //            }
        //            else
        //            {
        //                CorrespondentListDTO correspondent = await _assignCorrespondentToDocument.AssignCorrespondent(document, request.Document.Owner);
        //                if (correspondent != null)
        //                {
        //                    document.CorrespondentId = correspondent.Id;
        //                }

        //            }
        //        }
        //        else
        //        {
        //            document.CorrespondentId = request.Document.CorrespondentId;
        //        }
        //    }





        //    if (templates.Count > 0)
        //    {

        //        // check if we're in the case of Document Added
        //        foreach (Template template in templates)
        //        {
        //            if (template.Type == GetListByType.Added && template.Is_Enabled == true)
        //            {

        //                bool filter_result = FileHelper.Document_Added_filter(document, template);
        //                //check if we're in the case of Document Added 
        //                if (filter_result)
        //                {

        //                    if (template.AssignTitle != null)
        //                    {
        //                        document.Title = template.AssignTitle;
        //                    }
        //                    if (template.AssignDocumentType != null)
        //                    {
        //                        document.DocumentTypeId = template.AssignDocumentType;
        //                    }
        //                    if (template.AssignCorrespondent != null)
        //                    {
        //                        document.CorrespondentId = template.AssignCorrespondent;
        //                    }
        //                    if (template.AssignStoragePath != null)
        //                    {
        //                        document.StoragePathId = template.AssignStoragePath;
        //                    }
        //                    if (template.AssignTags != null)
        //                    {
        //                        List<DocumentTags> tags = new List<DocumentTags>();
        //                        foreach (Guid id in template.AssignTags)
        //                        {
        //                            Tag tagtoadd = _tagRepository.FindByIdAsync(id).GetAwaiter().GetResult();

        //                            DocumentTags documentTag = new DocumentTags
        //                            {
        //                                Document = document,
        //                                DocumentId = document.Id,
        //                                Tag = tagtoadd,
        //                                TagId = tagtoadd.Id
        //                            };
        //                            tags.Add(documentTag);

        //                        }
        //                        document.Tags = tags;
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    await _repository.AddAsync(document, cancellationToken);
        //    // File Task 
        //    Problem the_problem = Problem.None;

        //    if (document.Owner == null || document.CorrespondentId == null)
        //    {
        //        if (document.Owner == null && document.CorrespondentId == null)
        //        {
        //            the_problem = Problem.NoOwnerNoCorrespondent;
        //        }
        //        else if (document.Owner == null)
        //        {
        //            the_problem = Problem.NoOwner;
        //        }
        //        else if (document.CorrespondentId == null)
        //        {
        //            the_problem = Problem.NoCorrespondent;
        //        }

        //        FileTasks fileTaskToAdd = new FileTasks
        //        {
        //            Source = document.Source,
        //            Task_document = document,
        //            Task_problem = the_problem
        //        };

        //        await _fileTasksRepository.AddAsync(fileTaskToAdd);
        //    }
        //    Logs create = Logs.Create(LogLevel.DEBUG, LogName.Mail, "Saving record to database");

        //    return document;
        //}




        public async Task<string> ConvertFileToStream(string filePath)
        {
            // Initialize a string to hold the file content
            string content = string.Empty;

            // Ensure to use async stream reading for efficiency
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            {
                byte[] buffer = new byte[fileStream.Length];
                await fileStream.ReadAsync(buffer, 0, buffer.Length);

                // Convert buffer to string using UTF8 encoding
                content = System.Text.Encoding.UTF8.GetString(buffer);
                Console.WriteLine(content);
            }

            return content;
        }

        public Microsoft.AspNetCore.Http.IFormFile ConvertByteArrayToIFormFile(byte[] fileBytes, string fileName, string contentType)
            {
                var stream = new MemoryStream(fileBytes);

                IFormFile formFile = new FormFile(stream, 0, fileBytes.Length, "formData", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = contentType
                };

                return formFile;
            }
        

        public async Task<DocumentType> AddDocumentType(string docType, string ownerId, string content)
        {
            var words = FileProccess.ExtractWords(content);
            var newDocumentType = new DocumentType
            {
                Name = docType,
                Slug = docType,
                // edit
                Match = words,
                Matching_algorithm = Matching_Algorithms.MATCH_ANY,
                Is_insensitive = true,
                Owner = ownerId,
                Document_count = 1,
                Documents = new List<Document>(),
                ExtractedData = new List<Guid>(),
                UsersView = new List<string>(),
                GroupsView = new List<string>(),
                UsersChange = new List<string>(),
                GroupsChange = new List<string>()
            };


            await _documentTypeRepository.AddAsync(newDocumentType);
            return newDocumentType;
        }

    }
}