

using Application.Exceptions;
using Application.Helper;
using Application.Respository;
using Domain.Documents;
using MediatR;

namespace Application.Features.FeaturesDocument
{
    public class PreviewDocument
    {
        public sealed record Query : IRequest<byte[]>
        {
            public readonly Guid Id;
            public Query(Guid id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Query, byte[]>
        {
            private readonly IDocumentRepository _repository;
            private readonly EncryptionHelper _encryptionHelper;

            public Handler(IDocumentRepository repository, EncryptionHelper encryptionHelper)
            {
                _repository = repository;
                _encryptionHelper = encryptionHelper;
            }

            public async Task<byte[]> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    Document documentDetails = await _repository.FindByIdAsync(request.Id, cancellationToken);
                    if (documentDetails == null)
                    {
                        throw new DocumentsException($"Document with ID {request.Id} not found.");
                    }
                    byte[] key = _encryptionHelper.DecryptKey(documentDetails.Key);
                    byte[] iv = _encryptionHelper.DecryptKey(documentDetails.Iv);
                    byte[] memoryStream = await FileProccess.GetImageOrPdfFromZip(documentDetails.FileData);
                    byte[] decryptedPdfBytes = FileHelper.DecryptDocument(memoryStream, key, iv);

                    return decryptedPdfBytes;
                    //byte[] memoryStream = await FileProccess.GetImageOrPdfFromZip(documentDetails.FileData);
                    //return memoryStream;
                }
                catch (Exception)
                {
                    throw;
                }

            }
        }
    }
}
