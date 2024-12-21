using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace VanillaBot;

public static class ConfigLoader
{
    private static readonly string ConfigPath = Path.Combine("data", "config.json");

    // Метод для загрузки конфигурации
    public static async Task<Config> LoadConfigAsync()
    {
        //Создание рыбного конфига при первом запуске бота
        if (!File.Exists(ConfigPath))
        {
            Console.WriteLine("Конфиг не найден, создаем новый...");
            var defaultConfig = new Config
            {
                Token = "YOUR_TOKEN_HERE",
                GuildId = 123456789012345678,
                AWarnsChannelId = 123456789012345678,
                ReportChannelId = 123456789012345678,
                RoleManagementPermissions = new Dictionary<ulong, List<ulong>>
                {
                    { 111111111111111111, new List<ulong> { 222222222222222222, 333333333333333333 } }
                },
                RolePermissions = new Dictionary<ulong, List<ulong>>
                {
                    { 111111111111111111, new List<ulong> { 222222222222222222, 333333333333333333 } }
                }
            };
            await SaveConfigAsync(defaultConfig); 
            return defaultConfig;
        }

        var json = await File.ReadAllTextAsync(ConfigPath);
        var config = JsonConvert.DeserializeObject<Config>(json);

        return config ?? throw new InvalidOperationException("Ошибка загрузки конфигурации.");
    }

    public static async Task SaveConfigAsync(Config config)
    {
        Directory.CreateDirectory("data");
        var json = JsonConvert.SerializeObject(config, Formatting.Indented);
        await File.WriteAllTextAsync(ConfigPath, json);
    }
}

public class Config
{
    public required string Token { get; set; }
    public required ulong GuildId { get; set; }
    public required ulong AWarnsChannelId { get; set; }
    public required ulong ReportChannelId { get; set; } 
    public Dictionary<ulong, List<ulong>> RolePermissions { get; set; } = new();
    public Dictionary<ulong, List<ulong>> RoleManagementPermissions { get; set; } = new();
}
