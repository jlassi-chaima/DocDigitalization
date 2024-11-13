////using Domain.MailRules.Enum;
////using Domain.MailRules;
////using MimeKit;
////using Application.Consumers.RestAPIDocuments.Dtos;
////using MailKit;
////using Microsoft.Extensions.Configuration;
////using Domain.Templates.Enum;
////using System.Net.Mail;
////using NPOI.XWPF.UserModel;
////using System.Text;
////using Document = Application.Consumers.RestAPIDocuments.Dtos.Document;
////using Microsoft.Office.Core;
////using Microsoft.Office.Interop.PowerPoint;
////using static System.Net.WebRequestMethods;

////namespace Application.Features.Helper.ProcessorHelper
////{
////    public static class ProcessorInlineAttachementHelper
////    {
////        private static readonly IConfiguration _configuration;

////        public async static void save_eml_files_for_online_attachement_including_inline(MimeMessage message, string directorypath, MailRule mail, string title, List<Document> documents, UniqueId uid, Guid corr_id, HttpClient http)
////        {


////            if (message.BodyParts.OfType<MimePart>().Any(part => part.IsAttachment == false))
////            {
////                Document document = new Document();
////                string filename = "";
////                string emailDetails = AttachmentUtilities.get_mail_details(message);
////                // Convert the BodyBuilder to HTML
////                string htmlBody = message.HtmlBody;
////                // save 
////                var folderName = string.IsNullOrWhiteSpace(message.Subject) ? "NoSubject" : message.Subject;
////                folderName = string.Join("_", folderName.Split(Path.GetInvalidFileNameChars())); // Remove invalid characters from folder name
////                var folderPath = Path.Combine(directorypath, folderName);
////                Directory.CreateDirectory(folderPath); // Create directory if not exists
////                string pdfFileName = $"Email_Details_{uid}.pdf";
////                string pdfPath = Path.Combine(folderPath, pdfFileName);
////                // Convert the HTML body to PDF Gotenberg Server
////                AttachmentUtilities.ConvertHtmlToPdf(htmlBody, emailDetails, pdfPath);
////                string emlFileName = $"Email_Content_{uid}.eml";
////                string emlPath = Path.Combine(folderPath, emlFileName);
////                // Save the content of the email message to a .eml file
////                AttachmentUtilities.SaveEmlContent(message, emlPath);
////                // Create a Document object
////                if (mail.Assign_title_from == MailMetadataTitleOption.FromSubject)
////                {
////                    title = message.Subject;
////                }
////                if (mail.Assign_title_from == MailMetadataTitleOption.FromFilename)
////                {
////                    title = filename;
////                }
////                string strippedEmailDetails = AttachmentUtilities.StripHtmlTags(emailDetails);
////                // Get Id User 
////                var response = await http.GetAsync($"http://localhost:5183/user/getuserbymail?mail={message.To}");
////                string idfromidentity = await response.Content.ReadAsStringAsync();
////                // inti Owner
////                string ownerofdocument = "";
////                if (idfromidentity != null)
////                {
////                    ownerofdocument = idfromidentity;
////                }
////                else
////                    ownerofdocument = null;

////                if (corr_id == Guid.Empty)
////                {
////                    document = new Document
////                    {
////                        Title = title, // Set title to the file name
////                        Content = strippedEmailDetails,
////                        FileData = pdfPath,
////                        MimeType = "application/pdf",
////                        Archive_Serial_Number = "",
////                        Checksum = "",
////                        Tags = mail.Assign_tags,
////                        DocumentTypeId = mail.Assign_document_type,
////                        CorrespondentId = null,
////                        Owner = message.To.ToString(),
////                        Source = DocumentSource.MailFetch,
////                        Mailrule = mail.Name
////                    };
////                }
////                else
////                {
////                    document = new Document
////                    {
////                        Title = title, // Set title to the file name
////                        Content = strippedEmailDetails,
////                        FileData = pdfPath,
////                        MimeType = "application/pdf",
////                        Archive_Serial_Number = "",
////                        Checksum = "",
////                        Tags = mail.Assign_tags,
////                        DocumentTypeId = mail.Assign_document_type,
////                        CorrespondentId = corr_id,
////                        Owner = message.To.ToString(),
////                        Source = DocumentSource.MailFetch,
////                        Mailrule = mail.Name
////                    };

////                }
////                documents.Add(document);

////            }
////        }



////        public static void save_pdf_and_images_for_online_attachement_including_inline(MimeMessage message, string directorypath, MailRule mail, string title, List<Document> documents, string content, string tessDataPath, Guid corr_id)
////        {
////            Document document = new Document();
////            bool hasInlineAttachments = false;
////            foreach (var part in message.BodyParts)
////            {
////                //in case we specify filter_filename 
////                if (!string.IsNullOrEmpty(mail.Filter_attachment_filename) && AttachmentUtilities.MatchWildcard(part.ContentType.Name, mail.Filter_attachment_filename))
////                {
////                    Adding_documents_inline_attachment(message, directorypath, mail, title, documents, content, tessDataPath, corr_id, part, document);
////                }
////                //in case we don't specify a filter_filename
////                else if (string.IsNullOrEmpty(mail.Filter_attachment_filename))
////                {
////                    Adding_documents_inline_attachment(message, directorypath, mail, title, documents, content, tessDataPath, corr_id, part, document);
////                }


////            }
////        }
////        public static void Adding_documents_inline_attachment(MimeMessage message, string directorypath, MailRule mail, string title, List<Document> documents, string content, string tessDataPath, Guid corr_id, MimeEntity part, Document document)
////        {
////            if (part is MimePart mimePart && mimePart.IsAttachment == true)
////            {
////                if (mimePart.ContentType.MediaSubtype == "vnd.openxmlformats-officedocument.wordprocessingml.document")
////                {
////                    if (mail.Assign_title_from == MailMetadataTitleOption.FromSubject)
////                    {
////                        title = message.Subject;

////                    }
////                    if (mail.Assign_title_from == MailMetadataTitleOption.FromFilename)
////                    {
////                        title = mimePart.FileName;
////                    }
////                    var filePath = AttachmentUtilities.file_emplacement(message, mimePart, directorypath);

////                    // Create an instance of Word Application
////                    Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();

////                    // Open the Word document
////                    Microsoft.Office.Interop.Word.Document doc_document = wordApp.Documents.Open(filePath);

////                    // Save the document as PDF
////                    doc_document.SaveAs2(Path.ChangeExtension(filePath, "pdf"), Microsoft.Office.Interop.Word.WdSaveFormat.wdFormatPDF);

////                    // Close the document
////                    doc_document.Close();

////                    // Quit Word application
////                    wordApp.Quit();

////                    // Open the saved Word file directly for reading
////                    XWPFDocument wordDocument = new XWPFDocument(File.OpenRead(filePath));

////                    // Create a StringBuilder to store the text content
////                    StringBuilder result = new StringBuilder();

////                    // Iterate over paragraphs in the document
////                    foreach (XWPFParagraph paragraph in wordDocument.Paragraphs)
////                    {
////                        // Get text from the paragraph
////                        string text = paragraph.Text;

////                        // Append text to the result StringBuilder
////                        result.AppendLine(text);
////                    }

////                    // Dispose of the Word document object
////                    wordDocument.Close();

////                    string strippedDetails = AttachmentUtilities.StripHtmlTags(result.ToString());
////                    // Create a Document object
////                    if (corr_id == Guid.Empty)
////                    {
////                        document = new Document
////                        {
////                            Title = title, // Set title to the file name
////                            Content = strippedDetails, // You may set content as needed
////                            FileData = filePath, // Convert file content to Base64 string
////                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
////                            Archive_Serial_Number = "",
////                            Checksum = "",// Set other properties as needed
////                            Tags = mail.Assign_tags,
////                            DocumentTypeId = mail.Assign_document_type,
////                            CorrespondentId = null,
////                            Owner = message.To.ToString(),
////                            Source = DocumentSource.MailFetch,
////                            Mailrule = mail.Name

////                        };
////                    }
////                    else
////                    {
////                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
////                        {
////                            Title = title, // Set title to the file name
////                            Content = strippedDetails, // You may set content as needed
////                            FileData = filePath, // Convert file content to Base64 string
////                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
////                            Archive_Serial_Number = "",
////                            Checksum = "",// Set other properties as needed
////                            Tags = mail.Assign_tags,
////                            DocumentTypeId = mail.Assign_document_type,
////                            CorrespondentId = corr_id,
////                            Owner = message.To.ToString(),
////                            Source = DocumentSource.MailFetch,
////                            Mailrule = mail.Name

////                        };

////                    }
////                    documents.Add(document);



////                }
////                else if (mimePart.ContentType.MediaSubtype == "vnd.openxmlformats-officedocument.presentationml.presentation")

////                {
////                    if (mail.Assign_title_from == MailMetadataTitleOption.FromSubject)
////                    {
////                        title = message.Subject;

