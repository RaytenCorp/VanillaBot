using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;

namespace VanillaBot;
public static class AWarnManager
{
    private static readonly string FilePath = Path.Combine("data", "awarns.json");

    public static List<AwarnRecord> Awarns { get; private set; } = new();

    static AWarnManager()
    {

    }
    
    public static void Initialize()
    {
        LoadAwarns();
        RemoveExpiredAwarns();
        Console.WriteLine("AWarnManager инициализирован.");
    }

    public static void AddAwarn(ulong userId, string reason, AwarnType type, int id)
    {
        var awarn = new AwarnRecord
        {
            ID = id,
            UserId = userId,
            Reason = reason,
            Type = type,
            AwarnDate = DateTime.UtcNow
        };

        Awarns.Add(awarn);
        SaveAwarns();
    }
    public static bool RemAwarn(int id)
    {
        var AwarnToRemove = Awarns.FirstOrDefault(s => s.ID == id);
        
        if (AwarnToRemove != null)
        {
            Awarns.RemoveAll(s => s.ID == id);
            SaveAwarns();
            return true;
        }
        
        return false;
    }

    public static ulong? GetUserByAwarnID(int id)
    {
        var Awarn = Awarns.FirstOrDefault(s => s.ID == id);
        return Awarn?.UserId;
    }

    public static List<AwarnRecord> GetAwarnsForUser(ulong userId)
    {
        return Awarns.Where(s => s.UserId == userId).ToList();
    }

    public static float GetAwarnCounter(ulong userId)
    {
        var awarnList = GetAwarnsForUser(userId);
        float total = 0f;
        
        foreach (var awarn in awarnList)
        {
            switch (awarn.Type)
            {
                case AwarnType.HalfWarn:
                    total += 0.5f;
                    break;
                case AwarnType.FullWarn:
                    total += 1f;
                    break;
            }
        }
        
        return total;
    }


    public static void RemoveExpiredAwarns()
    {
        Awarns.RemoveAll(s =>
            s.AwarnDate.AddMonths(3) <= DateTime.UtcNow);

        SaveAwarns();
    }

    private static void LoadAwarns()
    {
        if (File.Exists(FilePath))
        {
            var json = File.ReadAllText(FilePath);
            Awarns = JsonConvert.DeserializeObject<List<AwarnRecord>>(json) ?? new List<AwarnRecord>();
        }
    }

    private static void SaveAwarns()
    {
        var directoryPath = Path.GetDirectoryName(FilePath);
        if (directoryPath != null)
        {
            Directory.CreateDirectory(directoryPath);
        }
        var json = JsonConvert.SerializeObject(Awarns, Formatting.Indented);
        File.WriteAllText(FilePath, json);
    }
}

public class AwarnRecord
{
    public int ID { get; set; }
    public ulong UserId { get; set; }
    public required string Reason { get; set; }
    public AwarnType Type { get; set; }
    public DateTime AwarnDate { get; set; }
}

public enum AwarnType
{
    FullWarn = 0,
    HalfWarn = 1
}
