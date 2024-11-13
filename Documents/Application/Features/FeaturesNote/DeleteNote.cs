
using Application.Respository;
using Domain.DocumentManagement.DocumentNote;
using MediatR;

namespace Application.Features.FeaturesNote
{
    public class DeleteNote
    {
        public class Command : IRequest<DocumentNote>
        {
            public Guid DocumentId { get; }
            public Guid NoteId { get; }

            public Command(Guid documentId, Guid noteId)
            {
                DocumentId = documentId;
                NoteId = noteId;
            }
        }
        public sealed class Handler : IRequestHandler<Command, DocumentNote>
        {
            private readonly IDocumentNoteRepository _repository;

            public Handler(IDocumentNoteRepository repository)
            {
                _repository = repository;
            }

            public async Task<DocumentNote> Handle(Command request, CancellationToken cancellationToken)
            {
                var NoteTodelete = _repository.FindByIdAsync(request.NoteId).Result;
                await _repository.DeleteByIdAsync(request.NoteId);
                return NoteTodelete;
            }
        }
    }
}
