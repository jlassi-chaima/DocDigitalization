using Infrastructure.Service;
using Application.Dtos.DocumentCustomField;
using Application.Features.FeaturesDocument;
using Domain.Documents;
using Infrastructure.Controllers;
using Microsoft.AspNetCore.Mvc;
using Application.Parameters;
using Application.Features.FeaturesNote;
using Application.Dtos.DocumentNote;
using Application.Dtos.Documents;
using Domain.DocumentManagement.DocumentNote;
using Application.Dtos.Suggestions;
using Application.Dtos.SelectionData;
using Application.Dtos.BulkEdit;
using Application.Respository;
using Nest;
using Application.Features.FeatureDashboard;
using Application.Statistics.Model;
using Application.Dashboard;
using static MassTransit.ValidationResultExtensions;
using PdfSharp.Pdf.IO;
using Application.Features.FeaturesDocument.Documents;
using Serilog;
using Application.Features.FeaturesDocument.DocToSharePoint;
using Domain.DocumentManagement.DocumentTypes;
using Core.Domain.AllEntity;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Collections.Generic;
using Xceed.Words.NET;
using Aspose.Pdf.Operators;
using Microsoft.AspNetCore.Http.HttpResults;
using Application.Dtos.ArchivenSerialNumber;
using Domain.DocumentManagement.StoragePath;
using Microsoft.Graph.SecurityNamespace;
using Pipelines.Sockets.Unofficial.Buffers;




namespace API.Controller
{
    [ApiController]
    public class DocumentController : BaseApiController
    {
        private readonly ServiceUploadPowerPointFile _uploadpowerpointfile;
        private readonly ServiceUploadExcelFile _uploadexcelfile;
        private readonly ServiceUploadWordFile _uploadexwordfile;
        private readonly ServiceUploadOCR _serviceupload;
        private readonly DownloadPDF _downloadpdf;
        private readonly ServiceUploadImage _uploadimage;
        private readonly ServiceUploadTextFile _uploadextextfile;
        private readonly IElasticsearchRepository _elasticsearchRepository;
        private readonly IElasticClient _elasticClient;
        private readonly DownloadOriginalFile _downloadOriginalFile;
        private readonly IUploadDocumentUseCase _uploadDocumentUseCase;
        private static readonly HttpClient client = new HttpClient();
        private readonly IGraphApiUseCase _graphApi;

