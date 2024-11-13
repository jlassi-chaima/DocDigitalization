using Core.Exceptions;
using System.Net;


namespace Application.Exceptions
{
    public class TemplateException : CustomException
    {
        public TemplateException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
        {
        }
    }
}
