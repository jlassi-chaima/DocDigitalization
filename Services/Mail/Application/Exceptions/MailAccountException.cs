

using Core.Exceptions;
using System.Net;

namespace Application.Exceptions
{
    public class MailAccountException : CustomException
    {
        public MailAccountException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
        {
        }
    }
}
