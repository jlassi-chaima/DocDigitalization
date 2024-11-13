using Application.Dtos.DocumentCustomField;
using Application.Exceptions;
using Application.Respository;
using Domain.DocumentManagement.CustomFields;
using Domain.Documents;
using MapsterMapper;
using MediatR;

namespace Application.Features.FeaturesDocument
{
    public class AssignCustomFieldToDocument
    {
        public sealed record Command : IRequest<Document>
        {
            public readonly AssignCustomFieldToDocumentDto customfieldId;

            public Command(AssignCustomFieldToDocumentDto custom)
            {
                customfieldId = custom;
            }
        }
        public sealed class Handler : IRequestHandler<Command, Document>
        {
            private readonly IDocumentRepository _repository;
            private readonly ICustomFieldRepository _fieldRepository;
            private readonly IMapper _mapper;
            
            public Handler(IDocumentRepository repository, ICustomFieldRepository fieldRepository, IMapper mapper)
            {
                _repository = repository;
                _fieldRepository = fieldRepository;
                _mapper = mapper;
            }

            public async Task<Document> Handle(Command request, CancellationToken cancellationToken)
            {
                Document document = await _repository.FindByIdAsync(request.customfieldId.DocumentId, cancellationToken);
                if (document == null)
                    throw new DocumentsException("Document not found.");
                CustomField customfield = await _fieldRepository.FindByIdAsync(request.customfieldId.CustomFieldId, cancellationToken);
                if (customfield == null)
                    throw new CustomFieldException("Custom Field not found.");
               
                customfield.DocumentsCustomFields = new List<DocumentCustomField>();
                    
               
                DocumentCustomField documentCustomField = new DocumentCustomField
                {
                    Document = document,
                    DocumentId = document.Id,
                    CustomFieldId = customfield.Id,
                    CustomField = customfield

                };

                customfield.DocumentsCustomFields.Add(documentCustomField);
                    await _fieldRepository.UpdateAsync(customfield);
                
                return document;



            }
        }
    }
}
