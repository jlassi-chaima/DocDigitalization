
namespace Application.Dtos.BulkEdit
{
    public class BulkEdit
    {
        public Guid[]? Documents { get; set; }
        public string? Method { get; set; }
        public dynamic? Parameters { get; set; }
    }
}
