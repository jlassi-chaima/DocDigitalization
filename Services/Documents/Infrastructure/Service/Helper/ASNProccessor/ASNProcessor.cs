using Application.Services;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace Infrastructure.Service.Helper.ASNProccessor
{
    // Processes the content of a PDF file to extract ASN(Archiver Serial Number) information.
    public class ASNProcessor
    {
        public static List<ASNInfo> ProcessASN(byte[] fileContent)
        {
            List<ASNInfo> asnInfoList = new List<ASNInfo>();

            // Convert byte[] to PDF document
            PdfDocument document = PdfDocument.Open(fileContent);

            // Process the PDF document
            for (int i = 0; i < document.NumberOfPages; i++)
            {
                var page = document.GetPage(i + 1);

                foreach (var word in page.GetWords())
                {
                    var match = Regex.Match(word.Text, @"ASN\d+");

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

            return asnInfoList;
        }

    }
}

