using Application.Features.AssignDocumentMangement;
using Application.Respository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;
using Domain.DocumentManagement.tags;
using Domain.Templates.Enum;
using Newtonsoft.Json.Linq;
using Domain.FileTasks;
using Domain.Logs;
using Domain.Templates;
using Application.Helper;

namespace Infrastructure.Service
{
    public class ServiceUploadTextFile
    {
        private readonly IConfiguration _configuration;
        private readonly AssignTagToDocument _assignTag;
        private readonly AssignCorrespondentToDocument _assignCorrespondentToDocument;
        private readonly AssignDocumentTypeToDocument _assignDocumentTypeToDocument;
        private readonly AssignStoragePathToDocument _assignStoragePathToDocument;
        private readonly ITemplateRepository _templateRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly ITagRepository _repository;
        private readonly IFileTasksRepository _fileTasksRepository;
        private readonly ILogRepository _logRepository;

        public ServiceUploadTextFile(IConfiguration configuration, AssignTagToDocument assignTag, AssignCorrespondentToDocument assignCorrespondentToDocument, AssignDocumentTypeToDocument assignDocumentTypeToDocument, AssignStoragePathToDocument assignStoragePathToDocument, ITemplateRepository templateRepository, IDocumentRepository documentRepository, ITagRepository repository, IFileTasksRepository fileTasksRepository, ILogRepository logRepository)
        {
            _configuration = configuration;
            _assignTag = assignTag;
            _assignCorrespondentToDocument = assignCorrespondentToDocument;
            _assignDocumentTypeToDocument = assignDocumentTypeToDocument;
            _assignStoragePathToDocument = assignStoragePathToDocument;
            _templateRepository = templateRepository;
            _documentRepository = documentRepository;
            _repository = repository;
            _fileTasksRepository = fileTasksRepository;
            _logRepository = logRepository;
        }

        public async Task SaveTextFile(IFormFile file, string? type, string idowner)
        {
            Logs init = Logs.Create(LogLevel.INFO, LogName.EasyDoc, "Consuming " + file.FileName);
            Logs mimeType = Logs.Create(LogLevel.DEBUG, LogName.EasyDoc, "mime type: " + file.ContentType);
            await _logRepository.AddAsync(init);
            await _logRepository.AddAsync(mimeType);
            // Create a base destination path
            string baseDestinationPath = GetBaseDestinationPath();

            // Extract the file name without extension
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);

            // Create a directory with the file name
            string destinationFolderPath = Path.Combine(baseDestinationPath, fileNameWithoutExtension);
            Directory.CreateDirectory(destinationFolderPath);

