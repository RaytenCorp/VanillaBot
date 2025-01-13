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
                BDpath = "Path to auth.json",
                GuildId = 123456789012345678,
                AWarnsChannelId = 123456789012345678,
                HelpChannelId = 123456789012345678,
                SanctionChannelID = 123456789012345678,
                ReportChannelId = 123456789012345678,
                PhotocardsChannelId = 1236387986078302280,
                VideoChannelId = 123456789012345678,
                AudioChannelId = 123456789012345678,
                MemesChannelId = 123456789012345678,
                DrawingChannelId = 123456789012345678,
                QuentaChannelId = 123456789012345678,
                ReviewsChannelId = 123456789012345678,
                HeadHuntChannelId = 123456789012345678,
                TimeChannelId = 123456789012345678,
                SS14ReportsChannelId = 123456789012345678,
                AppealChannelId = 123456789012345678,
                AdminReportsChannelId = 123456789012345678,
                interrogationChannelId = 123456789012345678,
                arrivalChannelId = 123456789012345678,
                roleselectChannelId = 123456789012345678,
                EventReportChannelId = 123456789012345678,
                RoleManagementPermissions = new Dictionary<ulong, List<ulong>>
                {
                    { 111111111111111111, new List<ulong> { 222222222222222222, 333333333333333333 } }
                },
                RolePermissions = new Dictionary<ulong, List<ulong>>
                {
                    { 111111111111111111, new List<ulong> { 222222222222222222, 333333333333333333 } }
                },
                RoleSanctionPermissions = new List<ulong> { 111111111111111111, 222222222222222222 },
                MuteRoleID = 123456789012345678,
                PoopRoleID = 123456789012345678,
                AuthRoleID = 123456789012345678,
                NotAuthRoleID = 123456789012345678,
                NewsRoleID = 123456789012345678,
                EventsRoleID = 123456789012345678,
                HighPopRoleID = 123456789012345678,
                HOSTRoleID = 123456789012345678
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
    public required string BDpath { get; set; }
    public required ulong GuildId { get; set; }
    public required ulong AWarnsChannelId { get; set; }
    public required ulong SanctionChannelID { get; set; }
    public required ulong ReportChannelId { get; set; } 
    public required ulong PhotocardsChannelId { get; set; } 
    public required ulong HelpChannelId { get; set; } 
    public required ulong VideoChannelId { get; set; } 
    public required ulong AudioChannelId { get; set; } 
    public required ulong MemesChannelId { get; set; } 
    public required ulong DrawingChannelId { get; set; } 
    public required ulong QuentaChannelId { get; set; } 
    public required ulong ReviewsChannelId { get; set; } 
    public required ulong HeadHuntChannelId { get; set; } 
    public required ulong TimeChannelId { get; set; } 
    public required ulong SS14ReportsChannelId { get; set; } 
    public required ulong AppealChannelId { get; set; } 
    public required ulong AdminReportsChannelId { get; set; } 
    public required ulong interrogationChannelId { get; set; } 
    public required ulong arrivalChannelId { get; set; } 
    public required ulong roleselectChannelId { get; set; } 
    public required ulong EventReportChannelId { get; set; } 
    public Dictionary<ulong, List<ulong>> RolePermissions { get; set; } = new();
    public Dictionary<ulong, List<ulong>> RoleManagementPermissions { get; set; } = new();
    public List<ulong> RoleSanctionPermissions { get; set; } = new();
    public required ulong PoopRoleID { get; set; }
    public required ulong MuteRoleID { get; set; }
    public required ulong AuthRoleID { get; set; }
    public required ulong NotAuthRoleID { get; set; }
    //анонсы
    public required ulong NewsRoleID { get; set; }
    public required ulong EventsRoleID { get; set; }
    public required ulong HighPopRoleID { get; set; }
    //Роли
    public required ulong HOSTRoleID { get; set; }
}
