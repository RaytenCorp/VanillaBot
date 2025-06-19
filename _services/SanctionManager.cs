using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Discord.WebSocket;
using Timer = System.Timers.Timer;
using System.Threading.Tasks;

namespace VanillaBot;

public static class SanctionManager
{
    private static readonly string FilePath = Path.Combine("data", "sanctions.json");
    private static readonly Timer SanctionCheckTimer = new(60 * 1000); // 1 минута

    public static List<SanctionRecord> Sanctions { get; private set; } = new();
    private static DiscordSocketClient? _client = null;
    private static Config? _config = null;

    static SanctionManager()
    {
    }

    public static void Initialize(Config config, DiscordSocketClient client)
    {
        LoadSanctions();
        RemoveExpiredSanctions();
        SanctionCheckTimer.Elapsed += async (sender, e) => await CheckForExpiredSanctions();
        SanctionCheckTimer.Start();
        _client = client;
        _config = config;
        Console.WriteLine("SanctionManager инициализирован.");
    }

    public static void AddSanction(ulong userId, string reason, SanctionType type, int id, DateTime? muteExpiry = null)
    {
        var sanction = new SanctionRecord
        {
            ID = id,
            UserId = userId,
            Reason = reason,
            Type = type,
            DateIssued = DateTime.UtcNow,
            MuteExpiry = type == SanctionType.Mute ? muteExpiry : null
        };

        Sanctions.Add(sanction);
        SaveSanctions();
    }
    public static async Task<bool> RemSanction(int id)
    {
        var sanctionToRemove = Sanctions.FirstOrDefault(s => s.ID == id);

        if (sanctionToRemove != null)
        {
            if (sanctionToRemove.Type == SanctionType.Mute)
            {
                await unmute(sanctionToRemove.UserId);
            }

            Sanctions.RemoveAll(s => s.ID == id);
            SaveSanctions();
            return true;
        }

        return false;
    }
    public static ulong? GetUserBySanctionID(int id)
    {
        var sanction = Sanctions.FirstOrDefault(s => s.ID == id);
        return sanction?.UserId;
    }


    public static List<SanctionRecord> GetSanctionsForUser(ulong userId)
    {
        return Sanctions.Where(s => s.UserId == userId).ToList();
    }

    public static void RemoveExpiredSanctions()
    {
        Sanctions.RemoveAll(s =>
            s.DateIssued.AddMonths(3) <= DateTime.UtcNow);

        SaveSanctions();
    }

    private static async Task CheckForExpiredSanctions()
    {
        var expiredSanctions = Sanctions.Where(s => s.Type == SanctionType.Mute && s.MuteExpiry.HasValue && s.MuteExpiry <= DateTime.UtcNow).ToList();
        foreach (var sanction in expiredSanctions)
        {
            sanction.Type = SanctionType.Warn;
            sanction.MuteExpiry = null;
            await unmute(sanction.UserId);
        }

        SaveSanctions();
    }


    private static void LoadSanctions()
    {
        if (File.Exists(FilePath))
        {
            var json = File.ReadAllText(FilePath);
            Sanctions = JsonConvert.DeserializeObject<List<SanctionRecord>>(json) ?? new List<SanctionRecord>();
        }
    }

    private static void SaveSanctions()
    {
        var directoryPath = Path.GetDirectoryName(FilePath);
        if (directoryPath != null)
        {
            Directory.CreateDirectory(directoryPath);
        }
        var json = JsonConvert.SerializeObject(Sanctions, Formatting.Indented);
        File.WriteAllText(FilePath, json);
    }

    private static async Task unmute(ulong userId)
    {
        if (_client == null || _config == null)
            return;

        var guild = _client.GetGuild(_config.GuildId);
        var muteRole = guild.GetRole(_config.MuteRoleID);



        var guildUser = guild.GetUser(userId);
        if (guildUser == null)
        {
            Console.WriteLine($"Хотели бы мы его размутить, но пользователя нет на сервере");
            return;
        }

        // Удаляем роль
        await guildUser.RemoveRoleAsync(muteRole);
    }
}

public class SanctionRecord
{
    public int ID { get; set; }
    public ulong UserId { get; set; }
    public required string Reason { get; set; }
    public SanctionType Type { get; set; }
    public DateTime DateIssued { get; set; }
    public DateTime? MuteExpiry { get; set; }
}

public enum SanctionType
{
    Mute = 0,
    Warn = 1
}
