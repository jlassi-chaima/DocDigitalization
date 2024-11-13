using Application.Dtos.Documents;
using Application.Dtos.StoragePath;
using Application.Helper;
using Application.Respository;
using Application.Services;
using Domain.DocumentManagement.CustomFields;
using Domain.DocumentManagement.DocumentNote;
using Domain.DocumentManagement.StoragePath;
using Domain.DocumentManagement.tags;
using Domain.Documents;
using Domain.Templates;
using Domain.Templates.Enum;
using MediatR;

namespace Application.Features.FeaturesDocument
{
    public class UpdateDocument
    {
        public sealed record Command : IRequest<Document>
        {
            public readonly Guid DocumentId;
            public readonly DocumentUpdate Documentupdate;
           

            public Command(DocumentUpdate documentpdate, Guid id)
            {
                Documentupdate = documentpdate;
                DocumentId = id;
               
            }
        }
        public sealed class Handler : IRequestHandler<Command, Document>
        {
           
            private readonly IDocumentRepository _documentrepository;
            private readonly IStoragePathRepository _storagePath; 
            private readonly ITagRepository _tagRepository;
            private readonly IDocumentNoteRepository _documentnoterepository;
            private readonly ICustomFieldRepository _customFieldrepository;
            private readonly ArchiveStoragePath _archiveStoragePath;
            private readonly ITemplateRepository TemplateRepository;
            public Handler(IDocumentRepository repository, ITagRepository tagRepository, IDocumentNoteRepository documentnoterepository, ICustomFieldRepository customFieldRepository, IStoragePathRepository storagePath, ArchiveStoragePath archiveStoragePath, ITemplateRepository templateRepository)
            {

                _documentrepository = repository;
                _tagRepository = tagRepository;
                _documentnoterepository = documentnoterepository;
                _customFieldrepository = customFieldRepository;
                _storagePath = storagePath;
                _archiveStoragePath = archiveStoragePath;
                TemplateRepository = templateRepository;
            }

