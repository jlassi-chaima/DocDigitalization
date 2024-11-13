using Application.Respository;
using Domain.FileTasks;
using MediatR;


namespace Application.Features.FeaturesFileTasks
{
    public class ListFileTasks
    {
        public sealed record Query : IRequest<List<FileTasks>>
        {


        }
        public sealed class Handler : IRequestHandler<Query, List<FileTasks>>
        {
            private readonly IFileTasksRepository _fileTasksRepository;

            public Handler(IFileTasksRepository fileTasksRepository)
            {
                _fileTasksRepository = fileTasksRepository;
            }

            public async Task<List<FileTasks>> Handle(Query request, CancellationToken cancellationToken)
            {
                return (List<FileTasks>)await _fileTasksRepository.GetAllAsync();
            }
        }
    }
}
