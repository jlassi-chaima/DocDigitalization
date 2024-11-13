using Core.Database;
using Domain.Settings;


namespace Application.Repository
{
    public interface IUISettingsRepository : IRepository<UISettings, Guid>
    {
    }
}