            // Save the .txt file to the destination folder
            string txtFilePath = Path.Combine(destinationFolderPath, file.FileName);
            using (FileStream fileStream = new FileStream(txtFilePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Read the text content from the .txt file
            string textContent = File.ReadAllText(txtFilePath);

            // Convert the text content to PDF using MigraDoc
            await ConvertTextToPdfUsingMigraDoc(textContent, fileNameWithoutExtension, destinationFolderPath);

            // Output the path of the converted PDF file
            string pdfFilePath = Path.Combine(destinationFolderPath, $"{fileNameWithoutExtension}.pdf");
            Console.WriteLine($"PDF file saved at: {pdfFilePath}");







            // Calculate checksum
            string checksum = FileHelper.CalculateMD5(txtFilePath);
            string lang = FileHelper.DetectLanguage(textContent);
            // Create a Document object
            Domain.Documents.Document documenttoadd = Domain.Documents.Document.Create(fileNameWithoutExtension, "", textContent, baseDestinationPath + fileNameWithoutExtension + "\\" + fileNameWithoutExtension + ".pdf", idowner, file.ContentType, checksum);
            documenttoadd.Lang = lang;
            if (type != null && type.Equals("FileShare"))
            {
                documenttoadd.Source = DocumentSource.ConsumeFolder;
            }
            else
            {
                documenttoadd.Source = DocumentSource.ApiUpload;
            }
            string thumbnail_url = FileHelper.CreateThumbnailOfADocumentAsync(documenttoadd, _logRepository).Result;
            documenttoadd.ThumbnailUrl = thumbnail_url;
            documenttoadd.Mailrule = null;


            //verify if the document has an owner or not
            if (!string.IsNullOrEmpty(idowner))
            {
                string responseObject = null;
                string classification_result = null;
                string uniqueSubdir = null;
                JObject uniqueSubdirJson = null;
                // Bring the ML Classification result 
                using (var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromMinutes(5) // Set timeout to 5 minutes
                })
                {
                    var apiUrl = "http://localhost:5003/classify_extract";
                    // Append the result as a query parameter to the URL
                    apiUrl += "?result=" + Uri.EscapeDataString(textContent);

                    try
                    {
                        // Send the POST request to the Flask endpoint with the result as a query parameter
                        var response = await httpClient.PostAsync(apiUrl, null);

                        // Ensure the request completed successfully
                        response.EnsureSuccessStatusCode();

                        // Read the response content as a string
                        var responseBody = await response.Content.ReadAsStringAsync();

                        //// Parse the JSON string into a JObject
                        //JObject jsonResponse = JObject.Parse(responseBody);

                        JArray jsonResponse = JArray.Parse(responseBody);

                        classification_result = (string)jsonResponse[0];
                        string uniqueSubdirJsonString = (string)jsonResponse[1];

                        uniqueSubdirJson = JObject.Parse(uniqueSubdirJsonString);


                        Console.WriteLine("Classification Result: " + classification_result);
                        Console.WriteLine("Unique Subdir JSON: " + uniqueSubdirJson.ToString());

                        //// Access the text and path fields
                        //classification_result = jsonResponse["generated_texts"].ToString();
                        //uniqueSubdir = jsonResponse["unique_subdir"].ToString();

                        // Handle the response as needed
                        Console.WriteLine("Generated Texts:");

                        Console.WriteLine("Unique Subdirectory: " + uniqueSubdir);

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("Result sent successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Failed to send result. Status code: " + response.StatusCode);
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine("Failed to send result. Error: " + ex.Message);
                    }
                }


                //List<Domain.Templates.Template> templates = await _templateRepository.GetAllByOrderAsync();
                List<Template> templates = await _templateRepository.GetAllByOwner(idowner);

                foreach (Template template in templates)
                {
                    if (template.Type == GetListByType.Started && template.Is_Enabled == true) {
                        bool filter_result = FileHelper.ConsumptionStartedFilter(documenttoadd, template, classification_result);
                        //check if we're in the case of consumption started 
                        if ( filter_result )
                        {

                            if (template.AssignTitle != null)
                            {
                                documenttoadd.Title = template.AssignTitle;
                            }
                            if (template.AssignDocumentType != null)
                            {
                                documenttoadd.DocumentTypeId = template.AssignDocumentType;
                            }
                            if (template.AssignCorrespondent != null)
                            {
                                documenttoadd.CorrespondentId = template.AssignCorrespondent;
                            }
                            if (template.AssignStoragePath != null)
                            {
                                documenttoadd.StoragePathId = template.AssignStoragePath;
                            }
                            if (template.AssignTags != null)
                            {
                                List<DocumentTags> tags = new List<DocumentTags>();
                                foreach (Guid id in template.AssignTags)
                                {
                                    Tag tagtoadd = _repository.FindByIdAsync(id).GetAwaiter().GetResult();

                                    DocumentTags documentTag = new DocumentTags
                                    {
                                        Document = documenttoadd,
                                        DocumentId = documenttoadd.Id,
                                        Tag = tagtoadd,
                                        TagId = tagtoadd.Id
                                    };
                                    tags.Add(documentTag);

                                }
                                documenttoadd.Tags = tags;
                                break;
                            }
                        } 
                    }
                }
                if (documenttoadd.Tags == null && documenttoadd.CorrespondentId == null && documenttoadd.DocumentTypeId == null && documenttoadd.StoragePathId == null)
                {
                    // assign tags, documentType, Correspondent , storage path
                    //await FileHelper.AssignDocumentProperties(documenttoadd, idowner, _assignTag, _assignCorrespondentToDocument, _assignDocumentTypeToDocument, _assignStoragePathToDocument, classification_result, uniqueSubdirJson);
                }
                // check if we're in the case of Document Added
                foreach (Domain.Templates.Template template in templates)
                {
                    if (template.Type == GetListByType.Added && template.Is_Enabled == true) {


                        bool filter_result = FileHelper.DocumentAddedFilter(documenttoadd, template);
                        //check if we're in the case of Document Added 
                        if ( filter_result)
                        {

                            if (template.AssignTitle != null)
                            {
                                documenttoadd.Title = template.AssignTitle;
                            }
                            if (template.AssignDocumentType != null)
                            {
                                documenttoadd.DocumentTypeId = template.AssignDocumentType;
                            }
                            if (template.AssignCorrespondent != null)
                            {
                                documenttoadd.CorrespondentId = template.AssignCorrespondent;
                            }
                            if (template.AssignStoragePath != null)
                            {
                                documenttoadd.StoragePathId = template.AssignStoragePath;
                            }
                            if (template.AssignTags != null)
                            {
                                List<DocumentTags> tags = new List<DocumentTags>();
                                foreach (Guid id in template.AssignTags)
                                {
                                    Tag tagtoadd = _repository.FindByIdAsync(id).GetAwaiter().GetResult();

                                    DocumentTags documentTag = new DocumentTags
                                    {
                                        Document = documenttoadd,
                                        DocumentId = documenttoadd.Id,
                                        Tag = tagtoadd,
                                        TagId = tagtoadd.Id
                                    };
                                    tags.Add(documentTag);

                                }
                                documenttoadd.Tags = tags;
                                break;
                            }
                        }
                    }
                }
            }

            Problem the_problem = Problem.None;

            if (documenttoadd.Owner == null || documenttoadd.CorrespondentId == null)
            {
                if (documenttoadd.Owner == null && documenttoadd.CorrespondentId == null)
                {
                    the_problem = Problem.NoOwnerNoCorrespondent;
                }
                else if (documenttoadd.Owner == null)
                {
                    the_problem = Problem.NoOwner;
                }
                else if (documenttoadd.CorrespondentId == null)
                {
                    the_problem = Problem.NoCorrespondent;
                }

                FileTasks fileTaskToAdd = new FileTasks
                {
                    Source = documenttoadd.Source,
                    Task_document = documenttoadd,
                    Task_problem = the_problem
                };

                await _fileTasksRepository.AddAsync(fileTaskToAdd);
            }


            Logs create = Logs.Create(LogLevel.DEBUG, LogName.EasyDoc, "Saving record to database");

            await _logRepository.AddAsync(create);

            //await Assign_tags_correspondents_storagepaths_doctype_in_case_of_template_existing(documenttoadd);
            await _documentRepository.AddAsync(documenttoadd);

        }

        private async Task ConvertTextToPdfUsingMigraDoc(string textContent, string fileNameWithoutExtension, string destinationFolderPath)
        {
            // Create a new MigraDoc document
            MigraDocCore.DocumentObjectModel.Document document = new MigraDocCore.DocumentObjectModel.Document();
            Section section = document.AddSection();

            // Add the text content to the document
            Paragraph paragraph = section.AddParagraph();
            paragraph.AddText(textContent);

            // Create a renderer for the MigraDoc document
            PdfDocumentRenderer renderer = new PdfDocumentRenderer();
            renderer.Document = document;

            // Render the document
            renderer.RenderDocument();

            // Save the PDF to a file
            string pdfFilePath = Path.Combine(destinationFolderPath, $"{fileNameWithoutExtension}.pdf");
            renderer.PdfDocument.Save(pdfFilePath);

            Console.WriteLine($"PDF file '{pdfFilePath}' created successfully.");


        }

        private string GetBaseDestinationPath()
        {
            return _configuration["OriginalsSettings:OutputFolder"];
        }
    }
}

