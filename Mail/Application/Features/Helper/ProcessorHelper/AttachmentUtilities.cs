

using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Text;
using Tesseract;
using UglyToad.PdfPig;
using System.Text.RegularExpressions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using MigraDocCore.Rendering;
using MigraDocCore.DocumentObjectModel;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.Office.Interop.Word;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Graphics.Operations.PathPainting;
using UglyToad.PdfPig.XObjects;

namespace Application.Features.Helper.ProcessorHelper
{
    public static class AttachmentUtilities
    {

        //check for *.pdf and *invoice*
        public static bool MatchWildcard(string text, string pattern)
        {
            if (pattern.StartsWith("*") && pattern.EndsWith("*"))
            {
                string substring = pattern.Trim('*');
                return text.Contains(substring);
            }
            else if (pattern.StartsWith("*"))
            {
                string suffix = pattern.TrimStart('*');
                return text.EndsWith(suffix);
            }
            else
            {
                return text == pattern;
            }
        }

        public static string ExtractTextFromPdf(MemoryStream pdfStream)
        {
            StringBuilder text = new StringBuilder();

            List<IPdfImage> images = new List<IPdfImage>();
            using (PdfDocument document = PdfDocument.Open(pdfStream))
            {
                foreach (var page in document.GetPages())
                {
                    foreach (var textLine in page.GetWords())
                    {
                        text.AppendLine(textLine.Text);
                    }
                    foreach (var image in page.GetImages())
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

                        Console.WriteLine($"Image with  bytes of type '{type}' on page {page.Number}. Location: {image.Bounds}.");

                    }
                }
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
                                    text.AppendLine(pageOcr.GetText());
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
            return text.ToString();
        }
        public static string file_emplacement(MimeMessage message, MimePart mimePart, string directorypath)
        {
            var folderName = string.IsNullOrWhiteSpace(message.Subject) ? "NoSubject" : message.Subject;
            folderName = string.Join("_", folderName.Split(Path.GetInvalidFileNameChars())); // Remove invalid characters from folder name
            var folderPath = Path.Combine(directorypath, folderName);
            Directory.CreateDirectory(folderPath); // Create directory if not exists

            var fileName = mimePart.FileName ?? "unknown";
            var sanitizedFileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(folderPath, sanitizedFileName);
            const int maxRetries = 5;
            const int delay = 1000; // milliseconds
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    using (var stream = File.Create(filePath))
                    {
                        mimePart.Content.DecodeTo(stream);
                    }

                    Console.WriteLine("Document saved: " + filePath);
                    return filePath;
                }
                catch (IOException ex)
                {
                    if (i == maxRetries - 1)
                    {
                        // Log or rethrow the exception if all retries are exhausted
                        throw new IOException($"Failed to create file at {filePath} after {maxRetries} attempts", ex);
                    }
                    Thread.Sleep(delay); // Wait before retrying
                }
            }

            //Console.WriteLine("Document saved: " + filePath);
            return filePath;
        }
        public static string ExtractTextFromImage(string imagePath, string tessDataPathh)
        {
            string tessDataPath = tessDataPathh;
            using (var engine = new TesseractEngine(tessDataPath, "ara+eng+fra", EngineMode.Default))
            {
                using (var img = Pix.LoadFromFile(imagePath))
                {
                    using (var page = engine.Process(img))
                    {
                        return page.GetText();
                    }
                }
            }
        }
        public static string get_mail_details(MimeMessage message)
        {
            string emailDetails = $"<h2>From: {message.From}</h2><br>" +
                                    $"<h2>To:{message.To}</h2><br>" +
                                    $"<h2>Cc:{message.Cc}</h2><br>" +
                                    $"<h2>Subject:{message.Subject}</h2><br>" +
                                    $"<h2>Body:</h2> <h3>{message.TextBody}</h3> <br>";
            return emailDetails;
        }
        public static async System.Threading.Tasks.Task ConvertHtmlToPdf(string htmlContent, string emailDetails, string outputPath)
        {
            try
            {
                var sharpClient = new GotenbergSharpClient("http://localhost:3000");
                string concatenatedText = $"{emailDetails}\n{htmlContent}";
                var builder = new HtmlRequestBuilder()
                    .AddAsyncDocument(async doc =>
                    {

                        doc.SetBody(concatenatedText); // Set the HTML content obtained from the method argument
                    });

                var request = await builder.BuildAsync();

                var response = await sharpClient.HtmlToPdfAsync(request);

                using (var destinationStream = File.Create(outputPath))
                {
                    await response.CopyToAsync(destinationStream);
                }

                Console.WriteLine($"PDF successfully generated: {outputPath}");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void SaveEmlContent(MimeMessage message, string filePath)
        {
            using (var stream = File.Create(filePath))
            {
                message.WriteTo(stream);
            }
        }
        public static string StripHtmlTags(string html)
        {
            return Regex.Replace(html, "<.*?>", string.Empty);
        }

        public static string DetectTextBoxesInSheet(ISheet sheet)
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
        public static string DetectImagesInSheet(ISheet sheet)
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
                                catch (System.Exception ex)
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
        public static string ConvertTextToPdfUsingMigraDoc(string textContent, string fileNameWithoutExtension, string destinationFolderPath)
        {
            // Create a new MigraDoc document
            MigraDocCore.DocumentObjectModel.Document document = new MigraDocCore.DocumentObjectModel.Document();
            MigraDocCore.DocumentObjectModel.Section section = document.AddSection();

            // Add the text content to the document
            MigraDocCore.DocumentObjectModel.Paragraph paragraph = section.AddParagraph();
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
            return pdfFilePath;


        }

        public static string ExportShapeAsImage(Microsoft.Office.Interop.PowerPoint.Shape shape)
        {
            try
            {
                // Create a unique file name for the image
                string imagePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");

                // Export the shape as an image file
                shape.Export(imagePath, PpShapeFormat.ppShapeFormatPNG);

                return imagePath;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error exporting shape as image: {ex.Message}");
                return null;
            }
        }

    }
}
