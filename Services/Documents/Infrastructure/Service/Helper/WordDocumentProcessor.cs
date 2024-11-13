using Microsoft.AspNetCore.Http;
using NPOI.XWPF.UserModel;


namespace Infrastructure.Service.Helper
{
    public static class WordDocumentProcessor
    {
        public static void ReadWordDocument(IFormFile file)
        {
            using (Stream stream = file.OpenReadStream())
            {
                XWPFDocument document = new XWPFDocument(stream);

                // Access paragraphs in the document
                foreach (XWPFParagraph paragraph in document.Paragraphs)
                {
                    // Process each paragraph
                    Console.WriteLine(paragraph.ParagraphText);
                }
            }
        }
    }
}