////                    }
////                    if (mail.Assign_title_from == MailMetadataTitleOption.FromFilename)
////                    {
////                        title = mimePart.FileName;
////                    }
////                    var filePath = AttachmentUtilities.file_emplacement(message, mimePart, directorypath);
////                    // Create an instance of PowerPoint Application
////                    Microsoft.Office.Interop.PowerPoint.Application powerPointApp = new Microsoft.Office.Interop.PowerPoint.Application();

////                    // Open the presentation
////                    Presentation presentation = powerPointApp.Presentations.Open(filePath, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);

////                    // Save the presentation as PDF
////                    presentation.SaveAs(filePath, PpSaveAsFileType.ppSaveAsPDF);

////                    // Create a StringBuilder to store the text content
////                    StringBuilder result = new StringBuilder();

////                    // Iterate over slides in the presentation
////                    foreach (Slide slide in presentation.Slides)
////                    {
////                        // Iterate over shapes in the slide
////                        foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
////                        {
////                            // Check if the shape contains text
////                            if (shape.HasTextFrame == MsoTriState.msoTrue && shape.TextFrame.HasText == MsoTriState.msoTrue)
////                            {
////                                // Append text from the shape to the result StringBuilder
////                                result.AppendLine(shape.TextFrame.TextRange.Text);
////                            }
////                        }
////                    }

////                    // Close the presentation
////                    presentation.Close();

////                    // Quit PowerPoint Application
////                    powerPointApp.Quit();

////                    string strippedDetails = AttachmentUtilities.StripHtmlTags(result.ToString());
////                    // Create a Document object
////                    if (corr_id == Guid.Empty)
////                    {
////                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
////                        {
////                            Title = title, // Set title to the file name
////                            Content = strippedDetails, // You may set content as needed
////                            FileData = filePath, // Convert file content to Base64 string
////                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
////                            Archive_Serial_Number = "",
////                            Checksum = "",// Set other properties as needed
////                            Tags = mail.Assign_tags,
////                            DocumentTypeId = mail.Assign_document_type,
////                            CorrespondentId = null,
////                            Owner = message.To.ToString(),
////                            Source = DocumentSource.MailFetch,
////                            Mailrule = mail.Name

////                        };
////                    }
////                    else
////                    {
////                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
////                        {
////                            Title = title, // Set title to the file name
////                            Content = strippedDetails, // You may set content as needed
////                            FileData = filePath, // Convert file content to Base64 string
////                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
////                            Archive_Serial_Number = "",
////                            Checksum = "",// Set other properties as needed
////                            Tags = mail.Assign_tags,
////                            DocumentTypeId = mail.Assign_document_type,
////                            CorrespondentId = corr_id,
////                            Owner = message.To.ToString(),
////                            Source = DocumentSource.MailFetch,
////                            Mailrule = mail.Name

////                        };

////                    }
////                    documents.Add(document);


////                }
////               else if (mimePart.ContentType.MediaSubtype == "pdf")
////                {

////                    using (var memoryStream = new MemoryStream())
////                    {
////                        mimePart.Content.DecodeTo(memoryStream); // Assuming Content is a byte[] containing the PDF content
////                        memoryStream.Position = 0;

////                        string pdfText = AttachmentUtilities.ExtractTextFromPdf(memoryStream);
////                        Console.WriteLine($"Extracted text from PDF:\n{pdfText}");
////                        content = pdfText;

////                    }
////                    var filePath = AttachmentUtilities.file_emplacement(message, mimePart, directorypath);

////                    Console.WriteLine("Document saved: " + filePath);

////                    if (mail.Assign_title_from == MailMetadataTitleOption.FromSubject)
////                    {
////                        title = message.Subject;

////                    }
////                    if (mail.Assign_title_from == MailMetadataTitleOption.FromFilename)
////                    {
////                        title = mimePart.FileName;
////                    }
////                    string strippedDetails = AttachmentUtilities.StripHtmlTags(content);

////                    // Create a Document object
////                    if (corr_id == Guid.Empty)
////                    {
////                        document = new Document
////                        {
////                            Title = title, // Set title to the file name
////                            Content = strippedDetails,
////                            FileData = filePath, // Convert file content to Base64 string
////                            MimeType = mimePart.ContentType.MediaType, // Set MIME type
////                            Archive_Serial_Number = "",
////                            Checksum = "",// Set other properties as needed
////                            Tags = mail.Assign_tags,
////                            DocumentTypeId = mail.Assign_document_type,
////                            CorrespondentId = null,
////                            Owner = message.To.ToString(),
////                            Source = DocumentSource.MailFetch,
////                            Mailrule = mail.Name
////                        };
////                    }
////                    else
////                    {
////                        document = new Document
////                        {
////                            Title = title, // Set title to the file name
////                            Content = strippedDetails,
////                            FileData = filePath, // Convert file content to Base64 string
////                            MimeType = mimePart.ContentType.MediaType, // Set MIME type
////                            Archive_Serial_Number = "",
////                            Checksum = "",// Set other properties as needed
////                            Tags = mail.Assign_tags,
////                            DocumentTypeId = mail.Assign_document_type,
////                            CorrespondentId = corr_id,
////                            Owner = message.To.ToString(),
////                            Source = DocumentSource.MailFetch,
////                            Mailrule = mail.Name
////                        };

////                    }
////                    documents.Add(document);
////                }
////                else if (mimePart.ContentType.MediaType.StartsWith("image"))
////                {
////                    var filePath = AttachmentUtilities.file_emplacement(message, mimePart, directorypath);
////                    using (var stream = File.Create(filePath))
////                    {
////                        mimePart.Content.DecodeTo(stream);
////                    }

////                    Console.WriteLine("Image saved: " + filePath);

////                    // Extract text from image using Tesseract
////                    string extractedText = AttachmentUtilities.ExtractTextFromImage(filePath, tessDataPath);
////                    Console.WriteLine("Extracted text: " + extractedText);

////                    if (mail.Assign_title_from == MailMetadataTitleOption.FromSubject)
////                    {
////                        title = message.Subject;

////                    }
////                    if (mail.Assign_title_from == MailMetadataTitleOption.FromFilename)
////                    {
////                        title = mimePart.FileName;
////                    }
////                    string strippedDetails = AttachmentUtilities.StripHtmlTags(extractedText);
////                    // Create a Document object
////                    if (corr_id == Guid.Empty)
////                    {
////                        document = new Document
////                        {
////                            Title = title, // Set title to the file name
////                            Content = strippedDetails, // You may set content as needed
////                            FileData = filePath, // Convert file content to Base64 string
////                            MimeType = mimePart.ContentType.MediaSubtype, // Set MIME type
////                            Archive_Serial_Number = "",
////                            Checksum = "",// Set other properties as needed
////                            Tags = mail.Assign_tags,
////                            DocumentTypeId = mail.Assign_document_type,
////                            CorrespondentId = null,
////                            Owner = message.To.ToString(),
////                            Source = DocumentSource.MailFetch,
////                            Mailrule = mail.Name

////                        };
////                    }
////                    else
////                    {
////                        document = new Document
////                        {
////                            Title = title, // Set title to the file name
////                            Content = strippedDetails, // You may set content as needed
////                            FileData = filePath, // Convert file content to Base64 string
////                            MimeType = mimePart.ContentType.MediaSubtype, // Set MIME type
////                            Archive_Serial_Number = "",
////                            Checksum = "",// Set other properties as needed
////                            Tags = mail.Assign_tags,
////                            DocumentTypeId = mail.Assign_document_type,
////                            CorrespondentId = corr_id,
////                            Owner = message.To.ToString(),
////                            Source = DocumentSource.MailFetch,
////                            Mailrule = mail.Name

////                        };

////                    }
////                    documents.Add(document);
////                }
////            }

////        }
////    }
////}
//using Application.Consumers.RestAPIDocuments.Dtos;
//using Domain.MailRules;
//using Domain.MailRules.Enum;
//using Domain.Templates.Enum;
//using MailKit;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Office.Core;
//using Microsoft.Office.Interop.PowerPoint;
//using MimeKit;
//using NPOI.SS.UserModel;
//using NPOI.XSSF.UserModel;
//using NPOI.XWPF.UserModel;
//using System.Collections.Generic;
//using System.Text;
//using MigraDocCore.DocumentObjectModel;
//using MigraDocCore.Rendering;
//using static MassTransit.ValidationResultExtensions;
//using Tesseract;
//using PdfSharp.Pdf;
//using PdfSharp.Drawing;
//using System.Net.Http;
//using static System.Net.WebRequestMethods;

//namespace Application.Features.Helper.ProcessorHelper
//{
//    public static class ProcessorInlineAttachementHelper
//    {
//        private static readonly IConfiguration _configuration;

//        public async static void save_eml_files_for_online_attachement_including_inline(MimeMessage message, string directorypath, MailRule mail, string title, List<Application.Consumers.RestAPIDocuments.Dtos.Document> documents, UniqueId uid, Guid corr_id, HttpClient http)
//        {
//            Application.Consumers.RestAPIDocuments.Dtos.Document document = new Application.Consumers.RestAPIDocuments.Dtos.Document();
//            if (message.BodyParts.OfType<MimePart>().Any(part => part.IsAttachment == false))
//            {
//                // Create a BodyBuilder to construct the email body
//                var bodyBuilder = new BodyBuilder();

