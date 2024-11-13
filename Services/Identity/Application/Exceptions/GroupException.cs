using Core.Exceptions;
using System.Net;

namespace Application.Exceptions
{
    public class GroupException : CustomException
    {
        public GroupException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
        {
        }
    }
}
