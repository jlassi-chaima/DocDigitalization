using Application.Exceptions;
using Application.Respository;
using Domain.DocumentManagement.CustomFields;
using Domain.DocumentManagement.DocumentNote;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class DocumentNoteRepository : IDocumentNoteRepository
    {
        private readonly DBContext _context;

        public DocumentNoteRepository(DBContext context)
        {
            _context = context;
        }
        public Task AddAsync(DocumentNote entity, CancellationToken cancellationToken = default)
        {
            try
            {
                
                _context.AddAsync(entity, cancellationToken);
                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                throw new DocumentNoteException(ex.Message.ToString());
            }
            return Task.FromResult(new DocumentNote());
        }


        public Task AddRangeAsync(IEnumerable<DocumentNote> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Expression<Func<DocumentNote, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(DocumentNote entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var documentnote = await _context.DocumentNotes.FindAsync(id, cancellationToken);

                if (documentnote == null)
                {
                    throw new DocumentNoteException($"Document Note with ID {id} not found."); 
                }

                _context.DocumentNotes.Remove(documentnote);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new DocumentNoteException(ex.Message.ToString());
            }
        }

        public Task DeleteRangeAsync(IReadOnlyList<DocumentNote> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public async Task<bool> ExistsAsync(Expression<Func<DocumentNote, bool>> predicate, CancellationToken cancellationToken = default)
        {
            // Use the predicate to filter the DocumentNotes DbSet
            var exists = await _context.DocumentNotes.AnyAsync(predicate, cancellationToken);

            // Return true if any matching records exist, otherwise return false
            return exists;
        }

        public Task<IReadOnlyList<DocumentNote>> FindAsync(Expression<Func<DocumentNote, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<DocumentNote?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var documentNote = await _context.DocumentNotes.FindAsync(new object[] { id }, cancellationToken);
            return documentNote;

        }

        public Task<DocumentNote?> FindOneAsync(Expression<Func<DocumentNote, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<DocumentNote>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<DocumentNote>> GetAllByDocIdAsync(Guid id,CancellationToken cancellationToken = default)
        {
            var documentnotes = await _context.DocumentNotes.ToListAsync(cancellationToken).ConfigureAwait(false);
            List<DocumentNote> result = new List<DocumentNote>();
            foreach (var documentnote in documentnotes)
            {
                if (documentnote.Id == id)
                    result.Add(documentnote);
                
            }
            return result;
        }

        public async Task<bool> NoteExistsAsync(DocumentNote note, CancellationToken cancellationToken = default)
        {
            // Check if a DocumentNote with the same properties as the given note exists in the database
            return await ExistsAsync(n => n.Note == note.Note && n.CreatedAt == note.CreatedAt && n.User == note.User, cancellationToken);

        }

        public Task UpdateAsync(DocumentNote entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
