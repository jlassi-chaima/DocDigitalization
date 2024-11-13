using Core.Exceptions;
using System.Net;


namespace Application.Exceptions
{
    public class UISettingsException : CustomException
    {
        public UISettingsException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
        {
        }
    }
}
