
using Core.Database;
using Domain.DocumentManagement.DocumentNote;

namespace Application.Respository
{
    public interface IDocumentNoteRepository : IRepository<DocumentNote, Guid>
    {
        Task<IReadOnlyList<DocumentNote>> GetAllByDocIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> NoteExistsAsync(DocumentNote note, CancellationToken cancellationToken = default);
    }
}