        public DocumentController(ServiceUploadOCR serviceupload,
            DownloadPDF downloadpdf,
            ServiceUploadImage uploadimage,
            ServiceUploadWordFile uploadexwordfile,
            ServiceUploadExcelFile uploadexcelfile,
            ServiceUploadPowerPointFile uploadpowerpointfile,
            ServiceUploadTextFile uploadextextfile,
            IElasticClient elasticClient,
            DownloadOriginalFile downloadOriginalFile,
            IUploadDocumentUseCase uploadDocumentUseCase,
            IGraphApiUseCase graphApi)
        {
            _serviceupload = serviceupload;
            _downloadpdf = downloadpdf;
            _uploadimage = uploadimage;
            _uploadexwordfile = uploadexwordfile;
            _uploadexcelfile = uploadexcelfile;
            _uploadpowerpointfile = uploadpowerpointfile;
            _uploadextextfile = uploadextextfile;
            _elasticClient = elasticClient;
            _downloadOriginalFile = downloadOriginalFile;
            _uploadDocumentUseCase = uploadDocumentUseCase;
            _graphApi = graphApi;
        }
        [HttpGet("list_document")]
        public async Task<IActionResult> ListDocumentAsync([FromQuery] DocumentParameters documentparameters, [FromQuery] string? tags__id__all, [FromQuery] string? correspondent__id__in, [FromQuery] string? document_type__id__in, [FromQuery] string? storage_path__id__in, [FromQuery] string? title__icontains, [FromQuery] string? query, [FromQuery] string owner, [FromQuery] string? owner__id, [FromQuery] string? owner__id__none, [FromQuery] int? owner__isnull, [FromQuery] string? ordering, [FromQuery] int? archive_serial_number, [FromQuery] int? archive_serial_number__isnull, [FromQuery] int? archive_serial_number__gt, [FromQuery] int? archive_serial_number__lt, [FromQuery] bool truncate_content, [FromQuery] string? title_content, [FromQuery] DateTime? created__date__gt)
        {
            try
            {
                //if (tags__id__all != null || correspondent__id__in != null || document_type__id__in != null || storage_path__id__in != null || title__icontains != null || query != null || owner != null || owner__id != null || owner__id__none != null || owner__isnull != null || query != null || ordering != null)
                //{
                new DocumentSearchParameters()
                {
                    TagID = tags__id__all,
                    CorrespondentID = correspondent__id__in,
                    DocumentTypeID = document_type__id__in,
                    StoragePathID = storage_path__id__in,
                    TitleIcontains = title__icontains,
                    Created = query,
                    Owner = owner,
                    OwnerId = owner__id,
                    OwnerIdNone = owner__id__none,
                    Search = query,
                    OwnerIsNull = owner__isnull,
                    Ordering = ordering,
                    ArchiveSerialNumber = archive_serial_number,
                    ArchiveSerialNumberIsNull = archive_serial_number__isnull,
                    ArchiveSerialNumberGT = archive_serial_number__gt,
                    ArchiveSerialNumberLT = archive_serial_number__lt,
                    TitleContent = title_content,
                    DocumentParameters = documentparameters ?? new DocumentParameters()
                };
                var command = new GetDocumentByTagCorrespondentDocumentType.Query(new DocumentSearchParameters()
                {
                    TagID = tags__id__all,
                    CorrespondentID = correspondent__id__in,
                    DocumentTypeID = document_type__id__in,
                    StoragePathID = storage_path__id__in,
                    TitleIcontains = title__icontains,
                    Created = query,
                    Owner = owner,
                    OwnerId = owner__id,
                    OwnerIdNone = owner__id__none,
                    Search = query,
                    OwnerIsNull = owner__isnull,
                    Ordering = ordering,
                    ArchiveSerialNumber = archive_serial_number,
                    ArchiveSerialNumberIsNull = archive_serial_number__isnull,
                    ArchiveSerialNumberGT = archive_serial_number__gt,
                    ArchiveSerialNumberLT = archive_serial_number__lt,
                    TitleContent = title_content,
                    DocumentParameters = documentparameters ?? new DocumentParameters()
                });

                var commandResponse = await Mediator.Send(command);

                return Ok(commandResponse);
                //}

                //return Ok();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");
            }



        }
        //[HttpGet("covid")]
        //public async Task<string> getLastCountries([FromQuery] DocumentParameters documentparameters, [FromQuery] string? tags__id__all, [FromQuery] string? correspondent__id__in, [FromQuery] string? document_type__id__in, [FromQuery] string? storage_path__id__in, [FromQuery] string? title__icontains, [FromQuery] string? query, [FromQuery] string owner, [FromQuery] string? owner__id, [FromQuery] string? owner__id__none, [FromQuery] int? owner__isnull, [FromQuery] string? ordering, [FromQuery] int? archive_serial_number, [FromQuery] int? archive_serial_number__isnull, [FromQuery] int? archive_serial_number__gt, [FromQuery] int? archive_serial_number__lt, [FromQuery] bool truncate_content)
        //{
        //    string requestUrl = $"https://covid-19-data.p.rapidapi.com/country/code?format=json&code={countryCode}";

        //    // Set up the headers
        //    client.DefaultRequestHeaders.Add("x-rapidapi-host", "covid-19-data.p.rapidapi.com");
        //    client.DefaultRequestHeaders.Add("x-rapidapi-key", "52f570ebccmshf44d78f8fcb54d2p152674jsn02736bf9575e");

        //    try
        //    {
        //        // Send the GET request
        //        HttpResponseMessage response = await client.GetAsync(requestUrl);
        //        response.EnsureSuccessStatusCode();

        //        // Read the response content
        //        string responseBody = await response.Content.ReadAsStringAsync();
        //        return responseBody;
        //    }
        //    catch (HttpRequestException e)
        //    {
        //        // Handle errors
        //        Console.WriteLine($"Request error: {e.Message}");
        //        return null;
        //    }


