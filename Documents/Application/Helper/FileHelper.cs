using Application.Dtos.Correspondent;
using Application.Dtos.DocumentType;
using Application.Dtos.StoragePath;
using Application.Features.AssignDocumentMangement;
using Application.Respository;
using Application.RestApiMail.Dto;
using Application.RestApiMail.EndPoints;
using Aspose.Pdf.Operators;
using Domain.DocumentManagement;
using Domain.DocumentManagement.tags;
using Domain.Documents;
using Domain.Logs;
using Domain.Templates;
using Infrastructure.Service.Helper;
using Newtonsoft.Json.Linq;
using NTextCat;
using NTextCat.Commons;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Serilog;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig.Logging;
namespace Application.Helper
{
    public static class FileHelper
    {
        ////check for *.pdf and *invoice*
        //public static bool MatchWildcard(string text, string pattern)
        //{
        //    if (pattern.StartsWith("*") && pattern.EndsWith("*"))
        //    {
        //        string substring = pattern.Trim('*');
        //        return text.Contains(substring);
        //    }
        //    else if (pattern.StartsWith("*"))
        //    {
        //        string suffix = pattern.TrimStart('*');
        //        return text.EndsWith(suffix);
        //    }
        //    else
        //    {
        //        return text == pattern;
        //    }
        //}
        public static bool MatchWildcard(string title, string mimeType, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return false;
            }
            if (pattern.Contains("*"))
            {
                string[] parts = pattern.Split('*');
                bool containsTitle = parts.Any(part => title.Contains(part));
                bool containsMimeType = parts.Any(part => mimeType.Contains(part));
                return containsTitle || containsMimeType;
            }

            else
            {
                return title.Contains(pattern) || mimeType.Contains(pattern);
            }
        }
        // filter by Consumption Started
        //public static bool Consumption_started_filter(Document document, Template template, string classification_result)

        //{
        //    //getting the document to add emplacement
        //    string document_emplacement = Path.GetDirectoryName(document.FileData);
        //    if (MatchWildcard(document.Title, template.FilterFilename) && document_emplacement == template.FilterPath && template.Sources.Contains(document.Source) && document.Mailrule == template.FilterMailrule && classification_result.Contains(template.DocumentClassification))
        //    {
        //        return true;
        //    }
        //    return false;
        //}


        public static bool ConsumptionStartedFilter(Document document, Template template, string? classification_result)
        {
            // If template path is not empty and doesn't match document emplacement, return false
            if (!string.IsNullOrEmpty(template.FilterPath) && document.FileData != template.FilterPath)
                return false;

            // If template filename is not empty and doesn't match document title, return false
            //if (!string.IsNullOrEmpty(template.FilterFilename) && !MatchWildcard(document.Title+"."+document.MimeType, template.FilterFilename))
            //    return false;
            if (!MatchWildcard(document.Title, document.MimeType, template.FilterFilename))
                return false;

            // If template mailrule is not empty and doesn't match document mailrule, return false
            if (!string.IsNullOrEmpty(template.FilterMailrule) && document.Mailrule != template.FilterMailrule)
                return false;

            // If template sources are not empty and document source is not in template sources, return false
            if (template.Sources.Any() && !template.Sources.Contains(document.Source))
                return false;
            if (classification_result != null)
            {
                // If template DocumentClassification is not empty and classification_result doesn't contain it, return false
                if (!string.IsNullOrEmpty(template.DocumentClassification) && !classification_result.ToLower().Contains(template.DocumentClassification.ToLower()))
                    return false;
            }


            // If all conditions pass, return true
            return true;
        }






        //filter by Document Added
        //public static bool Document_Added_filter(Document document, Template template)
        //{
        //    //getting the document to add emplacement
        //    string document_emplacement = Path.GetDirectoryName(document.FileData);

