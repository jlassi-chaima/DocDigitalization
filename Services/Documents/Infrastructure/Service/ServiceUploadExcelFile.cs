using Application.Features.AssignDocumentMangement;
using Application.Helper;
using Application.Respository;
using Domain.DocumentManagement.tags;
using Domain.FileTasks;
using Domain.Logs;
using Domain.Templates;
using Domain.Templates.Enum;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Text;
using Tesseract;


namespace Infrastructure.Service
{
    public class ServiceUploadExcelFile
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

        public ServiceUploadExcelFile(IConfiguration configuration, AssignTagToDocument assignTag, AssignCorrespondentToDocument assignCorrespondentToDocument, AssignDocumentTypeToDocument assignDocumentTypeToDocument, AssignStoragePathToDocument assignStoragePathToDocument, ITemplateRepository templateRepository, IDocumentRepository documentRepository, ITagRepository repository, IFileTasksRepository fileTasksRepository, ILogRepository logRepository)
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

        public async Task SaveExelFile(IFormFile file, string? type, string idowner)
        {
            Logs init = Logs.Create(LogLevel.INFO, LogName.EasyDoc, "Consuming " + file.FileName);
            Logs mimeType = Logs.Create(LogLevel.DEBUG, LogName.EasyDoc, "Detecting " + file.ContentType);
            await _logRepository.AddAsync(init);
            await _logRepository.AddAsync(mimeType);
            // Create a base destination path
            string baseDestinationPath = GetBaseDestinationPath();

            // Extract the file name without extension
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);

            // Create a directory with the file name
            string destinationFolderPath = Path.Combine(baseDestinationPath, fileNameWithoutExtension);
            Directory.CreateDirectory(destinationFolderPath);

            // Save the Excel file to the destination folder
            string excelFilePath = Path.Combine(destinationFolderPath, file.FileName);
            using (FileStream fileStream = new FileStream(excelFilePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }



            // Create an instance of Excel Application
            Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();

            // Open the Excel workbook
            Microsoft.Office.Interop.Excel.Workbook workbook_excel = excelApp.Workbooks.Open(excelFilePath);

            // Specify the PDF file path
            string pdfFilePath = Path.ChangeExtension(excelFilePath, "pdf");

            // Save the workbook as PDF
            workbook_excel.ExportAsFixedFormat(Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF, pdfFilePath);

            // Close the workbook
            workbook_excel.Close(false);

            // Quit Excel application
            excelApp.Quit();



            StringBuilder result = new StringBuilder();
            // Open the saved Excel file directly for reading
            using (var fileStream = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read))
            {
                // Get workbook
                IWorkbook workbook = new XSSFWorkbook(fileStream);

                // Iterate over each sheet in the workbook
                for (int sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
                {

                    ISheet sheet = workbook.GetSheetAt(sheetIndex);
                    result.Append($"{sheet.SheetName} \n");

                    // Iterate over rows in the sheet
                    for (int rowIndex = 0; rowIndex <= sheet.LastRowNum; rowIndex++)
                    {

                        IRow row = sheet.GetRow(rowIndex);
                        if (row != null) // null is when the row only contains empty cells
                        {
                            // Check for text boxes in this sheet (optional)
                            // **Text box detection (heuristic approach, limited accuracy)**


                            // Iterate over cells in the row
                            for (int cellIndex = 0; cellIndex < row.Cells.Count; cellIndex++)
                            {
                                NPOI.SS.UserModel.ICell cell = row.GetCell(cellIndex);

                                if (cell != null)
                                {
                                    // Append cell value to the result string
                                    result.Append(cell.ToString() + "\t"); // Assuming tab-separated values
                                }


                            }
                            result.AppendLine(); // Add newline after each row

                        }
                    }
                    // Check for text boxes in this sheet (optional)
                    string textBoxResults = DetectTextBoxesInSheet(sheet);
                    string imageDetectionResult = DetectImagesInSheet(sheet);
                    if (!string.IsNullOrEmpty(textBoxResults)) // Check if text boxes were found
                    {
                        result.AppendLine(textBoxResults); // Append text box detection results
                    }
                    if (!string.IsNullOrEmpty(imageDetectionResult)) // Check if text boxes were found
                    {
                        result.AppendLine(imageDetectionResult); // Append text box detection results
                    }



                    // Output the content of the current sheet to the console
                    Console.WriteLine("Sheet Name: " + sheet.SheetName);
                    Console.WriteLine("Content: \n" + result.ToString());
                    Console.WriteLine("--------------------------------------");
                }






                // Save the content of the current sheet to a .txt file
                string textFilePath = Path.Combine(destinationFolderPath, $"{fileNameWithoutExtension}.txt");
                // Save the content of the current sheet to a text file
                File.WriteAllText(textFilePath, result.ToString());
                string lang = FileHelper.DetectLanguage(result.ToString());
                string checksum = FileHelper.CalculateMD5(baseDestinationPath + fileNameWithoutExtension + "\\" + file.FileName);

                Domain.Documents.Document documenttoadd = Domain.Documents.Document.Create(fileNameWithoutExtension, "", result.ToString(), baseDestinationPath + fileNameWithoutExtension + "\\" + fileNameWithoutExtension + ".pdf", idowner, file.ContentType, checksum);
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
                        if (template.Type == GetListByType.Started && template.Is_Enabled == true)
                        {
                            documenttoadd.Title = fileNameWithoutExtension;
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
                    List<Template> templateadded = templates.Where(d=>d.Type == GetListByType.Added).ToList();
                    foreach (Template template in templateadded)
                    {
                        if (template.Type == GetListByType.Added && template.Is_Enabled == true)
                        {

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
        }
        private string GetBaseDestinationPath()
        {
            return _configuration["OriginalsSettings:OutputFolder"];
        }


        private string DetectTextBoxesInSheet(ISheet sheet)
        {
            // Access the drawings associated with the sheet
            XSSFDrawing drawing = (XSSFDrawing)sheet.CreateDrawingPatriarch();
            //return result 
            var output = "";
            // Check if shapes exist (optional)
            if (drawing.GetShapes() != null)
            {
                // Loop through each drawing element
                foreach (XSSFShape dr in drawing.GetShapes())
                {
                    // Check if the element is an XSSFTextBox
                    if (dr is XSSFTextBox textBox)
                    {


                        // Access text content (if set)
                        if (!string.IsNullOrEmpty(textBox.Text))
                        {
                            output += " " + textBox.Text;
                        }
                    }
                    if (dr is XSSFSimpleShape simpleShape)
                    {

                        //    result.AppendLine($"** Text Box Found (Sheet: {sheet.SheetName})**");

                        // Access text content (if set)
                        if (!string.IsNullOrEmpty(simpleShape.Text))
                        {
                            output += " " + simpleShape.Text;
                        }
                    }
                }
            }
            return output;
        }



        private string DetectImagesInSheet(ISheet sheet)
        {
            // Access the drawings associated with the sheet
            XSSFDrawing drawing = (XSSFDrawing)sheet.CreateDrawingPatriarch();

            // Initialize a StringBuilder to store the detection results
            StringBuilder imageDetectionResult = new StringBuilder();

            // Check if shapes exist
            if (drawing != null && drawing.GetShapes() != null)
            {
                // Loop through each drawing element
                foreach (XSSFShape shape in drawing.GetShapes())
                {
                    // Check if the element is an XSSFPicture
                    if (shape is XSSFPicture picture)
                    {
                        byte[] data = picture.PictureData.Data;
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
                                            imageDetectionResult.AppendLine(text);
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
                }
            }

            // Return the detection results
            return imageDetectionResult.ToString();
        }






    }
}
