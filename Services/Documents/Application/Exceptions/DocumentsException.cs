using Core.Exceptions;
using System.Net;


namespace Application.Exceptions
{
    public class DocumentsException : CustomException
    {
        public DocumentsException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
        {
        }
    }
}
