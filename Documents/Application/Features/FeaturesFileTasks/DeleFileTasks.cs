using Application.Respository;
using Domain.Documents;
using Domain.FileTasks;
using MediatR;



namespace Application.Features.FeaturesFileTasks
{
    public class DeleFileTasks
    {
        public sealed record Command : IRequest
        {
            public readonly Guid Id;
            public readonly string Document_owner;
            public readonly Guid Document_correspondent;
            public Command(Guid id, string document_owner, Guid document_correspondent)
            {
                Id = id;
                Document_owner = document_owner;
                Document_correspondent = document_correspondent;
            }
        }
        public sealed class Handler : IRequestHandler<Command>
        {
            private readonly IFileTasksRepository _repository;
            private readonly IDocumentRepository _documentRepository;
            private readonly ICorrespondentRepository _correspondentRepository;

            public Handler(IFileTasksRepository repository, IDocumentRepository documentRepository, ICorrespondentRepository correspondentRepository)
            {
                _repository = repository;
                _documentRepository = documentRepository;
                _correspondentRepository = correspondentRepository;
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                FileTasks filetask = _repository.FindByIdAsync(request.Id, cancellationToken).GetAwaiter().GetResult();
                Document filetaskdocument = _documentRepository.FindByIdAsync(filetask.Task_document.Id, cancellationToken).GetAwaiter().GetResult();

                if (filetaskdocument.CorrespondentId == null || filetaskdocument.Owner == null)
                {
                    if (filetaskdocument.CorrespondentId == null)
                    {

                        //verify owner  (user) exists
                        if (await _correspondentRepository.ExistsByIdAsync(request.Document_correspondent))
                        {

                            filetaskdocument.CorrespondentId = request.Document_correspondent;
                        }



                    }

                    if (filetaskdocument.Owner == null)
                    {
                        //verify owner  (user) exists
                        var responseBody = "";
                        var apiUrl = "http://localhost:5183/user/verify_user_exist";
                        apiUrl += "?id=" + Uri.EscapeDataString(request.Document_owner);
                        using (var httpClient = new HttpClient())
                        {
                            var response = await httpClient.GetAsync(apiUrl);
                            // Ensure the request completed successfully
                            response.EnsureSuccessStatusCode();
                            // Read the response content as a string
                            responseBody = await response.Content.ReadAsStringAsync();
                        }
                        if (responseBody == "true")
                        {
                            filetaskdocument.Owner = request.Document_owner;
                        }

                    }

                    await _documentRepository.UpdateAsync(filetaskdocument);


                }
                if (filetaskdocument.CorrespondentId != null && filetaskdocument.Owner != null)
                {
                    await _repository.DeleteByIdAsync(request.Id);
                }

            }
        }
    }
}
