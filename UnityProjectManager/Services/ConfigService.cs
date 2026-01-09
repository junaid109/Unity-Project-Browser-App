using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using UnityProjectManager.Models;

namespace UnityProjectManager.Services
{
    public interface IConfigService
    {
        Task<AppConfig> LoadConfigAsync();
        Task SaveConfigAsync(AppConfig config);
    }

    public class ConfigService : IConfigService
    {
        private readonly string _configPath;

        public ConfigService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appData, "UnityProjectManager");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            _configPath = Path.Combine(folder, "config.json");
        }

        public async Task<AppConfig> LoadConfigAsync()
        {
            if (!File.Exists(_configPath)) return new AppConfig();

            try
            {
                var json = await File.ReadAllTextAsync(_configPath);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            catch
            {
                return new AppConfig();
            }
        }

        public async Task SaveConfigAsync(AppConfig config)
        {
            try
            {
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_configPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save config: {ex.Message}");
            }
        }
    }
}
