using Application.Dtos.Documents;
using Application.Exceptions;
using Application.Respository;
using Domain.Documents;
using MediatR;
using NTextCat.Commons;


namespace Application.Features.FeaturesDocument
{
    public class MetaDataDocument
    {
        public sealed record Query : IRequest<DocumentMetadata>
        {
            public readonly Guid Id;
            public Query(Guid id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Query, DocumentMetadata>
        {
            private readonly IDocumentRepository _repository;

            public Handler(IDocumentRepository repository)
            {
                _repository = repository;
            }

            public async Task<DocumentMetadata> Handle(Query request, CancellationToken cancellationToken)
            {
                Document documentdetails = await _repository.FindByIdAsync(request.Id, cancellationToken);
                Console.WriteLine(documentdetails);
                if (documentdetails == null)
                {
                    throw new DocumentsException($"Document with ID {request.Id} not found.");
                }
                FileInfo fileInfo = new FileInfo(documentdetails.FileData);

                var MetaDataDocument = new DocumentMetadata
                {
                    DateCreated = documentdetails.CreatedOn,
                    DateModified = documentdetails.LastModifiedOn,
                    MediaFilename = documentdetails.Title,
                    OriginalFileSize = fileInfo.Exists ? fileInfo.Length : (long?)null,
                    Checksum = documentdetails.Checksum,
                    MimeType = documentdetails.MimeType,
                    MailRule = documentdetails.Mailrule,
                    Lang = documentdetails.Lang,
                    Source = documentdetails.Source
                };
                return MetaDataDocument;
            }
        }
    }
}
