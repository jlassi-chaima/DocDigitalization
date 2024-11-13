using Application.Respository;
using Domain.Documents;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class DownloadOriginalFile
    {
        private IDocumentRepository _repository;
        public DownloadOriginalFile(IDocumentRepository repository)
        {

            _repository = repository;
        }
        private static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>
        {
            { ".pdf", "application/pdf" },
            { ".jpeg", "image/jpeg" },
            { ".jpg", "image/jpeg" },
            { ".png", "image/png" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
            { ".txt", "text/plain" },
            { ".eml", "message/rfc822" }
        };


        public async Task<IActionResult> uploadction(Guid Id)
        {
            // Retrieve the document from the repository
            Document document = await _repository.FindByIdAsync(Id);

            // Check if the document exists and has a valid file path
            if (document == null || string.IsNullOrEmpty(document.FileData))
            {
                throw new Exception("Document not found or FileData is empty.");
            }

            // Get the original file path
            string originalFilePath = GetOriginalFilePath(document.FileData, document.MimeType);

            // Read the content of the original file
            byte[] fileContent = await File.ReadAllBytesAsync(originalFilePath);

            // Set the file name for download
            string fileName = Path.GetFileName(originalFilePath);

            // Return the file content as a FileContentResult
            return new FileContentResult(fileContent, document.MimeType)
            {
                FileDownloadName = fileName
            };
        }

        private string GetOriginalFilePath(string convertedPdfFilePath, string desiredMimeType)
        {
            // Get the directory path of the converted PDF file
            string directoryPath = Path.GetDirectoryName(convertedPdfFilePath);

            // Search for files with the desired MIME type in the directory
            string[] files = Directory.GetFiles(directoryPath);

            foreach (var file in files)
            {
                // Check if the file matches the desired MIME type
                string mimeType = GetMimeType(file);
                if (mimeType == desiredMimeType)
                {
                    return file; // Return the path of the file with matching MIME type
                }
            }

            throw new FileNotFoundException($"File with MIME type '{desiredMimeType}' not found in directory '{directoryPath}'.");
        }

        private string GetMimeType(string filePath)
        {
            // Get the file extension
            string extension = Path.GetExtension(filePath)?.ToLower();

            // Determine and return MIME type based on the file extension
            switch (extension)
            {
                case ".pdf":
                    return "application/pdf";
                case ".jpeg":
                case ".jpg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".pptx":
                    return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case ".txt":
                    return "text/plain";
                case ".eml":
                    return "message/rfc822";
                default:
                    // Handle other file types or throw exception if unsupported
                    throw new NotSupportedException($"Unsupported file extension: {extension}");
            }

        }
    }
}
