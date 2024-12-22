using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;

namespace VanillaBot;
public static class SanctionManager
{
    private static readonly string FilePath = Path.Combine("data", "sanctions.json");
    private static readonly Timer SanctionCheckTimer = new(60 * 1000); // 1 минута

    public static List<SanctionRecord> Sanctions { get; private set; } = new();

    public static event Action<ulong> MuteExpired = delegate { };

    static SanctionManager()
    {

    }
    
    public static void Initialize()
    {
        LoadSanctions();
        RemoveExpiredSanctions();
        SanctionCheckTimer.Elapsed += (sender, e) => CheckForExpiredSanctions();
        SanctionCheckTimer.Start();
        Console.WriteLine("SanctionManager инициализирован.");
    }

    public static void AddSanction(ulong userId, string reason, SanctionType type, DateTime? muteExpiry = null)
    {
        var sanction = new SanctionRecord
        {
            UserId = userId,
            Reason = reason,
            Type = type,
            DateIssued = DateTime.UtcNow,
            MuteExpiry = type == SanctionType.Mute ? muteExpiry : null
        };

        Sanctions.Add(sanction);
        SaveSanctions();
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

    private static void CheckForExpiredSanctions()
    {
        var expiredSanctions = Sanctions.Where(s => s.Type == SanctionType.Mute && s.MuteExpiry.HasValue && s.MuteExpiry <= DateTime.UtcNow).ToList();
        foreach (var sanction in expiredSanctions)
        {
            sanction.Type = SanctionType.Warn;
            sanction.MuteExpiry = null;
            MuteExpired?.Invoke(sanction.UserId);
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
}

public class SanctionRecord
{
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
