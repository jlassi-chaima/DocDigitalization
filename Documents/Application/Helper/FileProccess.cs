
using Application.Services;
using Microsoft.AspNetCore.Http;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using PdfDocument = PdfSharp.Pdf.PdfDocument;
using PdfPigDocument = UglyToad.PdfPig.PdfDocument;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using System.Reflection;
using Serilog;
using PdfSharpCore.Pdf;
using PdfPage = PdfSharp.Pdf.PdfPage;
using System.Drawing;
using Aspose.Pdf.Operators;
using PdfSharp.UniversalAccessibility.Drawing;
using Aspose.Pdf.Text;
using System.Text;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.Content;
using Tesseract;
using Page = UglyToad.PdfPig.Content.Page;
using Elasticsearch.Net;
using System;
using OpenCvSharp;
using Size = OpenCvSharp.Size;
using OpenCvSharp.Extensions;
using System.IO.Compression;
using Microsoft.AspNetCore.StaticFiles;
namespace Application.Helper
{
    public static class FileProccess
    {
        public static string Mimetype(string filename)
        {
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            if (contentTypeProvider.TryGetContentType(filename, out string mimeType))
            {
                Console.WriteLine("MIME Type: " + mimeType);
            }
            else
            {
                Console.WriteLine("MIME Type not found.");
            }
            return mimeType;
        }
        //public static MemoryStream ExtractPageAsStream(int startPage, int endPage, byte[] pdfBytes)
        //{
        //    try
        //    {
        //        // MemoryStream to read and write data in memory.
        //        var outputMemoryStream = new MemoryStream();
        //        var outputDocument = new PdfDocument();
        //        CustomFontResolver.Apply();

        //        // Load the PDF into PdfPig for text extraction
        //        using (UglyToad.PdfPig.PdfDocument pdfPigDoc = UglyToad.PdfPig.PdfDocument.Open(pdfBytes))
        //        {
        //            for (int pageIndex = startPage; pageIndex < endPage; pageIndex++)
        //            {
        //                // Extract the current page using PdfPig
        //                var page = pdfPigDoc.GetPage(pageIndex + 1); // PdfPig uses 1-based indexing

        //                // Extract text from the page
        //                string pageText = page.Text;
        //                Log.Information("Original Text: " + pageText);

        //                // Remove ASN using regex
        //                pageText = Regex.Replace(pageText, @"ASN\d+", "");
        //                Log.Information("Modified Text: " + pageText);

        //                // Create a new page in the output PdfSharp document
        //                var newPage = outputDocument.AddPage();

        //                using (XGraphics gfx = XGraphics.FromPdfPage(newPage))
        //                {
        //                    // Draw the modified text on the new page
        //                    XFont font = new XFont("Arial", 12);
        //                    gfx.DrawString(pageText, font, XBrushes.Black, new XRect(0, 0, newPage.Width, newPage.Height), XStringFormats.TopLeft);

        //                    // Draw images from the original page
        //                    foreach (var image in page.GetImages())
        //                    {
        //                        byte[] bytes;

        //                        // Attempt to get the PNG bytes; otherwise, fall back to raw bytes
        //                        if (image.TryGetPng(out byte[] pngBytes))
        //                        {
        //                            bytes = pngBytes;
        //                        }
        //                        else
        //                        {
        //                            bytes = image.RawBytes.ToArray(); // Assuming RawBytes is IEnumerable<byte>
        //                        }

        //                        // Create the XImage directly from the byte array
        //                        using (MemoryStream stream = new MemoryStream(bytes,0,bytes.Length,true,true))
        //                        {
        //                            if (stream.Length > 0)
        //                            {
        //                                stream.Position = 0;

        //                                // Create the XImage from the stream
        //                                XImage xImage = XImage.FromStream(stream);

        //                                // Now draw the image on the graphics context
        //                                gfx.DrawImage(xImage, new XRect(0, 0, xImage.PixelWidth, xImage.PixelHeight));
        //                            }
        //                        }
        //                    }
        //                }

        //            }
        //        }

