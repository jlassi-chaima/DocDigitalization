using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options
{
    public class AppOptions : IOptionsRoot
    {
        
        public string Name { get; set; } = "Service";
    }
}
