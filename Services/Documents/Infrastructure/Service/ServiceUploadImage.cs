using Application.Features.AssignDocumentMangement;
using Application.Helper;
using Application.Respository;
using Domain.DocumentManagement.tags;
using Domain.Documents;
using Domain.FileTasks;
using Domain.Logs;
using Domain.Templates;
using Domain.Templates.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Tesseract;

namespace Infrastructure.Service
{
    public class ServiceUploadImage
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

        public ServiceUploadImage(IConfiguration configuration, AssignTagToDocument assignTag, AssignCorrespondentToDocument assignCorrespondentToDocument, AssignDocumentTypeToDocument assignDocumentTypeToDocument, AssignStoragePathToDocument assignStoragePathToDocument, ITemplateRepository templateRepository, IDocumentRepository documentRepository, ITagRepository repository, IFileTasksRepository fileTasksRepository, ILogRepository logRepository)
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

        public async Task SaveImageFile(IFormFile file, string? type, string idowner,string result)
        {
            Logs init = Logs.Create(LogLevel.INFO, LogName.EasyDoc, "Consuming " + file.FileName);
            //get mimeType
            Logs mimeType = Logs.Create(LogLevel.DEBUG, LogName.EasyDoc, "Detecting " + file.ContentType);
            await _logRepository.AddAsync(init);
            await _logRepository.AddAsync(mimeType);
            //create directory orginals  
            string baseDestinationPath = GetBaseDestinationPath();
            Directory.CreateDirectory(baseDestinationPath);
            var imageFile = file;
            //string result = "";

            using (var stream = new MemoryStream())
            {
                // pour lire le contenu d'image
                imageFile.CopyTo(stream);
                stream.Position = 0;
                var tessDataPath = @"C:\Users\MSI\Documents\GitHub\paperless\Tessdata\tesseract-ocr-tesseract-bc059ec\tessdata";
                using (var engine = new TesseractEngine(tessDataPath, "ara+eng+fra", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromMemory(stream.ToArray()))
                    {
                        // extraxt text 
                        using (var page = engine.Process(img))
                        {
                            result = page.GetText();

                        }
                    }
                }
            }




            // Create Directory for image
            string NameFileWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
            string folderPath = Path.Combine(baseDestinationPath, NameFileWithoutExtension);
            Directory.CreateDirectory(folderPath);
            string fileName = Path.GetFileName(file.FileName);
            string filePath = Path.Combine(folderPath, fileName);
            //Création du fichier et copie des données
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }



            // Create a new PDF document
            PdfDocument document = new PdfDocument();

            // Add a page to the document
            PdfPage pdf_page = document.AddPage();

            // Get an XGraphics object for drawing on the page
            XGraphics gfx = XGraphics.FromPdfPage(pdf_page);

            // Load an image from file
            XImage image = XImage.FromFile(filePath); // Provide the path to your image file

            // Get the width and height of the image
            double width = image.PixelWidth * 0.75; // Adjust the scaling factor as needed
            double height = image.PixelHeight * 0.75; // Adjust the scaling factor as needed

            // Draw the image on the page
            gfx.DrawImage(image, 0, 0, width, height); // Adjust the position and size as needed

            // Save the document to a file
            string filename = NameFileWithoutExtension + ".pdf";
            // Provide the desired output file name
            document.Save(folderPath + "\\" + filename);

            // Close the document
            document.Close();



            string textFileName = Path.GetFileNameWithoutExtension(file.FileName) + ".txt";
            string textFilePath = Path.Combine(folderPath, textFileName);
            File.WriteAllText(textFilePath, result);

            string checksum = FileHelper.CalculateMD5(filePath);
            string lang = FileHelper.DetectLanguage(result);
            Document documenttoadd = Document.Create(NameFileWithoutExtension, "", result, baseDestinationPath + NameFileWithoutExtension + "\\" + filename, idowner, file.ContentType, checksum);
            documenttoadd.Lang = lang;
            if (type != null && type.Equals("FileShare"))
            {
                documenttoadd.Source = DocumentSource.ConsumeFolder;
            }
            else
            {
                documenttoadd.Source = DocumentSource.ApiUpload;
            }
            documenttoadd.Mailrule = null;
            string thumbnail_url = FileHelper.CreateThumbnailOfADocumentAsync(documenttoadd, _logRepository).Result;
            documenttoadd.ThumbnailUrl = thumbnail_url;



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
                    //apiUrl += "?result=" + Uri.EscapeDataString(result);
                    apiUrl += "?result=" + result;

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






                //List<Template> templates = await _templateRepository.GetAllByOrderAsync(); 
                List<Template> templates = await _templateRepository.GetAllByOwner(idowner);
                if (templates != null)
                {
                    foreach (Template template in templates)
                    {
                        if (template.Type == GetListByType.Started && template.Is_Enabled == true)
                        {

                            bool filter_result = FileHelper.ConsumptionStartedFilter(documenttoadd, template, classification_result);
                            //check if we're in the case of consumption started 
                            if (filter_result )
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


                               
                                List<DocumentTags> tags = new List<DocumentTags>();
                                if (template.AssignTags != null)
                                {
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
                                    if (documenttoadd.Tags.Count == 0)
                                    {
                                        documenttoadd.Tags = null;
                                    }
                                    break;
                                }
                            }
                        }
                    }

                }

                if (documenttoadd.Tags == null && documenttoadd.CorrespondentId == null && documenttoadd.DocumentTypeId == null && documenttoadd.StoragePathId == null)
                {
                    // assign tags, documentType, Correspondent , storage path
                  //  await FileHelper.AssignDocumentProperties(documenttoadd, idowner, _assignTag, _assignCorrespondentToDocument, _assignDocumentTypeToDocument, _assignStoragePathToDocument, classification_result, uniqueSubdirJson);
                }
                if (templates != null)
                {

                    // check if we're in the case of Document Added
                    foreach (Template template in templates)
                    {
                        if (template.Type == GetListByType.Added && template.Is_Enabled == true)
                        {
                            bool filter_result = FileHelper.DocumentAddedFilter(documenttoadd, template);

                            //check if we're in the case of Document Added 

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



            await _documentRepository.AddAsync(documenttoadd);
            //await _documentRepository.IndexDocumentToElasticsearchAsync(documenttoadd);
        }
        private string GetBaseDestinationPath()
        {
            return _configuration["OriginalsSettings:OutputFolder"];
        }
    }
}
