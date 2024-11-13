namespace Application.Dtos.GroupDTO
{
    public class GroupDto
    {
        public string Name { get; set; }

        public IList<string>? Permissions { get; set; }
    }
}