        //}
        [HttpGet("download_original/{id}")]
        public async Task<IActionResult> DownloadOriginal(Guid id)
        {
            return await _downloadOriginalFile.uploadction(id);

        }
        [HttpGet("documents-per-month/{ownerid}")]
        public async Task<ActionResult<DocumentsPerMonth>> GetDocumentsPerMonthAsync(string ownerid)
        {
            try
            {

                var commandResponse = await Mediator.Send(new ChartsDetails.Query(ownerid));
                return Ok(commandResponse);

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }

        }
        [HttpGet("documents_count_depending_on_source/{ownerid}")]
        public async Task<ActionResult<DocumentsSource>> GetDocumentCountsBySource(string ownerid)
        {
            try
            {


                return Ok(await Mediator.Send(new DocumentsDependingOnSource.Query(ownerid)));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }
        }
        [HttpGet("document-type-statistics")]
        public async Task<ActionResult<Dictionary<string, int>>> GetDocumentTypeStatistics()
        {
            try
            {


                return Ok(await Mediator.Send(new DocumentTypeStatistics.Query()));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }
        }
        [HttpGet("documets-groups-statistics")]
        public async Task<ActionResult<Dictionary<string, int>>> GetDocumetsGroupsStatistics()
        {
            try
            {

                return Ok(await Mediator.Send(new DocumetsGroupsStatistics.Query()));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");
            }
        }
        [HttpGet("documents_count_depensing_on_mimeType/{ownerid}")]
        public async Task<ActionResult<DocumentsPerMonth>> GetDocumentsDependingOnMimeTypeAsync(string ownerid)
        {
            var command = new DocumentsCountDeendingOnMimeType.Query(ownerid);
            var commandResponse = await Mediator.Send(command);

            return Ok(commandResponse);
        }
        [HttpGet("autocomplete")]
        public async Task<IActionResult> Autocomplete([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest("Search term cannot be empty.");
            }

            var suggestions = await GetAutocompleteSuggestions(term);
            return Ok(suggestions);
        }

        private async Task<IEnumerable<string>> GetAutocompleteSuggestions(string term)
        {
            var searchResponse = await _elasticClient.SearchAsync<Document>(s => s
               .Query(q => q
                   .MultiMatch(m => m
                       .Fields(f => f
                           .Field(ff => ff.Title) // Search in the Title field
                           .Field(ff => ff.Content) // Search in the Content field
                       )
               //     .Type(TextQueryType.BestFields) // Use best fields for combining scores
               //.Fuzziness(Fuzziness.Auto) // Enable fuzzy matching
               .Query(term) // Use .Query(term) to specify the search term
                   )));

            //.Size(10) // Limit number of results to 10
            //.Pretty(true) // Optional: enable pretty-printing of the response
            //.TypedKeys(true)); // Optional: ensure response uses typed keys
            if (!searchResponse.IsValid)
            {
                // Handle errors
                var debugInfo = searchResponse.DebugInformation;
                var error = searchResponse?.ServerError?.Error;
                // Handle error here
                return Enumerable.Empty<string>(); // Return empty collection if error occurs
            }

            var suggestions = searchResponse.Hits.Select(hit => hit.Source.Title); // Extract titles from hits

            return suggestions;
        }

        [HttpPost(Name = "add")]
        public async Task<IResult> AddDocumentAsync([FromForm] IFormFile formData, [FromForm] string document)
        {
            var type = Request.Headers.TryGetValue("Type", out var typeValues) ? typeValues.FirstOrDefault() : null;
            //await _elasticsearchRepository.IndexDocumentAsync(commandResponse);
            List<Document> docs = await Mediator.Send(new AddDocument.Command(formData, document, type));
            return Results.Ok(docs);

        }
        [HttpPost("Upload")]
        public async Task<IResult> UploadFile([FromForm] IFormFile formData, [FromQuery] string? id, [FromForm] string? document)
        {
            try
            {
                var type = Request.Headers.TryGetValue("Type", out var typeValues) ? typeValues.FirstOrDefault() : "ApiUpload";
               
                    await _uploadexcelfile.SaveExelFile(formData, type, id);
                
                List<Document> docs = await _uploadDocumentUseCase.UplaodDocument(formData, id, type, document);
                return Results.Ok();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");

            }
        }
        //[HttpPost("Upload")]
        //public async Task<ActionResult<Document>> UploadFile([FromForm] IFormFile formData, [FromQuery] string? id)
        //{
        //    try
        //    {


