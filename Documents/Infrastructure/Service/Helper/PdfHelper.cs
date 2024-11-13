using Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace Infrastructure.Service.Helper
{
    public static class PdfHelper
    {
        public static void ProcessPdfPages(List<ASNInfo> pages, PdfSharp.Pdf.PdfDocument pdfDocument, string DestinationPath, PdfDocument document)
        {
            foreach (var pageInfo in pages)
            {
                int startPage = pageInfo.PageNumber;
                int endPage = pageInfo.PageNumber < pages.Count - 1 ? pages[pageInfo.PageNumber + 1].PageNumber : pdfDocument.Pages.Count;
                string baseDestinationPath = DestinationPath;
                Directory.CreateDirectory(baseDestinationPath);
                string subPdfFolder = Path.Combine(baseDestinationPath, pageInfo.WordASN);
                Directory.CreateDirectory(subPdfFolder);

                PdfSharp.Pdf.PdfDocument subDocument = new PdfSharp.Pdf.PdfDocument();
                for (int page = startPage; page < endPage; page++)
                {
                    subDocument.AddPage(pdfDocument.Pages[page]);
                }

                string subPdfPath = Path.Combine(subPdfFolder, $"{pageInfo.WordASN}.pdf");
                subDocument.Save(subPdfPath);
                subDocument.Close();

                string textFilePath = Path.Combine(subPdfFolder, "dataOCR.txt");
                var output = "";

                for (int page = startPage + 1; page <= endPage; page++)
                {
                    var currentPage = document.GetPage(page);
                    List<string> words = currentPage.GetWords().Select(w => w.Text).ToList();
                    output += string.Join(" ", words) + "\n";
                }

                File.WriteAllText(textFilePath, output);
                subDocument.Dispose();
            }
        }
    }
}
