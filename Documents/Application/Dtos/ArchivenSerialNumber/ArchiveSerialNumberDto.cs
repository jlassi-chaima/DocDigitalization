﻿

namespace Application.Dtos.ArchivenSerialNumber
{
    public class ArchiveSerialNumberDto
    {
 
        public required string Prefix { get; set; }
        public required string GroupName { get; set; }
        public required Guid GroupId { get; set; }
        public required DateOnly Year { get; set; }
        public required string Owner { get; set; }
        public int? DocumentCount { get; set; }

    }
}
