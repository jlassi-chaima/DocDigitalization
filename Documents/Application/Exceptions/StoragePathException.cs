using Core.Exceptions;
using System.Net;


namespace Application.Exceptions
{
    public class StoragePathException : CustomException
    {
        public StoragePathException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
        {
        }
    }
}
