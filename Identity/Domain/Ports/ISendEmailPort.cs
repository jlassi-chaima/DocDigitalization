
namespace Domain.Ports
{
    public interface ISendEmailPort
    {
        public Task SendEmail(string email,string firstName, string subject, string body);
    }
}