        //        Console.WriteLine(formData.ContentType);
        //        Access the custom "Type" header if it exists
        //       var type = Request.Headers.TryGetValue("Type", out var typeValues) ? typeValues.FirstOrDefault() : null;
        //        Console.Write(type);
        //        Log the received id owner of document
        //        Console.WriteLine($"Received ID: {id}");
        //        string result = await _serviceupload.UploadDoc(formData, id, type);
        //        return Ok(new ResultEntity
        //        {
        //            OutCode = StatusCodes.Status200OK.ToString(),
        //            OutMessage = result
        //        });
        //        await _serviceupload.ExtractDoc(formData, id, type);
        //        if (formData.ContentType.Contains("application/pdf") || formData.FileName.Contains(".pdf"))
        //        {

        //            List<ASNInfo> contentTask = _serviceupload.ExtractAndProcessASN(formData);
        //            await _serviceupload.SplitAndSaveASNPages(contentTask, formData, type, id);
        //        }
        //        if (formData.ContentType.Contains("image/png") || formData.ContentType.Contains("image/jpeg"))
        //        {
        //            string textWithoutNewlines = result.Replace("\n", " ");
        //            await _uploadimage.SaveImageFile(formData, type, id, textWithoutNewlines);
        //        }
        //        if (formData.ContentType.Contains("application/vnd.openxmlformats-officedocument .spreadsheetml.sheet"))
        //        {
           //         await _uploadexcelfile.SaveExelFile(formData, type, id);
        //        }
    //            if (formData.ContentType.Contains("application/vnd.openxmlformats-officedocument.wordprocessingml.document"))
    //            {
    //                await _uploadexwordfile.SaveWordFile(formData, type, id);
    //}
    //        if (formData.ContentType.Contains("application/vnd.openxmlformats-officedocument.presentationml.presentation"))
    //        {
    //            await _uploadpowerpointfile.SavePowerPointFile(formData, type, id);
    //        }
    //        if (formData.ContentType.Contains("text/plain"))
    //        {
    //            await _uploadextextfile.SaveTextFile(formData, type, id);
    //        }

    //    }
    //    catch (Exception ex)
    //    {
    //        throw new Exception(ex.Message);
    //    }



    //}
    //string fileName = "example.pdf"; // Provide the appropriate file name here

