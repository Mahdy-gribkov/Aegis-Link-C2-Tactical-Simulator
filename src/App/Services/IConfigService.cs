using AegisLink.App.Models;

namespace AegisLink.App.Services;

public interface IConfigService
{
    AppConfig Load();
    void Save(AppConfig config);
}
