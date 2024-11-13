
namespace Application.Features.FeaturesDocument.DocToSharePoint
{
    public interface  IGraphApiUseCase
    {
        public Task<string> AddDocumentToList(Guid id);
    }
}
