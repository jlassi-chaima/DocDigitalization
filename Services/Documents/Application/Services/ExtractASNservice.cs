using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace Application.Services
{
    public class ASNInfo
    {
        public int PageNumber { get; set; }
        public string WordASN { get; set; }

    }
    public class ExtractASNservice
    {
        private List<int> pagesWithASN = new List<int>();
        private List<string> words = new List<string>();
        public List<ASNInfo> ExtractAndProcessASN(string file)
        {
            List<ASNInfo> asnInfoList = new List<ASNInfo>();
            PdfDocument document = PdfDocument.Open(file);
            for (var i = 0; i < document.NumberOfPages; i++)
            {
                var page = document.GetPage(i+1);

                foreach (var word in page.GetWords())
                {
                    //Ce numéro définit le réseau du propriétaire
                    var match = Regex.Match(word.Text, @"ASN\s?\-?\d+");

                    if (match.Success)
                    {
                        Console.WriteLine($"Found ASN on page {i}: {match.Value}");
                        ASNInfo asnInfo = new ASNInfo
                        {
                            PageNumber = i,
                            WordASN = match.Value
                        };
                        asnInfoList.Add(asnInfo);

                    }
                }
            }
            Console.WriteLine("Total ASN count: " + asnInfoList.Count);
            //retour des numéros de page
            return asnInfoList;
        }
        public void CreateSubPdfs(List<int> pages, string pathpdf, string outputFolder)
        {
            PdfSharp.Pdf.PdfDocument pdfDocument = PdfReader.Open(pathpdf, PdfDocumentOpenMode.Import);
           PdfDocument document = PdfDocument.Open(pathpdf);
            int startPage = 0;
            int endPage = 0;

            for (int i = 0; i < pages.Count; i++)
            {
                startPage = pagesWithASN[i];
                endPage = i < pagesWithASN.Count - 1 ? pagesWithASN[i + 1] : pdfDocument.Pages.Count;


                // Create sub-PDF folder
                string subPdfFolder = Path.Combine(outputFolder, $"ASN_{startPage + 1}_{endPage + 1}");
                Directory.CreateDirectory(subPdfFolder);

                // Create sub-PDF document
                PdfSharp.Pdf.PdfDocument subDocument = new PdfSharp.Pdf.PdfDocument();

                for (int page = startPage; page <= endPage - 1; page++)
                {
                    subDocument.AddPage(pdfDocument.Pages[page]);
                }

                // Save sub-PDF
                string subPdfPath = Path.Combine(subPdfFolder, $"SubPDF_Page_{startPage}_to_{endPage}.pdf");
                subDocument.Save(subPdfPath);

                // Extract and save text 
                string textFilePath = Path.Combine(subPdfFolder, "dataOCR.txt");
                var output = "";
                //boucle pour lire text
                for (int page = startPage + 1; page <= endPage; page++)
                {
                    var x = document.GetPage(page);
                    words = new List<string>();

                    foreach (var wordBlock in x.GetWords())
                    {
                        words.Add(wordBlock.Text);
                    }

                    output += string.Join(" ", words) + "\n";
                }

                File.WriteAllText(textFilePath, output);
            }
        }
    }
}
