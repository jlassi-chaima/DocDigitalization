
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Domain.User
{
   

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Permissions
    {
        // Represents no permission

        // Document permissions
        [JsonProperty("add_document")]
        AddDocument , //(int)PermissionAction.Add + ((int)PermissionType.Document * 4),// 0 + (0 * 4) = 0

        [JsonProperty("view_document")]
        ViewDocument = (int)PermissionAction.View + ((int)PermissionType.Document * 4),// 1 + (0 * 4) = 1

        [JsonProperty("change_document")]
        ChangeDocument = (int)PermissionAction.Change + ((int)PermissionType.Document * 4),// 2 + (0 * 4) = 2

        [JsonProperty("delete_document")]
        DeleteDocument = (int)PermissionAction.Delete + ((int)PermissionType.Document * 4), // 3 + (0 * 4) = 3

        // Tag permissions
        [JsonProperty("add_tag")]
        AddTag = (int)PermissionAction.Add + ((int)PermissionType.Tag * 4),

        [JsonProperty("view_tag")]
        ViewTag = (int)PermissionAction.View + ((int)PermissionType.Tag * 4),

        [JsonProperty("change_tag")]
        ChangeTag = (int)PermissionAction.Change + ((int)PermissionType.Tag * 4),

        [JsonProperty("delete_tag")]
        DeleteTag = (int)PermissionAction.Delete + ((int)PermissionType.Tag * 4),

        // Correspondent permissions
        [JsonProperty("add_correspondent")]
        AddCorrespondent = (int)PermissionAction.Add + ((int)PermissionType.Correspondent * 4),

        [JsonProperty("view_correspondent")]
        ViewCorrespondent = (int)PermissionAction.View + ((int)PermissionType.Correspondent * 4),

        [JsonProperty("change_correspondent")]
        ChangeCorrespondent = (int)PermissionAction.Change + ((int)PermissionType.Correspondent * 4),

        [JsonProperty("delete_correspondent")]
        DeleteCorrespondent = (int)PermissionAction.Delete + ((int)PermissionType.Correspondent * 4),

        // DocumentType permissions
        [JsonProperty("add_document_type")]
        AddDocumentType = (int)PermissionAction.Add + ((int)PermissionType.DocumentType * 4),

        [JsonProperty("view_document_type")]
        ViewDocumentType = (int)PermissionAction.View + ((int)PermissionType.DocumentType * 4),

        [JsonProperty("change_document_type")]
        ChangeDocumentType = (int)PermissionAction.Change + ((int)PermissionType.DocumentType * 4),

        [JsonProperty("delete_document_type")]
        DeleteDocumentType = (int)PermissionAction.Delete + ((int)PermissionType.DocumentType * 4),

        // StoragePath permissions
        [JsonProperty("add_storage_path")]
        AddStoragePath = (int)PermissionAction.Add + ((int)PermissionType.StoragePath * 4),

        [JsonProperty("view_storage_path")]
        ViewStoragePath = (int)PermissionAction.View + ((int)PermissionType.StoragePath * 4),

        [JsonProperty("change_storage_path")]
        ChangeStoragePath = (int)PermissionAction.Change + ((int)PermissionType.StoragePath * 4),

        [JsonProperty("delete_storage_path")]
        DeleteStoragePath = (int)PermissionAction.Delete + ((int)PermissionType.StoragePath * 4),

        // SavedView permissions
        [JsonProperty("add_saved_view")]
        AddSavedView = (int)PermissionAction.Add + ((int)PermissionType.SavedView * 4),

        [JsonProperty("view_saved_view")]
        ViewSavedView = (int)PermissionAction.View + ((int)PermissionType.SavedView * 4),

        [JsonProperty("change_saved_view")]
        ChangeSavedView = (int)PermissionAction.Change + ((int)PermissionType.SavedView * 4),

        [JsonProperty("delete_saved_view")]
        DeleteSavedView = (int)PermissionAction.Delete + ((int)PermissionType.SavedView * 4),

        // PaperlessTask permissions
        [JsonProperty("add_paperless_task")]
        AddPaperlessTask = (int)PermissionAction.Add + ((int)PermissionType.PaperlessTask * 4),

        [JsonProperty("view_paperless_task")]
        ViewPaperlessTask = (int)PermissionAction.View + ((int)PermissionType.PaperlessTask * 4),

        [JsonProperty("change_paperless_task")]
        ChangePaperlessTask = (int)PermissionAction.Change + ((int)PermissionType.PaperlessTask * 4),

        [JsonProperty("delete_paperless_task")]
        DeletePaperlessTask = (int)PermissionAction.Delete + ((int)PermissionType.PaperlessTask * 4),

        // UISettings permissions
        [JsonProperty("add_ui_settings")]
        AddUISettings = (int)PermissionAction.Add + ((int)PermissionType.UISettings * 4),

        [JsonProperty("view_ui_settings")]
        ViewUISettings = (int)PermissionAction.View + ((int)PermissionType.UISettings * 4),

        [JsonProperty("change_ui_settings")]
        ChangeUISettings = (int)PermissionAction.Change + ((int)PermissionType.UISettings * 4),

        [JsonProperty("delete_ui_settings")]
        DeleteUISettings = (int)PermissionAction.Delete + ((int)PermissionType.UISettings * 4),

        // Note permissions
        [JsonProperty("add_note")]
        AddNote = (int)PermissionAction.Add + ((int)PermissionType.Note * 4),

        [JsonProperty("view_note")]
        ViewNote = (int)PermissionAction.View + ((int)PermissionType.Note * 4),

        [JsonProperty("change_note")]
        ChangeNote = (int)PermissionAction.Change + ((int)PermissionType.Note * 4),

        [JsonProperty("delete_note")]
        DeleteNote = (int)PermissionAction.Delete + ((int)PermissionType.Note * 4),

        // MailAccount permissions
        [JsonProperty("add_mail_account")]
        AddMailAccount = (int)PermissionAction.Add + ((int)PermissionType.MailAccount * 4),

        [JsonProperty("view_mail_account")]
        ViewMailAccount = (int)PermissionAction.View + ((int)PermissionType.MailAccount * 4),

        [JsonProperty("change_mail_account")]
        ChangeMailAccount = (int)PermissionAction.Change + ((int)PermissionType.MailAccount * 4),

        [JsonProperty("delete_mail_account")]
        DeleteMailAccount = (int)PermissionAction.Delete + ((int)PermissionType.MailAccount * 4),

        // MailRule permissions
        [JsonProperty("add_mail_rule")]
        AddMailRule = (int)PermissionAction.Add + ((int)PermissionType.MailRule * 4),

        [JsonProperty("view_mail_rule")]
        ViewMailRule = (int)PermissionAction.View + ((int)PermissionType.MailRule * 4),

        [JsonProperty("change_mail_rule")]
        ChangeMailRule = (int)PermissionAction.Change + ((int)PermissionType.MailRule * 4),

        [JsonProperty("delete_mail_rule")]
        DeleteMailRule = (int)PermissionAction.Delete + ((int)PermissionType.MailRule * 4),

        // User permissions
        [JsonProperty("add_user")]
        AddUser = (int)PermissionAction.Add + ((int)PermissionType.User * 4),

        [JsonProperty("view_user")]
        ViewUser = (int)PermissionAction.View + ((int)PermissionType.User * 4),

        [JsonProperty("change_user")]
        ChangeUser = (int)PermissionAction.Change + ((int)PermissionType.User * 4),

        [JsonProperty("delete_user")]
        DeleteUser = (int)PermissionAction.Delete + ((int)PermissionType.User * 4),

        // Group permissions
        [JsonProperty("add_group")]
        AddGroup = (int)PermissionAction.Add + ((int)PermissionType.Group * 4),

        [JsonProperty("view_group")]
        ViewGroup = (int)PermissionAction.View + ((int)PermissionType.Group * 4),

        [JsonProperty("change_group")]
        ChangeGroup = (int)PermissionAction.Change + ((int)PermissionType.Group * 4),

        [JsonProperty("delete_group")]
        DeleteGroup = (int)PermissionAction.Delete + ((int)PermissionType.Group * 4),

        // Admin permissions
        [JsonProperty("add_admin")]
        AddAdmin = (int)PermissionAction.Add + ((int)PermissionType.Admin * 4),

        [JsonProperty("view_admin")]
        ViewAdmin = (int)PermissionAction.View + ((int)PermissionType.Admin * 4),

        [JsonProperty("change_admin")]
        ChangeAdmin = (int)PermissionAction.Change + ((int)PermissionType.Admin * 4),

        [JsonProperty("delete_admin")]
        DeleteAdmin = (int)PermissionAction.Delete + ((int)PermissionType.Admin * 4),

        // ShareLink permissions
        [JsonProperty("add_share_link")]
        AddShareLink = (int)PermissionAction.Add + ((int)PermissionType.ShareLink * 4),

        [JsonProperty("view_share_link")]
        ViewShareLink = (int)PermissionAction.View + ((int)PermissionType.ShareLink * 4),

        [JsonProperty("change_share_link")]
        ChangeShareLink = (int)PermissionAction.Change + ((int)PermissionType.ShareLink * 4),


        [JsonProperty("delete_share_link")]
        DeleteShareLink = (int)PermissionAction.Delete + ((int)PermissionType.ShareLink * 4),

        // ConsumptionTemplate permissions
        [JsonProperty("add_consumption_template")]
        AddConsumptionTemplate = (int)PermissionAction.Add + ((int)PermissionType.ConsumptionTemplate * 4),
        [JsonProperty("view_consumption_template")]
        ViewConsumptionTemplate = (int)PermissionAction.View + ((int)PermissionType.ConsumptionTemplate * 4),

        [JsonProperty("change_consumption_template")]
        ChangeConsumptionTemplate = (int)PermissionAction.Change + ((int)PermissionType.ConsumptionTemplate * 4),

        [JsonProperty("delete_consumption_template")]
        DeleteConsumptionTemplate = (int)PermissionAction.Delete + ((int)PermissionType.ConsumptionTemplate * 4),

        // CustomField permissions
        [JsonProperty("add_custom_field")]
        AddCustomField = (int)PermissionAction.Add + ((int)PermissionType.CustomField * 4),

        [JsonProperty("view_custom_field")]
        ViewCustomField = (int)PermissionAction.View + ((int)PermissionType.CustomField * 4),

        [JsonProperty("change_custom_field")]
        ChangeCustomField = (int)PermissionAction.Change + ((int)PermissionType.CustomField * 4),

        [JsonProperty("delete_custom_field")]
        DeleteCustomField = (int)PermissionAction.Delete + ((int)PermissionType.CustomField * 4)

    }
}