            public async Task<Document> Handle(Command request, CancellationToken cancellationToken)
            {
                // from dto
                var documentdto = request.Documentupdate;
                // retrieve document to update
                
                var documenttoupadte = _documentrepository.FindByIdAsync(request.DocumentId).Result;
                if (documenttoupadte != null)
                {
                    documenttoupadte.Archive_Serial_Number = documentdto.Archive_Serial_Number;
                    List<DocumentTags> new_tags = new List<DocumentTags>();
                    // Update Tags 
                    // Tags : list of Guids
                    if (documentdto.Tags != null)
                    {
                        foreach (var tagid in documentdto.Tags)
                        {
                            Tag tagtoadd = _tagRepository.FindByIdAsync(tagid).GetAwaiter().GetResult();
                            DocumentTags documentTag = new DocumentTags
                            {
                                Document = documenttoupadte,
                                DocumentId = documenttoupadte.Id,
                                Tag = tagtoadd,
                                TagId = tagtoadd.Id
                            };
                            new_tags.Add(documentTag);
                        }
                    }
                    
                    if (documenttoupadte.Tags == null || documenttoupadte.Tags != null)
                    {
                        documenttoupadte.Tags = new List<DocumentTags>();
                    }
                    if (new_tags.Any()) 
                    {
                        foreach (var tag in new_tags)
                        {

                            if(documenttoupadte.Tags.Any(t=>t.TagId == tag.TagId)) 
                            {
                                continue;
                            }
                            documenttoupadte.Tags.Add(tag);
                        }
                    }
                   
                    // Update Document Type 
                    if (documentdto.DocumentTypeId.HasValue)
                    {
                        documenttoupadte.DocumentTypeId = documentdto.DocumentTypeId;
                    }
                    //Update Correspondent
                    if (documentdto.CorrespondentId.HasValue)
                    {
                        documenttoupadte.CorrespondentId = documentdto.CorrespondentId;
                    }
                    //Update StoragePath
                    // find storage path with find by id 
                   
                    
                    if (documentdto.StoragePathId.HasValue)
                    {
                        StoragePath storagepathgetbyid = _storagePath.FindByIdAsync((Guid)documentdto.StoragePathId).Result;
                        if (documenttoupadte.StoragePathId != documentdto.StoragePathId)
                        {
                            documenttoupadte.StoragePathId = documentdto.StoragePathId;
                            // create instance of stroage path dto
                            UpdateStoragePathDto storgepath = new UpdateStoragePathDto
                            {
                                
                                    Name = storagepathgetbyid.Name,
                                    Path = storagepathgetbyid.Path,
                                    Match = storagepathgetbyid.Match,
                                    Matching_algorithm = storagepathgetbyid.Matching_algorithm,
                                    Is_insensitive = storagepathgetbyid.Is_insensitive,
                                    Document_count = storagepathgetbyid.Document_count,
                                    Owner = storagepathgetbyid.Owner
                            };
                        
                            _archiveStoragePath.addArchiveStoragePath(storgepath, documenttoupadte);
                        }
                    
                    }
                    //Add Notes
                    //init List Notes
                    List<DocumentNote> new_notes = new List<DocumentNote>();
                    if (documentdto.Notes != null)
                    {
                        foreach (var notedto in documentdto.Notes)
                        {
                            DocumentNote note = new DocumentNote
                            {
                                Note = notedto.Note,
                                CreatedAt = notedto.Created,
                                User = notedto.User,
                            };
                            // veify if Note
                           if(! _documentnoterepository.NoteExistsAsync(note, cancellationToken).Result)
                           {
                                new_notes.Add(note);
                           }

                          
                        }
                    }
                    if (_documentnoterepository != null && documenttoupadte != null)
                    {
                        if (new_notes.Any())
                        {
                            // Initialize the Notes collection if it's null
                            if (documenttoupadte.Notes == null)
                            {
                                documenttoupadte.Notes = new List<DocumentNote>();
                            }

                            foreach (var notetoadd in new_notes)
                            {
                                await _documentnoterepository.AddAsync(notetoadd);
                                documenttoupadte.Notes.Add(notetoadd);
                            }
                        }
                    }
                    // Add Custom Fields
                    if (documentdto.Custom_Fields!= null)
                    {
                        // Retrieve List CustomField
                        List<CustomField> customfields = (List<CustomField>)await _customFieldrepository.GetAllAsync();
                        // Init List CustomFields Document
                        List<DocumentCustomField> newcustomfieldstoadd = new List<DocumentCustomField>();
                        if (documenttoupadte?.DocumentsCustomFields != null) 
                        {
                            // Identify custom fields to remove
                            List<DocumentCustomField> customfieldsToRemove = documenttoupadte.DocumentsCustomFields
                                .Where(existingCustomField => !documentdto.Custom_Fields.Any(cf => cf.Field == existingCustomField.CustomFieldId))
                                .ToList();

                            // Remove custom fields that are no longer present
                            foreach (var customfieldToRemove in customfieldsToRemove)
                            {
                                documenttoupadte.DocumentsCustomFields.Remove(customfieldToRemove);
                            }
                        }
                       
                        foreach (var customfield in documentdto.Custom_Fields)
                        {   
                            
                            CustomField existingCustomField = customfields.Find(cf => cf.Id.Equals(customfield.Field));
                            var existingDocumentCustomField = documenttoupadte.DocumentsCustomFields
                                                                .FirstOrDefault(cf => cf.CustomFieldId == customfield.Field);
                            if (existingDocumentCustomField != null)

                            {
                                if (existingDocumentCustomField.Value != customfield.Value)
                                {
                                    int index = documenttoupadte.DocumentsCustomFields.ToList().FindIndex(cf => cf.CustomFieldId == customfield.Field);
                                    if (index >= 0)
                                    {
                                        // Update the existing custom field
                                        documenttoupadte.DocumentsCustomFields.ToList()[index].Value = customfield.Value;
                                    }
                                }
                            }
                            if(documenttoupadte.DocumentsCustomFields.Any(cf=>cf.CustomFieldId == customfield.Field))
                            {
                                continue;
                            }
                            // Find the corresponding CustomField from the retrieved list
                            //if (documenttoupadte.DocumentsCustomFields.Any(cf => cf.Value != customfield.Value))
                            //{

                            //    DocumentCustomField documentCustomFields = new DocumentCustomField
                            //    {
                            //        Document = documenttoupadte,
                            //        DocumentId = documenttoupadte.Id,
                            //        CustomFieldId = (Guid)customfield.Field,
                            //        CustomField = existingCustomField,
                            //        Value = customfield.Value
                            //    };
                            //    documenttoupadte.DocumentsCustomFields.Add(documentCustomFields);
                                
                            //}

                            DocumentCustomField documentCustomField = new DocumentCustomField
                            {
                                Document = documenttoupadte,
                                DocumentId = documenttoupadte.Id,
                                CustomFieldId = (Guid)customfield.Field,
                                CustomField = existingCustomField,
                                Value = customfield.Value
                            };
                            newcustomfieldstoadd.Add(documentCustomField);
                        }
                      
                        if (newcustomfieldstoadd.Any())
                            {
                                if (documenttoupadte.DocumentsCustomFields == null)
                                {
                                    documenttoupadte.DocumentsCustomFields = new List<DocumentCustomField>();
                                }
                                foreach (var newcustomfield in newcustomfieldstoadd)
                                {
                                    documenttoupadte.DocumentsCustomFields.Add(newcustomfield);
                                }
                            }
                        

                    }
                    if (documentdto.set_permissions != null)
                    {
                        if(documentdto.set_permissions?.View?.Users != null)
                        {
                            if (documenttoupadte?.UsersView == null)
                            {
                                documenttoupadte.UsersView = new List<string>();
                            }
                            foreach (var user in documentdto.set_permissions.View.Users)
                            {
                                if (!documenttoupadte.UsersView.Contains(user))
                                {
                                    documenttoupadte.UsersView.Add(user);
                                }
                              //  documenttoupadte.UsersView.Add(user);
                            }
                            documenttoupadte.UsersView.RemoveAll(user => !documentdto.set_permissions.View.Users.Contains(user));
                        }
                        if (documentdto.set_permissions?.View?.Groups != null)
                        {
                            if (documenttoupadte.GroupsView == null)
                            {
                                documenttoupadte.GroupsView = new List<string>();
                            }
                            foreach (var group in documentdto.set_permissions.View.Groups)
                            {
                                if (!documenttoupadte.GroupsView.Contains(group))
                                {
                                    documenttoupadte.GroupsView.Add(group);
                                }
                               // documenttoupadte.GroupsView.Add(group);
                            }
                            documenttoupadte.GroupsView.RemoveAll(group => !documentdto.set_permissions.View.Groups.Contains(group));

                        }
                        if (documentdto.set_permissions?.Change?.Users != null)
                        {
                            if (documenttoupadte.UsersChange == null)
                            {
                                documenttoupadte.UsersChange = new List<string>();
                            }
                            foreach (var user in documentdto.set_permissions.Change.Users)
                            {
                                if (!documenttoupadte.UsersChange.Contains(user))
                                {
                                    documenttoupadte.UsersChange.Add(user);
                                }
                               // documenttoupadte.UsersChange.Add(user);
                            }
                            documenttoupadte.UsersChange.RemoveAll(user => !documentdto.set_permissions.Change.Users.Contains(user));

                        }
                        if (documentdto.set_permissions?.Change?.Groups != null)
                        {
                            if (documenttoupadte.GroupsChange == null)
                            {

                                documenttoupadte.GroupsChange = new List<string>();
                            }

                            foreach (var group in documentdto.set_permissions.Change.Groups)
                            {
                                if (!documenttoupadte.GroupsChange.Contains(group))
                                {
                                    documenttoupadte.GroupsChange.Add(group);
                                }
                               // documenttoupadte.GroupsChange.Add(group);
                            }
                            documenttoupadte.UsersChange.RemoveAll(user => !documentdto.set_permissions.Change.Users.Contains(user));

                        }
                    }
                    List<Template> templates = await TemplateRepository.GetAllByOwner(documenttoupadte.Owner);
                    // check if we're in the case of Document Updated
                    foreach (Template template in templates)
                    {
                        if (template.Type == GetListByType.Updated)
                        {

                            bool filter_result = FileHelper.DocumentAddedFilter(documenttoupadte, template);
                            //check if we're in the case of Document Added 

                            if (filter_result && template.Is_Enabled == true)
                            {

                                if (template.AssignTitle != null)
                                {
                                    documenttoupadte.Title = template.AssignTitle;
                                }
                               if(template.AssignDocumentType != null)
                                {
                                    documenttoupadte.DocumentTypeId = template.AssignDocumentType;
                                }
                                if (template.AssignCorrespondent != null)
                                {
                                    documenttoupadte.CorrespondentId = template.AssignCorrespondent;
                                }
                                if (template.AssignStoragePath != null)
                                {
                                    documenttoupadte.StoragePathId = template.AssignStoragePath;
                                }

                              
                                List<DocumentTags> tags = new List<DocumentTags>();
                                if (template.AssignTags != null)
                                {
                                    foreach (Guid id in template.AssignTags)
                                    {
                                        Tag tagtoadd = _tagRepository.FindByIdAsync(id).GetAwaiter().GetResult();

                                        DocumentTags documentTag = new DocumentTags
                                        {
                                            Document = documenttoupadte,
                                            DocumentId = documenttoupadte.Id,
                                            Tag = tagtoadd,
                                            TagId = tagtoadd.Id
                                        };
                                        tags.Add(documentTag);

                                    }
                                    if (tags.Any())
                                    {
                                        documenttoupadte.Tags = tags;
                                    }
                                }
                             
                                
                                break;
                            }
                        }

                    }
                }
               await  _documentrepository.UpdateAsync(documenttoupadte);
                return documenttoupadte;
            }
        }
    }
}
