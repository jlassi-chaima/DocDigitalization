//using Application.Dtos.DocumentNote;
//using Application.Respository;
//using Domain.DocumentManagement.DocumentNote;
//using Domain.Documents;
//using MapsterMapper;
//using MediatR;

//namespace Application.Features.FeaturesNote
//{
//    public class AddNote
//    {
//        public sealed record Command : IRequest<DocumentNote>
//        {
//            public readonly DocumentNoteDto Documentnotetoadd;
//            public readonly Guid documentid;
//            public Command(DocumentNoteDto documentnote, Guid documentidcommand)
//            {
//                Documentnotetoadd = documentnote;
//                documentid = documentidcommand;
//            }
//        }
//        public sealed class Handler : IRequestHandler<Command, DocumentNote>
//        {
//            private readonly IDocumentRepository _documentrepository;
//            private readonly IDocumentNoteRepository _documentnoterepository;
//            private readonly IMapper _mapper;
//            public Handler(IMapper mapper, IDocumentRepository documentrepository, IDocumentNoteRepository documentnoterepository)
//            {
//                _documentrepository = documentrepository;
//                _mapper = mapper;
//                _documentnoterepository = documentnoterepository;
//            }
//            public async Task<DocumentNote> Handle(Command request, CancellationToken cancellationToken)
//            {
//                try
//                {
//                    // Retrieve the Document entity from the repository
//                    Document document = await _documentrepository.FindByIdAsync(request.documentid, cancellationToken);

//                    // Check if the DocumentNotes collection is null, and initialize it if needed
//                    if (document.Notes == null)
//                    {
//                        document.Notes = new List<DocumentNote>();
//                    }

//                    // Create a new DocumentNote object
//                    DocumentNote note = DocumentNote.Create(request.Documentnotetoadd.Note, request.Documentnotetoadd.User);

//                    // Add the new DocumentNote to the Document entity's DocumentNotes collection
//                    document.Notes.Add(note);

//                    // Save the changes to the database using the DocumentNote repository
//                    await _documentnoterepository.AddAsync(note);

//                    // Return the newly created DocumentNote
//                    return note;
//                }
//                catch (Exception ex)
//                {
//                    // Log the exception
//                    Console.WriteLine(ex.Message, "Error occurred while adding a DocumentNote to a Document.");

//                    // Rethrow the exception
//                    throw;
//                }
//            }

//        }
//    }
//}
using Application.Dtos.DocumentNote;
using Application.Respository;
using Domain.DocumentManagement.DocumentNote;
using Domain.Documents;
using Domain.Logs;
using MapsterMapper;
using MediatR;

namespace Application.Features.FeaturesNote
{
    public class AddNote
    {
        public sealed record Command : IRequest<DocumentNote>
        {
            public readonly DocumentNoteDto Documentnotetoadd;
            public readonly Guid documentid;
            public Command(DocumentNoteDto documentnote, Guid documentidcommand)
            {
                Documentnotetoadd = documentnote;
                documentid = documentidcommand;
            }
        }
        public sealed class Handler : IRequestHandler<Command, DocumentNote>
        {
            private readonly IDocumentRepository _documentrepository;
            private readonly IDocumentNoteRepository _documentnoterepository;
            private readonly IMapper _mapper;
            private readonly ILogRepository _logRepository;
            public Handler(IMapper mapper, IDocumentRepository documentrepository, IDocumentNoteRepository documentnoterepository, ILogRepository logRepository)
            {
                _documentrepository = documentrepository;
                _mapper = mapper;
                _documentnoterepository = documentnoterepository;
                _logRepository = logRepository;
            }
            public async Task<DocumentNote> Handle(Command request, CancellationToken cancellationToken)
            {
                Logs new_note = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"new Note added {request.Documentnotetoadd.Note}");
                await _logRepository.AddAsync(new_note);
                try
                {
                    // Retrieve the Document entity from the repository
                    Document document = await _documentrepository.FindByIdAsync(request.documentid, cancellationToken);

                    // Check if the DocumentNotes collection is null, and initialize it if needed
                    if (document.Notes == null)
                    {
                        document.Notes = new List<DocumentNote>();
                    }

                    // Create a new DocumentNote object
                    DocumentNote note = DocumentNote.Create(request.Documentnotetoadd.Note, document.Owner);
                    note.CreatedAt = DateTime.UtcNow;
                    // Add the new DocumentNote to the Document entity's DocumentNotes collection
                    document.Notes.Add(note);

                    // Save the changes to the database using the Document repository
                    await _documentnoterepository.AddAsync(note);

                    // Return the newly created DocumentNote
                    return note;
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine(ex.Message, "Error occurred while adding a DocumentNote to a Document.");

                    // Rethrow the exception
                    throw;
                }
            }

        }
    }
}
