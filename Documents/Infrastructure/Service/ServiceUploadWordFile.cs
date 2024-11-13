using Application.Features.AssignDocumentMangement;
using Application.Helper;
using Application.Respository;
using Domain.DocumentManagement.tags;
using Domain.FileTasks;
using Domain.Logs;
using Domain.Templates;
using Domain.Templates.Enum;
using Infrastructure.Service.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.XWPF.UserModel;
using System.Text;
using Tesseract;


namespace Infrastructure.Service
{
    public class ServiceUploadWordFile
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


        public ServiceUploadWordFile(IConfiguration configuration, AssignTagToDocument assignTag, AssignCorrespondentToDocument assignCorrespondentToDocument, AssignDocumentTypeToDocument assignDocumentTypeToDocument, AssignStoragePathToDocument assignStoragePathToDocument, ITemplateRepository templateRepository, IDocumentRepository documentRepository, ITagRepository repository, IFileTasksRepository fileTasksRepository, ILogRepository logRepository)
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

        public async Task SaveWordFile(IFormFile file, string? type, string idowner)
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

            // Save the Word file to the destination folder
            string wordFilePath = Path.Combine(destinationFolderPath, file.FileName);
            using (FileStream fileStream = new FileStream(wordFilePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }






            // Create an instance of Word Application
            Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();

            // Open the Word document
            Microsoft.Office.Interop.Word.Document document = wordApp.Documents.Open(wordFilePath);

            // Save the document as PDF
            document.SaveAs2(Path.ChangeExtension(wordFilePath, "pdf"), Microsoft.Office.Interop.Word.WdSaveFormat.wdFormatPDF);

            // Close the document
            document.Close();

            // Quit Word application
            wordApp.Quit();




            // Open the saved Word file directly for reading
            XWPFDocument wordDocument = new XWPFDocument(File.OpenRead(wordFilePath));

            // Create a StringBuilder to store the text content
            StringBuilder result = new StringBuilder();

            // Iterate over paragraphs in the document
            foreach (XWPFParagraph paragraph in wordDocument.Paragraphs)
            {
                // Get text from the paragraph
                string text = paragraph.Text;

                // Append text to the result StringBuilder
                result.AppendLine(text);
            }
            foreach (XWPFTable table in wordDocument.Tables)
            {
                string text = table.Text;
                result.AppendLine(text);
            }
            foreach (XWPFPictureData picture in wordDocument.AllPictures)
            {
                byte[] data = picture.Data;
                var tessDataPath = @"C:\Users\MSI\Documents\GitHub\paperless\Tessdata\tesseract-ocr-tesseract-bc059ec\tessdata";
                using (var engine = new TesseractEngine(tessDataPath, "ara+eng+fra", EngineMode.Default))
                {
                    using (var ms = new MemoryStream(data))
                    {
                        try
                        {
                            // Load the image data into a Pix object directly
                            using (var pix = Pix.LoadFromMemory(data))
                            {
                                if (pix == null)
                                {
                                    Console.WriteLine("Error: Failed to load image data into Pix object.");
                                    continue;
                                }
                                using (var pageOcr = engine.Process(pix))
                                {
                                    var text = pageOcr.GetText();
                                    result.AppendLine(text);
                                    Console.WriteLine($"Text extracted from image on page :\n{text}");
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing image on page : {ex.Message}");
                        }
                    }
                }
            }
            // Dispose of the Word document object
            wordDocument.Close();

            // Save the content of the Word document to a .txt file
            string textFilePath = Path.Combine(destinationFolderPath, $"{fileNameWithoutExtension}.txt");
            File.WriteAllText(textFilePath, result.ToString());








            // Calculate checksum
            string checksum = FileHelper.CalculateMD5(wordFilePath);
            // get Language 
            string lang = FileHelper.DetectLanguage(result.ToString());
            // Create a Document object
            Domain.Documents.Document documenttoadd = Domain.Documents.Document.Create(fileNameWithoutExtension, "", result.ToString(), baseDestinationPath + fileNameWithoutExtension + "\\" + fileNameWithoutExtension + ".pdf", idowner, file.ContentType, checksum);
            documenttoadd.Lang = lang;
            //documenttoadd.Source = DocumentSource.ApiUpload;
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
                    apiUrl += "?result=" + Uri.EscapeDataString(result.ToString());

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
                    if (template.Type == GetListByType.Started && template.Is_Enabled == true)
                    {

                        bool filter_result = FileHelper.ConsumptionStartedFilter(documenttoadd, template, classification_result);
                        //check if we're in the case of consumption started 
                        if (filter_result)
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
                   // await FileHelper.AssignDocumentProperties(documenttoadd, idowner, _assignTag, _assignCorrespondentToDocument, _assignDocumentTypeToDocument, _assignStoragePathToDocument, classification_result, uniqueSubdirJson);
                }
                // check if we're in the case of Document Added
                foreach (Template template in templates)
                {

                    if (template.Type == GetListByType.Added && template.Is_Enabled == true) {
                        bool filter_result = FileHelper.DocumentAddedFilter(documenttoadd, template);
                        //check if we're in the case of Document Added 
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

            // Output the content of the Word document to the console
            Console.WriteLine("Content: \n" + result.ToString());
            Console.WriteLine("--------------------------------------");
        }

        private string GetBaseDestinationPath()
        {
            return _configuration["OriginalsSettings:OutputFolder"];
        }


    }
}