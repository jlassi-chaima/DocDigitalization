

using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.AspNetCore.Http;

namespace Domain.Ports
{
    public interface IAzurePort
    {
        public  Task<AnalyzeResult> ExtractPdfDoc(Stream stream, string documenType);
        public  Task<AnalyzeResult> ExtractDoc(IFormFile file, string documenType);
        public Task<string> ClassifyDocumentType(IFormFile file);
        public Task<string> ClassifyPdfDocumentType(Stream stream);
        public Task<AnalyzeResult> ReadDoc(IFormFile file);


    }
}
