namespace Domain.User
{
    public enum PermissionType
    {

        Document = 0,       // Represents the type of a document
        Tag = 1,            // Represents the type of a tag
        Correspondent = 2,  // Represents the type of a correspondent
        DocumentType = 3,   // Represents the type of a document type
        StoragePath = 4,    // Represents the type of a storage path
        SavedView = 5,      // Represents the type of a saved view
        PaperlessTask = 6,  // Represents the type of a paperless task
        UISettings = 7,     // Represents the type of UI settings
        Note = 8,           // Represents the type of a note
        MailAccount = 9,    // Represents the type of a mail account
        MailRule = 10,      // Represents the type of a mail rule
        User = 11,          // Represents the type of a user
        Group = 12,         // Represents the type of a group
        Admin = 13,         // Represents the type of an admin
        ShareLink = 14,     // Represents the type of a share link
        ConsumptionTemplate = 15, // Represents the type of a consumption template
        CustomField = 16    // Represents the type of a custom field
    }
}