    //return File(fileBytes, "application/pdf", fileName);
    [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(Guid id)
        {
            return await _downloadpdf.uploadction(id);

        }
        [HttpGet("suggestions/{id}")]
        public async Task<ActionResult<SuggestionsDto>> GetSuggestionsAsync(Guid id)
        {
            try
            {

                return await Mediator.Send(new SuggestionsDocument.Query(id));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");

            }
        }
        [HttpPut("save/{id}")]
        public async Task<ActionResult<DocumentNote>> getDocumentNote(Guid id, DocumentUpdate document)
        {
            try
            {
                await Mediator.Send(new UpdateDocument.Command(document, id));
                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");

            }
        }

        [HttpDelete("delete_document/{id}")]
        public async Task<IActionResult> DeleteDocumentAsync(Guid id)
        {
            try
            {
                //await _elasticsearchRepository.DeleteDocumentAsync(id);
                var command = new DeleteDocument.Command(id);
                await Mediator.Send(command);

                return NoContent();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception("An error has occured, please try again later");

            }
        }
        [HttpPost("save_to_sharePoint/{id}")]
        public async Task<IActionResult> SaveDocumentToSharepoint(Guid id)
        {
            try
            {

                string result = await _graphApi.AddDocumentToList(id);
                if (!string.IsNullOrEmpty(result))
                {
                    return Ok(new ResultEntity()
                    {
                        OutCode = StatusCodes.Status200OK.ToString(),
                        OutMessage = result
                    });
                }
                else
                {
                    return BadRequest(new ResultEntity()
                    {
                        OutCode = StatusCodes.Status200OK.ToString(),
                        OutMessage = "Failed to upload document to SharePoint."
                    }
                 );
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");

            }
        }
        [HttpGet("get_document/{id}")]
        public async Task<ActionResult<DocumentDetailsDTO>> GetDocumentAsync(Guid id)
        {
            try
            {

                return await Mediator.Send(new DetailsDocument.Query(id));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest("An error has occured, please try again later");

            }
        }
        [HttpPut("assigncustomfield")]
        public async Task<IActionResult> assigncustomfieldAsync(AssignCustomFieldToDocumentDto request)
        {

            var command = new AssignCustomFieldToDocument.Command(request);
            await Mediator.Send(command);

            return Ok();
        }
        [HttpGet("thumb/{id}")]
        public async Task<IActionResult> GetthumbAsync(Guid id)
        {
            try
            {

                var imageBytes = await Mediator.Send(new ThumbnailDocument.Query(id));

                return File(imageBytes, "image/jpeg");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            // Chargez l'image depuis le chemin d'accès
            //byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(commandResponse);
            // Retourne l'image dans la réponse HTTP
            //return File(imageBytes, "image/jpeg");
            // return commandResponse;
        }
        [HttpGet("preview/{id}")]
        public async Task<IActionResult> GetPreviewAsync(Guid id)
        {
            try
            {
                var pdfBytes = await Mediator.Send(new PreviewDocument.Query(id));
                // var pdfBytes = await System.IO.File.ReadAllBytesAsync(commandResponse.FileData);
                return File(pdfBytes, "application/pdf");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        private async Task<byte[]> CombinePdfContentsAsync(IEnumerable<string> fileContents)
        {
            using (var outputDocument = new PdfSharp.Pdf.PdfDocument())
            {
                foreach (var fileContent in fileContents)
                {
                    if (!string.IsNullOrEmpty(fileContent))
                    {
                        var fileBytes = Convert.FromBase64String(fileContent);
                        using (var inputStream = new MemoryStream(fileBytes))
                        {
                            var inputDocument = PdfReader.Open(inputStream, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
                            foreach (var page in inputDocument.Pages)
                            {
                                outputDocument.AddPage(page);
                            }
                        }
                    }
                }

                using (var stream = new MemoryStream())
                {
                    outputDocument.Save(stream, false);
                    return stream.ToArray();
                }
            }
        }
        [HttpPost("notes/{id}")]
        public async Task<ActionResult<DocumentNote>> assignNoteToDocument(Guid id, DocumentNoteDto documentNotetoadd)
        {
            var command = new AddNote.Command(documentNotetoadd, id);
            var result = await Mediator.Send(command);
            return Ok(result);
        }
        [HttpGet("getnotes/{id}")]
        public async Task<ActionResult<DocumentNote>> getDocumentNote(Guid id)
        {
            var command = new ListNote.Query(id);
            await Mediator.Send(command);
            return Ok();
        }

        [HttpDelete("{documentId}/notes/{noteId}")]
        public async Task<ActionResult<DocumentNote>> DeleteDocumentNote(Guid documentId, Guid noteId)
        {
            var command = new DeleteNote.Command(documentId, noteId);
            await Mediator.Send(command);
            return Ok();
        }

        [HttpPost("selection_data/{owner}")]
        public async Task<ActionResult<SelectionDataDTO>> getSelectionData([FromBody] SelectionDataDocuments? ids, string owner)
        {
            var command = new SelectionData.Query(ids, owner);
            var commandResponse = await Mediator.Send(command);

            return commandResponse;
        }
        [HttpPut("fix_owner_correspondent/{id}")]
        public async Task<ActionResult<DocumentNote>> FixOwnerCorrespondent(Guid id, FixOwnerCorrespondentDto document)
        {
            var command = new FixOwnerCorrespondent.Command(document, id);
            await Mediator.Send(command);
            return Ok();
        }
        [HttpGet("getmetadata/{id}")]
        public async Task<ActionResult<DocumentMetadata>> GetDocumentMetaData(Guid id)
        {
            var command = new MetaDataDocument.Query(id);
            var commandResponse = await Mediator.Send(command);

            if (commandResponse == null)
            {
                return NotFound($"Document with ID {id} not found.");
            }

            return Ok(commandResponse);
        }
        [HttpPost("bulk_edit")]
        public async Task<ActionResult<DocumentMetadata>> BulkEditDocument(BulkEdit id)
        {
            var command = new BulkEditDocument.Command(id);

            await Mediator.Send(command);


            return Ok();
        }

    }
}
