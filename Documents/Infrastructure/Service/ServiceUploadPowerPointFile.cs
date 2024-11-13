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
using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace Infrastructure.Service
{
    public class ServiceUploadPowerPointFile
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

        public ServiceUploadPowerPointFile(IConfiguration configuration, AssignTagToDocument assignTag, AssignCorrespondentToDocument assignCorrespondentToDocument, AssignDocumentTypeToDocument assignDocumentTypeToDocument, AssignStoragePathToDocument assignStoragePathToDocument, ITemplateRepository templateRepository, IDocumentRepository documentRepository, ITagRepository repository, IFileTasksRepository fileTasksRepository, ILogRepository logRepository)
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

        public async Task SavePowerPointFile(IFormFile file, string? type, string idowner)
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

            // Save the PowerPoint file to the destination folder
            string powerPointFilePath = Path.Combine(destinationFolderPath, file.FileName);
            using (FileStream fileStream = new FileStream(powerPointFilePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            // Create an instance of PowerPoint Application
            Microsoft.Office.Interop.PowerPoint.Application powerPointApp = new Microsoft.Office.Interop.PowerPoint.Application();

            // Open the presentation
            Microsoft.Office.Interop.PowerPoint.Presentation presentation = powerPointApp.Presentations.Open(powerPointFilePath, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);

            string pdfFilePath = Path.ChangeExtension(powerPointFilePath, "pdf");
            // Save the presentation as PDF
            presentation.SaveAs(pdfFilePath, PpSaveAsFileType.ppSaveAsPDF);

            // Create a StringBuilder to store the text content
            StringBuilder result = new StringBuilder();

            // Iterate over slides in the presentation
            foreach (Slide slide in presentation.Slides)
            {
                // Iterate over shapes in the slide
                foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                {
                    // Check if the shape contains text
                    if (shape.HasTextFrame == MsoTriState.msoTrue && shape.TextFrame.HasText == MsoTriState.msoTrue)
                    {
                        // Append text from the shape to the result StringBuilder
                        result.AppendLine(shape.TextFrame.TextRange.Text);
                    }
                    if (shape.Type == MsoShapeType.msoPicture)
                    {

                        // Export the shape as an image file
                        string imagePath = ExportShapeAsImage(shape);

                        // Perform OCR on the exported image file using Tesseract
                        if (!string.IsNullOrEmpty(imagePath))
                        {
                            using (var engine = new TesseractEngine(@"C:\Users\MSI\Documents\GitHub\paperless\Tessdata\tesseract-ocr-tesseract-bc059ec\tessdata", "eng+ara+fra", EngineMode.Default))
                            {
                                using (var image = Pix.LoadFromFile(imagePath))
                                {
                                    using (var page = engine.Process(image))
                                    {
                                        var text = page.GetText();

                                        // Append the extracted text to the result StringBuilder
                                        result.AppendLine(text);

                                        // Print the extracted text
                                        Console.WriteLine($"Text extracted from image on page:\n{text}");
                                    }
                                }
                            }
                        }

                    }

                }
            }

            // Close the presentation
            presentation.Close();

            // Quit PowerPoint Application
            powerPointApp.Quit();

            // Save the text content of the PowerPoint presentation to a .txt file
            string textFilePath = Path.Combine(destinationFolderPath, $"{fileNameWithoutExtension}.txt");
            File.WriteAllText(textFilePath, result.ToString());







            // Output the content of the PowerPoint presentation to the console
            Console.WriteLine("Content: \n" + result.ToString());
            Console.WriteLine("--------------------------------------");

            string lang = FileHelper.DetectLanguage(result.ToString());
            // Calculate checksum
            string checksum = FileHelper.CalculateMD5(powerPointFilePath);

            // Create a Document object
            Domain.Documents.Document documenttoadd = Domain.Documents.Document.Create(fileNameWithoutExtension, "", result.ToString(), pdfFilePath, idowner, file.ContentType, checksum);
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



                //List<Template> templates = await _templateRepository.GetAllByOrderAsync();
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
                   // await FileHelper.AssignDocumentProperties(documenttoadd, idowner, _assignTag, _assignCorrespondentToDocument, _assignDocumentTypeToDocument, _assignStoragePathToDocument, classification_result, uniqueSubdirJson);
                }
                // check if we're in the case of Document Added
                foreach (Template template in templates)
                {

                    if (template.Type == GetListByType.Added && template.Is_Enabled == true) {
                        bool filter_result = FileHelper.DocumentAddedFilter(documenttoadd, template);
                        //check if we're in the case of Document Added 
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

        private string GetBaseDestinationPath()
        {
            return _configuration["OriginalsSettings:OutputFolder"];
        }



        // Function to export the shape as an image file
        private string ExportShapeAsImage(Microsoft.Office.Interop.PowerPoint.Shape shape)
        {
            try
            {
                // Create a unique file name for the image
                string imagePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");

                // Export the shape as an image file
                shape.Export(imagePath, PpShapeFormat.ppShapeFormatPNG);

                return imagePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting shape as image: {ex.Message}");
                return null;
            }
        }



    }
}