        //        // Save the modified document to the output memory stream
        //        outputDocument.Save(outputMemoryStream);

        //        // Reset the output stream position to the beginning before returning it
        //        outputMemoryStream.Position = 0;
        //        return outputMemoryStream;
        //    }
        //    catch(Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }

        //}

        //if (!image.IsInlineImage)
        //{
        //    if (image.TryGetPng(out byte[] pngBytes))
        //    {
        //        using (var imgStream = new MemoryStream(pngBytes))
        //        {
        //            XImage xImage = XImage.FromStream(new MemoryStream(pngBytes));
        //            gfx.DrawImage(xImage, new XRect(0, 0, xImage.PixelWidth, xImage.PixelHeight));
        //        }
        //    }
        //    // Optionally handle other image types here
        //}



        public static MemoryStream ExtractPageAsStream(PdfDocument pdfDocument, int startPage, int endPage)
        {

            var outputMemoryStream = new MemoryStream();
            var outputDocument = new PdfDocument();
            // Add the specified pages to the new document
            for (int page = startPage; page < endPage; page++)
            {

                var originalPage = pdfDocument.Pages[page];
                //var pageText = originalPage.Text;

                outputDocument.AddPage(pdfDocument.Pages[page]);
            }

            // Save the new document to the output memory stream
            outputDocument.Save(outputMemoryStream);


            // Reset the output stream position to the beginning before returning it
            outputMemoryStream.Position = 0;
            return outputMemoryStream;


        }
        //public static void RemoveSeparatorPages(string inputFilePath, string outputFilePath, string asnPattern)
        //{
        //    // Load the PDF document
        //    PdfDocument document = PdfReader.Open(inputFilePath, PdfDocumentOpenMode.Modify);

        //    // Regular expression to identify ASN pattern (adjust regex as needed for your ASN format)
        //    Regex asnRegex = new Regex(asnPattern, RegexOptions.IgnoreCase);

        //    // List to keep track of pages to remove
        //    List<int> pagesToRemove = new List<int>();

        //    // Iterate through each page
        //    for (int i = 0; i < document.PageCount; i++)
        //    {
        //        PdfPage page = document.Pages[i];

        //        // Extract text from the page (assuming you have a method to extract text)
        //        string pageText = ExtractTextFromPage(page); // You need to implement this method based on the library

        //        // Check if the page contains ASN (i.e., is a separator page)
        //        if (asnRegex.IsMatch(pageText))
        //        {
        //            // If it contains ASN, mark this page for removal
        //            pagesToRemove.Add(i);
        //        }
        //    }

        //    // Remove the identified separator pages
        //    for (int i = pagesToRemove.Count - 1; i >= 0; i--)
        //    {
        //        document.Pages.RemoveAt(pagesToRemove[i]);
        //    }

        //    // Save the modified document
        //    document.Save(outputFilePath);
        //}
        public static byte[] Base64ToByteArray(string base64String)
        {
            return Convert.FromBase64String(base64String);
        }
        public static MemoryStream ByteArrayToMemoryStream(byte[] byteArray)
        {
            return new MemoryStream(byteArray);
        }