//                string filename = "";

//                string emailDetails = AttachmentUtilities.get_mail_details(message);
//                // Convert the BodyBuilder to HTML
//                string htmlBody = message.HtmlBody;
//                // save 
//                string foldername = string.IsNullOrWhiteSpace(message.Subject) ? "NoSubject" : message.Subject;
//                foldername = string.Join("_", foldername.Split(Path.GetInvalidFileNameChars()));
//                var folderpath = Path.Combine(directorypath, foldername);
//                Directory.CreateDirectory(folderpath); // Create directory if not exists
//                string pdfFileName = $"Email_Details_{uid}.pdf";
//                string pdfPath = Path.Combine(folderpath, pdfFileName);

//                // Convert the HTML body to PDF Gotenberg Server
//                AttachmentUtilities.ConvertHtmlToPdf(htmlBody, emailDetails, pdfPath);

//                //save eml file 
//                string emlFileName = $"Email_Content_{uid}.eml";
//                var sanitizedFileName = string.Join("_", emlFileName.Split(Path.GetInvalidFileNameChars()));
//                string emlPath = Path.Combine(folderpath, sanitizedFileName);


//                // Save the content of the email message to a .eml file
//                AttachmentUtilities.SaveEmlContent(message, emlPath);

//                // Create a Document object
//                if (mail.Assign_title_from == MailMetadataTitleOption.FromSubject)
//                {
//                    title = message.Subject;

//                }
//                if (mail.Assign_title_from == MailMetadataTitleOption.FromFilename)
//                {
//                    title = filename;
//                }
//                string strippedEmailDetails = AttachmentUtilities.StripHtmlTags(emailDetails);

//                // Get Id User 
//                var response = await http.GetAsync($"http://localhost:5183/user/getuserbymail?mail={message.To}");
//                string idfromidentity = await response.Content.ReadAsStringAsync();
//                // inti Owner
//                string ownerofdocument = "";
//                if (idfromidentity != null)
//                {
//                    ownerofdocument = idfromidentity;
//                }
//                else
//                    ownerofdocument = null;

//                if (corr_id == Guid.Empty)
//                {
//                    document = new Application.Consumers.RestAPIDocuments.Dtos.Document
//                    {
//                        Title = title, // Set title to the file name
//                        Content = strippedEmailDetails,
//                        FileData = folderpath,
//                        MimeType = "application/pdf",
//                        Archive_Serial_Number = "",
//                        Checksum = "",
//                        Tags = mail.Assign_tags,
//                        DocumentTypeId = mail.Assign_document_type,
//                        CorrespondentId = null,
//                        Owner = ownerofdocument,
//                        Source = DocumentSource.MailFetch,
//                        Mailrule = mail.Name

//                    };
//                }
//                else
//                {
//                    document = new Application.Consumers.RestAPIDocuments.Dtos.Document
//                    {
//                        Title = title, // Set title to the file name
//                        Content = strippedEmailDetails,
//                        FileData = folderpath,
//                        MimeType = "application/pdf",
//                        Archive_Serial_Number = "",
//                        Checksum = "",
//                        Tags = mail.Assign_tags,
//                        DocumentTypeId = mail.Assign_document_type,
//                        CorrespondentId = corr_id,
//                        Owner = ownerofdocument,
//                        Source = DocumentSource.MailFetch,
//                        Mailrule = mail.Name

//                    };
//                }
//                documents.Add(document);
//            }
//        }



//        public static void save_pdf_and_images_for_online_attachement_including_inline(MimeMessage message, string directorypath, MailRule mail, string title, List<Application.Consumers.RestAPIDocuments.Dtos.Document> documents, string content, string tessDataPath, Guid corr_id, HttpClient http)
//        {
//            Application.Consumers.RestAPIDocuments.Dtos.Document document = new Application.Consumers.RestAPIDocuments.Dtos.Document();
//            foreach (var attachment in message.Attachments)
//            {
//                //in case we specify filter_filename 
//                if (!string.IsNullOrEmpty(mail.Filter_attachment_filename) && AttachmentUtilities.MatchWildcard(attachment.ContentType.Name, mail.Filter_attachment_filename))
//                {
//                    Adding_documents_inline_attachment(message, directorypath, mail, title, documents, content, tessDataPath, corr_id, attachment, document, http);
//                }
//                //in case we don't specify a filter_filename
//                else if (string.IsNullOrEmpty(mail.Filter_attachment_filename))
//                {
//                    Adding_documents_inline_attachment(message, directorypath, mail, title, documents, content, tessDataPath, corr_id, attachment, document, http);
//                }


//            }
//        }
//        public static async void Adding_documents_inline_attachment(MimeMessage message, string directorypath, MailRule mail, string title, List<Application.Consumers.RestAPIDocuments.Dtos.Document> documents, string content, string tessDataPath, Guid corr_id, MimeEntity attachment, Application.Consumers.RestAPIDocuments.Dtos.Document document, HttpClient http)
//        {

//            if (attachment is MimePart mimePart && mimePart.IsAttachment == true)
//            {
//                if (mimePart.ContentType.MediaSubtype == "plain")
//                {
//                    if (mail.Assign_title_from == MailMetadataTitleOption.FromSubject)
//                    {
//                        title = message.Subject;
//                    }
//                    if (mail.Assign_title_from == MailMetadataTitleOption.FromFilename)
//                    {
//                        title = mimePart.FileName;
//                    }

//                    // Extract the file name without extension
//                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(mimePart.FileName);

//                    // Create a directory with the file name
//                    string destinationFolderPath = Path.Combine(directorypath, message.Subject);
//                    var filePath = AttachmentUtilities.file_emplacement(message, mimePart, directorypath);

//                    // Read the text content from the .txt file
//                    string textContent = System.IO.File.ReadAllText(filePath);

//                    // Convert the text content to PDF using MigraDoc
//                    string pdfFilePath = AttachmentUtilities.ConvertTextToPdfUsingMigraDoc(textContent, fileNameWithoutExtension, destinationFolderPath);

//                    string strippedDetails = AttachmentUtilities.StripHtmlTags(textContent);
//                    // Get Id User 
//                    var response = await http.GetAsync($"http://localhost:5183/user/getuserbymail?mail={message.To}");
//                    string idfromidentity = await response.Content.ReadAsStringAsync();
//                    // inti Owner
//                    string ownerofdocument = "";
//                    if (idfromidentity != null)
//                    {
//                        ownerofdocument = idfromidentity;
//                    }
//                    else
//                        ownerofdocument = null;
//                    // Create a Document object
//                    if (corr_id == Guid.Empty)
//                    {
//                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
//                        {
//                            Title = title, // Set title to the file name
//                            Content = strippedDetails, // You may set content as needed
//                            FileData = pdfFilePath, // Convert file content to Base64 string
//                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
//                            Archive_Serial_Number = "",
//                            Checksum = "",// Set other properties as needed
//                            Tags = mail.Assign_tags,
//                            DocumentTypeId = mail.Assign_document_type,
//                            CorrespondentId = null,
//                            Owner = ownerofdocument,
//                            Source = DocumentSource.MailFetch,
//                            Mailrule = mail.Name

//                        };
//                    }
//                    else
//                    {
//                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
//                        {
//                            Title = title, // Set title to the file name
//                            Content = strippedDetails, // You may set content as needed
//                            FileData = pdfFilePath, // Convert file content to Base64 string
//                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
//                            Archive_Serial_Number = "",
//                            Checksum = "",// Set other properties as needed
//                            Tags = mail.Assign_tags,
//                            DocumentTypeId = mail.Assign_document_type,
//                            CorrespondentId = corr_id,
//                            Owner = ownerofdocument,
//                            Source = DocumentSource.MailFetch,
//                            Mailrule = mail.Name

//                        };

//                    }
//                    documents.Add(document);

//                }
//                else if (mimePart.ContentType.MediaSubtype == "vnd.openxmlformats-officedocument.spreadsheetml.sheet" || mimePart.ContentType.MediaSubtype == "vnd.ms-excel")
//                {
//                    if (mail.Assign_title_from == MailMetadataTitleOption.FromSubject)
//                    {
//                        title = message.Subject;
//                    }
//                    if (mail.Assign_title_from == MailMetadataTitleOption.FromFilename)
//                    {
//                        title = mimePart.FileName;
//                    }
//                    // Extract the file name without extension
//                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(mimePart.FileName);

//                    // Create a directory with the file name
//                    string destinationFolderPath = Path.Combine(directorypath, fileNameWithoutExtension);
//                    var filePath = AttachmentUtilities.file_emplacement(message, mimePart, directorypath);


//                    // Create an instance of Excel Application
//                    Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();

//                    // Open the Excel workbook
//                    Microsoft.Office.Interop.Excel.Workbook workbook_excel = excelApp.Workbooks.Open(filePath);

//                    // Specify the PDF file path
//                    string pdfFilePath = Path.ChangeExtension(filePath, "pdf");

//                    // Save the workbook as PDF
//                    workbook_excel.ExportAsFixedFormat(Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF, pdfFilePath);

//                    // Close the workbook
//                    workbook_excel.Close(false);

