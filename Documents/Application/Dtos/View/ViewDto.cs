
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.View
{
    public class ViewDto
    {
 
        [Required] 
        public required string Name { get; set; }
        [Required]
        public required DateTime StartDate { get; set; }
        [Required]
        public required DateTime EndDate { get; set; }
        [Required]
        public required Guid TagId { get; set; }
        public required string Owner { get; set; }

    }
}
