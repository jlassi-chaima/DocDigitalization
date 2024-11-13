using Core.Exceptions;
using System.Net;


namespace Application.Exceptions
{
    public class UserException : CustomException
    {
        public UserException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
        {
        }
    }
}
