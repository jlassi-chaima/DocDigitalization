using Domain.DocumentManagement.Correspondent;
using Domain.DocumentManagement.CustomFields;
using Domain.DocumentManagement.DocumentNote;
using Domain.DocumentManagement.DocumentTypes;
using Domain.DocumentManagement.StoragePath;
using Domain.DocumentManagement.tags;
using Domain.DocumentManagement.Views;
using Domain.Documents;
using Domain.FileTasks;
using Domain.Logs;
using Domain.Templates;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Persistence
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<DocumentTags> DocumentTags { get; set; }
        public DbSet<CustomField> CustomFields { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<Correspondent> Correspondents { get; set; }
        public DbSet<DocumentNote> DocumentNotes { get; set; }
        public DbSet<StoragePath> StoragePaths { get; set; }       
        public DbSet<Template> Templates { get; set; }
        public DbSet<FileTasks> FileTasks { get; set; }
        public DbSet<Logs> Logs { get; set; }
        public DbSet<View> Views { get; set; }
        public DbSet<ArchiveSerialNumbers> ArchiveSerialNumbers { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //Document Tags
            modelBuilder.Entity<DocumentTags>(x => x.HasKey(dt => new { dt.DocumentId, dt.TagId }));
            modelBuilder.Entity<DocumentTags>().HasOne(dt => dt.Tag).WithMany(t=>t.Documents).HasForeignKey(dt=>dt.TagId);
            modelBuilder.Entity<DocumentTags>().HasOne(dt => dt.Document).WithMany(d => d.Tags).HasForeignKey(dt => dt.DocumentId);
            //Document Custom Field
            modelBuilder.Entity<DocumentCustomField>(x => x.HasKey(dt => new { dt.DocumentId, dt.CustomFieldId }));
            modelBuilder.Entity<DocumentCustomField>().HasOne(dt => dt.CustomField).WithMany(t => t.DocumentsCustomFields).HasForeignKey(dt => dt.CustomFieldId);
            modelBuilder.Entity<DocumentCustomField>().HasOne(dt => dt.Document).WithMany(d => d.DocumentsCustomFields).HasForeignKey(dt => dt.DocumentId);
            //Document Types
            modelBuilder.Entity<Document>().HasOne(d => d.Document_Type).WithMany(dt => dt.Documents).HasForeignKey(d => d.DocumentTypeId).OnDelete(DeleteBehavior.SetNull);
            //Document Correspondents
            modelBuilder.Entity<Document>().HasOne(d => d.Correspondent).WithMany(dt => dt.Documents).HasForeignKey(d => d.CorrespondentId).OnDelete(DeleteBehavior.SetNull);
            ////one to many relationship storagepath
            modelBuilder.Entity<Document>().HasOne(d => d.StoragePath).WithMany(sp => sp.Documents).HasForeignKey(d => d.StoragePathId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Document>()
            .HasMany(d => d.Notes)
            .WithOne(n => n.Document)
            .HasForeignKey(n => n.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
                modelBuilder.Entity<ArchiveSerialNumbers>(x => x.HasKey(a => a.GroupId));
        
        }
    }
}
