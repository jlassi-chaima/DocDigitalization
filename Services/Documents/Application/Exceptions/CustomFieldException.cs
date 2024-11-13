using Core.Exceptions;
using System.Net;


namespace Application.Exceptions
{
    public class CustomFieldException : CustomException
    {
        public CustomFieldException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
        {
        }
    }
}
