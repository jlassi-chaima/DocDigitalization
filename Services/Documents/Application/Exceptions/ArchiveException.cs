using Core.Exceptions;
using System.Net;

namespace Application.Exceptions
{
    public class ArchiveException : CustomException
    {
        public ArchiveException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
        {
        }
    }
}