//                    // Quit Excel application
//                    excelApp.Quit();




//                    StringBuilder result = new StringBuilder();
//                    // Open the saved Excel file directly for reading
//                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
//                    {
//                        // Get workbook
//                        IWorkbook workbook = new XSSFWorkbook(fileStream);

//                        // Iterate over each sheet in the workbook
//                        for (int sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
//                        {

//                            ISheet sheet = workbook.GetSheetAt(sheetIndex);
//                            result.Append($"{sheet.SheetName} \n");

//                            // Iterate over rows in the sheet
//                            for (int rowIndex = 0; rowIndex <= sheet.LastRowNum; rowIndex++)
//                            {

//                                IRow row = sheet.GetRow(rowIndex);
//                                if (row != null) // null is when the row only contains empty cells
//                                {
//                                    // Check for text boxes in this sheet (optional)
//                                    // **Text box detection (heuristic approach, limited accuracy)**


//                                    // Iterate over cells in the row
//                                    for (int cellIndex = 0; cellIndex < row.Cells.Count; cellIndex++)
//                                    {
//                                        NPOI.SS.UserModel.ICell cell = row.GetCell(cellIndex);

//                                        if (cell != null)
//                                        {
//                                            // Append cell value to the result string
//                                            result.Append(cell.ToString() + "\t"); // Assuming tab-separated values
//                                        }


//                                    }
//                                    result.AppendLine(); // Add newline after each row

//                                }
//                            }
//                            // Check for text boxes in this sheet (optional)
//                            string textBoxResults = AttachmentUtilities.DetectTextBoxesInSheet(sheet);
//                            string imageDetectionResult = AttachmentUtilities.DetectImagesInSheet(sheet);
//                            if (!string.IsNullOrEmpty(textBoxResults)) // Check if text boxes were found
//                            {
//                                result.AppendLine(textBoxResults); // Append text box detection results
//                            }
//                            if (!string.IsNullOrEmpty(imageDetectionResult)) // Check if text boxes were found
//                            {
//                                result.AppendLine(imageDetectionResult); // Append text box detection results
//                            }


//                            // Output the content of the current sheet to the console
//                            Console.WriteLine("Sheet Name: " + sheet.SheetName);
//                            Console.WriteLine("Content: \n" + result.ToString());
//                            Console.WriteLine("--------------------------------------");
//                        }

//                    }
//                    // Save the content of the current sheet to a .txt file
//                    string textFilePath = Path.Combine(directorypath + message.Subject, $"{fileNameWithoutExtension}.txt");
//                    // Save the content of the current sheet to a text file
//                    System.IO.File.WriteAllText(textFilePath, result.ToString());

//                    string strippedDetails = AttachmentUtilities.StripHtmlTags(result.ToString());
//                    var response = await http.GetAsync($"http://localhost:5183/user/getuserbymail?mail={message.To}");
//                    string idfromidentity = await response.Content.ReadAsStringAsync();
//                    // inti Owner
//                    string ownerofdocument = "";
//                    if (idfromidentity != null)
//                    {
//                        ownerofdocument = idfromidentity;
//                    }
//                    else
//                        ownerofdocument = null;
//                    // Create a Document object
//                    if (corr_id == Guid.Empty)
//                    {
//                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
//                        {
//                            Title = title, // Set title to the file name
//                            Content = strippedDetails, // You may set content as needed
//                            FileData = pdfFilePath, // Convert file content to Base64 string
//                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
//                            Archive_Serial_Number = "",
//                            Checksum = "",// Set other properties as needed
//                            Tags = mail.Assign_tags,
//                            DocumentTypeId = mail.Assign_document_type,
//                            CorrespondentId = null,
//                            Owner = ownerofdocument,
//                            Source = DocumentSource.MailFetch,
//                            Mailrule = mail.Name

//                        };
//                    }
//                    else
//                    {
//                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
//                        {
//                            Title = title, // Set title to the file name
//                            Content = strippedDetails, // You may set content as needed
//                            FileData = pdfFilePath, // Convert file content to Base64 string
//                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
//                            Archive_Serial_Number = "",
//                            Checksum = "",// Set other properties as needed
//                            Tags = mail.Assign_tags,
//                            DocumentTypeId = mail.Assign_document_type,
//                            CorrespondentId = corr_id,
//                            Owner = ownerofdocument,
//                            Source = DocumentSource.MailFetch,
//                            Mailrule = mail.Name

//                        };

//                    }
//                    documents.Add(document);
//                }
//                else if (mimePart.ContentType.MediaSubtype == "vnd.openxmlformats-officedocument.wordprocessingml.document")
//                {
//                    if (mail.Assign_title_from == MailMetadataTitleOption.FromSubject)
//                    {
//                        title = message.Subject;
//                    }
//                    if (mail.Assign_title_from == MailMetadataTitleOption.FromFilename)
//                    {
//                        title = mimePart.FileName;
//                    }
//                    var filePath = AttachmentUtilities.file_emplacement(message, mimePart, directorypath);

//                    // Create an instance of Word Application
//                    Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();

//                    // Open the Word document
//                    Microsoft.Office.Interop.Word.Document doc_document = wordApp.Documents.Open(filePath);

//                    // Save the document as PDF
//                    doc_document.SaveAs2(Path.ChangeExtension(filePath, "pdf"), Microsoft.Office.Interop.Word.WdSaveFormat.wdFormatPDF);

//                    // Close the document
//                    doc_document.Close();

//                    // Quit Word application
//                    wordApp.Quit();

//                    // Open the saved Word file directly for reading
//                    XWPFDocument wordDocument = new XWPFDocument(System.IO.File.OpenRead(filePath));

//                    // Create a StringBuilder to store the text content
//                    StringBuilder result = new StringBuilder();

//                    // Iterate over paragraphs in the document
//                    foreach (XWPFParagraph paragraph in wordDocument.Paragraphs)
//                    {
//                        // Get text from the paragraph
//                        string text = paragraph.Text;

//                        // Append text to the result StringBuilder
//                        result.AppendLine(text);
//                    }
//                    foreach (XWPFTable table in wordDocument.Tables)
//                    {
//                        string text = table.Text;
//                        result.AppendLine(text);
//                    }
//                    foreach (XWPFPictureData picture in wordDocument.AllPictures)
//                    {
//                        byte[] data = picture.Data;
//                        var tessDataPathforword = @"C:\Users\MSI\Documents\GitHub\paperless\Tessdata\tesseract-ocr-tesseract-bc059ec\tessdata";
//                        using (var engine = new TesseractEngine(tessDataPathforword, "ara+eng+fra", EngineMode.Default))
//                        {
//                            using (var ms = new MemoryStream(data))
//                            {
//                                try
//                                {
//                                    // Load the image data into a Pix object directly
//                                    using (var pix = Pix.LoadFromMemory(data))
//                                    {
//                                        if (pix == null)
//                                        {
//                                            Console.WriteLine("Error: Failed to load image data into Pix object.");
//                                            continue;
//                                        }
//                                        using (var pageOcr = engine.Process(pix))
//                                        {
//                                            var text = pageOcr.GetText();
//                                            result.AppendLine(text);
//                                            Console.WriteLine($"Text extracted from image on page :\n{text}");
//                                        }

//                                    }
//                                }
//                                catch (System.Exception ex)
//                                {
//                                    Console.WriteLine($"Error processing image on page : {ex.Message}");
//                                }
//                            }
//                        }
//                    }
//                    // Dispose of the Word document object
//                    wordDocument.Close();

//                    string strippedDetails = AttachmentUtilities.StripHtmlTags(result.ToString());
//                    // get owner from service user
//                    var response = await http.GetAsync($"http://localhost:5183/user/getuserbymail?mail={message.To}");
//                    string idfromidentity = await response.Content.ReadAsStringAsync();
//                    // inti Owner
//                    string ownerofdocument = "";
//                    if (idfromidentity != null)
//                    {
//                        ownerofdocument = idfromidentity;
//                    }
//                    else
//                        ownerofdocument = null;
//                    // Create a Document object
//                    if (corr_id == Guid.Empty)
//                    {
//                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
//                        {
//                            Title = title, // Set title to the file name
//                            Content = strippedDetails, // You may set content as needed
//                            FileData = Path.ChangeExtension(filePath, "pdf"), // Convert file content to Base64 string
//                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
//                            Archive_Serial_Number = "",
//                            Checksum = "",// Set other properties as needed
//                            Tags = mail.Assign_tags,
//                            DocumentTypeId = mail.Assign_document_type,
//                            CorrespondentId = null,
//                            Owner = ownerofdocument,
//                            Source = DocumentSource.MailFetch,
//                            Mailrule = mail.Name

//                        };
//                    }
//                    else
//                    {
//                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
//                        {
//                            Title = title, // Set title to the file name
//                            Content = strippedDetails, // You may set content as needed
//                            FileData = Path.ChangeExtension(filePath, "pdf"), // Convert file content to Base64 string
//                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
//                            Archive_Serial_Number = "",
//                            Checksum = "",// Set other properties as needed
//                            Tags = mail.Assign_tags,
//                            DocumentTypeId = mail.Assign_document_type,
//                            CorrespondentId = corr_id,
//                            Owner = ownerofdocument,
//                            Source = DocumentSource.MailFetch,
//                            Mailrule = mail.Name

