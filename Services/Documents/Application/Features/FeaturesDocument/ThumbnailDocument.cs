using Application.Dtos.Documents;
using Application.Exceptions;
using Application.Helper;
using Application.Respository;
using Aspose.Pdf.Devices;
using Elasticsearch.Net;
using MediatR;
using Microsoft.Extensions.Configuration;
using PdfSharp.Pdf;
using Spire.Pdf;
using System.Drawing;
using System.Drawing.Imaging;
using Document = Domain.Documents.Document;



namespace Application.Features.FeaturesDocument
{
    public class ThumbnailDocument
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
            private readonly IConfiguration _configuration;
            private readonly EncryptionHelper _encryptionHelper;

            public Handler(IDocumentRepository repository, IConfiguration configuration, EncryptionHelper encryptionHelper)
            {
                _repository = repository;
                _configuration = configuration;
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
                    byte[] memoryStream = await FileProccess.GetImageOrPdfFromZip(documentDetails.ThumbnailUrl);
                    byte[] decryptedImageBytes = FileHelper.DecryptDocument(memoryStream, key, iv);

                    return decryptedImageBytes;
                    
                   // return memoryStream;
                }
                catch (Exception ex )
                {
                    throw;
                }
               
                //if (!File.Exists(documentDetails.FileData))
                //{
                //    throw new FileNotFoundException("PDF file not found.", documentDetails.FileData);
                //}
                //return documentDetails.ThumbnailUrl;

            }
        }
    }
}
