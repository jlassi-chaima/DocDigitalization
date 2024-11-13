using Core.Exceptions;
using System.Net;


namespace Application.Exceptions
{
    public class FileTasksException : CustomException
    {
        public FileTasksException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
        {
        }
    }
}
