

using Domain.Documents;

namespace Domain.Ports
{
    public interface IGraphApiPort
    {
        public Task AddDocumentToList(Document document);
    }
}