//                        };

//                    }
//                    documents.Add(document);



//                }
//                else if (mimePart.ContentType.MediaSubtype == "vnd.openxmlformats-officedocument.presentationml.presentation")

//                {
//                    if (mail.Assign_title_from == MailMetadataTitleOption.FromSubject)
//                    {
//                        title = message.Subject;

//                    }
//                    if (mail.Assign_title_from == MailMetadataTitleOption.FromFilename)
//                    {
//                        title = mimePart.FileName;
//                    }
//                    var filePath = AttachmentUtilities.file_emplacement(message, mimePart, directorypath);
//                    // Create an instance of PowerPoint Application
//                    Microsoft.Office.Interop.PowerPoint.Application powerPointApp = new Microsoft.Office.Interop.PowerPoint.Application();

//                    // Open the presentation
//                    Microsoft.Office.Interop.PowerPoint.Presentation presentation = powerPointApp.Presentations.Open(filePath, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);

//                    // Save the presentation as PDF
//                    presentation.SaveAs(filePath, PpSaveAsFileType.ppSaveAsPDF);

//                    // Create a StringBuilder to store the text content
//                    StringBuilder result = new StringBuilder();

//                    // Iterate over slides in the presentation
//                    foreach (Slide slide in presentation.Slides)
//                    {
//                        // Iterate over shapes in the slide
//                        foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
//                        {
//                            // Check if the shape contains text
//                            if (shape.HasTextFrame == MsoTriState.msoTrue && shape.TextFrame.HasText == MsoTriState.msoTrue)
//                            {
//                                // Append text from the shape to the result StringBuilder
//                                result.AppendLine(shape.TextFrame.TextRange.Text);
//                            }
//                            if (shape.Type == MsoShapeType.msoPicture)
//                            {

//                                // Export the shape as an image file
//                                string imagePath = AttachmentUtilities.ExportShapeAsImage(shape);

//                                // Perform OCR on the exported image file using Tesseract
//                                if (!string.IsNullOrEmpty(imagePath))
//                                {
//                                    using (var engine = new TesseractEngine(@"C:\Users\MSI\Documents\GitHub\paperless\Tessdata\tesseract-ocr-tesseract-bc059ec\tessdata", "eng+ara+fra", EngineMode.Default))
//                                    {
//                                        using (var image = Pix.LoadFromFile(imagePath))
//                                        {
//                                            using (var page = engine.Process(image))
//                                            {
//                                                var text = page.GetText();

//                                                // Append the extracted text to the result StringBuilder
//                                                result.AppendLine(text);

//                                                // Print the extracted text
//                                                Console.WriteLine($"Text extracted from image on page:\n{text}");
//                                            }
//                                        }
//                                    }
//                                }

//                            }
//                        }
//                    }

//                    // Close the presentation
//                    presentation.Close();

//                    // Quit PowerPoint Application
//                    powerPointApp.Quit();

//                    string strippedDetails = AttachmentUtilities.StripHtmlTags(result.ToString());
//                    // get owner from service user
//                    var response = await http.GetAsync($"http://localhost:5183/user/getuserbymail?mail={message.To}");
//                    string idfromidentity = await response.Content.ReadAsStringAsync();
//                    // inti Owner
//                    string ownerofdocument = "";
//                    if (idfromidentity != null)
//                    {
//                        ownerofdocument = idfromidentity;
//                    }
//                    else
//                        ownerofdocument = null;
//                    // Create a Document object
//                    if (corr_id == Guid.Empty)
//                    {
//                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
//                        {
//                            Title = title, // Set title to the file name
//                            Content = strippedDetails, // You may set content as needed
//                            FileData = filePath, // Convert file content to Base64 string
//                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
//                            Archive_Serial_Number = "",
//                            Checksum = "",// Set other properties as needed
//                            Tags = mail.Assign_tags,
//                            DocumentTypeId = mail.Assign_document_type,
//                            CorrespondentId = null,
//                            Owner = ownerofdocument,
//                            Source = DocumentSource.MailFetch,
//                            Mailrule = mail.Name

//                        };
//                    }
//                    else
//                    {
//                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
//                        {
//                            Title = title, // Set title to the file name
//                            Content = strippedDetails, // You may set content as needed
//                            FileData = filePath, // Convert file content to Base64 string
//                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
//                            Archive_Serial_Number = "",
//                            Checksum = "",// Set other properties as needed
//                            Tags = mail.Assign_tags,
//                            DocumentTypeId = mail.Assign_document_type,
//                            CorrespondentId = corr_id,
//                            Owner = ownerofdocument,
//                            Source = DocumentSource.MailFetch,
//                            Mailrule = mail.Name

//                        };

//                    }
//                    documents.Add(document);


//                }
//                // Check if attachment is PDF 
//                else if (mimePart.ContentType.MediaSubtype == "pdf")
//                {
//                    byte[] fileContent;
//                    using (var memoryStream = new MemoryStream())
//                    {
//                        mimePart.Content.DecodeTo(memoryStream);
//                        fileContent = memoryStream.ToArray();
//                    }

//                    // Extract text from PDF
//                    string pdfText = AttachmentUtilities.ExtractTextFromPdf(new MemoryStream(fileContent));
//                    Console.WriteLine(pdfText);

//                    if (mail.Assign_title_from == MailMetadataTitleOption.FromSubject)
//                    {
//                        title = message.Subject;

//                    }
//                    if (mail.Assign_title_from == MailMetadataTitleOption.FromFilename)
//                    {
//                        title = mimePart.FileName;
//                    }

//                    var filePath = AttachmentUtilities.file_emplacement(message, mimePart, directorypath);
//                    Console.WriteLine("File stored in : " + filePath);
//                    string strippedDetails = AttachmentUtilities.StripHtmlTags(pdfText);
//                    // get owner from service user
//                    var response = await http.GetAsync($"http://localhost:5183/user/getuserbymail?mail={message.To}");
//                    string idfromidentity = await response.Content.ReadAsStringAsync();
//                    // inti Owner
//                    string ownerofdocument = "";
//                    if (idfromidentity != null)
//                    {
//                        ownerofdocument = idfromidentity;
//                    }
//                    else
//                        ownerofdocument = null;
//                    // Create a Document object
//                    if (corr_id == Guid.Empty)
//                    {
//                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
//                        {
//                            Title = title, // Set title to the file name
//                            Content = strippedDetails, // You may set content as needed
//                            FileData = filePath, // Convert file content to Base64 string
//                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
//                            Archive_Serial_Number = "",
//                            Checksum = "",// Set other properties as needed
//                            Tags = mail.Assign_tags,
//                            DocumentTypeId = mail.Assign_document_type,
//                            CorrespondentId = null,
//                            Owner = ownerofdocument,
//                            Source = DocumentSource.MailFetch,
//                            Mailrule = mail.Name

//                        };
//                    }
//                    else
//                    {
//                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
//                        {
//                            Title = title, // Set title to the file name
//                            Content = strippedDetails, // You may set content as needed
//                            FileData = filePath, // Convert file content to Base64 string
//                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
//                            Archive_Serial_Number = "",
//                            Checksum = "",// Set other properties as needed
//                            Tags = mail.Assign_tags,
//                            DocumentTypeId = mail.Assign_document_type,
//                            CorrespondentId = corr_id,
//                            Owner = ownerofdocument,
//                            Source = DocumentSource.MailFetch,
//                            Mailrule = mail.Name

//                        };

//                    }
//                    documents.Add(document);
//                }
//                //check if the attachement is an image 
//                else if (mimePart.ContentType.MediaType.StartsWith("image"))
//                {
//                    var filePath = AttachmentUtilities.file_emplacement(message, mimePart, directorypath);
//                    Console.WriteLine("Image saved: " + filePath);
//                    // Create a new PDF document to convert image
//                    PdfDocument imagetopdf = new PdfDocument();

//                    // Add a page to the document
//                    PdfPage pdf_page = imagetopdf.AddPage();

//                    // Get an XGraphics object for drawing on the page
//                    XGraphics gfx = XGraphics.FromPdfPage(pdf_page);

//                    // Load an image from file
//                    XImage image = XImage.FromFile(filePath); // Provide the path to your image file

//                    // Get the width and height of the image
//                    double width = image.PixelWidth * 0.75; // Adjust the scaling factor as needed
//                    double height = image.PixelHeight * 0.75; // Adjust the scaling factor as needed

//                    // Draw the image on the page
//                    gfx.DrawImage(image, -2, -2, width, height); // Adjust the position and size as needed

//                    // Save the document to a file
//                    string filename = Path.ChangeExtension(filePath, "pdf");
//                    imagetopdf.Save(filename);
//                    // Close the document
//                    imagetopdf.Close();
//                    // Extract text from image using Tesseract
//                    string extractedText = AttachmentUtilities.ExtractTextFromImage(filePath, tessDataPath);
//                    Console.WriteLine("Extracted text: " + extractedText);

