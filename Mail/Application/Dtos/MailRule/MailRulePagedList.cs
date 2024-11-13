using Application.Dtos.MailAccount;
using Domain.MailRules.Enum;


namespace Application.Dtos.MailRule
{
    public class MailRulePagedList
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public MailAccountId Account { get; set; }

        public string Folder { get; set; }

        public long Maximum_age { get; set; }

        public MailFilterAttachmentType Attachment_type { get; set; }

        public MailRuleConsumptionScope Consumption_scope { get; set; }

        public long Order { get; set; }

        public string? Filter_from { get; set; }

        public string? Filter_to { get; set; }

        public string? Filter_subject { get; set; }

        public string? Filter_body { get; set; }

        public string? Filter_attachment_filename { get; set; }

        public MailAction? Action { get; set; }

        public MailMetadataTitleOption? Assign_title_from { get; set; }

        public string? Action_parameter { get; set; }

        public List<Guid>? Assign_tags { get; set; } // PaperlessTag.id
        public Guid? Assign_document_type { get; set; } // PaperlessDocumentType.id
        public MailMetadataCorrespondentOption? Assign_correspondent_from { get; set; }
        public Guid? Assign_correspondent { get; set; } // PaperlessCorrespondent.id
        public bool? Assign_owner_from_rule { get; set; }

    }
}
