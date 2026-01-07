using System.IO;
using System.Text.Json;
using TopBarDock.Models;

namespace TopBarDock.Services
{
    public static class ConfigService
    {
        static string Path => "config.json";

        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(Path))
                    return JsonSerializer.Deserialize<AppConfig>(
                        File.ReadAllText(Path)
                    )!;
            }
            catch { }

            return new AppConfig();
        }

        public static void Save(AppConfig cfg)
        {
            File.WriteAllText(
                "config.json",
                JsonSerializer.Serialize(cfg, new JsonSerializerOptions
                {
                    WriteIndented = true
                })
            );
        }
    }
}
