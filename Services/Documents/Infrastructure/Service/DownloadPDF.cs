
//using Application.Respository;
//using Domain.Documents;
//using Microsoft.AspNetCore.Mvc;



//namespace Infrastructure.Service
//{
//    public class DownloadPDF
//    {

//        private IDocumentRepository _repository;
//        public DownloadPDF(IDocumentRepository repository)
//        {

//            _repository = repository;
//        }


//        public async Task<IActionResult> uploadction(Guid Id)
//        {
//            Document document = await _repository.FindByIdAsync(Id);

//            if (document == null || string.IsNullOrEmpty(document.FileData))
//            {
//                throw new Exception("Document not found or FileData is empty.");
//            }
//            if (!File.Exists(document.FileData))
//            {
//                throw new FileNotFoundException("File not found at the specified path.", document.FileData);
//            }

//            // Read the content of the file
//            byte[] byteArray = await File.ReadAllBytesAsync(document.FileData);


//            string fileName = document.Title; 
//            string mimeType = document.MimeType;
//            if(mimeType == "image/png")
//            {

//            }
//            string contentType= "application/pdf";


//            return new FileContentResult(byteArray, contentType)
//            {
//                FileDownloadName = fileName
//            };
//        }
//    }

//}

using Application.Respository;
using Domain.Documents;
using Microsoft.AspNetCore.Mvc;



namespace Infrastructure.Service
{
    public class DownloadPDF
    {

        private IDocumentRepository _repository;
        public DownloadPDF(IDocumentRepository repository)
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
            Document document = await _repository.FindByIdAsync(Id);

            if (document == null || string.IsNullOrEmpty(document.FileData))
            {
                throw new Exception("Document not found or FileData is empty.");
            }
            if (!File.Exists(document.FileData))
            {
                throw new FileNotFoundException("File not found at the specified path.", document.FileData);
            }
            // Read the content of the file
            byte[] byteArray = await File.ReadAllBytesAsync(document.FileData);


            string fileName = document.Title + ".pdf";
            string mimeType = document.MimeType;
            string contentType = "application/pdf";

            return new FileContentResult(byteArray, contentType)
            {
                FileDownloadName = fileName
            };
        }
    }

}
