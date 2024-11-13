
namespace Application.Dtos.View
{
    public class ViewsDto
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }

        public required DateTime StartDate { get; set; }

        public required DateTime EndDate { get; set; }

        public required Guid TagId { get; set; }
        public required string Owner { get; set; }
    }
}
