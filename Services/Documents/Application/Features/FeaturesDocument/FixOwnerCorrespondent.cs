using Application.Dtos.Documents;
using Application.Features.FeaturesDocument.Documents;
using Application.Respository;
using Domain.Documents;
using MediatR;

namespace Application.Features.FeaturesDocument
{
    public class FixOwnerCorrespondent
    {
        public sealed record Command : IRequest<Document>
        {
            public readonly Guid DocumentId;
            public readonly FixOwnerCorrespondentDto Documentupdate;

            public Command(FixOwnerCorrespondentDto documentpdate, Guid id)
            {
                Documentupdate = documentpdate;
                DocumentId = id;
            }
        }
        public sealed class Handler : IRequestHandler<Command, Document>
        {

            private readonly IDocumentRepository _documentrepository;
            private readonly ITagRepository _tagrepository;
            private readonly ITemplateRepository _templaterepository;
            private readonly IFileTasksRepository _filetasksrepository;
            private readonly IUploadDocumentUseCase _uploadDocumentRepository;
            private readonly IDocumentTypeRepository _documentTypeRepository;


            public Handler(IDocumentRepository documentrepository, IDocumentTypeRepository documentTypeRepository, IUploadDocumentUseCase uploadDocumentRepository, IFileTasksRepository filetasksrepository, ITemplateRepository templaterepository, ITagRepository tagrepository)
            {
                _documentrepository = documentrepository;
                _filetasksrepository = filetasksrepository;
                _templaterepository = templaterepository;
                _tagrepository = tagrepository;
                _documentTypeRepository= documentTypeRepository;
                _uploadDocumentRepository= uploadDocumentRepository;    
            }