        public static PdfDocument OpenPdfDocumentAsync(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                file.CopyToAsync(memoryStream).Wait();
                memoryStream.Position = 0;
                byte[] fileContent = memoryStream.ToArray();

                return PdfReader.Open(new MemoryStream(fileContent), PdfDocumentOpenMode.Import);
            }

        }
        public static byte[] ExtractByteFromFile(IFormFile file)
        {
            byte[] pdfBytes;
            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                pdfBytes = memoryStream.ToArray();
            }
                return pdfBytes;
        }
        public static PdfDocument ExtractPages(PdfDocument sourceDocument, int startPage, int endPage)
        {
            try { 
            PdfDocument subDocument = new PdfDocument();
            ////var subDocument = new PdfDocument();
            //CustomFontResolver.Apply();

            //// Load the PDF into PdfPig for text extraction
            //using (UglyToad.PdfPig.PdfDocument pdfPigDoc = UglyToad.PdfPig.PdfDocument.Open(pdfBytes))
            //{
            //    for (int pageIndex = startPage; pageIndex < endPage; pageIndex++)
            //    {
            //        // Extract the current page using PdfPig
            //        var page = pdfPigDoc.GetPage(pageIndex + 1); // PdfPig uses 1-based indexing

            //        // Extract text from the page
            //        string pageText = page.Text;
            //        Log.Information("Original Text: " + pageText);

            //        // Remove ASN using regex
            //        pageText = Regex.Replace(pageText, @"ASN\d+", "");
            //        Log.Information("Modified Text: " + pageText);

            //        // Create a new page in the output PdfSharp document
            //        var newPage = subDocument.AddPage();

            //        using (XGraphics gfx = XGraphics.FromPdfPage(newPage))
            //        {
            //            // Draw the modified text on the new page
            //            XFont font = new XFont("Arial", 12);
            //            gfx.DrawString(pageText, font, XBrushes.Black, new XRect(0, 0, newPage.Width, newPage.Height), XStringFormats.TopLeft);
            //                double yPosition = 50;
            //                // Draw images from the original page
            //                foreach (var image in page.GetImages())
            //            {
            //                byte[] bytes;

            //                // Attempt to get the PNG bytes; otherwise, fall back to raw bytes
            //                if (image.TryGetPng(out byte[] pngBytes))
            //                {
            //                    bytes = pngBytes;
            //                }
            //                else
            //                {
            //                    bytes = image.RawBytes.ToArray(); // Assuming RawBytes is IEnumerable<byte>
            //                }

            //                // Create the XImage directly from the byte array
            //                using (MemoryStream stream = new MemoryStream(bytes, 0, bytes.Length, true, true))
            //                {
            //                    if (stream.Length > 0)
            //                    {
            //                        stream.Position = 0;

            //                        // Create the XImage from the stream
            //                        XImage xImage = XImage.FromStream(stream);

            //                            double maxWidth = newPage.Width - 40; // Leave some margin
            //                            double scale = xImage.PixelWidth > maxWidth ? maxWidth / xImage.PixelWidth : 1;

            //                            // Calculate new dimensions while maintaining aspect ratio
            //                            double scaledWidth = xImage.PixelWidth * scale;
            //                            double scaledHeight = xImage.PixelHeight * scale;

            //                            // Draw the image on the graphics context at the current Y position
            //                            gfx.DrawImage(xImage, new XRect(20, yPosition, scaledWidth, scaledHeight)); // Leave a margin on the left

            //                            // Increment the Y position for the next image
            //                            yPosition += scaledHeight + 10; // Add a small gap between images
            //                        }
            //                }
            //            }
            //        }

            //    }
            //}

                for (int page = startPage; page < endPage; page++)
                {
                    subDocument.AddPage(sourceDocument.Pages[page]);
                }
                return subDocument;
        }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
    }
}
        public static async Task<string> ConvertFormFileToBase64Async(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("The file is invalid.");
            }

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();
                string base64String = Convert.ToBase64String(fileBytes);
                return base64String;
            }
        }
        public static string ConvertPdfToBase64(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                throw new ArgumentException("The file path is invalid or the file does not exist.");
            }

            // Read the PDF file into a byte array
            byte[] pdfBytes = File.ReadAllBytes(filePath);

            // Convert the byte array to a Base64 string
            string base64String = Convert.ToBase64String(pdfBytes);

            return base64String;
        }
        public static MemoryStream CloneStream(Stream originalStream)
        {
            var clonedStream = new MemoryStream();
            originalStream.Position = 0;
            originalStream.CopyTo(clonedStream);
            clonedStream.Position = 0;
            return clonedStream;
        }
        public static string CreateZipFile(string subPdfFolder, string subPdfPath, string textFilePath,string fileName,string thumbnailUrl)
        {
            string zipFilePath = Path.Combine(subPdfFolder, $"{fileName}.zip");
            using (var zipStream = new FileStream(zipFilePath, FileMode.Create))
            {
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    // Add  PDF to the  zip folder
                    archive.CreateEntryFromFile(subPdfPath, Path.GetFileName(subPdfPath), CompressionLevel.Optimal);

                    // Add  OCR text file to the zip
                    archive.CreateEntryFromFile(textFilePath, "dataOCR.txt", CompressionLevel.Optimal);
                    // Add thumbnailUrl file to the zip
                    archive.CreateEntryFromFile(thumbnailUrl, Path.GetFileName(thumbnailUrl), CompressionLevel.Optimal);

                }
            }
            return zipFilePath;
        }
        public static async Task<byte[]> GetImageOrPdfFromZip(string zipFilePath)
        {
            string zipPath = GetZipPath(zipFilePath);
            string imageFileName = GetImageFileName(zipFilePath);
            using (var zipArchive = ZipFile.OpenRead(zipPath))
            {
                var entry = zipArchive.GetEntry(imageFileName);
                if (entry == null)
                {
                    throw new FileNotFoundException($"File {imageFileName} not found in ZIP archive.");
                }
                using (var entryStream = entry.Open())
                using (var memoryStream = new MemoryStream())
                {
                    await entryStream.CopyToAsync(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
        public static string GetZipPath(string zipPathWithImage)
        {
           
            int lastBackslashIndex = zipPathWithImage.LastIndexOf('\\');
            if (lastBackslashIndex >= 0)
            {
                // Return the substring up to the last backslash
                return zipPathWithImage.Substring(0, lastBackslashIndex);
            }
            return zipPathWithImage; // If no backslash found, return the full path
        }
        public static string GetImageFileName(string zipPathWithImage)
        {
            
            int lastBackslashIndex = zipPathWithImage.LastIndexOf('\\');
            if (lastBackslashIndex >= 0 && lastBackslashIndex < zipPathWithImage.Length - 1)
            {
                // Return the substring after the last backslash
                return zipPathWithImage.Substring(lastBackslashIndex + 1);
            }
            return string.Empty; // If no backslash found, return an empty string
        }
        public static string CreateDirectoryForFile(IFormFile file, string wordASN, string baseDestinationPath)
        {
            //string baseDestinationPath = pathDirectory;
            //string baseDestinationPath = GetBaseDestinationPath();

            string fileNameWithoutExtension = string.IsNullOrEmpty(wordASN) ? Path.GetFileNameWithoutExtension(file.FileName) : wordASN;
            // string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
            string subFileFolder = Path.Combine(baseDestinationPath, fileNameWithoutExtension);
            Directory.CreateDirectory(subFileFolder);
            return subFileFolder;
        }
        public static List<ASNInfo> ExtractAndProcessASN(IFormFile file)
        {

            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                byte[] fileContent = memoryStream.ToArray();
                List<ASNInfo> asnInfoList = ProcessASN(fileContent);

                return asnInfoList;
            }
        }

        //public static List<ASNInfo> ProcessASN(byte[] fileContent)
        //{
        //    List<ASNInfo> asnInfoList = new List<ASNInfo>();

        //    // Convert byte[] to PDF document
        //    PdfPigDocument document = PdfPigDocument.Open(fileContent);

        //    // Process the PDF document
        //    for (int i = 0; i < document.NumberOfPages; i++)
        //    {
        //        var page = document.GetPage(i + 1);
        //        Console.WriteLine($"Found Page words: {page.GetWords()}");

        //        foreach (var word in page.GetWords())
        //        {
        //            Console.WriteLine($"Found ASN on page {i}: {word}");
        //            var match = Regex.Match(word.Text, @"EasyDocSep");

        //            if (match.Success)
        //            {
        //                ASNInfo asnInfo = new ASNInfo
        //                {
        //                    PageNumber = i,
        //                    WordASN = match.Value
        //                };
        //                asnInfoList.Add(asnInfo);
        //            }
        //        }
        //    }

        //    Console.WriteLine("Total ASN count: " + asnInfoList.Count);

        //    return asnInfoList;
        //}
        public static List<ASNInfo> ProcessASN(byte[] fileContent)
        {
            List<ASNInfo> asnInfoList = new List<ASNInfo>();


            // Convert byte[] to PDF document
            using (var document = PdfPigDocument.Open(fileContent))
            {

                // Iterate over the pages
                for (int i = 0; i < document.NumberOfPages; i++)
                {
                    var page = document.GetPage(i + 1);
                //    if (string.IsNullOrWhiteSpace(page.Text) isText = true;
                    // If no text is found, use OCR to process images
                    if (string.IsNullOrWhiteSpace(page.Text))
                    {
                        
                        // Extract image from the page and run OCR
                        var ocrText = PerformOCROnPage(page);
                        Console.WriteLine($"Found OCR text on page {i + 1}: {ocrText}");

                        // Look for ASN info in the OCR text

                        var pattern = @"\b(?:[A-Z]*)?EasyDocSep(?:[A-Z]*)?\b"; // Allows uppercase prefix/suffix
                        var matching = Regex.Replace(ocrText, pattern, "EasyDocSep", RegexOptions.IgnoreCase);
                        string[] words = ocrText.Split(new char[] { ' ', '\n', '\t', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
                        var match = Regex.Match(matching, @"EasyDocSep");
                        if (match.Success || words.Length==1)
                        {
                            ASNInfo asnInfo = new ASNInfo
                            {
                                PageNumber = i,
                                WordASN = "EasyDocSep"
                            };
                            asnInfoList.Add(asnInfo);
                        }
                    }
                    else
                    {
                        // Process normal text in PDF
                        foreach (var word in page.GetWords())
                        {
                            Console.WriteLine($"Found ASN on page {i + 1}: {word.Text}");
                            var match = Regex.Match(word.Text, @"EasyDocSep");

                            if (match.Success)
                            {
                                ASNInfo asnInfo = new ASNInfo
                                {
                                    PageNumber = i,
                                    WordASN = match.Value
                                };
                                asnInfoList.Add(asnInfo);
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Total ASN count: " + asnInfoList.Count);
            return asnInfoList;
        }

        // Perform OCR on the image extracted from the PDF page
        public static string PerformOCROnPage(Page page)
        {
            // Extract images from the page
            List<Image> images = ExtractImageFromPage(page);
            string ocrText = "";
            var tessDataPath = @"C:\Users\MSI\Documents\GitHub\paperless\Tessdata\tesseract-ocr-tesseract-bc059ec\tessdata";
            using (var ocrEngine = new TesseractEngine(tessDataPath, "ara+eng+fra", EngineMode.Default))
            {
                ocrEngine.DefaultPageSegMode = PageSegMode.SingleBlock;

                // Optional: set character whitelist (customize as needed)
                ocrEngine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");
                foreach (var pdfImage in page.GetImages())
                {
                    byte[] imageBytes = pdfImage.RawBytes.ToArray();
                    using (var pix = Pix.LoadFromMemory(imageBytes))
                    {
                       if(pix != null)
                        {
                            using (var pageOcr = ocrEngine.Process(pix))
                            {
                                var text = pageOcr.GetText();
                                float confidence = pageOcr.GetMeanConfidence();
                               
                                // Post-process OCR text based on confidence and custom rules
                                if (confidence >= 0.85) // Confidence threshold
                                {
                                    ocrText += PostProcessOCRText(text);
                                }
                                else
                                {

                                    ocrText = text;
                                    Console.WriteLine($"Low-confidence OCR result: {confidence}, Text: {text}");
                              
                                }

                                Console.WriteLine($"Text extracted from image on page:\n{text}");
                            }
                        
                        }
                    }
                };
            }

            return ocrText;
        }
        private static string PostProcessOCRText(string ocrText)
        {
            // Replace common OCR errors with correct text
            ocrText = ocrText.Replace("351/005", "EasyDocSep");
            ocrText = ocrText.Replace("casyDocSep", "EasyDocSep");

            // Add custom regex-based pattern correction
            var pattern = @"\b(Easy|casy)Doc(?:Sep)?\b";
            ocrText = Regex.Replace(ocrText, pattern, "EasyDocSep", RegexOptions.IgnoreCase);

            return ocrText;
        }
        // Extract images from the PDF page
        public static List<Image> ExtractImageFromPage(Page page)
        {
            try
            {
                List<Image> images = new List<Image>();

                // Iterate through the images in the page
                foreach (var pdfImage in page.GetImages())
                {
                    // Get the image's bytes
                    byte[] imageBytes = pdfImage.RawBytes.ToArray(); // This gets the image as a byte array

                        using (var memoryStream = new MemoryStream(imageBytes, 0, imageBytes.Length, true, true))
                        {
                            // Convert image to OpenCvSharp Mat for preprocessing
                            Mat img = Cv2.ImDecode(imageBytes, ImreadModes.Grayscale);

                            // Preprocessing: noise reduction, thresholding, scaling, etc.
                            Cv2.GaussianBlur(img, img, new Size(5, 5), 0); // Noise removal
                            Cv2.Threshold(img, img, 0, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary); // Binarization
                            Cv2.Resize(img, img, new Size(), 2, 2, InterpolationFlags.Linear); // Scale image for better OCR

                            // Convert back to Bitmap for Tesseract
                            Bitmap bitmap = img.ToBitmap();
                            images.Add(bitmap);
                            // Convert back to Bitmap for Tesseract
                            //Image img = Image.FromStream(memoryStream);
                            //images.Add(img);
                        }
                    }

                return images;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        //public static List<ASNInfo> ProcessASN(byte[] fileContent)
        //{
        //    try
        //    {

        //        // Initialize Tesseract engine with the desired language (English)
        //        List<ASNInfo> asnInfoList = new List<ASNInfo>();

        // Load the PDF document from the byte array

        //List<ASNInfo> asnInfoList = new List<ASNInfo>();

        //using (var stream = new MemoryStream(fileContent))
        //{
        //    PdfDocument document = PdfReader.Open(stream, PdfDocumentOpenMode.ReadOnly);

        //    for (int i = 0; i < document.PageCount; i++)
        //    {
        //        PdfPage page = document.Pages[i];

        //        var text = ExtractTextFromPage(page);

        //        Console.WriteLine($"Page {i + 1}: {text}");

        //        // Search for the ASN in the text
        //        var match = Regex.Match(text, @"EasyDocSep");
        //        if (match.Success)
        //        {
        //            asnInfoList.Add(new ASNInfo
        //            {
        //                PageNumber = i ,
        //                WordASN = match.Value
        //            });
        //        }
        //    }
        //}

        //return asnInfoList;
        //    }
        //    catch(Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }

        //}
        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
        private static string ExtractTextFromPage(PdfPage page)
        {
            var content = ContentReader.ReadContent(page);
            return ExtractTextFromContent(content);
        }
        private static string ExtractTextFromContent(CObject content)
        {
            StringBuilder result = new StringBuilder();

            if (content is COperator)
            {
                var cOperator = content as COperator;
                foreach (var operand in cOperator.Operands)
                {
                    result.Append(ExtractTextFromContent(operand));
                }
            }
            else if (content is CSequence)
            {
                var cSequence = content as CSequence;
                foreach (var element in cSequence)
                {
                    result.Append(ExtractTextFromContent(element));
                }
            }
            else if (content is CString)
            {
                var cString = content as CString;
                result.Append(cString.Value);
            }

            return result.ToString();
        }
        public static string SavePdfDocument(PdfDocument subDocument, string subPdfFolder, string fileName)
        {
            string subPdfPath = Path.Combine(subPdfFolder, $"{fileName}.pdf");
            subDocument.Save(subPdfPath);
            return subPdfPath;
        }
        public static string SaveFileToDisk(IFormFile file, string folderPath)
        {
            string fileName = Path.GetFileName(file.FileName);
            string filePath = Path.Combine(folderPath, fileName);

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return filePath;
        }
        public static string CreatePdfFromImage(string imagePath, string folderPath, string pdfFileName)
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
        public static List<string> ExtractWords(string content)
        {
            // Normalize the content (convert to lower case, remove punctuation)
            var normalizedContent = content.ToLowerInvariant();
            var words = System.Text.RegularExpressions.Regex.Split(normalizedContent, @"\W+");

            // Filter out any empty strings
            return words.Where(word => !string.IsNullOrWhiteSpace(word)).ToList();
        }
        public static string GetContentTypeByExtension(string extension)
        {
            switch (extension.ToLower())
            {
                case ".pdf":
                    return "application/pdf";
                case ".png":
                    return "image/png";
                case ".jpg":
                    return "image/jpeg";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".pptx":
                    return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case ".txt":
                    return "text/plain";
                default:
                    return "application/octet-stream"; // Default for unknown types
            }
        }


    }

    public class CustomFontResolver : IFontResolver
    {

        public byte[] GetFont(string faceName)
        {
            switch (faceName)
            {
                case "Arial#":
                    return LoadFontData("C:\\Users\\MSI\\Documents\\GitHub\\travis-scripts-master\\travis-scripts-master\\fonts\\arial.ttf"); ;

                case "Arial#b":
                    return LoadFontData("MyProject.fonts.arial.arialbd.ttf"); ;

                case "Arial#i":
                    return LoadFontData("MyProject.fonts.arial.ariali.ttf");

                case "Arial#bi":
                    return LoadFontData("MyProject.fonts.arial.arialbi.ttf");
            }

            return null;
        }


        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // Ignore case of font names.
            var name = familyName.ToLower().TrimEnd('#');

            // Deal with the fonts we know.
            switch (name)
            {
                case "arial":
                    if (isBold)
                    {
                        if (isItalic)
                            return new FontResolverInfo("Arial#bi");
                        return new FontResolverInfo("Arial#b");
                    }
                    if (isItalic)
                        return new FontResolverInfo("Arial#i");
                    return new FontResolverInfo("Arial#");
            }

            // We pass all other font requests to the default handler.
            // When running on a web server without sufficient permission, you can return a default font at this stage.
            return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
        }
        private byte[] LoadFontData(string fontPath)
        {
            if (!File.Exists(fontPath))
            {
                throw new ArgumentException($"The file {fontPath} does not exist.");
            }

            return File.ReadAllBytes(fontPath);
            //var assembly = Assembly.GetExecutingAssembly();

            //// Test code to find the names of embedded fonts - put a watch on "ourResources"
            ////var ourResources = assembly.GetManifestResourceNames();

            //using (Stream stream = assembly.GetManifestResourceStream(name))
            //{
            //    if (stream == null)
            //        throw new ArgumentException("No resource with name " + name);

            //    int count = (int)stream.Length;
            //    byte[] data = new byte[count];
            //    stream.Read(data, 0, count);
            //    return data;
            //}
        }
        internal static CustomFontResolver OurGlobalFontResolver = null;

        /// <summary>
        /// Ensure the font resolver is only applied once (or an exception is thrown)
        /// </summary>
        internal static void Apply()
        {
            if (OurGlobalFontResolver == null || GlobalFontSettings.FontResolver == null)
            {
                if (OurGlobalFontResolver == null)
                    OurGlobalFontResolver = new CustomFontResolver();

                GlobalFontSettings.FontResolver = OurGlobalFontResolver;
            }
        }


    }
    public static class PixConverter
    {
        public static Pix ToPix(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image), "Image cannot be null.");
            }

            if (image is Bitmap bitmap)
            {
                using (var memoryStream = new MemoryStream())
                {
                    // Save the bitmap to the memory stream in a format Tesseract can handle
                    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                    memoryStream.Position = 0; // Reset stream position

                    // Load the image into Pix
                    return Pix.LoadFromMemory(memoryStream.ToArray());
                }
            }
            else
            {
                throw new ArgumentException("Provided image must be a Bitmap.", nameof(image));
            }
        }
     
    }
}
