

using Core.Events;

namespace Domain.Documents
{
    public class ArchiveSerialNumberEvent : DomainEvent
    {
        public Guid ArchiveId { get; }
        public  string GroupName { get; set; }
        public  DateOnly Year { get; set; }

        public ArchiveSerialNumberEvent(Guid archiveId, string groupName , DateOnly year)
        {
            ArchiveId = archiveId;
            GroupName = groupName;
            GroupName = groupName;
            Year = year;
        }
    }
}
