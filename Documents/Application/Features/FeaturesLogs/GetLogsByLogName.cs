using Application.Respository;
using Domain.Logs;
using MediatR;


namespace Application.Features.FeaturesLogs
{
    public class GetLogsByLogName
    {
        public sealed record Query : IRequest<List<string>>
        {
            public readonly string Source;
            public Query(string source)
            {
                Source = source;
            }
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
                LogName source = new LogName();
                if (request.Source == "DigitalWork")
                {
                    source = LogName.EasyDoc;
                }
                else
                {
                    source = LogName.Mail;
                }
                List<Logs> logs = await _repository.getLogsBySource(source);
                List<string> result = new List<string>();
                foreach (Logs log in logs)
                {
                    result.Add(log.Message);
                }
                return result;

            }
        }
    }
}
