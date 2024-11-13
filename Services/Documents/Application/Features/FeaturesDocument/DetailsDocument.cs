using Application.Dtos.Documents;
using Application.Exceptions;
using Application.Respository;
using Domain.Documents;
using Mapster;
using MediatR;
using Serilog;


namespace Application.Features.FeaturesDocument
{
    public class DetailsDocument
    {
        public sealed record Query : IRequest<DocumentDetailsDTO>
        {
            public readonly Guid Id;
            public Query(Guid id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Query, DocumentDetailsDTO>
        {
            private readonly IDocumentRepository _documentRepository;

            public Handler(IDocumentRepository documentRepository)
            {
                _documentRepository = documentRepository;
            }

            public async Task<DocumentDetailsDTO> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    Document documentdetails = await _documentRepository.FindByIdDetailsAsync(request.Id, cancellationToken);

                    if (documentdetails == null)
                    {
                        throw new DocumentsException($"Document with ID {request.Id} not found.");
                    }
                    var doc = documentdetails.Adapt<DocumentDetailsDTO>();
                    return documentdetails.Adapt<DocumentDetailsDTO>(); 
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    throw new Exception(ex.Message);
                }

            }
        }

    }
}
