using Core.Exceptions;
using System.Net;

namespace Application.Exceptions
{
    public class MailRuleException : CustomException
    {
        public MailRuleException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
        {
        }
    }
}