//                    if (mail.Assign_title_from == MailMetadataTitleOption.FromSubject)
//                    {
//                        title = message.Subject;
//                    }
//                    if (mail.Assign_title_from == MailMetadataTitleOption.FromFilename)
//                    {
//                        title = mimePart.FileName;
//                    }
//                    string strippedDetails = AttachmentUtilities.StripHtmlTags(extractedText);
//                    // get owner from service user
//                    var response = await http.GetAsync($"http://localhost:5183/user/getuserbymail?mail={message.To}");
//                    string idfromidentity = await response.Content.ReadAsStringAsync();
//                    // inti Owner
//                    string ownerofdocument = "";
//                    if (idfromidentity != null)
//                    {
//                        ownerofdocument = idfromidentity;
//                    }
//                    else
//                        ownerofdocument = null;
//                    // Create a Document object
//                    if (corr_id == Guid.Empty)
//                    {
//                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
//                        {
//                            Title = title, // Set title to the file name
//                            Content = strippedDetails, // You may set content as needed
//                            FileData = filename,//Path.ChangeExtension(filePath, "pdf"), // Convert file content to Base64 string
//                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
//                            Archive_Serial_Number = "",
//                            Checksum = "",// Set other properties as needed
//                            Tags = mail.Assign_tags,
//                            DocumentTypeId = mail.Assign_document_type,
//                            CorrespondentId = null,
//                            Owner = ownerofdocument,
//                            Source = DocumentSource.MailFetch,
//                            Mailrule = mail.Name

//                        };
//                    }
//                    else
//                    {
//                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
//                        {
//                            Title = title, // Set title to the file name
//                            Content = strippedDetails, // You may set content as needed
//                            FileData = filePath, // Convert file content to Base64 string
//                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
//                            Archive_Serial_Number = "",
//                            Checksum = "",// Set other properties as needed
//                            Tags = mail.Assign_tags,
//                            DocumentTypeId = mail.Assign_document_type,
//                            CorrespondentId = corr_id,
//                            Owner = ownerofdocument,
//                            Source = DocumentSource.MailFetch,
//                            Mailrule = mail.Name

//                        };
//                    }
//                    documents.Add(document);
//                }
//            }

//        }
//    }
//}
using Application.Consumers.RestAPIDocuments.Dtos;
using Domain.MailRules;
using Domain.MailRules.Enum;
using Domain.Templates.Enum;
using MailKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
using MimeKit;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using System.Collections.Generic;
using System.Text;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;
using static MassTransit.ValidationResultExtensions;
using Tesseract;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Net.Http;
using static System.Net.WebRequestMethods;

namespace Application.Features.Helper.ProcessorHelper
{
    public static class ProcessorInlineAttachementHelper
    {
        private static readonly IConfiguration _configuration;

        public async static void save_eml_files_for_online_attachement_including_inline(MimeMessage message, string directorypath, MailRule mail, string title, List<Application.Consumers.RestAPIDocuments.Dtos.Document> documents, UniqueId uid, Guid corr_id, HttpClient http)
        {
            Application.Consumers.RestAPIDocuments.Dtos.Document document = new Application.Consumers.RestAPIDocuments.Dtos.Document();
            if (message.BodyParts.OfType<MimePart>().Any(part => part.IsAttachment == false))
            {
                // Create a BodyBuilder to construct the email body
                var bodyBuilder = new BodyBuilder();

                string filename = "";

                string emailDetails = AttachmentUtilities.get_mail_details(message);
                // Convert the BodyBuilder to HTML
                string htmlBody = message.HtmlBody;
                // save 
                string foldername = string.IsNullOrWhiteSpace(message.Subject) ? "NoSubject" : message.Subject;
                foldername = string.Join("_", foldername.Split(Path.GetInvalidFileNameChars()));
                var folderpath = Path.Combine(directorypath, foldername);
                Directory.CreateDirectory(folderpath); // Create directory if not exists
                string pdfFileName = $"Email_Details_{uid}.pdf";
                string pdfPath = Path.Combine(folderpath, pdfFileName);

                // Convert the HTML body to PDF Gotenberg Server
                AttachmentUtilities.ConvertHtmlToPdf(htmlBody, emailDetails, pdfPath);

                //save eml file 
                string emlFileName = $"Email_Content_{uid}.eml";
                var sanitizedFileName = string.Join("_", emlFileName.Split(Path.GetInvalidFileNameChars()));
                string emlPath = Path.Combine(folderpath, sanitizedFileName);


                // Save the content of the email message to a .eml file
                AttachmentUtilities.SaveEmlContent(message, emlPath);

                // Create a Document object
                if (mail.Assign_title_from == MailMetadataTitleOption.FromSubject)
                {
                    title = message.Subject;

                }
                if (mail.Assign_title_from == MailMetadataTitleOption.FromFilename)
                {
                    title = filename;
                }
                string strippedEmailDetails = AttachmentUtilities.StripHtmlTags(emailDetails);

                //// Get Id User 
                //var response =  await http.GetAsync($"http://localhost:5183/user/getuserbymail?mail={message.To}");
                //string idfromidentity = await response.Content.ReadAsStringAsync();
                //// inti Owner
                //string ownerofdocument = "";
                //if (idfromidentity != null)
                //{
                //    ownerofdocument = idfromidentity;
                //}
                //else
                //    ownerofdocument = null;

                if (corr_id == Guid.Empty)
                {
                    document = new Application.Consumers.RestAPIDocuments.Dtos.Document
                    {
                        Title = title, // Set title to the file name
                        Content = strippedEmailDetails,
                        FileData = pdfPath,
                        MimeType = ".eml",
                        Archive_Serial_Number = "",
                        Checksum = "",
                        Tags = mail.Assign_tags,
                        DocumentTypeId = mail.Assign_document_type,
                        CorrespondentId = null,
                        Owner = mail.Owner,
                        Source = DocumentSource.MailFetch,
                        Mailrule = mail.Name

                    };
                }
                else
                {
                    document = new Application.Consumers.RestAPIDocuments.Dtos.Document
                    {
                        Title = title, // Set title to the file name
                        Content = strippedEmailDetails,
                        FileData = pdfPath,
                        MimeType = ".eml",
                        Archive_Serial_Number = "",
                        Checksum = "",
                        Tags = mail.Assign_tags,
                        DocumentTypeId = mail.Assign_document_type,
                        CorrespondentId = corr_id,
                        Owner = mail.Owner,
                        Source = DocumentSource.MailFetch,
                        Mailrule = mail.Name

                    };
                }
                documents.Add(document);
            }
        }



