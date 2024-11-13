using Application.Respository;
using Domain.Logs;
using MediatR;


namespace Application.Features.FeaturesLogs
{
    public class ListLogs
    {
        public sealed record Query : IRequest<List<string>>
        {

        }
        public sealed class Handler : IRequestHandler<Query, List<string>>
        {
            private readonly ILogRepository _repository;

            public Handler(ILogRepository repository)
            {
                _repository = repository;
            }

            public async Task<List<string>> Handle(Query request, CancellationToken cancellationToken)
            {

                List<string> result = new List<string>();

                foreach (LogName logName in Enum.GetValues(typeof(LogName)))
                {
                    result.Add(logName.ToString());
                }

                return result;
              
            }
        }
    }
}
