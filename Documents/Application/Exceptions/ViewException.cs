using Core.Exceptions;
using System.Net;

namespace Application.Exceptions
{
    public class ViewException : CustomException
    {
        public ViewException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
        {
        }
    }
}