        public static void save_pdf_and_images_for_online_attachement_including_inline(MimeMessage message, string directorypath, MailRule mail, string title, List<Application.Consumers.RestAPIDocuments.Dtos.Document> documents, string content, string tessDataPath, Guid corr_id, HttpClient http)
        {
            Application.Consumers.RestAPIDocuments.Dtos.Document document = new Application.Consumers.RestAPIDocuments.Dtos.Document();
            foreach (var attachment in message.Attachments)
            {
                //in case we specify filter_filename 
                if (!string.IsNullOrEmpty(mail.Filter_attachment_filename) && AttachmentUtilities.MatchWildcard(attachment.ContentType.Name, mail.Filter_attachment_filename))
                {
                    Adding_documents_inline_attachment(message, directorypath, mail, title, documents, content, tessDataPath, corr_id, attachment, document, http);
                }
                //in case we don't specify a filter_filename
                else if (string.IsNullOrEmpty(mail.Filter_attachment_filename))
                {
                    Adding_documents_inline_attachment(message, directorypath, mail, title, documents, content, tessDataPath, corr_id, attachment, document, http);
                }

            }
        }
        public static async void Adding_documents_inline_attachment(MimeMessage message, string directorypath, MailRule mail, string title, List<Application.Consumers.RestAPIDocuments.Dtos.Document> documents, string content, string tessDataPath, Guid corr_id, MimeEntity attachment, Application.Consumers.RestAPIDocuments.Dtos.Document document, HttpClient http)
        {
            if (attachment is MimePart mimePart && mimePart.IsAttachment == true)
            {
                if (mimePart.ContentType.MediaSubtype == "plain")
                {
                    if (mail.Assign_title_from == MailMetadataTitleOption.FromSubject)
                    {
                        title = message.Subject;
                    }
                    if (mail.Assign_title_from == MailMetadataTitleOption.FromFilename)
                    {
                        title = mimePart.FileName;
                    }

                    // Extract the file name without extension
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(mimePart.FileName);

                    // Create a directory with the file name
                    string destinationFolderPath = Path.Combine(directorypath, message.Subject);
                    var filePath = AttachmentUtilities.file_emplacement(message, mimePart, directorypath);

                    // Read the text content from the .txt file
                    string textContent = System.IO.File.ReadAllText(filePath);

                    // Convert the text content to PDF using MigraDoc
                    string pdfFilePath = AttachmentUtilities.ConvertTextToPdfUsingMigraDoc(textContent, fileNameWithoutExtension, destinationFolderPath);

                    string strippedDetails = AttachmentUtilities.StripHtmlTags(textContent);
                    // Get Id User 
                    string idfromidentity = "";
                    using (var httpClient = new HttpClient
                    {
                        Timeout = TimeSpan.FromMinutes(5) // Set timeout to 5 minutes
                    })
                    {
                        var response = await http.GetAsync($"http://localhost:5183/user/getuserbymail?mail={message.To}");
                        // Ensure the request completed successfully
                        response.EnsureSuccessStatusCode();
                        idfromidentity = await response.Content.ReadAsStringAsync();

                    }

                    //// inti Owner
                    //string ownerofdocument = "";
                    //if (idfromidentity != null)
                    //{
                    //    ownerofdocument = idfromidentity;
                    //}
                    //else
                    //    ownerofdocument = null;
                    // Create a Document object
                    if (corr_id == Guid.Empty)
                    {
                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
                        {
                            Title = title, // Set title to the file name
                            Content = strippedDetails, // You may set content as needed
                            FileData = pdfFilePath, // Convert file content to Base64 string
                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
                            Archive_Serial_Number = "",
                            Checksum = "",// Set other properties as needed
                            Tags = mail.Assign_tags,
                            DocumentTypeId = mail.Assign_document_type,
                            CorrespondentId = null,
                            Owner = mail.Owner,
                            Source = DocumentSource.MailFetch,
                            Mailrule = mail.Name

                        };
                    }
                    else
                    {
                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
                        {
                            Title = title, // Set title to the file name
                            Content = strippedDetails, // You may set content as needed
                            FileData = pdfFilePath, // Convert file content to Base64 string
                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
                            Archive_Serial_Number = "",
                            Checksum = "",// Set other properties as needed
                            Tags = mail.Assign_tags,
                            DocumentTypeId = mail.Assign_document_type,
                            CorrespondentId = corr_id,
                            Owner = mail.Owner,
                            Source = DocumentSource.MailFetch,
                            Mailrule = mail.Name

                        };

                    }
                    documents.Add(document);

                }
                else if (mimePart.ContentType.MediaSubtype == "vnd.openxmlformats-officedocument.spreadsheetml.sheet" || mimePart.ContentType.MediaSubtype == "vnd.ms-excel")
                {
                    if (mail.Assign_title_from == MailMetadataTitleOption.FromSubject)
                    {
                        title = message.Subject;
                    }
                    if (mail.Assign_title_from == MailMetadataTitleOption.FromFilename)
                    {
                        title = mimePart.FileName;
                    }
                    // Extract the file name without extension
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(mimePart.FileName);

                    // Create a directory with the file name
                    string destinationFolderPath = Path.Combine(directorypath, fileNameWithoutExtension);
                    var filePath = AttachmentUtilities.file_emplacement(message, mimePart, directorypath);


                    // Create an instance of Excel Application
                    Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();

                    // Open the Excel workbook
                    Microsoft.Office.Interop.Excel.Workbook workbook_excel = excelApp.Workbooks.Open(filePath);

                    // Specify the PDF file path
                    string pdfFilePath = Path.ChangeExtension(filePath, "pdf");

                    // Save the workbook as PDF
                    workbook_excel.ExportAsFixedFormat(Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF, pdfFilePath);

                    // Close the workbook
                    workbook_excel.Close(false);

                    // Quit Excel application
                    excelApp.Quit();




                    StringBuilder result = new StringBuilder();
                    // Open the saved Excel file directly for reading
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
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
                            string textBoxResults = AttachmentUtilities.DetectTextBoxesInSheet(sheet);
                            string imageDetectionResult = AttachmentUtilities.DetectImagesInSheet(sheet);
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

                    }
                    // Save the content of the current sheet to a .txt file
                    string textFilePath = Path.Combine(directorypath + message.Subject, $"{fileNameWithoutExtension}.txt");
                    // Save the content of the current sheet to a text file
                    System.IO.File.WriteAllText(textFilePath, result.ToString());

                    string strippedDetails = AttachmentUtilities.StripHtmlTags(result.ToString());
                    var response = await http.GetAsync($"http://localhost:5183/user/getuserbymail?mail={message.To}");
                    string idfromidentity = await response.Content.ReadAsStringAsync();
                    // inti Owner
                    //string ownerofdocument = "";
                    //if (idfromidentity != null)
                    //{
                    //    ownerofdocument = idfromidentity;
                    //}
                    //else
                    //    ownerofdocument = null;
                    // Create a Document object
                    if (corr_id == Guid.Empty)
                    {
                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
                        {
                            Title = title, // Set title to the file name
                            Content = strippedDetails, // You may set content as needed
                            FileData = pdfFilePath, // Convert file content to Base64 string
                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
                            Archive_Serial_Number = "",
                            Checksum = "",// Set other properties as needed
                            Tags = mail.Assign_tags,
                            DocumentTypeId = mail.Assign_document_type,
                            CorrespondentId = null,
                            Owner = mail.Owner,
                            Source = DocumentSource.MailFetch,
                            Mailrule = mail.Name

                        };
                    }
                    else
                    {
                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
                        {
                            Title = title, // Set title to the file name
                            Content = strippedDetails, // You may set content as needed
                            FileData = pdfFilePath, // Convert file content to Base64 string
                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
                            Archive_Serial_Number = "",
                            Checksum = "",// Set other properties as needed
                            Tags = mail.Assign_tags,
                            DocumentTypeId = mail.Assign_document_type,
                            CorrespondentId = corr_id,
                            Owner = mail.Owner,
                            Source = DocumentSource.MailFetch,
                            Mailrule = mail.Name

                        };

                    }
                    documents.Add(document);
                }
                else if (mimePart.ContentType.MediaSubtype == "vnd.openxmlformats-officedocument.wordprocessingml.document")
                {
                    if (mail.Assign_title_from == MailMetadataTitleOption.FromSubject)
                    {
                        title = message.Subject;
                    }
                    if (mail.Assign_title_from == MailMetadataTitleOption.FromFilename)
                    {
                        title = mimePart.FileName;
                    }
                    var filePath = AttachmentUtilities.file_emplacement(message, mimePart, directorypath);

                    // Create an instance of Word Application
                    Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();

                    // Open the Word document
                    Microsoft.Office.Interop.Word.Document doc_document = wordApp.Documents.Open(filePath);

                    // Save the document as PDF
                    doc_document.SaveAs2(Path.ChangeExtension(filePath, "pdf"), Microsoft.Office.Interop.Word.WdSaveFormat.wdFormatPDF);

                    // Close the document
                    doc_document.Close();

                    // Quit Word application
                    wordApp.Quit();

                    // Open the saved Word file directly for reading
                    XWPFDocument wordDocument = new XWPFDocument(System.IO.File.OpenRead(filePath));

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
                        var tessDataPathforword = @"C:\Users\MSI\Documents\GitHub\paperless\Tessdata\tesseract-ocr-tesseract-bc059ec\tessdata";
                        using (var engine = new TesseractEngine(tessDataPathforword, "ara+eng+fra", EngineMode.Default))
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
                                catch (System.Exception ex)
                                {
                                    Console.WriteLine($"Error processing image on page : {ex.Message}");
                                }
                            }
                        }
                    }
                    // Dispose of the Word document object
                    wordDocument.Close();