            public async Task<Document> Handle(Command request, CancellationToken cancellationToken)
            {
                // from dto
                var documentdto = request.Documentupdate;

                var documenttoupadte = _documentrepository.FindByIdAsync(request.DocumentId).Result;
         

                var fileTaskTodelete = _filetasksrepository.FindFileTaskByDocument(documenttoupadte).Result;
                if (documenttoupadte != null)
                {
                    if (documenttoupadte != null && documenttoupadte.DocumentTypeId.HasValue)
                    {
                        var docType = await _documentTypeRepository.FindByIdAsync(documenttoupadte.DocumentTypeId.Value, cancellationToken);
                        if (documentdto.CorrespondentId.HasValue)
                        {
                            documenttoupadte.CorrespondentId = documentdto.CorrespondentId;
                        }
                        if (documentdto.OwnerId != null)
                        {
                            documenttoupadte.Owner = documentdto.OwnerId;
                            await _uploadDocumentRepository.AssignPropertiesToDocument(documenttoupadte, documentdto.OwnerId, docType.Name);

                                    //string classification_result = null;
                                    //string uniqueSubdir = null;
                                    //JObject uniqueSubdirJson = null;
                                    //// Bring the ML Classification result 
                                    //using (var httpClient = new HttpClient
                                    //{
                                    //    Timeout = TimeSpan.FromMinutes(5) // Set timeout to 5 minutes
                                    //})
                                    //{
                                    //    var apiUrl = "http://localhost:5003/classify_extract";
                                    //    // Append the result as a query parameter to the URL
                                    //    apiUrl += "?result=" + Uri.EscapeDataString(documenttoupadte.Content);

                            //    try
                            //    {
                            //        // Send the POST request to the Flask endpoint with the result as a query parameter
                            //        var response = await httpClient.PostAsync(apiUrl, null);

                            //        // Ensure the request completed successfully
                            //        response.EnsureSuccessStatusCode();

                            //        // Read the response content as a string
                            //        var responseBody = await response.Content.ReadAsStringAsync();

                            //        //// Parse the JSON string into a JObject
                            //        //JObject jsonResponse = JObject.Parse(responseBody);

                            //        JArray jsonResponse = JArray.Parse(responseBody);

                            //        classification_result = (string)jsonResponse[0];
                            //        string uniqueSubdirJsonString = (string)jsonResponse[1];

                            //        uniqueSubdirJson = JObject.Parse(uniqueSubdirJsonString);


                            //        Console.WriteLine("Classification Result: " + classification_result);
                            //        Console.WriteLine("Unique Subdir JSON: " + uniqueSubdirJson.ToString());

                            //        //// Access the text and path fields
                            //        //classification_result = jsonResponse["generated_texts"].ToString();
                            //        //uniqueSubdir = jsonResponse["unique_subdir"].ToString();

                            //        // Handle the response as needed
                            //        Console.WriteLine("Generated Texts:");

                            //        Console.WriteLine("Unique Subdirectory: " + uniqueSubdir);

                            //        if (response.IsSuccessStatusCode)
                            //        {
                            //            Console.WriteLine("Result sent successfully.");
                            //        }
                            //        else
                            //        {
                            //            Console.WriteLine("Failed to send result. Status code: " + response.StatusCode);
                            //        }
                            //    }
                            //    catch (HttpRequestException ex)
                            //    {
                            //        Console.WriteLine("Failed to send result. Error: " + ex.Message);
                            //    }
                            //}


                            //List<Template> templates = await _templaterepository.GetAllByOwner(documentdto.OwnerId);
                            //if (templates != null)
                            //{
                            //    foreach (Template template in templates)
                            //    {

                            //        bool filter_result = FileHelper.ConsumptionStartedFilter(documenttoupadte, template, classification_result);
                            //        //check if we're in the case of consumption started 
                            //        if (template.Type == GetListByType.Started && filter_result)
                            //        {

                            //            documenttoupadte.Title = template.AssignTitle;
                            //            documenttoupadte.DocumentTypeId = template.AssignDocumentType;
                            //            documenttoupadte.CorrespondentId = template.AssignCorrespondent;
                            //            documenttoupadte.StoragePathId = template.AssignStoragePath;
                            //            List<DocumentTags> tags = new List<DocumentTags>();
                            //            foreach (Guid id in template.AssignTags)
                            //            {
                            //                Tag tagtoadd = _tagrepository.FindByIdAsync(id).GetAwaiter().GetResult();

                            //                DocumentTags documentTag = new DocumentTags
                            //                {
                            //                    Document = documenttoupadte,
                            //                    DocumentId = documenttoupadte.Id,
                            //                    Tag = tagtoadd,
                            //                    TagId = tagtoadd.Id
                            //                };
                            //                tags.Add(documentTag);

                            //            }
                            //            documenttoupadte.Tags = tags;
                            //            break;
                            //        }
                            //    }
                            //}

                            //if (documenttoupadte.Tags == null && documenttoupadte.CorrespondentId == null && documenttoupadte.DocumentTypeId == null && documenttoupadte.StoragePathId == null)
                            //{
                            //    // assign tags, documentType, Correspondent , storage path
                            //   // await FileHelper.AssignDocumentProperties(documenttoupadte, documentdto.OwnerId, assignTagToDocument, assignCorrespondentToDocument, assignDocumentTypeToDocument, assignStoragePathToDocument, classification_result, uniqueSubdirJson);
                            //}
                            //if (templates != null)
                            //{ 
                            //    List<Template> templateAdded = templates.Where(t=>t.Type == GetListByType.Added).ToList();
                            //    if(templateAdded.Count > 0)
                            //    {
                            //        foreach (Template template in templateAdded)
                            //        {


                            //            bool filter_result = FileHelper.DocumentAddedFilter(documenttoupadte, template);
                            //            //check if we're in the case of Document Added 
                            //            if (template.Type == GetListByType.Added && filter_result)
                            //            {

                            //                documenttoupadte.Title = template.AssignTitle;
                            //                documenttoupadte.DocumentTypeId = template.AssignDocumentType;
                            //                documenttoupadte.CorrespondentId = template.AssignCorrespondent;
                            //                documenttoupadte.StoragePathId = template.AssignStoragePath;
                            //                List<DocumentTags> tags = new List<DocumentTags>();
                            //                foreach (Guid id in template.AssignTags)
                            //                {
                            //                    Tag tagtoadd = _tagrepository.FindByIdAsync(id).GetAwaiter().GetResult();

                            //                    DocumentTags documentTag = new DocumentTags
                            //                    {
                            //                        Document = documenttoupadte,
                            //                        DocumentId = documenttoupadte.Id,
                            //                        Tag = tagtoadd,
                            //                        TagId = tagtoadd.Id
                            //                    };
                            //                    tags.Add(documentTag);

                            //                }
                            //                documenttoupadte.Tags = tags;
                            //                break;
                            //            }

                            //        }

                            //    }





                            //}
                                    await _documentrepository.UpdateAsync(documenttoupadte);
                            await _filetasksrepository.DeleteByIdAsync(fileTaskTodelete.Id);
                        }
                    }
                    await _documentrepository.UpdateAsync(documenttoupadte);

                }
                
                return documenttoupadte;
            }
        }
    }
}