        //    if (document.Title.Contains(template.FilterFilename) && document.Tags.All(dt => template.Has_Tags.Contains(dt.TagId)) && document.CorrespondentId == template.Has_Correspondent && document.DocumentTypeId == template.Has_Document_Type)
        //    {
        //        if (template.Content_matching_algorithm != null && Enum.IsDefined(typeof(Matching_Algorithms), template.Content_matching_algorithm))
        //        {
        //            // Content matching pattern 
        //            if (MatchingAlgorithmsWorkflow.ExistingDocumentMatchesWorkflow(document, template))
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }
        //    }
        //    return false;
        //}
        //public static bool Document_Added_filter(Document document, Template template)
        //{
        //    // Getting the document to add emplacement
        //    string document_emplacement = Path.GetDirectoryName(document.FileData);

        //    // Ensure document.Title and template.FilterFilename are not null
        //    if (  document.Title.Contains(template.FilterFilename) 
        //         && document.Tags.All(dt => template.Has_Tags.Contains(dt.TagId)) &&
        //        document.CorrespondentId == template.Has_Correspondent && document.DocumentTypeId == template.Has_Document_Type)
        //    {
        //        if (template.Content_matching_algorithm != null && Enum.IsDefined(typeof(Matching_Algorithms), template.Content_matching_algorithm))
        //        {
        //            // Content matching pattern
        //            if (MatchingAlgorithmsWorkflow.ExistingDocumentMatchesWorkflow(document, template))
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }
        //    }
        //    return false;
        //}
        public static bool DocumentAddedFilter(Document document, Template template)
        {
            // Getting the document to add emplacement
            string document_emplacement = Path.GetDirectoryName(document.FileData);

            // Ensure document.Title and template.FilterFilename are not null
            if ((string.IsNullOrEmpty(template.FilterFilename) || document.Title.Contains(template.FilterFilename))
                && (template.Has_Tags == null || document.Tags != null && document.Tags.All(dt => template.Has_Tags.Contains(dt.TagId)))
                && (template.Has_Correspondent == null || document.CorrespondentId == template.Has_Correspondent)
                && (template.Has_Document_Type == null || document.DocumentTypeId == template.Has_Document_Type))
            {
                if (template.Content_matching_algorithm != null && Enum.IsDefined(typeof(Matching_Algorithms), template.Content_matching_algorithm))
                {
                    // Content matching pattern
                    if (MatchingAlgorithmsWorkflow.ExistingDocumentMatchesWorkflow(document, template))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        // assign tags, documentType, Correspondent , storage path
        public static async Task AssignDocumentProperties(Document documenttoadd, string idowner, AssignTagToDocument assignTag, AssignCorrespondentToDocument assignCorrespondentToDocument, AssignDocumentTypeToDocument assignDocumentTypeToDocument, AssignStoragePathToDocument assignStoragePathToDocument)
        {
            //, string classification_result, JObject uniqueSubdirJson
            // Check and assign tags 
            List<DocumentTags> documentTags = await assignTag.AssignTag(documenttoadd, idowner);
            documenttoadd.Tags = documentTags != null ? documentTags : null;

            // Check and assign Correspondent
            CorrespondentListDTO correspondent = await assignCorrespondentToDocument.AssignCorrespondent(documenttoadd, idowner);
            documenttoadd.CorrespondentId = correspondent?.Id;

            // Check and assign Document Type
            //DocumentTypeDetailsDTO documentType = await assignDocumentTypeToDocument.AssignDocumenttype(documenttoadd, idowner, classification_result, uniqueSubdirJson);
            //documenttoadd.DocumentTypeId = documentType?.Id;

            // Check and assign storage path
            StoragePathDto storagePath = await assignStoragePathToDocument.AssignStoragePath(documenttoadd, idowner);
            documenttoadd.StoragePathId = storagePath?.Id;
        }
        public static string DetectLanguage(string content)
        {
            Regex regex = new Regex("[\u0600-\u06ff]|[\u0750-\u077f]|[\ufb50-\ufc3f]|[\ufe70-\ufefc]");
            if (regex.IsMatch(content))
            {
                return "ara";
            }
            else
            {
                var factory = new RankedLanguageIdentifierFactory();
                //set the dictionary path
                var identifier = factory.Load(@"C:\Users\MSI\Documents\GitHub\paperless\Core14.profile.xml");
                //get the language
                var languages = identifier.Identify(content);
                var mostCertainLanguage = languages.FirstOrDefault();
                if (mostCertainLanguage != null)
                {
                    //get the language in two-digit form e.g. en, de, fr...
                    string language = mostCertainLanguage.Item1.Iso639_3;
                    Console.WriteLine("Most certain language recognized is" + language, false);
                    return language;
                }
                else
                {

                    return string.Empty;

                }
            }
        }

        public static DateTime ExtractCreated(string output)
        {
            // Define a regular expression pattern to match various date formats
            string pattern = @"\b(\d{4}-\d{2}-\d{2}|\d{2}/\d{2}/\d{4}|(0?[1-9]|[12]\d|3[01])(\/|-|\.|\s)(0?[1-9]|1[0-2])\3\d{4})\b";

            // Find the first match using Regex.Match
            Match match = Regex.Match(output, pattern);

            // If a match is found, parse it to DateTime and return
            if (match.Success)
            {
                DateTime parsedDate;
                if (DateTime.TryParse(match.Value, out parsedDate))
                {
                    return parsedDate;
                }
            }

            // If no match is found or parsing fails, return DateTime.MinValue
            return DateTime.MinValue;
        }
        // calculate MD5
        public static string CalculateMD5(string filename)
        {
            MD5 md5 = MD5.Create();
            Stream stream = File.OpenRead(filename);

            var hash = md5.ComputeHash(stream);


            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }


        public static byte[] EncryptDocument(string filePath, byte[] key, byte[] iv)
        {
            try
            {

                byte[] fileBytes = File.ReadAllBytes(filePath);
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.KeySize = 256;
                    aesAlg.BlockSize = 128;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Key = key;
                    aesAlg.IV = iv;
                    // Encrypt the document
                    using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                    using (var msEncrypt = new MemoryStream())
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(fileBytes, 0, fileBytes.Length);
                        csEncrypt.FlushFinalBlock();
                        return msEncrypt.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

        }
        private static readonly byte[] StaticKey = new byte[32]
        {
                0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF,
                0x10, 0x32, 0x54, 0x76, 0x98, 0xBA, 0xDC, 0xFE,
                0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88,
                0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x00
        };

        // Encrypt using a static key
        public static byte[] EncryptKey(byte[] key)
        {
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.KeySize = 256;
                    aesAlg.BlockSize = 128;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.Mode = CipherMode.CBC;

                    aesAlg.GenerateIV(); // Generate a new IV for each encryption
                    byte[] iv = aesAlg.IV;

                    using (var encryptor = aesAlg.CreateEncryptor(StaticKey, iv))
                    using (var msEncrypt = new MemoryStream())
                    {
                        msEncrypt.Write(iv, 0, iv.Length); // Store IV at the beginning of the encrypted data
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(key, 0, key.Length);
                            csEncrypt.FlushFinalBlock();
                        }
                        return msEncrypt.ToArray(); // Return the IV + encrypted key
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public static byte[] DecryptKey(byte[] encryptedKey)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.KeySize = 256;
                aesAlg.BlockSize = 128;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Mode = CipherMode.CBC;


                if (encryptedKey.Length < 16)
                {
                    throw new ArgumentException("Invalid encrypted key length.");
                }


                byte[] iv = new byte[16];
                Array.Copy(encryptedKey, 0, iv, 0, iv.Length);
                aesAlg.IV = iv;
                using (var msDecrypt = new MemoryStream(encryptedKey, iv.Length, encryptedKey.Length - iv.Length)) // Skip the IV part
                {
                    using (var decryptor = aesAlg.CreateDecryptor(StaticKey, aesAlg.IV))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var msPlainText = new MemoryStream())
                    {
                        csDecrypt.CopyTo(msPlainText);
                        return msPlainText.ToArray(); // Return the decrypted key
                    }
                }
            }
        }

        public static byte[] GenerateEncryptionKey(int size)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] key = new byte[size];
                rng.GetBytes(key);  // Generate a random key of the specified size
                return key;
            }
        }

        public static byte[] GenerateEncryptionIV(int size)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] iv = new byte[size];
                rng.GetBytes(iv);  // Generate a random IV of the specified size
                return iv;
            }
        }
        public static byte[] DecryptDocument(byte[] encryptedFileBytes, byte[] key, byte[] iv)
        {
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.KeySize = 256;
                    aesAlg.BlockSize = 128;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.Mode = CipherMode.CBC;

                    aesAlg.Key = key; // 32 bytes = 256 bits
                    aesAlg.IV = iv;   // 16 bytes = 128 bits


                    using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                    using (var msDecrypt = new MemoryStream(encryptedFileBytes))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var msResult = new MemoryStream())
                    {
                        csDecrypt.CopyTo(msResult);
                        return msResult.ToArray();
                    }
                }
            }
            catch (CryptographicException ex)
            {
                throw new Exception("Cryptographic error occurred during decryption: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error decrypting the document: " + ex.Message, ex);
            }
        }

        //public static string CreateThumbnailOfADocument(Document document)
        //{
        //    // Load the PDF document using Aspose.PDF
        //    Spire.Pdf.PdfDocument pdfDocument = new Spire.Pdf.PdfDocument(document.FileData);

        //    // Specify the desired thumbnail dimensions
        //    int thumbnailWidth = 800;
        //    int thumbnailHeight = 500;
        //    // Get the dimensions of the first page of the PDF
        //    Spire.Pdf.PdfPageBase page = pdfDocument.Pages[0];
        //    float pdfPageWidth = page.Size.Width;
        //    float pdfPageHeight = page.Size.Height;

        //    // Calculate the scaling factor to maintain the aspect ratio
        //    float widthScale = (float)thumbnailWidth / pdfPageWidth;
        //    float heightScale = (float)thumbnailHeight / pdfPageHeight;
        //    float scaleFactor = Math.Min(widthScale, heightScale);

        //    // Adjust the thumbnail dimensions based on the scaling factor
        //    int scaledWidth = (int)(pdfPageWidth * scaleFactor);
        //    int scaledHeight = (int)(pdfPageHeight * scaleFactor);

        //    // Generate the thumbnail image with adjusted dimensions
        //    Bitmap thumbnailImage = (Bitmap)pdfDocument.SaveAsImage(0, scaledWidth, scaledHeight);


        //    // Specify the output folder
        //    string pathToSave = Path.GetDirectoryName(document.FileData);

        //    string imagePath = Path.Combine(pathToSave, "thumbnail.jpg");

        //    thumbnailImage.Save(imagePath, ImageFormat.Jpeg);
        //    return imagePath;
        //}

        //public static async Task<string> CreateThumbnailOfADocumentAsync(Document document, ILogRepository logRepository)
        //{
        //    Logs log = Logs.Create(LogLevel.DEBUG, LogName.DigitalWork, "Generating thumbnail for " + document.Title);
        //    await logRepository.AddAsync(log);

        //    // Load the PDF document using Aspose.PDF
        //    Spire.Pdf.PdfDocument pdfDocument = new Spire.Pdf.PdfDocument(document.FileData);
        //    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(document.Title);
        //    string uniqueFileName = $"{fileNameWithoutExtension}_{DateTime.Now:yyyyMMdd}";

        //    // Specify the desired thumbnail dimensions
        //    int thumbnailWidth = 800;
        //    int thumbnailHeight = 500;


        //    Bitmap thumbnailImage = (Bitmap)pdfDocument.SaveAsImage(0, thumbnailWidth, thumbnailHeight);

        //    // Specify the output folder
        //    string pathToSave = Path.GetDirectoryName(document.FileData);

        //    string imagePath = Path.Combine(pathToSave, uniqueFileName + ".jpg");

        //    thumbnailImage.Save(imagePath, ImageFormat.Jpeg);
        //    return imagePath;
        //}
        public static async Task<string> CreateThumbnailOfADocumentAsync(Document document, ILogRepository logRepository)
        {
            if (document == null || document.FileData == null || string.IsNullOrEmpty(document.Title))
            {
                throw new ArgumentException("Document or required fields cannot be null or empty.");
            }

            try
            {
                Logs log = Logs.Create(LogLevel.DEBUG, LogName.EasyDoc, "Generating thumbnail for " + document.Title);
                await logRepository.AddAsync(log);

                // Load the PDF document
                using (Spire.Pdf.PdfDocument pdfDocument = new Spire.Pdf.PdfDocument(document.FileData))
                {
                    Bitmap thumbnailImage = (Bitmap)pdfDocument.SaveAsImage(0);  // First page as image
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(document.Title);
                    string uniqueFileName = $"{fileNameWithoutExtension}_{DateTime.Now:yyyyMMdd}.jpg";

                    string pathToSave = Path.GetDirectoryName(document.FileData) ?? throw new InvalidOperationException("Cannot determine directory.");
                    string imagePath = Path.Combine(pathToSave, uniqueFileName);

                    thumbnailImage.Save(imagePath, ImageFormat.Jpeg);
                    return imagePath;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error generating thumbnail for {document.Title}: {ex.Message}");
                throw;
            }
        }

        // using for Custom Fields Machine learning
        public static bool IsString(string value)
        {
            // Assuming anything can be a string if it doesn't match other patterns.
            return true;
        }

        public static bool IsUrl(string value)
        {
            string pattern = @"^(http|https|ftp)://[^\s/$.?#].[^\s]*$";
            return Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase);
        }

        public static bool IsDate(string value)
        {
            string pattern = @"^\d{4}-\d{2}-\d{2}$"; // Simple YYYY-MM-DD format
            DateTime tempDate;
            return Regex.IsMatch(value, pattern) && DateTime.TryParse(value, out tempDate);
        }

        public static bool IsBoolean(string value)
        {
            return value.Equals("true", StringComparison.OrdinalIgnoreCase) || value.Equals("false", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsInteger(string value)
        {
            string pattern = @"^-?\d+$";
            return Regex.IsMatch(value, pattern);
        }

        public static bool IsFloat(string value)
        {
            string pattern = @"^-?\d+(\.\d+)?$";
            return Regex.IsMatch(value, pattern);
        }

        public static bool IsMonetary(string value)
        {
            string pattern = @"^\$?\d+(\.\d{2})?$";
            return Regex.IsMatch(value, pattern);
        }
    }
}
