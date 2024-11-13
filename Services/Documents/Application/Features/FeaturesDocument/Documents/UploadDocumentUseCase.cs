using Application.Dtos.Correspondent;
using Application.Dtos.Documents;
using Application.Dtos.StoragePath;
using Application.Features.ArchiveSerialNumbersFeature;
using Application.Features.AssignDocumentMangement;
using Application.Helper;
using Application.Respository;
using Application.Services;
using Aspose.Pdf.Operators;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Core;
using Core.Exceptions;
using Domain.DocumentManagement;
using Domain.DocumentManagement.CustomFields;
using Domain.DocumentManagement.DocumentTypes;
using Domain.DocumentManagement.StoragePath;
using Domain.DocumentManagement.tags;
using Domain.Documents;
using Domain.FileTasks;
using Domain.Logs;
using Domain.Ports;
using Domain.Templates;
using Domain.Templates.Enum;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PdfSharp.Pdf;
using Serilog;




namespace Application.Features.FeaturesDocument.Documents
{
    public class UploadDocumentUseCase(IAzurePort azurePort,
        IDocumentTypeRepository documentTypeRepository,
        IConfiguration configuration,
        ILogRepository logRepository,
        IDocumentRepository documentRepository,
        ICustomFieldRepository customFieldRepository,
        AssignTagToDocument assignTag,
        AssignCorrespondentToDocument assignCorrespondentToDocument,
        // AssignDocumentTypeToDocument assignDocumentTypeToDocument,
        AssignStoragePathToDocument assignStoragePathToDocument,
        IFileTasksRepository fileTasksRepository,
        ITemplateRepository templateRepository,
        ITagRepository tagRepository,
        IUserGroupPort userGroupPort,
        IArchiveSerialNumberRepository archiveSerialNumberRepository,
        EncryptionHelper encryptionHelper

        ) : IUploadDocumentUseCase
    {

        public async Task<List<Document>> UplaodDocument(IFormFile file, string idOwner, string type, string document)
        {
            try
            {
                List<Document> docs = new List<Document>();
                DocumentAPI documentAPI = new DocumentAPI();
                //List<ASNInfo> pages = new List<ASNInfo>();


                Guid groupId = await GetGroupForUser(idOwner);

                if (file.ContentType.Contains("application/pdf") || file.FileName.Contains(".pdf"))
                {
                    List<ASNInfo> pages = FileProccess.ExtractAndProcessASN(file);
                    PdfDocument pdfDocument = FileProccess.OpenPdfDocumentAsync(file);
                    //var text = await azurePort.ReadDoc(file);
                    //var page = 0;
                    //var docNumber = 0;
                    //if (page < pdfDocument.PageCount)
                    //{
                    //    page = text.Pages.Count;
                    //    for (int i = 0; i < text.Pages.Count; i++)
                    //    {
                    //        Console.WriteLine($"Found Page words: {text.Pages[i].Words}");

                    //        foreach (var word in text.Pages[i].Words)
                    //        {
                    //            Console.WriteLine($"Found ASN on page {i}: {word}");
                    //            var match = Regex.Match(word.Content, @"EasyDocSep");

                    //            if (match.Success)
                    //            {
                    //                docNumber = docNumber + 1;
                    //                ASNInfo asnInfo = new ASNInfo
                    //                {
                    //                    PageNumber = docNumber,
                    //                    WordASN = match.Value
                    //                };
                    //                pages.Add(asnInfo);
                    //            }
                    //        }
                    //    }

                    //}

                    ArchiveSerialNumbers archive = await GetArchiveNumberByGroupId(groupId);



                    if (pages.Count > 0)
                    {

                        for (int i = 0; i < pages.Count; i++)
                        {
                            await UpdateArchiveNumber(archive);
                            int startPage = pages[i].PageNumber + 1; // Start from the next page after ASN
                            //int endPage=pages[i + 1].PageNumber - 1; ;

                            //// If it's the last ASN, set endPage to the last page of the document
                            //if (i < pages.Count - 1)
                            //{
                            //    endPage = pages[i + 1].PageNumber - 1; // Up to the page before the next ASN
                            //}
                            //else
                            //{
                            //    endPage = pdfDocument.PageCount; // Last page of the document
                            //}
                            int endPage = (i < pages.Count - 1) ? pages[i + 1].PageNumber : pdfDocument.Pages.Count;
                            using (MemoryStream pageStream = FileProccess.ExtractPageAsStream(pdfDocument, startPage, endPage))
                            {

                                // Clone the stream for classification (to avoid reusing a closed stream)
                                using (MemoryStream clonedStream = FileProccess.CloneStream(pageStream))
                                {
                                    string docType = await azurePort.ClassifyPdfDocumentType(clonedStream);
                                    pageStream.Position = 0;

                                    // Extract document details
                                    var dataExtract = await azurePort.ExtractPdfDoc(pageStream, docType);

                                    // Check if the document type exists in the repository
                                    var docTypeExist = await documentTypeRepository.FindByName(docType) ?? await AddDocumentType(docType, idOwner, dataExtract.Content);
                                    Document documentToAdd = new Document();
                                    if (!string.IsNullOrEmpty(document))
                                    {
                                        documentAPI = JsonConvert.DeserializeObject<DocumentAPI>(document);
                                        documentToAdd = documentAPI.Adapt<Document>();
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
                                    }
                                    documentToAdd.GroupId = groupId;
                                    pages[i].WordASN = await GenerateUniqueWordASN(archive, pages);
                                    idOwner = !string.IsNullOrEmpty(idOwner) ? idOwner : documentToAdd.Owner;
                                    // Save the page as a document and process it
                                    Document doc = await SavePageAsDocument(file, pdfDocument, dataExtract, docTypeExist, type, idOwner, pages[i], i, startPage, endPage, documentToAdd, pages.Count);
                                    docs.Add(doc);
                                    // Add or update custom fields for the document
                                    if (doc != null)
                                    {
                                        await AddOrUpdateCustomFields(doc, dataExtract);

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

                        Document documentToAdd = new Document();
                        if (!string.IsNullOrEmpty(document))
                        {
                            documentAPI = JsonConvert.DeserializeObject<DocumentAPI>(document);
                            documentToAdd = documentAPI.Adapt<Document>();
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
                        }
                        documentToAdd.GroupId = groupId;
                        idOwner = !string.IsNullOrEmpty(idOwner) ? idOwner : documentToAdd.Owner;
                        Document doc = await ProcessEntireDocument(file, type, idOwner, documentToAdd, archive);
                        docs.Add(doc);

                    }
                }
                if (file.ContentType.Contains("image/png") || file.ContentType.Contains("image/jpeg"))
                {
                    string docType = await azurePort.ClassifyDocumentType(file);
                    var dataExtract = await azurePort.ExtractDoc(file, docType);
                    var docTypeExist = await documentTypeRepository.FindByName(docType) ?? await AddDocumentType(docType, idOwner, type);
                    Document documentToAdd = new Document();
                    if (!string.IsNullOrEmpty(document))
                    {
                        documentAPI = JsonConvert.DeserializeObject<DocumentAPI>(document);
                        documentToAdd = documentAPI.Adapt<Document>();
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
                    }
                    documentToAdd.GroupId = groupId;
                    idOwner = !string.IsNullOrEmpty(idOwner) ? idOwner : documentToAdd.Owner;
                    Document doc = await ProcessSaveImage(documentToAdd, file, idOwner, type, docTypeExist, dataExtract);
                    await AddOrUpdateCustomFields(doc, dataExtract);
                }
                return docs;
            }
            catch (NotFoundException ex)
            {
                Log.Error(ex.Message);
                throw new NotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw new Exception("An error has occured, please try again later");
            }
        }

        public async Task<string> GenerateUniqueWordASN(ArchiveSerialNumbers archive, List<ASNInfo> pages)
        {

            string baseWordASN = $"{archive.Prefix}-{archive.GroupName}-{archive.Year.ToString("dd-MM-yyyy")}";

            Document lastDoc = await documentRepository.GetDocumentByGroupIdAndASN(archive.GroupId, baseWordASN);
            int nextSequenceNumber = 1;
            if (lastDoc != null)
            {

                string lastWordASN = lastDoc.Archive_Serial_Number;
                string[] parts = lastWordASN.Split('-');
                if (parts.Length > 5 && int.TryParse(parts[5], out int lastSequence))
                {
                    nextSequenceNumber = lastSequence + 1;
                }
            }
            string uniqueWordASN = $"{baseWordASN}-{nextSequenceNumber:D10}";

            while (pages.Any(p => p.WordASN == uniqueWordASN))
            {
                nextSequenceNumber++;
                uniqueWordASN = $"{baseWordASN}-{nextSequenceNumber:D10}";
            }

            // Assign the unique WordASN
            return uniqueWordASN;
        }
        //  {DateTime.Now.Ticks

        public async Task<Guid> GetGroupForUser(string idOwner)
        {
            try
            {
                var res = await userGroupPort.GetFirstGroupForUser(idOwner);
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
        private async Task<ArchiveSerialNumbers> GetArchiveNumberByGroupId(Guid groupId)
        {
            try
            {
                ArchiveSerialNumbers archiveSerialNumbers = await archiveSerialNumberRepository.GetArchiveNumberByGroupId(groupId);
                if (archiveSerialNumbers == null)
                {
                    throw new NotFoundException($"archive serial number not found.");
                }
                return archiveSerialNumbers;
            }
            catch (NotFoundException ex)
            {
                Log.Error(ex.Message);
                throw new NotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }

        }
        private async Task UpdateArchiveNumber(ArchiveSerialNumbers achiveNumber)
        {
            try
            {
                achiveNumber.DocumentCount += 1;
                await archiveSerialNumberRepository.UpdateAsync(achiveNumber);

            }
            catch (NotFoundException ex)
            {
                Log.Error(ex.Message);
                throw new NotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }

        }
        private async Task<DocumentType> AddDocumentType(string docType, string ownerId, string content)
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


            await documentTypeRepository.AddAsync(newDocumentType);
            return newDocumentType;
        }

        public async Task<Document> SavePageAsDocument(
       IFormFile file,
       PdfDocument pdfDocument,
       AnalyzeResult result,
       DocumentType docTypeExist,
       string type,
       string idOwner,
       ASNInfo asnPage,
       int i, int startPage, int endPage,
       Document documentToAdd, int count)
        {
            try
            {
                await LogDocumentProcessing(file);

                // Extract PDF pages for this document
                PdfDocument subDocument = FileProccess.ExtractPages(pdfDocument, startPage, endPage);
                if (subDocument.PageCount == 0) return documentToAdd;  // No pages to process

                // Determine file name and output folder
                string fileName = count == 0 ? Path.GetFileNameWithoutExtension(file.FileName) : asnPage.WordASN;
                string pathFolder = configuration.GetSection("OriginalsSettings:OutputFolder").Value!;
                string subPdfFolder = FileProccess.CreateDirectoryForFile(file, asnPage.WordASN, pathFolder);

                // Save extracted PDF and generate paths
                string subPdfPath = FileProccess.SavePdfDocument(subDocument, subPdfFolder, fileName);
                byte[] key = EncryptionHelper.GenerateEncryptionKey(32);
                byte[] iv = EncryptionHelper.GenerateEncryptionIV(16);
                string textFilePath = SaveOcrData(result.Content, subPdfFolder, fileName, key, iv);

                // Generate document metadata
                string checksum = FileHelper.CalculateMD5(subPdfPath);
                string lang = FileHelper.DetectLanguage(result.Content);

                // Create Document object
                documentToAdd = CreateDocument(
                    documentToAdd, fileName, asnPage.WordASN, result.Content,
                    subPdfPath, idOwner, file.ContentType, checksum, lang, type, docTypeExist.Id
                );

                string encryptedPdfPath = EncryptAnyDocument(subPdfPath, subPdfFolder, $"{fileName}.pdf", key, iv);
                EncryptAnyDocument(documentToAdd.ThumbnailUrl, subPdfFolder, $"{fileName}_{DateTime.Now:yyyyMMdd}.jpg", key, iv);

                // Assign tags, documentType, correspondent, and storage path
                await AssignDocumentProperties(documentToAdd, idOwner, docTypeExist.Name);

                // Create a zip file containing the PDF, OCR data, and thumbnail
                string zipFilePath = FileProccess.CreateZipFile(subPdfFolder, subPdfPath, textFilePath, fileName, documentToAdd.ThumbnailUrl);

                // Clean up temporary files
                CleanupFiles(encryptedPdfPath, textFilePath, documentToAdd.ThumbnailUrl);

                // Update document paths with zip content
                documentToAdd.FileData = Path.Combine(zipFilePath, $"{fileName}.pdf");
                documentToAdd.ThumbnailUrl = Path.Combine(zipFilePath, $"{fileName}_{DateTime.Now:yyyyMMdd}.jpg");
                documentToAdd.Key = encryptionHelper.EncryptKey(key);
                documentToAdd.Iv = encryptionHelper.EncryptKey(iv);
                // Add document 
                await documentRepository.AddAsync(documentToAdd);
                await HandleDocumentProblems(documentToAdd);

                return documentToAdd;
            }
            catch (Exception ex)
            {
                Log.Error($"Error processing document {file.FileName}: {ex.Message}");
                throw new Exception($"Error processing document: {ex.Message}", ex);
            }
        }

        private string SaveOcrData(string ocrContent, string outputFolder, string fileName, byte[] key, byte[] iv)
        {
            string textFilePath = Path.Combine(outputFolder, $"{fileName}.txt");
            File.WriteAllText(textFilePath, ocrContent);
            EncryptAnyDocument(textFilePath, outputFolder, $"{fileName}.txt", key, iv);
            return textFilePath;
        }

        private void CleanupFiles(params string[] filePaths)
        {
            foreach (var path in filePaths)
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        private async Task AssignDocumentProperties(Document document, string idOwner, string docTypeName)
        {
            if (!string.IsNullOrEmpty(idOwner))
            {
                await AssignPropertiesToDocument(document, idOwner, docTypeName);
            }
        }

        private async Task HandleDocumentProblems(Document document)
        {
            if (document.Owner == null || document.CorrespondentId == null)
            {
                Problem theProblem = document.Owner == null && document.CorrespondentId == null
                    ? Problem.NoOwnerNoCorrespondent
                    : document.Owner == null ? Problem.NoOwner : Problem.NoCorrespondent;

                FileTasks fileTaskToAdd = new FileTasks
                {
                    Source = document.Source,
                    Task_document = document,
                    Task_problem = theProblem
                };
                await fileTasksRepository.AddAsync(fileTaskToAdd);
            }
        }


        public Document CreateDocument(Document doc, string fileNameWithoutExtension, string wordASN, string content, string pdfPath, string idowner, string mimetype, string checksum, string lang, string? typee, Guid docTypeId)
        {

            Document documentToAdd = Document.Create(fileNameWithoutExtension, wordASN, content, pdfPath, idowner, mimetype, checksum);
            documentToAdd.Lang = lang;
            documentToAdd.Source = typee != null && typee.Equals("FileShare") ? DocumentSource.ConsumeFolder : typee.Equals("Mail") ? DocumentSource.MailFetch : DocumentSource.ApiUpload;
            documentToAdd.Mailrule = null;
            documentToAdd.ThumbnailUrl = FileHelper.CreateThumbnailOfADocumentAsync(documentToAdd, logRepository).Result;
            documentToAdd.Content = content;
            documentToAdd.DocumentTypeId = docTypeId;
            documentToAdd.StoragePathId = doc?.StoragePathId;
            documentToAdd.CorrespondentId = doc.CorrespondentId;
            documentToAdd.Tags = doc?.Tags != null ? doc?.Tags : null;
            documentToAdd.Base64Data = FileProccess.ConvertPdfToBase64(pdfPath);
            documentToAdd.GroupId = doc?.GroupId;
            documentToAdd.Archive_Serial_Number = wordASN;

            return documentToAdd;
        }
        private string EncryptAnyDocument(string subPath, string subFolder, string fileName, byte[] key, byte[] iv)
        {
            byte[] encryptedPdfBytes = EncryptionHelper.EncryptDocument(subPath, key, iv);
            string encryptedPdfPath = Path.Combine(subFolder, fileName);
            File.WriteAllBytes(encryptedPdfPath, encryptedPdfBytes);
            return encryptedPdfPath;
        }
        public async Task LogDocumentProcessing(IFormFile file)
        {
            Logs initcon = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Consuming {file.FileName}");
            Logs mime = Logs.Create(LogLevel.DEBUG, LogName.EasyDoc, $"mime type: {file.ContentType}");
            await logRepository.AddAsync(initcon);
            await logRepository.AddAsync(mime);
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

                        var customField = await customFieldRepository.FindByNameAsync(fieldName);
                        if (customField != null)
                        {

                            customField.Data_type = field.FieldType.ToString().ToLower() == "Unknown".ToLower() ? CustomFieldMaper.HandleCustomFieldType(field.ExpectedFieldType.ToString()) : CustomFieldMaper.HandleCustomFieldType(field.FieldType.ToString());

                            DocumentCustomField documentCustomField = new DocumentCustomField
                            {
                                Document = docToAdd,
                                DocumentId = docToAdd.Id,
                                CustomFieldId = customField.Id,
                                CustomField = customField,
                                Value = field.Content

                            };


                            // customField.Data_type = CustomFieldMaper.HandleCustomFieldType(field.FieldType.ToString());
                            customField?.DocumentsCustomFields?.Add(documentCustomField);
                            await customFieldRepository.UpdateAsync(customField);

                        }
                        else
                        {
                            // Add a new custom field if it doesn't exist
                            CustomField customFieldToAdd = new CustomField()
                            {
                                Data_type = field.FieldType.ToString() == "Unknown" ? CustomFieldMaper.HandleCustomFieldType(field.ExpectedFieldType.ToString()) : CustomFieldMaper.HandleCustomFieldType(field.FieldType.ToString()),
                                Name = fieldName,
                            };

                            await customFieldRepository.AddAsync(customFieldToAdd);
                            DocumentCustomField documentCustomField = new DocumentCustomField
                            {
                                Document = docToAdd,
                                DocumentId = docToAdd.Id,
                                CustomFieldId = customFieldToAdd.Id,
                                CustomField = customFieldToAdd,
                                Value = field.Content

                            };
                            customFieldToAdd?.DocumentsCustomFields?.Add(documentCustomField);
                            await customFieldRepository.UpdateAsync(customFieldToAdd);


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }



        }
        public async Task<Document> ProcessEntireDocument(IFormFile file, string type, string idOwner, Document documentToAdd, ArchiveSerialNumbers archive)
        {
            try
            {
                await LogDocumentProcessing(file);
                using (var pdfDocument = FileProccess.OpenPdfDocumentAsync(file))
                {
                    int startPage = 0;
                    int endPage = pdfDocument.Pages.Count;
                    string docType = await azurePort.ClassifyDocumentType(file);
                    var dataExtract = await azurePort.ExtractDoc(file, docType);

                    var asnPage = new ASNInfo()
                    {
                        PageNumber = startPage,
                        WordASN = await GenerateUniqueWordASN(archive, new List<ASNInfo>()),
                    };
                    var docTypeExist = await documentTypeRepository.FindByName(docType) ?? await AddDocumentType(docType, idOwner, dataExtract.Content);
                    Document doc = await SavePageAsDocument(file, pdfDocument, dataExtract, docTypeExist, type, idOwner, asnPage, startPage, startPage, endPage, documentToAdd, asnPage.PageNumber);
                    if (doc != null)
                    {
                        await AddOrUpdateCustomFields(doc, dataExtract);

                    }
                    else
                    {
                        Log.Error("Can not add null Document");
                        throw new Exception("Can not add null Document");
                    }

                    return doc;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Message Error: {ex.Message}");
                throw new Exception(ex.Message);
            }

        }

        private async Task<Document> ProcessSaveImage(Document documentToAdd, IFormFile file, string idOwner, string type, DocumentType documentType, AnalyzeResult result)
        {




            var pathFolder = configuration.GetSection("OriginalsSettings:OutputFolder").Value!;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);

            string folderPath = FileProccess.CreateDirectoryForFile(file, string.Empty, pathFolder);

            // Save the uploaded image to the file system
            string filePath = FileProccess.SaveFileToDisk(file, folderPath);

            // Create and save PDF from the image
            string pdfFileName = System.IO.Path.GetFileNameWithoutExtension(file.FileName) + ".pdf";
            string pdfFilePath = FileProccess.CreatePdfFromImage(filePath, folderPath, pdfFileName);



            string textFileName = Path.GetFileNameWithoutExtension(file.FileName) + ".txt";
            string textFilePath = Path.Combine(folderPath, textFileName);
            File.WriteAllText(textFilePath, result.Content.ToString());

            string checksum = FileHelper.CalculateMD5(filePath);
            string lang = FileHelper.DetectLanguage(result.Content.ToString());

            documentToAdd = CreateDocument(documentToAdd, fileNameWithoutExtension, "", result.Content, pdfFilePath, idOwner, file.ContentType, checksum, lang, type, documentType.Id);

            //if (documentToAdd.Tags == null && documentToAdd.CorrespondentId == null && documentToAdd.StoragePathId == null)
            //{
            //    // assign tags, documentType, Correspondent , storage path
            //    await AssignDocumentProperties(documentToAdd, idOwner);
            //}
            if (!string.IsNullOrEmpty(idOwner))
            {
                await AssignPropertiesToDocument(documentToAdd, idOwner, documentType.Name);

            }
            if (documentToAdd != null)
            {

                await documentRepository.AddAsync(documentToAdd);
                Problem the_problem = Problem.None;
                if (documentToAdd.Owner == null || documentToAdd.CorrespondentId == null)
                {
                    if (documentToAdd.Owner == null && documentToAdd.CorrespondentId == null)
                    {
                        the_problem = Problem.NoOwnerNoCorrespondent;
                    }
                    else if (documentToAdd.Owner == null)
                    {
                        the_problem = Problem.NoOwner;
                    }
                    else if (documentToAdd.CorrespondentId == null)
                    {
                        the_problem = Problem.NoCorrespondent;
                    }

                    FileTasks fileTaskToAdd = new FileTasks
                    {
                        Source = documentToAdd.Source,
                        Task_document = documentToAdd,
                        Task_problem = the_problem
                    };

                    await fileTasksRepository.AddAsync(fileTaskToAdd);
                }
            }
            return documentToAdd;
            //List<Document> documentList = await FilterAndSaveDoc(file, result, documentType, type, id);

        }
        public async Task AssignTagsToDocument(Document documentToAdd, string idOwner)
        {
            if (documentToAdd.Tags == null)
            {
                List<DocumentTags> documentTags = await assignTag.AssignTag(documentToAdd, idOwner);
                documentToAdd.Tags = documentTags != null ? documentTags : null;

            }
        }
        public async Task AssignCorrespondentToDocument(Document documentToAdd, string idOwner)
        {
            if (documentToAdd.CorrespondentId == null)
            {
                CorrespondentListDTO correspondent = await assignCorrespondentToDocument.AssignCorrespondent(documentToAdd, idOwner);
                documentToAdd.CorrespondentId = correspondent?.Id;
            }
        }
        public async Task<StoragePathDto> AssignStorageToDocument(Document documentToAdd, string idOwner)
        {
            if (documentToAdd.StoragePathId == null)
            {
                StoragePathDto storagePath = await assignStoragePathToDocument.AssignStoragePath(documentToAdd, idOwner);
                documentToAdd.StoragePathId = storagePath?.Id;

                return storagePath;
            }
            return null;
        }
        public async Task AssignPropertiesToDocument(Document documentToAdd, string idOwner, string docType)
        {

            List<Template> templates = await templateRepository.GetAllByOwner(idOwner);

            bool filterResult = false;
            foreach (Template template in templates)
            {
                if ((bool)!template.Is_Enabled)
                    continue;



                if (template.Type == GetListByType.Started)
                {
                    filterResult = FileHelper.ConsumptionStartedFilter(documentToAdd, template, docType);
                }
                await AssignTagsToDocument(documentToAdd, idOwner);
                await AssignCorrespondentToDocument(documentToAdd, idOwner);
                StoragePathDto storage = await AssignStorageToDocument(documentToAdd, idOwner);
                if (storage != null)
                {   // Copy the document to Storage Folder
                    addArchiveStoragePath(storage, documentToAdd);
                }
                if (template.Type == GetListByType.Added)
                {
                    filterResult = FileHelper.DocumentAddedFilter(documentToAdd, template);
                }


                if (filterResult)
                {
                    await AssignTemplateValuesToDocument(documentToAdd, template);
                }

            }

        }

        private async Task AssignTemplateValuesToDocument(Document document, Template template)
        {
            // Assign simple fields
            document.Title = template.AssignTitle ?? document.Title;
            document.DocumentTypeId = template.AssignDocumentType ?? document.DocumentTypeId;
            document.CorrespondentId = template.AssignCorrespondent ?? document.CorrespondentId;
            document.StoragePathId = template.AssignStoragePath ?? document.StoragePathId;

            // Assign tags if available
            if (template.AssignTags != null && template.AssignTags.Any())
            {
                List<DocumentTags> tags = new List<DocumentTags>();
                foreach (Guid tagId in template.AssignTags)
                {
                    Tag tagToAdd = await tagRepository.FindByIdAsync(tagId); // Use await to avoid blocking
                    if (tagToAdd != null)
                    {
                        DocumentTags documentTag = new DocumentTags
                        {
                            Document = document,
                            DocumentId = document.Id,
                            Tag = tagToAdd,
                            TagId = tagToAdd.Id
                        };
                        tags.Add(documentTag);
                    }
                }

                if (tags.Any())
                {
                    document.Tags = tags;
                }
            }
        }

        //public async Task<StoragePathDto> AssignDocumentProperties(Document documentToAdd, string idowner)
        //{
        //    //, string classification_result, JObject uniqueSubdirJson

        //    if (documentToAdd.Tags == null )
        //    {
        //        List<DocumentTags> documentTags = await _assignTag.AssignTag(documentToAdd, idowner);
        //        documentToAdd.Tags = documentTags != null ? documentTags : null;
        //    }


        //    if (documentToAdd.CorrespondentId == null)
        //    {
        //        CorrespondentListDTO correspondent = await _assignCorrespondentToDocument.AssignCorrespondent(documentToAdd, idowner);
        //        documentToAdd.CorrespondentId = correspondent?.Id;
        //    }

        //    // Check and assign Document Type
        //    //DocumentTypeDetailsDTO documentType = await assignDocumentTypeToDocument.AssignDocumenttype(documenttoadd, idowner, classification_result, uniqueSubdirJson);
        //    //documenttoadd.DocumentTypeId = documentType?.Id;

        //    // Check and assign storage path
        //    if(documentToAdd.StoragePathId == null)
        //    {
        //        StoragePathDto storagePath = await _assignStoragePathToDocument.AssignStoragePath(documentToAdd, idowner);
        //        documentToAdd.StoragePathId = storagePath?.Id;

        //    }
        //    return storagePath;
        //}
        public void addArchiveStoragePath(StoragePathDto storagepath, Document document)
        {
            // Copy PDF files to the destination folder
            if (storagepath != null && !string.IsNullOrEmpty(storagepath.Path))
            {
                string[] pathParts = storagepath.Path.Split('/', '-');

                // Create folder structure
                string currentPath = storagepath.Path;
                foreach (string part in pathParts)
                {
                    currentPath = Path.Combine(currentPath, part);
                    Directory.CreateDirectory(currentPath);
                }

                // Destination folder for the PDF
                string fileName = Path.GetFileName(document.FileData);
                string destinationPath = Path.Combine(currentPath, fileName);

                // Copy the file
                File.Copy(document.FileData, destinationPath, true);
            }

        }
    }

}
