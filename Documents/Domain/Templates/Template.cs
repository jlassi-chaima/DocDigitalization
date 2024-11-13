using Core.Domain;
using Domain.DocumentManagement;
using Domain.DocumentManagement.tags;
using Domain.Templates.Enum;


namespace Domain.Templates
{
    public class Template : BaseEntity
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public List<DocumentSource>? Sources { get; set; }
        public string? FilterFilename { get; set; }

        public string? Owner { get; set; }
        public string? FilterPath { get; set; }
        public string? FilterMailrule { get; set; } // PaperlessMailRule.id
        public string? AssignTitle { get; set; }
        public List<Guid>? AssignTags { get; set; } // PaperlessTag.id
        public Guid? AssignDocumentType { get; set; } // PaperlessDocumentType.id
        public Guid? AssignCorrespondent { get; set; } // PaperlessCorrespondent.id
        public Guid? AssignStoragePath { get; set; } // PaperlessStoragePath.id
        public GetListByType Type { get; set; }
        public Matching_Algorithms? Content_matching_algorithm { get; set; }
        public List<string>? Content_matching_pattern { get; set; }
        public string? DocumentClassification { get; set; }
        public List<Guid>? Has_Tags { get; set; }
        public Guid? Has_Correspondent { get; set; }
        public Guid? Has_Document_Type { get; set; }
        public bool? Is_Insensitive { get; set; }
        public bool? Is_Enabled { get; set; }

        public List<string>? Assign_view_users { get; set; }
        public List<string>? Assign_view_groups { get; set; }
        public List<string>? Assign_change_users { get; set; }
        public List<string>? Assign_change_groups { get; set; }
    }
}
