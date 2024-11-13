using Domain.Logs;


namespace Application.Services
{
    public interface ILogService
    {
        Task AddLogs(LogLevel logLevel, LogName logName, string message);
    }
}
