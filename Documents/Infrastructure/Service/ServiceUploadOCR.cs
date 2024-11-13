using Application.Respository;
using Application.Services;
using Domain.Documents;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Application.Features.AssignDocumentMangement;
using Infrastructure.Service.Helper;
using Domain.Templates;
using Domain.Templates.Enum;
using Domain.DocumentManagement.tags;
using Infrastructure.Service.Helper.ASNProccessor;
using UglyToad.PdfPig.Content;
using Tesseract;
using UglyToad.PdfPig.XObjects;
using Newtonsoft.Json.Linq;
using Domain.FileTasks;
using Domain.Logs;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Domain.DocumentManagement.DocumentTypes;
using Domain.DocumentManagement;
using Domain.DocumentManagement.CustomFields;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharpDocument = PdfSharp.Pdf.PdfDocument;
using PdfPigDocument = UglyToad.PdfPig.PdfDocument;
using PdfSharp.Pdf.IO;
using Application.Dtos.Documents;
using System.Text;
using Path = System.IO.Path;
using System.Text.RegularExpressions;
using Application.Helper;
namespace Infrastructure.Service
{
    //to do verify 
    public class ServiceUploadOCR
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
        private readonly IDocumentTypeRepository _documentTypeRepository;
        private readonly ICustomFieldRepository _customFieldRepository;
        byte[] fileContent;

