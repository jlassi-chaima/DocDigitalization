using Core.Exceptions;
using System.Net;


namespace Application.Exceptions
{
    public class ShareFolderException : CustomException
    {
        public ShareFolderException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
        {
        }
    }
}
