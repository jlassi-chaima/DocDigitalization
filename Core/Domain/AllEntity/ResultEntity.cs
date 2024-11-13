using System.Text;

namespace Core.Domain.AllEntity
{
    public class ResultEntity
    {
        public ResultEntity(string? outCode, string? outMessage)
        {
            OutCode = outCode;
            OutMessage = outMessage;
        }
        public ResultEntity()
        {

        }
        public string? OutCode { get; set; }

        public string? OutMessage { get; set; }

        public override string ToString()
        {
            return new StringBuilder().AppendLine("OutCode is " + OutCode).AppendLine("OutMesssage is " + OutMessage).ToString();
        }
   
    }
}