        public ServiceUploadOCR(IDocumentTypeRepository documentTypeRepository, ICustomFieldRepository customFieldRepository,IDocumentRepository documentRepository, AssignTagToDocument assignTag, AssignCorrespondentToDocument assignCorrespondentToDocument, AssignStoragePathToDocument assignStoragePathToDocument, AssignDocumentTypeToDocument assignDocumentTypeToDocument, IConfiguration configuration, ITemplateRepository templateRepository, IFileTasksRepository fileTasksRepository, ILogRepository logRepository)
        {
            _documentRepository = documentRepository;
            _assignTag = assignTag;
            _assignCorrespondentToDocument = assignCorrespondentToDocument;
            _assignStoragePathToDocument = assignStoragePathToDocument;
            _assignDocumentTypeToDocument = assignDocumentTypeToDocument;
            _configuration = configuration;
            _templateRepository = templateRepository;
            _fileTasksRepository = fileTasksRepository;
            _logRepository = logRepository;
            _documentTypeRepository = documentTypeRepository;
            _customFieldRepository = customFieldRepository;
        }
        public string GetBaseDestinationPath()
        {

            return _configuration["OriginalsSettings:OutputFolder"];
        }
        public async Task<AnalyzeResult>  ReadDoc(IFormFile file, DocumentAnalysisClient client)
        {
            try
            {

                //AzureKeyCredential credential = new AzureKeyCredential(key);
                 using var stream = file.OpenReadStream();
                //DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(endpoint), credential);
                Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", stream);

                return operation.Value;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        public async Task<AnalyzeResult> ReadPdfDoc(string pageContent, string key, string endpoint)
        {
            try
            {
                AzureKeyCredential credential = new AzureKeyCredential(key);
                DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(endpoint), credential);

                // Convert the page content to a stream
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(pageContent);
                using var stream = new MemoryStream(byteArray);

                Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", stream);
                return operation.Value;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async  Task<string> UploadDoc(IFormFile file,string idOwner, string type)
        {
            try
            {
                 string key = _configuration["AzureConfig:DI_KEY"];
                 string endpoint = _configuration["AzureConfig:DI_ENDPOINT"];
                /// 1)Read Doc
                if (file.ContentType.Contains("application/pdf") || file.FileName.Contains(".pdf"))
                {
                  
                    var client = new DocumentAnalysisClient(new Uri(endpoint), new AzureKeyCredential(key));

                    // Read the document
                    //var result = await ReadDoc(file, client);

                    // Extract ASN information
                    List<ASNInfo> pages = ExtractAndProcessASN(file);
                
                    // Open the PDF document
                    PdfDocument pdfDocument = FileProccess.OpenPdfDocumentAsync(file);

                    // Process each page with ASN information
                    if (pages.Count > 0)
                    {
                        for (int i = 0; i < pages.Count; i++)
                        {
                            int startPage = pages[i].PageNumber;
                            int endPage = (i < pages.Count - 1) ? pages[i + 1].PageNumber : pdfDocument.Pages.Count;

                            // Extract pages as a MemoryStream
                            using (MemoryStream pageStream = ExtractPageAsStream(pdfDocument, startPage, endPage))
                            {
                                // Clone the stream for classification (to avoid reusing a closed stream)
                                using (MemoryStream clonedStream = CloneStream(pageStream))
                                {
                                    // Classify the document type
                                    string docType = await ClassifyPdfDocumentType(clonedStream, client);

                                    // Reset the original stream position before reuse
                                    pageStream.Position = 0;

                                    // Extract document details
                                    var dataExtract = await ExtractPdfDoc(pageStream, docType, client);

                                    // Check if the document type exists in the repository
                                    var docTypeExist = await _documentTypeRepository.FindByName(docType) ?? await AddDocumentType(docType, idOwner);

                                    // Save the page as a document and process it
                                    Document doc = await SavePageAsDocument(file, pdfDocument, dataExtract, docTypeExist, type, idOwner, pages[i].PageNumber, pages[i].WordASN, pages, i, startPage, endPage);

                                    // Add or update custom fields for the document
                                    await AddOrUpdateCustomFields(doc, dataExtract);
                                }
                            }
                        }
                    }
                
                    else
                    {
                         
                            await ProcessEntireDocument(file, key, endpoint, type, idOwner);
                        
                    }
                    

                }
                else
                {
                    var client = new DocumentAnalysisClient(new Uri(endpoint), new AzureKeyCredential(key));
                    //var result = await ReadDoc(file, client);
                 
                    string docType = await ClassifyDocumentType(file, client);
                    var dataExtract = await ExtractDoc(file,docType,client);
                    var docTypeExist = await _documentTypeRepository.FindByName(docType) ?? await AddDocumentType(docType, idOwner);

                    Document doc = await ProcessSaveImage(file, idOwner, type, docTypeExist, dataExtract);
                    await AddOrUpdateCustomFields(doc, dataExtract);
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
            return "the file add Successfully";
            //var docTypeExist= await _documentTypeRepository.FindByName(docType);
            //if (docTypeExist == null)
            //{
            //    // Document type does not exist, create a new one
            //    await AddDocumentType(docType, id);
            //    //var getdocType = await _documentTypeRepository.FindByName(docType);

            //    Document documenttoadd = await FilterAndSaveDoc(file, result, docTypeExist, type, id);

            //    await HandleCustomFieldsAsync(result, documenttoadd);

            //}
            //else
            //{

            //    Document documenttoadd = await FilterAndSaveDoc(file, result, docTypeExist, type, id);

            //    // Handle custom fields associated with the document
            //    await HandleCustomFieldsAsync(result, documenttoadd);
            //}





            //StringBuilder textBuilder = new StringBuilder();
            //foreach (DocumentPage page in result.Pages)
            //{
            //    Console.WriteLine($"Document Page {page.PageNumber} has {page.Lines.Count} line(s), {page.Words.Count} word(s),");
            //    Console.WriteLine($"and {page.SelectionMarks.Count} selection mark(s).");
            //    foreach (DocumentWord word in page.Words)
            //    {
            //        // Append each word to the StringBuilder, followed by a space
            //        textBuilder.Append(word.Content + " ");
            //    }
            //    for (int i = 0; i < page.Lines.Count; i++)
            //    {
            //    {
            //        DocumentLine line = page.Lines[i];
            //        Console.WriteLine($"  Line {i} has content: '{line.Content}'.");

            //        Console.WriteLine($"    Its bounding polygon (points ordered clockwise):");

            //        for (int j = 0; j < line.BoundingPolygon.Count; j++)
            //        {
            //            Console.WriteLine($"      Point {j} => X: {line.BoundingPolygon[j].X}, Y: {line.BoundingPolygon[j].Y}");
            //        }
            //    }
            //}

            //foreach (DocumentStyle style in result.Styles)
            //{
            //    // Check the style and style confidence to see if text is handwritten.
            //    // Note that value '0.8' is used as an example.

            //    bool isHandwritten = style.IsHandwritten.HasValue && style.IsHandwritten == true;

            //    if (isHandwritten && style.Confidence > 0.8)
            //    {
            //        Console.WriteLine($"Handwritten content found:");

            //        foreach (DocumentSpan span in style.Spans)
            //        {
            //            Console.WriteLine($"  Content: {result.Content.Substring(span.Index, span.Length)}");
            //        }
            //    }
            //}

            //Console.WriteLine("Detected languages:");

            //foreach (DocumentLanguage language in result.Languages)
            //{
            //    Console.WriteLine($"  Found language with locale'{language.Locale}' with confidence {language.Confidence}.");
            //}
            //var extractedFields = new System.Text.StringBuilder("Extracted fields:\n");
            //foreach (var kvp in result.Documents[0].Fields)
            //{
            //    extractedFields.AppendLine($"Field '{kvp.Key}': '{kvp.Value.Content}', Confidence: {kvp.Value.Confidence}");
            //}
            //foreach (var kvp in result.KeyValuePairs)
            //{
            //    Console.WriteLine($"Key: {kvp.Key.Content}, Value: {kvp.Value.Content}");
            //}

        }
       

        MemoryStream CloneStream(Stream originalStream)
        {
            var clonedStream = new MemoryStream();
            originalStream.Position = 0;
            originalStream.CopyTo(clonedStream);
            clonedStream.Position = 0;
            return clonedStream;
        }
        private async Task ProcessEntireDocument(IFormFile file, string key, string endpoint, string type, string idOwner)
        {
         
            var client = new DocumentAnalysisClient(new Uri(endpoint), new AzureKeyCredential(key));
            using (var pdfDocument = FileProccess.OpenPdfDocumentAsync(file))
            {
                int startPage = 0;
                int endPage = pdfDocument.Pages.Count;
                string docType = await ClassifyDocumentType(file, client);
                var dataExtract = await ExtractDoc(file,docType, client);
                var docTypeExist = await _documentTypeRepository.FindByName(docType) ?? await AddDocumentType(docType, idOwner);                      
                Document doc = await SavePageAsDocument(file, pdfDocument, dataExtract, docTypeExist, type, idOwner, startPage,null, new List<ASNInfo>(), 0, startPage, endPage);                   
                await AddOrUpdateCustomFields(doc, dataExtract);
                
            }

        }

        private MemoryStream ExtractPageAsStream(PdfDocument pdfDocument, int startPage, int endPage)
        {
           
                    var outputMemoryStream = new MemoryStream();                
                    var outputDocument = new PdfDocument();
                    // Add the specified pages to the new document
                    for (int page = startPage; page < endPage; page++)
                    {
                        outputDocument.AddPage(pdfDocument.Pages[page]);
                    }

                    // Save the new document to the output memory stream
                    outputDocument.Save(outputMemoryStream);
                

                    // Reset the output stream position to the beginning before returning it
                    outputMemoryStream.Position = 0;
                    return outputMemoryStream;
            
        }





        private async Task<List<PdfPageContentDto>> ExtractPdfPagesContentAsync(IFormFile file)
        {
            var pagesContent = new List<PdfPageContentDto>();

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(stream))
                    {
                        foreach (var page in pdfDocument.GetPages())
                        {
                            // Use the PdfPig method to extract content from the page
                            string content = string.Join(" ", page.GetWords().Select(w => w.Text));
                            if (!string.IsNullOrEmpty(content))
                            {
                                pagesContent.Add(new PdfPageContentDto
                                {
                                    PageNumber = page.Number,
                                    Content = content
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error extracting PDF pages: " + ex.Message);
            }

            return pagesContent;
        }
        public string ExtractTextFromPage(UglyToad.PdfPig.Content.Page page)
        {
            StringBuilder text = new StringBuilder();
            foreach (var word in page.GetWords())
            {
                text.Append(word.Text + " ");
            }
            return text.ToString().Trim();
        }
        private byte[] ExtractPageBytes(PdfSharp.Pdf.PdfPage page)
        {
            using (var stream = new MemoryStream())
            {
                // Export the page to a byte array
                PdfSharp.Pdf.PdfDocument singlePageDoc = new PdfSharp.Pdf.PdfDocument();
                singlePageDoc.AddPage(page);
                singlePageDoc.Save(stream, false);
                return stream.ToArray();
            }
        }
        public async Task<AnalyzeResult> ExtractPdfDoc(Stream stream, string documenType,DocumentAnalysisClient client)
        {

            try
            {

                string modelId = documenType;             
                // Convert the page content to a stream
                //byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(pageContent);
                //using var stream = new MemoryStream(byteArray);
  
                Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, documenType, stream);
                return operation.Value;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
            public async Task<AnalyzeResult> ExtractDoc(IFormFile file,string documenType,DocumentAnalysisClient client)
                {

          
                     string modelId = documenType;
                    using var streamm = file.OpenReadStream();
                    //DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(endpoint), credential);
                    Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, modelId, streamm);
                    return operation.Value;
                  //  Console.WriteLine($"Document was analyzed with model with ID: {result.ModelId}");

                    //foreach (AnalyzedDocument document in result.Documents)
                    //{
                    //    Console.WriteLine($"Document of type: {document.DocumentType}");

                    //    foreach (KeyValuePair<string, DocumentField> fieldKvp in document.Fields)
                    //    {
                    //        string fieldName = fieldKvp.Key;
                    //        DocumentField field = fieldKvp.Value;

                    //        Console.WriteLine($"Field '{fieldName}': ");

                    //        Console.WriteLine($"  Content: '{field.Content}'");
                    //        Console.WriteLine($"  Confidence: '{field.Confidence}'");
                    //    }
                    //}
                  //  string docType = ClassifyDocumentType(result.Content);
           
           



                }
            public async Task<Document> ProcessSaveImage(IFormFile file, string idOwner, string type, DocumentType documentType, AnalyzeResult result)
            {
                Document documentToAdd = new Document();

                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
   
                string folderPath = CreateDirectoryForFile(file,"");

                // Save the uploaded image to the file system
                string filePath = SaveFileToDisk(file, folderPath);

                // Create and save PDF from the image
                string pdfFileName = System.IO.Path.GetFileNameWithoutExtension(file.FileName) + ".pdf";
                string pdfFilePath = CreatePdfFromImage(filePath, folderPath, pdfFileName);



                string textFileName = Path.GetFileNameWithoutExtension(file.FileName) + ".txt";
                string textFilePath = Path.Combine(folderPath, textFileName);
                File.WriteAllText(textFilePath, result.Content.ToString());

                string checksum = FileHelper.CalculateMD5(filePath);
                string lang = FileHelper.DetectLanguage(result.Content.ToString());
                documentToAdd = CreateDocument(fileNameWithoutExtension, "", result.Content, pdfFilePath, idOwner, file.ContentType, checksum, lang, type, documentType.Id);
                await _documentRepository.AddAsync(documentToAdd);
                return documentToAdd;
               //List<Document> documentList = await FilterAndSaveDoc(file, result, documentType, type, id);
           
            }
        //private string ClassifyDocumentType(string content)
        //{
           
        //    if (content.ToLower().Contains("formulaire d'abonnement"))
        //    {
        //        return "contractTT2"; 
        //    }
        //    else if (content.ToLower().Contains("resume") || content.Contains("curriculum vitae"))
        //    {
        //        return "CV";
        //    }
        //    else if (content.ToLower().Contains("certificate"))
        //    {
        //        return "Certificate";
        //    }
        //    else if (content.ToLower().Contains("rne"))
        //    {
        //        return "RNE";
        //    }
          

        //    return "contractTT2";
        //}
        private async Task<string> ClassifyDocumentType(IFormFile file,DocumentAnalysisClient client)
        {

            

            //var client = new DocumentAnalysisClient(new Uri(endpoint), new AzureKeyCredential(key));

            string modelId = "NewClassification";
        
          
            using var content = file.OpenReadStream();

            Operation<AnalyzeResult> operation = await client.ClassifyDocumentAsync(WaitUntil.Completed, modelId, content);

            AnalyzeResult result = operation.Value;

            Console.WriteLine($"Document was analyzed with model with ID: {result.ModelId}");
            string docType = "";
            foreach (AnalyzedDocument document in result.Documents)
            {
                Console.WriteLine($"Document of type: {document.DocumentType}, with confidence {document.Confidence}");
                docType = document.DocumentType;
            }
            return docType;
        }
        private async Task<string> ClassifyPdfDocumentType(Stream stream, DocumentAnalysisClient client)
        {




            string modelId = "NewClassification";

            //using var content = file.OpenReadStream();
            Operation<AnalyzeResult> operation = await client.ClassifyDocumentAsync(WaitUntil.Completed, modelId, stream);

            AnalyzeResult result = operation.Value;
            string docType = "";
            foreach (AnalyzedDocument document in result.Documents)
            {
                Console.WriteLine($"Document of type: {document.DocumentType}, with confidence {document.Confidence}");
                docType = document.DocumentType;
            }
            return docType;

        }
        private async Task<List<Document>> FilterAndSaveDoc(IFormFile file, AnalyzeResult result, DocumentType docTypeExist, string type, string idOwner)
        {
            Document documentToAdd =new Document();
            List<Document> documentList = new List<Document>();

            if (file.ContentType.Contains("image/png") || file.ContentType.Contains("image/jpeg"))
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
                string baseDestinationPath = GetBaseDestinationPath();
                string folderPath = CreateDirectoryForFile(file, "");

                // Save the uploaded image to the file system
                string filePath = SaveFileToDisk(file, folderPath);

                // Create and save PDF from the image
                string pdfFileName = System.IO.Path.GetFileNameWithoutExtension(file.FileName) + ".pdf";
                string pdfFilePath = CreatePdfFromImage(filePath, folderPath, pdfFileName);



                string textFileName = Path.GetFileNameWithoutExtension(file.FileName) + ".txt";
                string textFilePath = Path.Combine(folderPath, textFileName);
                File.WriteAllText(textFilePath, result.Content.ToString());

                string checksum = FileHelper.CalculateMD5(filePath);
                string lang = FileHelper.DetectLanguage(result.Content.ToString());
                documentToAdd = CreateDocument(fileNameWithoutExtension, "", result.Content, pdfFilePath, idOwner, file.ContentType, checksum, lang, type, docTypeExist.Id);
                documentList.Add(documentToAdd);

            }
            if(file.ContentType.Contains("application/pdf") || file.FileName.Contains(".pdf"))
            {
               // List<ASNInfo> pages = ExtractAndProcessASN(file);
                documentList=await SplitDocument(file, type, idOwner, result.Content, docTypeExist.Id);

            }

            
            await _documentRepository.AddRangeAsync(documentList);

           
            return documentList;
        }
        public async Task<Document> SavePageAsDocument(
                IFormFile file,
                PdfDocument pdfDocument,
                AnalyzeResult result,
                DocumentType docTypeExist,
                string type,
                string idOwner,
                int pageNumber,
                string wordASN,
                List<ASNInfo> pages,
                int i,int startPage , int endPage)
        {
       
            Document documentToAdd = null;

          

            // Create and save extracted PDF document
            PdfDocument subDocument = ExtractPages(pdfDocument, startPage, endPage);
            if (subDocument.PageCount > 0)
            {

                string subPdfFolder = CreateDirectoryForFile(file, wordASN);
                string subPdfPath = SavePdfDocument(subDocument, subPdfFolder, wordASN);

                // Save OCR data and generate metadata
                string textFilePath = Path.Combine(subPdfFolder, "dataOCR.txt");
                File.WriteAllText(textFilePath, result.Content);
                string checksum = FileHelper.CalculateMD5(subPdfPath);
                string lang = FileHelper.DetectLanguage(result.Content);

                // Create and save Document object
                documentToAdd = CreateDocument(string.IsNullOrEmpty(wordASN) ? Path.GetFileNameWithoutExtension(file.FileName) : wordASN, "", result.Content, subPdfPath, idOwner, file.ContentType, checksum, lang, type, docTypeExist.Id);
                documentToAdd.ThumbnailUrl = await FileHelper.CreateThumbnailOfADocumentAsync(documentToAdd, _logRepository);
                documentToAdd.Lang = lang;
                documentToAdd.Archive_Serial_Number = wordASN;
            }


            pdfDocument.Dispose();

            if (documentToAdd != null)
            {
                await _documentRepository.AddAsync(documentToAdd);
            }

            return documentToAdd;
        }


        

        private async Task LogDocumentProcessing(IFormFile file)
        {
            Logs initcon = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Consuming {file.FileName}");
            Logs mime = Logs.Create(LogLevel.DEBUG, LogName.EasyDoc, $"mime type: {file.ContentType}");
            await _logRepository.AddAsync(initcon);
            await _logRepository.AddAsync(mime);
        }

       

       
        
        public async Task<DocumentType> AddDocumentType(string docType,string ownerId)
        {
            var newDocumentType = new DocumentType
            {
                Name = docType,
                Slug = docType,
                // edit
                Match = new List<string>(),
                Matching_algorithm = Matching_Algorithms.MATCH_NONE,
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

            // Add the new document type 
            await _documentTypeRepository.AddAsync(newDocumentType);
            return newDocumentType;
        }
        public async Task AddOrUpdateCustomFields(Document docToAdd, AnalyzeResult result)
        {
            try
            {
                foreach (AnalyzedDocument document in result.Documents)
                {
                    foreach (KeyValuePair<string, DocumentField> fieldKvp in document.Fields)
                    {
                        string fieldName = fieldKvp.Key;
                        DocumentField field = fieldKvp.Value;

                        var customField = await _customFieldRepository.FindByNameAsync(fieldName);
                        if (customField != null)
                        {


                            DocumentCustomField documentCustomField = new DocumentCustomField
                            {
                                Document = docToAdd,
                                DocumentId = docToAdd.Id,
                                CustomFieldId = customField.Id,
                                CustomField = customField,
                                Value = field.Content

                            };
                            
                            //customField.Data_type = field.FieldType.ToString() == "Unknown"? HandleCustomFieldType(field.ExpectedFieldType.ToString())  : HandleCustomFieldType(field.FieldType.ToString());

                            customField.Data_type = HandleCustomFieldType(field.FieldType.ToString());
                            customField?.DocumentsCustomFields?.Add(documentCustomField);
                            await _customFieldRepository.UpdateAsync(customField);

                        }
                        else
                        {
                            // Add a new custom field if it doesn't exist
                            CustomField customFieldToAdd = new CustomField()
                            {
                                Data_type = HandleCustomFieldType(field.FieldType.ToString()),
                                Name = fieldName,
                            };

                            await _customFieldRepository.AddAsync(customFieldToAdd);
                            DocumentCustomField documentCustomField = new DocumentCustomField
                            {
                                Document = docToAdd,
                                DocumentId = docToAdd.Id,
                                CustomFieldId = customFieldToAdd.Id,
                                CustomField = customFieldToAdd,
                                Value = field.Content

                            };
                            customFieldToAdd?.DocumentsCustomFields?.Add(documentCustomField);
                            await _customFieldRepository.UpdateAsync(customFieldToAdd);


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
                
            

        }
        public TypeField HandleCustomFieldType(string dataType)
        {
            switch (dataType.ToLower())
            {
                case "string":
                    return TypeField.STRING;

                case "url":
                    return TypeField.URL;

                case "date":
                    return TypeField.DATE;

                case "boolean":
                    return TypeField.BOOLEAN;

                case "integer":
                    return TypeField.INTEGER;

                case "float":
                    return TypeField.FLOAT;

                case "monetary":
                    return TypeField.MONETARY;

                default:
                    return TypeField.UNKNOWN; // Handle unknown or unexpected types
            }
        }


        public List<ASNInfo> ExtractAndProcessASN(IFormFile file)
        {

            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                fileContent = memoryStream.ToArray();
            }

            // Call the ProcessASN to get  list of ASNInfo objects containing information about ASN occurrences
            List<ASNInfo> asnInfoList = ASNProcessor.ProcessASN(fileContent);


            return asnInfoList;
        }
        public async Task<List<Document>> SplitDocument(IFormFile file, string? typee, string idowner, string content, Guid docTypeId)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
            List<Document> documents = new List<Document>();
            string mimetype = FileProccess.Mimetype(file.FileName);
            Console.WriteLine(mimetype);

            // Read the PDF document
            using var fileStream = file.OpenReadStream();

            // PdfSharp: Open the PDF document from fileStream
            PdfSharp.Pdf.PdfDocument pdfDocument;
            try
            {
                pdfDocument = PdfReader.Open(fileStream, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening PDF with PdfSharp: {ex.Message}");
                throw;
            }

            // Reset the stream position for PdfPig
            fileStream.Position = 0;

            // PdfPig: Open the PDF document from fileStream
            UglyToad.PdfPig.PdfDocument pigDocument;
            try
            {
                pigDocument = UglyToad.PdfPig.PdfDocument.Open(fileStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening PDF with PdfPig: {ex.Message}");
                throw;
            }
            // Determine how you want to split the PDF
            // Example: Split into chunks of 1 page each
            for (int i = 0; i < pdfDocument.PageCount; i++)
            {
                int startPage = i + 1;
                int endPage = (i + 1 < pdfDocument.PageCount) ? (i + 1) : pdfDocument.PageCount;

                // Create directory for the file
                string subPdfFolder = CreateDirectoryForFile(file, "");

                // Extract pages from the PDF document
                PdfSharp.Pdf.PdfDocument subDocument = ExtractPages(pdfDocument, startPage, endPage);

                // Check if subDocument has pages before saving
                if (subDocument.PageCount > 0)
                {
                    // Save the extracted pages as a new PDF
                    string subPdfPath = SavePdfDocument(subDocument, subPdfFolder, $"{fileNameWithoutExtension}_Page{startPage}");

                    // Extract text content from the PDF pages using OCR
                    string ocrContent = ExtractPageContent(pigDocument, startPage, endPage);
                    string textFilePath = Path.Combine(subPdfFolder, "dataOCR.txt");
                    File.WriteAllText(textFilePath, ocrContent);

                    // Generate checksum and detect language
                    string checksum = FileHelper.CalculateMD5(subPdfPath);
                    string lang = FileHelper.DetectLanguage(ocrContent);

                    // Create Document object
                    Document documentToAdd = Document.Create(
                        fileNameWithoutExtension,
                        $"{fileNameWithoutExtension}_Page{startPage}",
                        content,
                        subPdfPath,
                        idowner,
                        mimetype,
                        checksum
                    );

                    documentToAdd.Source = typee != null && typee.Equals("FileShare") ?
                        DocumentSource.ConsumeFolder :
                        DocumentSource.ApiUpload;

                    // Generate thumbnail asynchronously and add it to the document
                    string thumbnailUrl = await FileHelper.CreateThumbnailOfADocumentAsync(documentToAdd, _logRepository);
                    documentToAdd.ThumbnailUrl = thumbnailUrl;
                    documentToAdd.Lang = lang;
                
                    documents.Add(documentToAdd);
                }
                //else
                //{
                //    Logs initcon = Logs.Create(LogLevel.INFO, LogName.DigitalWork, "Consuming " + file.FileName);
                //Logs mime = Logs.Create(LogLevel.DEBUG, LogName.DigitalWork, "mime type: " + file.ContentType);
                //await _logRepository.AddAsync(initcon);
                //await _logRepository.AddAsync(mime);
                //// Create sub-PDF folder
                ////create directory orginals  
                //string baseDestinationPath = GetBaseDestinationPath();
                //Directory.CreateDirectory(baseDestinationPath);
                //string NameFileWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
                //string subPdfFolderr = Path.Combine(baseDestinationPath, NameFileWithoutExtension);
                //Directory.CreateDirectory(subPdfFolder);
                //// this for read content file
                //PdfPigDocument document = PdfPigDocument.Open(fileContent);
                //PdfSharp.Pdf.PdfDocument pdfDocumentt = FileProccess.OpenPdfDocumentAsync(file);
                //var output = "";
                ////for new document 
                //PdfSharp.Pdf.PdfDocument subDocumentt = new PdfSharp.Pdf.PdfDocument();
                //for (int page = 0; page < pdfDocument.PageCount; page++)
                //{
                //    subDocument.AddPage(pdfDocument.Pages[page]);
                //}
                //// Save 
                //string subPdfPath = Path.Combine(subPdfFolder, $"{file.FileName}");
                //subDocument.Save(subPdfPath);

                //}
            }

            // Dispose of the original PDF document
            pdfDocument.Dispose();

            return documents;
        }

        //create a directory for a file
        private string CreateDirectoryForFile(IFormFile file,string wordASN)
        {
            string baseDestinationPath = GetBaseDestinationPath();
            string fileNameWithoutExtension = string.IsNullOrEmpty(wordASN)? Path.GetFileNameWithoutExtension(file.FileName): wordASN;
           // string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
            string subFileFolder = Path.Combine(baseDestinationPath, fileNameWithoutExtension);
            Directory.CreateDirectory(subFileFolder);
            return subFileFolder;
        }
        // extract pages from a PDF document
        private PdfDocument ExtractPages(PdfDocument sourceDocument, int startPage, int endPage)
        {
            PdfDocument subDocument = new PdfSharp.Pdf.PdfDocument();

            for (int page = startPage; page < endPage; page++)
            {
                subDocument.AddPage(sourceDocument.Pages[page]);
            }
            return subDocument;
        }

        private string SavePdfDocument(PdfSharpDocument subDocument, string subPdfFolder, string fileName)
        {
            string subPdfPath = Path.Combine(subPdfFolder, $"{fileName}.pdf");
            subDocument.Save(subPdfPath);
            return subPdfPath;
        }
        //  extract OCR content
         private string ExtractPageContent(PdfPigDocument document, int startPage, int endPage)
        {
            var output = "";
            for (int page = startPage; page <= endPage; page++)
            {
                var currentPage = document.GetPage(page);
                List<string> words = currentPage.GetWords().Select(w => w.Text).ToList();
                output += string.Join(" ", words) + "\n";
            }
            return output;
        }

        private Document CreateDocument(string fileNameWithoutExtension, string wordASN, string content, string pdfPath, string idowner, string mimetype, string checksum, string lang, string? typee,Guid docTypeId)
        {
            Document documentToAdd = Document.Create(fileNameWithoutExtension, wordASN, content, pdfPath, idowner, mimetype, checksum);
            documentToAdd.Lang = lang;
            documentToAdd.Source = typee != null && typee.Equals("FileShare") ? DocumentSource.ConsumeFolder : DocumentSource.ApiUpload;
            documentToAdd.Mailrule = null;
            documentToAdd.ThumbnailUrl = FileHelper.CreateThumbnailOfADocumentAsync(documentToAdd, _logRepository).Result;
            documentToAdd.Content = content;
            documentToAdd.DocumentTypeId = docTypeId;
            return documentToAdd;
        }
        private string SaveFileToDisk(IFormFile file, string folderPath)
        {
            string fileName = Path.GetFileName(file.FileName);
            string filePath = Path.Combine(folderPath, fileName);

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return filePath;
        }

        private string CreatePdfFromImage(string imagePath, string folderPath, string pdfFileName)
        {
            // Create a new PDF document and add a page
            PdfDocument documentPdf = new PdfDocument();
            PdfPage pdfPage = documentPdf.AddPage();

            // Get XGraphics object for drawing on the page
            XGraphics gfx = XGraphics.FromPdfPage(pdfPage);

            // Load image and draw on the PDF page
            XImage image = XImage.FromFile(imagePath);
            double width = image.PixelWidth * 0.75;  // Adjust scaling factor as needed
            double height = image.PixelHeight * 0.75;

            gfx.DrawImage(image, 0, 0, width, height);  // Draw image on the PDF

            // Save PDF document to the file system
            string pdfFilePath = Path.Combine(folderPath, pdfFileName);
            documentPdf.Save(pdfFilePath);
            documentPdf.Close();

            return pdfFilePath;
        }
        public async Task SplitAndSaveASNPages(List<ASNInfo> pages, IFormFile file, string? typee, string idowner)
        {
            // Logs
            Logs init = Logs.Create(LogLevel.INFO, LogName.EasyDoc, "Consuming " + file.FileName);
            Logs mimeType = Logs.Create(LogLevel.DEBUG, LogName.EasyDoc, "mime type: " + file.ContentType);
            await _logRepository.AddAsync(init);
            await _logRepository.AddAsync(mimeType);
            //
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
            List<Document> documents = new List<Document>();
            string mimetype = FileProccess.Mimetype(file.FileName);
            Console.WriteLine(mimetype);
            // if file contains ASN      
            if (pages.Count > 0)
            {

                // Read the PDF document

                PdfSharp.Pdf.PdfDocument pdfDocument = FileProccess.OpenPdfDocumentAsync(file);

                PdfPigDocument document = PdfPigDocument.Open(fileContent);

                for (int i = 0; i < pages.Count; i++)
                {
                    int startPage = pages[i].PageNumber;
                    int endPage = i < pages.Count - 1 ? pages[i + 1].PageNumber : pdfDocument.Pages.Count;
                    //create directory orginals  
                    string baseDestinationPath = GetBaseDestinationPath();
                    Directory.CreateDirectory(baseDestinationPath);
                    // Create folder
                    string subPdfFolder = Path.Combine(baseDestinationPath, pages[i].WordASN);
                    Directory.CreateDirectory(subPdfFolder);

                    // Create  document pdf
                    PdfSharpDocument subDocument = new PdfSharpDocument();
                    for (int page = startPage; page < endPage; page++)
                    {
                        subDocument.AddPage(pdfDocument.Pages[page]);
                    }

                    // Save 
                    string subPdfPath = Path.Combine(subPdfFolder, $"{pages[i].WordASN}.pdf");

                    subDocument.Save(subPdfPath);




                    string extracted_text = null;
                    //bring the ML Classification result 
                    using (var httpClient = new HttpClient())
                    {
                        var apiUrl = "http://localhost:6000/Extract_text_of_pdf";
                        // Append the result as a query parameter to the URL
                        apiUrl += "?path=" + Uri.EscapeDataString(subPdfPath);


                        try
                        {
                            // Send the POST request to the Flask endpoint with the result as a query parameter
                            var response = await httpClient.PostAsync(apiUrl, null);

                            // Ensure the request completed successfully
                            response.EnsureSuccessStatusCode();

                            // Read the response content as a string
                            var responseBody = await response.Content.ReadAsStringAsync();
                            extracted_text = responseBody;

                            // Deserialize the JSON string into an object
                            //extracted_text = JsonConvert.DeserializeObject<string>(responseBody);

                            // Handle the response as needed
                            Console.WriteLine("Response from other service: " + responseBody);
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










                    // Extract and save text 
                    string textFilePath = Path.Combine(subPdfFolder, "dataOCR.txt");
                    var output = "";
                    //List<IPdfImage> images = new List<IPdfImage>();
                    //for (int page = startPage + 1; page <= endPage; page++)
                    //{
                    //    var currentPage = document.GetPage(page);

                    //    List<string> words = currentPage.GetWords().Select(w => w.Text).ToList();
                    //     images = currentPage.GetImages().ToList();

                    //    output += string.Join(" ", words) + "\n";

                    //}
                    List<IPdfImage> images = new List<IPdfImage>();
                    for (int page = startPage + 1; page <= endPage; page++)
                    {
                        var currentPage = document.GetPage(page);

                        List<string> words = currentPage.GetWords().Select(w => w.Text).ToList();
                        foreach (var image in currentPage.GetImages())
                        {
                            if (!image.IsInlineImage)
                            {
                                var b = image.RawBytes;
                                images.Add(image);
                            }

                            var type = string.Empty;
                            switch (image)
                            {
                                case XObjectImage ximg:
                                    type = "XObject";
                                    break;
                                case InlineImage inline:
                                    type = "Inline";
                                    break;
                            }

                            Console.WriteLine($"Image with  bytes of type '{type}' on page {currentPage.Number}. Location: {image.Bounds}.");
                        }
                        //images.AddRange(currentPage.GetImages());
                        //if(!images.)

                        output += string.Join(" ", words) + "\n";
                    }
                    foreach (IPdfImage image in images)
                    {
                        byte[] imageBytes = (byte[])image.RawBytes; // Assuming GetBytes() is the method to get the byte array
                                                                    // Consider checking for null or empty data
                        if (imageBytes == null || imageBytes.Length == 0)
                        {
                            Console.WriteLine($"Error: Empty image data for image on page ");
                            continue;
                        }
                        var tessDataPath = @"C:\Users\MSI\Documents\GitHub\paperless\Tessdata\tesseract-ocr-tesseract-bc059ec\tessdata";
                        using (var engine = new TesseractEngine(tessDataPath, "ara+eng+fra", EngineMode.Default))
                        {
                            using (var ms = new MemoryStream(imageBytes))
                            {
                                try
                                {
                                    // Load the image data into a Pix object directly
                                    using (var pix = Pix.LoadFromMemory(imageBytes))
                                    {
                                        if (pix == null)
                                        {
                                            Console.WriteLine("Error: Failed to load image data into Pix object.");
                                            continue;
                                        }
                                        using (var pageOcr = engine.Process(pix))
                                        {
                                            var text = pageOcr.GetText();
                                            extracted_text += text;
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



                    File.WriteAllText(textFilePath, output);
                    subDocument.Close();
                    pdfDocument.Dispose();

                    // Generate Checksum and get mime type 
                    string checksum = FileHelper.CalculateMD5(baseDestinationPath + pages[i].WordASN + "\\" + pages[i].WordASN + ".pdf");
                    //extract DateCreated
                    DateTime created = FileHelper.ExtractCreated(output);
                    // Detetct language
                    string lang = FileHelper.DetectLanguage(output);
                    //extract mimetype
                    mimetype = FileProccess.Mimetype(file.FileName);

                    // Create Document 
                    Document documenttoadd = Document.Create(fileNameWithoutExtension, pages[i].WordASN, extracted_text, baseDestinationPath + pages[i].WordASN + "\\" + pages[i].WordASN + ".pdf", idowner, mimetype, checksum);

                    if (typee != null && typee.Equals("FileShare"))
                    {
                        documenttoadd.Source = DocumentSource.ConsumeFolder;
                    }
                    else
                    {
                        documenttoadd.Source = DocumentSource.ApiUpload;
                    }
                    documenttoadd.Mailrule = null;
                    //string thumbnail_url = FileHelper.CreateThumbnailOfADocument(documenttoadd);
                    string thumbnail_url = FileHelper.CreateThumbnailOfADocumentAsync(documenttoadd, _logRepository).Result;
                    documenttoadd.ThumbnailUrl = thumbnail_url;
                    //  documenttoadd.Created = created;
                    documenttoadd.Lang = lang;



                    //verify if the document has an owner or not
                    if (!string.IsNullOrEmpty(idowner))
                    {
                        string responseObject = null;
                        string classification_result = null;
                        string uniqueSubdir = null;
                        JObject uniqueSubdirJson = null;
                        if (documenttoadd.Source != DocumentSource.ConsumeFolder)
                        {

                            // Bring the ML Classification result 
                            using (var httpClient = new HttpClient
                            {
                                Timeout = TimeSpan.FromMinutes(5) // Set timeout to 5 minutes
                            })
                            {
                                var apiUrl = "http://localhost:5003/classify_extract";
                                // Append the result as a query parameter to the URL
                                apiUrl += "?result=" + Uri.EscapeDataString(extracted_text);

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
                                if (template.Type == GetListByType.Started) {

                                    bool filter_result = FileHelper.ConsumptionStartedFilter(documenttoadd, template, classification_result);
                                    //check if we're in the case of consumption started 
                                    if (filter_result && template.Is_Enabled == true)
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
                            //// check if we're in the case of Document Added
                            foreach (Template template in templates)
                            {
                                if (template.Type == GetListByType.Added) {

                                    bool filter_result = FileHelper.DocumentAddedFilter(documenttoadd, template);
                                    //check if we're in the case of Document Added 
                                    if ( filter_result && template.Is_Enabled == true)
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

                    


                    await _documentRepository.AddAsync(documenttoadd);
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
                }

            }

            else
            {
                Logs initcon = Logs.Create(LogLevel.INFO, LogName.EasyDoc, "Consuming " + file.FileName);
                Logs mime = Logs.Create(LogLevel.DEBUG, LogName.EasyDoc, "mime type: " + file.ContentType);
                await _logRepository.AddAsync(initcon);
                await _logRepository.AddAsync(mime);
                // Create sub-PDF folder
                //create directory orginals  
                string baseDestinationPath = GetBaseDestinationPath();
                Directory.CreateDirectory(baseDestinationPath);
                string NameFileWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
                string subPdfFolder = Path.Combine(baseDestinationPath, NameFileWithoutExtension);
                Directory.CreateDirectory(subPdfFolder);
                // this for read content file
                PdfPigDocument document = PdfPigDocument.Open(fileContent);
                PdfSharp.Pdf.PdfDocument pdfDocument = FileProccess.OpenPdfDocumentAsync(file);
                var output = "";
                //for new document 
                PdfSharp.Pdf.PdfDocument subDocument = new PdfSharp.Pdf.PdfDocument();
                for (int page = 0; page < pdfDocument.PageCount; page++)
                {
                    subDocument.AddPage(pdfDocument.Pages[page]);
                }
                // Save 
                string subPdfPath = Path.Combine(subPdfFolder, $"{file.FileName}");
                subDocument.Save(subPdfPath);




                string extracted_text = null;
                //bring the ML Classification result 
                using (var httpClient = new HttpClient())
                {
                    var apiUrl = "http://localhost:6000/Extract_text_of_pdf";
                    // Append the result as a query parameter to the URL
                    apiUrl += "?path=" + Uri.EscapeDataString(subPdfPath);


                    try
                    {
                        // Send the POST request to the Flask endpoint with the result as a query parameter
                        var response = await httpClient.PostAsync(apiUrl, null);

                        // Ensure the request completed successfully
                        response.EnsureSuccessStatusCode();

                        // Read the response content as a string
                        var responseBody = await response.Content.ReadAsStringAsync();
                        extracted_text = responseBody;

                        // Deserialize the JSON string into an object


                        // Handle the response as needed
                        Console.WriteLine("Response from other service: " + responseBody);
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








                List<IPdfImage> images = new List<IPdfImage>();
                for (int page = 1; page <= document.NumberOfPages; page++)
                {
                    var currentPage = document.GetPage(page);

                    List<string> words = currentPage.GetWords().Select(w => w.Text).ToList();
                    foreach (var image in currentPage.GetImages())
                    {
                        if (!image.IsInlineImage)
                        {
                            var b = image.RawBytes;
                            images.Add(image);
                        }

                        var type = string.Empty;
                        switch (image)
                        {
                            case XObjectImage ximg:
                                type = "XObject";
                                break;
                            case InlineImage inline:
                                type = "Inline";
                                break;
                        }

                        Console.WriteLine($"Image with  bytes of type '{type}' on page {currentPage.Number}. Location: {image.Bounds}.");
                    }
                    //images.AddRange(currentPage.GetImages());
                    //if(!images.)

                    output += string.Join(" ", words) + "\n";
                }
                foreach (IPdfImage image in images)
                {
                    byte[] imageBytes = (byte[])image.RawBytes; // Assuming GetBytes() is the method to get the byte array
                                                                // Consider checking for null or empty data
                    if (imageBytes == null || imageBytes.Length == 0)
                    {
                        Console.WriteLine($"Error: Empty image data for image on page ");
                        continue;
                    }
                    var tessDataPath = @"C:\Users\MSI\Documents\GitHub\paperless\Tessdata\tesseract-ocr-tesseract-bc059ec\tessdata";
                    using (var engine = new TesseractEngine(tessDataPath, "ara+eng+fra", EngineMode.Default))
                    {
                        using (var ms = new MemoryStream(imageBytes))
                        {
                            try
                            {
                                // Load the image data into a Pix object directly
                                using (var pix = Pix.LoadFromMemory(imageBytes))
                                {
                                    if (pix == null)
                                    {
                                        Console.WriteLine("Error: Failed to load image data into Pix object.");
                                        continue;
                                    }
                                    using (var pageOcr = engine.Process(pix))
                                    {
                                        var text = pageOcr.GetText();
                                        output += text;
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


                string textFilePath = Path.Combine(subPdfFolder, "dataOCR.txt");
                File.WriteAllText(textFilePath, output);

                string checksum = FileHelper.CalculateMD5(baseDestinationPath + NameFileWithoutExtension + "\\" + file.FileName);
                mimetype = FileProccess.Mimetype(file.FileName);
                string lang = FileHelper.DetectLanguage(output);



                //Document documenttoadd = Document.Create(fileNameWithoutExtension, "ASN000", output, baseDestinationPath + NameFileWithoutExtension + "\\" + file.FileName, "user", mimetype, checksum);
                Document documenttoadd = Document.Create(fileNameWithoutExtension, "", output, baseDestinationPath + NameFileWithoutExtension + "\\" + file.FileName, idowner, mimetype, checksum);

                documenttoadd.Lang = lang;
                if (typee != null && typee.Equals("FileShare"))
                {
                    documenttoadd.Source = DocumentSource.ConsumeFolder;
                }
                else
                {
                    documenttoadd.Source = DocumentSource.ApiUpload;
                }
                try
                {
                    string thumbnail_url = await FileHelper.CreateThumbnailOfADocumentAsync(documenttoadd, _logRepository);
                    documenttoadd.ThumbnailUrl = thumbnail_url;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    Console.WriteLine(ex.Message);
                }
               
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
                        apiUrl += "?result=" + Uri.EscapeDataString(extracted_text);

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
            }
        }


    }
}
