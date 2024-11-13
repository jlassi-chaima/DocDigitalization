

using Core.Domain;
using Domain.DocumentManagement.CustomFields;
using Domain.DocumentManagement.DocumentTypes;
using System.ComponentModel.DataAnnotations;

namespace Domain.Documents
{
    public class ArchiveSerialNumbers : BaseEntity
    {
       
        public required string Prefix { get; set; }
        public required string GroupName { get; set; }
        public required string Owner { get; set; }
        public  int DocumentCount { get; set; }
         
        public required Guid GroupId { get; set; }
        public required DateOnly Year { get; set; }
        public string GetFormattedYear() => Year.ToString("dd-MM-yyyy");
        public static ArchiveSerialNumbers Create(
        string prefix,
        string groupName,
        Guid groupId,
        DateOnly year,
        string owner


           )
        {
            ArchiveSerialNumbers archive = new ArchiveSerialNumbers()
            {
                Prefix = prefix,
                GroupName = groupName,
                GroupId = groupId,
                Year = year,
                Owner=owner 
            };
            var @event = new ArchiveSerialNumberEvent(archive.Id, archive.Prefix,archive.Year);
            archive.AddDomainEvent(@event);
            return archive;
        }


    }
}
