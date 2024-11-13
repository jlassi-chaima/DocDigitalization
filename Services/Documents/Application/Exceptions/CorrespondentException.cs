using Core.Exceptions;
using System.Net;


namespace Application.Exceptions
{
    public class CorrespondentException : CustomException
    {
        public CorrespondentException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
        {
        }
    }
}
