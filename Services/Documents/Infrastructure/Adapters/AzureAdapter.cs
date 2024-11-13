using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Http;
using Serilog;
using Aspose.Pdf.Operators;
using Domain.Ports;

namespace Infrastructure.Adapters
{
    public class AzureAdapter : IAzurePort
    {
        private readonly string apiKey;
        private readonly string endpoint;
        private readonly string modelId;
        private readonly DocumentAnalysisClient client;
        public AzureAdapter(IConfiguration configuration)
        {
            apiKey = configuration.GetSection("AzureConfig:DI_KEY").Value!;
            endpoint = configuration.GetSection("AzureConfig:DI_ENDPOINT").Value!;
            modelId = configuration.GetSection("AzureConfig:DI_MODELID").Value!;
            client = new DocumentAnalysisClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

        }
        public async Task<AnalyzeResult> ReadDoc(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", stream);
                return operation.Value;
            }
            catch (Exception ex)
            {
                Log.Error($"Error Message:{ex.Message}");
                throw new Exception(ex.Message);
            }
        }
        public async Task<string> ClassifyPdfDocumentType(Stream stream)
        {
            try
            {
                Operation<AnalyzeResult> operation = await client.ClassifyDocumentAsync(WaitUntil.Completed, modelId, stream);
                AnalyzeResult result = operation.Value;
                return result.Documents.First().DocumentType;
            }
            catch(Exception ex)
            {
                Log.Error($"Error Message:{ex.Message}");
                throw new Exception(ex.Message);
             }
    //string docType = "";
    //foreach (AnalyzedDocument document in result.Documents)
    //{
    //    Console.WriteLine($"Document of type: {document.DocumentType}, with confidence {document.Confidence}");
    //    docType = document.DocumentType;
    //}

         }
        public async Task<string> ClassifyDocumentType(IFormFile file)
        {

            try
            {

                //var client = new DocumentAnalysisClient(new Uri(endpoint), new AzureKeyCredential(key));
                using var content = file.OpenReadStream();
                Operation<AnalyzeResult> operation = await client.ClassifyDocumentAsync(WaitUntil.Completed, modelId, content);
                AnalyzeResult result = operation.Value;
                return result.Documents.First().DocumentType;
            }
            catch(Exception ex)
            {
                Log.Error($"Error Message:{ex.Message}");
                throw new Exception(ex.Message);
            }
        }
        public async Task<AnalyzeResult> ExtractDoc(IFormFile file, string documenType)
        {
            try
            {
                using var stream = file.OpenReadStream();
                Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, documenType, stream);
                return operation.Value;
             }
            catch(Exception ex)
            {
                Log.Error($"Error Message:{ex.Message}");
                throw new Exception(ex.Message);
             }
         }
        public async Task<AnalyzeResult> ExtractPdfDoc(Stream stream, string documenType)
        {

            try
            {

                Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, documenType, stream);
                return operation.Value;
            }
            catch (Exception ex)
            {
                Log.Error($"Error Message:{ex.Message}");
                throw new Exception(ex.Message);
            }
            // Convert the page content to a stream
            //byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(pageContent);
            //using var stream = new MemoryStream(byteArray);
        }


    }
}
