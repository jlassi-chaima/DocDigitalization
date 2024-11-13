using Core.Exceptions;
using System.Net;

namespace Application.Exceptions
{
    public class TagException: CustomException
    {
        public TagException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
        {
        }
    }
}
