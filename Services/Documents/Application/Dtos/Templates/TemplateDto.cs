using Application.Dtos.Permission;
using Domain.DocumentManagement;
using Domain.Templates.Enum;


namespace Application.Dtos.Templates
{
    public class TemplateDto
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public string? DocumentClassification { get; set; }
        public List<DocumentSource>? Sources { get; set; }
        public string? Filter_filename { get; set; }
        public string? Filter_path { get; set; }
        public string? Filter_mailrule { get; set; } // PaperlessMailRule.id
        public string? Assign_title { get; set; }
        public List<Guid>? Assign_tags { get; set; } // PaperlessTag.id
        public Guid? Assign_document_type { get; set; } // PaperlessDocumentType.id
        public Guid? Assign_correspondent { get; set; } // PaperlessCorrespondent.id
        public Guid? Assign_storage_path { get; set; } // PaperlessStoragePath.id
        public GetListByType Type { get; set; }
        public Matching_Algorithms? Matching_algorithm { get; set; }
        public List<string>? match { get; set; }
        public List<Guid>? Filter_has_tags { get; set; }
        public Guid? Filter_has_correspondent { get; set; }
        public Guid? Filter_has_document_type { get; set; }
        public string? Owner { get; set; }
        public bool? Is_insensitive { get; set; }
        public bool? Is_Enabled { get; set; }
        public List<string>? Assign_view_users { get; set; }
        public List<string>? Assign_view_groups { get; set; }
        public List<string>? Assign_change_users { get; set; }
        public List<string>? Assign_change_groups { get; set; }
    }
}
