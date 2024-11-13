using Application.Services;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Domain.DocumentManagement.DocumentTypes;
using Domain.Documents;
using Microsoft.AspNetCore.Http;
using PdfSharp.Pdf;

namespace Application.Features.FeaturesDocument.Documents
{
    public interface IUploadDocumentUseCase
    {
        Task<List<Document>> UplaodDocument(IFormFile formData, string id,string type,string document);
        Task<Document> ProcessEntireDocument(IFormFile file, string type, string idOwner, Document documentToAdd, ArchiveSerialNumbers archive);
        Task<Document> SavePageAsDocument(IFormFile file, PdfDocument pdfDocument, AnalyzeResult result, DocumentType docTypeExist, string type, string idOwner, ASNInfo asnPage, int i, int startPage, int endPage, Document documentToAdd,int count);
        Document CreateDocument(Document doc, string fileNameWithoutExtension, string wordASN, string content, string pdfPath, string idowner, string mimetype, string checksum, string lang, string? typee, Guid docTypeId);
        Task LogDocumentProcessing(IFormFile file);
        Task AddOrUpdateCustomFields(Document docToAdd, AnalyzeResult result);
        Task AssignPropertiesToDocument(Document documentToAdd, string idOwner, string docType);
        Task<Guid> GetGroupForUser(string idOwner);

    }
}