                    string strippedDetails = AttachmentUtilities.StripHtmlTags(result.ToString());
                    // get owner from service user
                    var response = await http.GetAsync($"http://localhost:5183/user/getuserbymail?mail={message.To}");
                    string idfromidentity = await response.Content.ReadAsStringAsync();
                    // inti Owner
                    //string ownerofdocument = "";
                    //if (idfromidentity != null)
                    //{
                    //    ownerofdocument = idfromidentity;
                    //}
                    //else
                    //    ownerofdocument = null;
                    // Create a Document object
                    if (corr_id == Guid.Empty)
                    {
                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
                        {
                            Title = title, // Set title to the file name
                            Content = strippedDetails, // You may set content as needed
                            FileData = Path.ChangeExtension(filePath, "pdf"), // Convert file content to Base64 string
                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
                            Archive_Serial_Number = "",
                            Checksum = "",// Set other properties as needed
                            Tags = mail.Assign_tags,
                            DocumentTypeId = mail.Assign_document_type,
                            CorrespondentId = null,
                            Owner = mail.Owner,
                            Source = DocumentSource.MailFetch,
                            Mailrule = mail.Name

                        };
                    }
                    else
                    {
                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
                        {
                            Title = title, // Set title to the file name
                            Content = strippedDetails, // You may set content as needed
                            FileData = Path.ChangeExtension(filePath, "pdf"), // Convert file content to Base64 string
                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
                            Archive_Serial_Number = "",
                            Checksum = "",// Set other properties as needed
                            Tags = mail.Assign_tags,
                            DocumentTypeId = mail.Assign_document_type,
                            CorrespondentId = corr_id,
                            Owner = mail.Owner,
                            Source = DocumentSource.MailFetch,
                            Mailrule = mail.Name

                        };

                    }
                    documents.Add(document);



                }
                else if (mimePart.ContentType.MediaSubtype == "vnd.openxmlformats-officedocument.presentationml.presentation")

                {
                    if (mail.Assign_title_from == MailMetadataTitleOption.FromSubject)
                    {
                        title = message.Subject;

                    }
                    if (mail.Assign_title_from == MailMetadataTitleOption.FromFilename)
                    {
                        title = mimePart.FileName;
                    }
                    var filePath = AttachmentUtilities.file_emplacement(message, mimePart, directorypath);
                    // Create an instance of PowerPoint Application
                    Microsoft.Office.Interop.PowerPoint.Application powerPointApp = new Microsoft.Office.Interop.PowerPoint.Application();

                    // Open the presentation
                    Microsoft.Office.Interop.PowerPoint.Presentation presentation = powerPointApp.Presentations.Open(filePath, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
                    string pdfFilePath = Path.ChangeExtension(filePath, "pdf");
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
                                string imagePath = AttachmentUtilities.ExportShapeAsImage(shape);

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


                    string strippedDetails = AttachmentUtilities.StripHtmlTags(result.ToString());
                    // get owner from service user
                    var response = await http.GetAsync($"http://localhost:5183/user/getuserbymail?mail={message.To}");
                    string idfromidentity = await response.Content.ReadAsStringAsync();
                    // inti Owner
                    //string ownerofdocument = "";
                    //if (idfromidentity != null)
                    //{
                    //    ownerofdocument = idfromidentity;
                    //}
                    //else
                    //    ownerofdocument = null;
                    // Create a Document object
                    if (corr_id == Guid.Empty)
                    {
                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
                        {
                            Title = title, // Set title to the file name
                            Content = strippedDetails, // You may set content as needed
                            FileData = pdfFilePath, // Convert file content to Base64 string
                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
                            Archive_Serial_Number = "",
                            Checksum = "",// Set other properties as needed
                            Tags = mail.Assign_tags,
                            DocumentTypeId = mail.Assign_document_type,
                            CorrespondentId = null,
                            Owner = mail.Owner,
                            Source = DocumentSource.MailFetch,
                            Mailrule = mail.Name

                        };
                    }
                    else
                    {
                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
                        {
                            Title = title, // Set title to the file name
                            Content = strippedDetails, // You may set content as needed
                            FileData = pdfFilePath, // Convert file content to Base64 string
                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
                            Archive_Serial_Number = "",
                            Checksum = "",// Set other properties as needed
                            Tags = mail.Assign_tags,
                            DocumentTypeId = mail.Assign_document_type,
                            CorrespondentId = corr_id,
                            Owner = mail.Owner,
                            Source = DocumentSource.MailFetch,
                            Mailrule = mail.Name

                        };

                    }
                    documents.Add(document);


                }
                // Check if attachment is PDF 
                else if (mimePart.ContentType.MediaSubtype == "pdf")
                {
                    byte[] fileContent;
                    using (var memoryStream = new MemoryStream())
                    {
                        mimePart.Content.DecodeTo(memoryStream);
                        fileContent = memoryStream.ToArray();
                    }

                    // Extract text from PDF
                    string pdfText = AttachmentUtilities.ExtractTextFromPdf(new MemoryStream(fileContent));
                    Console.WriteLine(pdfText);

                    if (mail.Assign_title_from == MailMetadataTitleOption.FromSubject)
                    {
                        title = message.Subject;

                    }
                    if (mail.Assign_title_from == MailMetadataTitleOption.FromFilename)
                    {
                        title = mimePart.FileName;
                    }

                    var filePath = AttachmentUtilities.file_emplacement(message, mimePart, directorypath);
                    Console.WriteLine("File stored in : " + filePath);
                    string strippedDetails = AttachmentUtilities.StripHtmlTags(pdfText);
                    // get owner from service user
                    var response = await http.GetAsync($"http://localhost:5183/user/getuserbymail?mail={message.To}");
                    string idfromidentity = await response.Content.ReadAsStringAsync();
                    // inti Owner
                    //string ownerofdocument = "";
                    //if (idfromidentity != null)
                    //{
                    //    ownerofdocument = idfromidentity;
                    //}
                    //else
                    //    ownerofdocument = null;
                    // Create a Document object
                    if (corr_id == Guid.Empty)
                    {
                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
                        {
                            Title = title, // Set title to the file name
                            Content = strippedDetails, // You may set content as needed
                            FileData = filePath, // Convert file content to Base64 string
                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
                            Archive_Serial_Number = "",
                            Checksum = "",// Set other properties as needed
                            Tags = mail.Assign_tags,
                            DocumentTypeId = mail.Assign_document_type,
                            CorrespondentId = null,
                            Owner = mail.Owner,
                            Source = DocumentSource.MailFetch,
                            Mailrule = mail.Name

                        };
                    }
                    else
                    {
                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
                        {
                            Title = title, // Set title to the file name
                            Content = strippedDetails, // You may set content as needed
                            FileData = filePath, // Convert file content to Base64 string
                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
                            Archive_Serial_Number = "",
                            Checksum = "",// Set other properties as needed
                            Tags = mail.Assign_tags,
                            DocumentTypeId = mail.Assign_document_type,
                            CorrespondentId = corr_id,
                            Owner = mail.Owner,
                            Source = DocumentSource.MailFetch,
                            Mailrule = mail.Name

                        };

                    }
                    documents.Add(document);
                }
                //check if the attachement is an image 
                else if (mimePart.ContentType.MediaType.StartsWith("image"))
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(mimePart.FileName);
                    var filePath = AttachmentUtilities.file_emplacement(message, mimePart, directorypath);
                    Console.WriteLine("Image saved: " + filePath);
                    // Create a new PDF document to convert image
                    PdfDocument imagetopdf = new PdfDocument();

                    // Add a page to the document
                    PdfPage pdf_page = imagetopdf.AddPage();

                    // Get an XGraphics object for drawing on the page
                    XGraphics gfx = XGraphics.FromPdfPage(pdf_page);

                    // Load an image from file
                    XImage image = XImage.FromFile(filePath); // Provide the path to your image file

                    // Get the width and height of the image
                    double width = image.PixelWidth * 0.75; // Adjust the scaling factor as needed
                    double height = image.PixelHeight * 0.75; // Adjust the scaling factor as needed

                    // Draw the image on the page
                    gfx.DrawImage(image, -2, -2, width, height); // Adjust the position and size as needed

                    // Save the document to a file
                    string filename = Path.ChangeExtension(filePath, "pdf");
                    imagetopdf.Save(filename);
                    // Close the document
                    imagetopdf.Close();
                    // Extract text from image using Tesseract
                    string extractedText = AttachmentUtilities.ExtractTextFromImage(filePath, tessDataPath);
                    Console.WriteLine("Extracted text: " + extractedText);

                    if (mail.Assign_title_from == MailMetadataTitleOption.FromSubject)
                    {
                        title = message.Subject;
                    }
                    if (mail.Assign_title_from == MailMetadataTitleOption.FromFilename)
                    {
                        title = mimePart.FileName;
                    }
                    string strippedDetails = AttachmentUtilities.StripHtmlTags(extractedText);
                    // get owner from service user
                    //string idfromidentity = "";
                    //using (var httpClient = new HttpClient
                    //{
                    //    Timeout = TimeSpan.FromMinutes(5) // Set timeout to 5 minutes
                    //})
                    //{
                    //    var response = await http.GetAsync($"http://localhost:5183/user/getuserbymail?mail={message.To}");
                    //    // Ensure the request completed successfully
                    //    response.EnsureSuccessStatusCode();
                    //    idfromidentity = await response.Content.ReadAsStringAsync();

                    //}
                    //// inti Owner
                    //string ownerofdocument = "";
                    //if (idfromidentity != null)
                    //{
                    //    ownerofdocument = idfromidentity;
                    //}
                    //else
                    //    ownerofdocument = null;
                    // Create a Document object
                    if (corr_id == Guid.Empty)
                    {
                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
                        {
                            Title = fileNameWithoutExtension, // Set title to the file name
                            Content = strippedDetails, // You may set content as needed
                            FileData = filename,//Path.ChangeExtension(filePath, "pdf"), // Convert file content to Base64 string
                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
                            Archive_Serial_Number = "",
                            Checksum = "",// Set other properties as needed
                            Tags = mail.Assign_tags,
                            DocumentTypeId = mail.Assign_document_type,
                            CorrespondentId = null,
                            Owner = mail.Owner,
                            Source = DocumentSource.MailFetch,
                            Mailrule = mail.Name

                        };
                    }
                    else
                    {
                        document = new Application.Consumers.RestAPIDocuments.Dtos.Document
                        {
                            Title = fileNameWithoutExtension, // Set title to the file name
                            Content = strippedDetails, // You may set content as needed
                            FileData = filename, // Convert file content to Base64 string
                            MimeType = "application/" + mimePart.ContentType.MediaSubtype, // Set MIME type
                            Archive_Serial_Number = "",
                            Checksum = "",// Set other properties as needed
                            Tags = mail.Assign_tags,
                            DocumentTypeId = mail.Assign_document_type,
                            CorrespondentId = corr_id,
                            Owner = mail.Owner,
                            Source = DocumentSource.MailFetch,
                            Mailrule = mail.Name

                        };
                    }
                    documents.Add(document);
                }
            }



        }
    }
}
