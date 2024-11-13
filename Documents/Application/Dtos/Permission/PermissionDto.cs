

using System.Text.Json.Serialization;

namespace Application.Dtos.Permission
{
    public class PermissionDto
    {
        [JsonPropertyName("view")]
        public UserGroupPermission? View { get; set; }=new UserGroupPermission();
        [JsonPropertyName("change")]
        public UserGroupPermission? Change { get; set; }=new UserGroupPermission();
    }
}
