using Application.Respository;
using Application.Services;
using Domain.Logs;
using Serilog;


namespace Infrastructure.Services
{
    public class LogService: ILogService
    {
        private readonly ILogRepository _logRepository;
        public LogService(ILogRepository logRepository)
        {
            _logRepository = logRepository;
        }
        public async Task AddLogs(LogLevel logLevel, LogName logName, string message)
        {
            try
            {
                Logs archiveSerialNumberLog = Logs.Create(logLevel, logName, message);
                await _logRepository.AddAsync(archiveSerialNumberLog);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw new Exception(ex.Message);
            }
        }
    }
}
