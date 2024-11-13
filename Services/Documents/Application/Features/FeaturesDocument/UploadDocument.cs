using Application.Dtos;
using Application.Respository;
using Application.Services;
using Domain.Documents;
using FluentValidation;
using MapsterMapper;
using MediatR;


namespace Application.Features.FeaturesDocument
{
    public class UploadDocument
    {
        public sealed record Command : IRequest<Document>
        {
            public Document FileUploadDto { get; }
            public Command(Document fileUploadDto)
            {
                FileUploadDto = fileUploadDto;
            }
        }
        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator(IDocumentRepository _repository)
            {
                RuleFor(p => p.FileUploadDto.Title).NotEmpty();
                RuleFor(p => p.FileUploadDto.Content).NotEmpty();
            }
        }
        public sealed class Handler : IRequestHandler<Command, Document>
        {
            private readonly IDocumentRepository _repository;
            private readonly IMapper _mapper;
            private readonly ExtractASNservice _extractASNservice;

            public Handler(IDocumentRepository repository, IMapper mapper, ExtractASNservice extractASNservice)
            {
                _repository = repository;
                _mapper = mapper;
                _extractASNservice = extractASNservice;
            }

            public async Task<Document> Handle(Command request, CancellationToken cancellationToken)
            {

                var documentupload =Document.Upload(request.FileUploadDto.Title, request.FileUploadDto.FileData, request.FileUploadDto.Content,request.FileUploadDto.Archive_Serial_Number,"user");
               // List<ASNInfo> pages = _extractASNservice.ExtractAndProcessASN(documentupload.Title);
                await _repository.AddAsync(documentupload, cancellationToken);
                return _mapper.Map<Document>(documentupload);
            }
            
        }
    }
}
