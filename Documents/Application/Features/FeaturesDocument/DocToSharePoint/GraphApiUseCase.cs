using Application.Helper;
using Application.Respository;
using Domain.Documents;
using Domain.Ports;
using Serilog;


namespace Application.Features.FeaturesDocument.DocToSharePoint
{
    public class GraphApiUseCase(IDocumentRepository documentRepository,
        IGraphApiPort graphPort) : IGraphApiUseCase
    {
        public async Task<string> AddDocumentToList(Guid id)
        {
            try
            {

                    Document doc= await documentRepository.FindByIdAsync(id);
                    if (doc != null)
                    {
                           
                        await graphPort.AddDocumentToList(doc);
                            
                     }
                return "Document uploaded successfully.";
            }

            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw new Exception("An error has occured, please try again later");
            }
        }
    }
}
