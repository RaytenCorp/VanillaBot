using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;

namespace VanillaBot;

public class RoleUpdateHandler
{
    private readonly DiscordSocketClient _client;
    private readonly Config _config;

    public RoleUpdateHandler(DiscordSocketClient client, Config config)
    {
        _client = client;
        _config = config;
    }

    public void Initialize()
    {
        _client.GuildMemberUpdated += HandleRoleChange;
        Console.WriteLine("RoleUpdateHandler инициализирован.");
    }

    private async Task HandleRoleChange(Cacheable<SocketGuildUser, ulong> before, SocketGuildUser after)
    {
        // Словарь соответствия ролей и деняк
        Dictionary<SponsorRank, decimal> roleToRoyalty = new Dictionary<SponsorRank, decimal>
        {
            { SponsorRank.None, 0},
            { SponsorRank.GrayTide, 200m},
            { SponsorRank.Revolutionary, 400m},
            { SponsorRank.Syndicate, 800m},
            { SponsorRank.SpaceNinja, 1200m}
        };

        var discordId = after.Id.ToString();
        Console.WriteLine($"Обработка изменений ролей для пользователя {after.Username}");

        // Получаем состояние до и после
        var beforeUser = await before.GetOrDownloadAsync();
        SponsorRank newRank = GetSponsorHighestRank(after.Roles);
        SponsorRank oldRank = GetSponsorHighestRank(beforeUser.Roles);

        // Если ранг не изменился — выходим
        if (newRank == oldRank)
        {
            Console.WriteLine($"Высший ранг остался таким же ({newRank}). Уходим");
            return;
        }

        // Читаем БД
        var sponsorData = JObject.Parse(await File.ReadAllTextAsync(_config.SponsorBDpath));

        // Получаем SSID
        string? ssid = await GetSSID(discordId);
        if (ssid == null)
        {
            Console.WriteLine($"У {after.Username} не найден Space Station ID");
            return;
        }

        // Устанавливаем роялти
        await RoyaltyManager.SetProfitAsync(ssid, roleToRoyalty[newRank]);

        // Если подписка закончилась
        if (newRank == SponsorRank.None)
        {
            if (sponsorData.Remove(ssid))
            {
                Console.WriteLine($"У пользователя {after.Username} закончилась подписка.");
                announce(
                    $"У пользователя <@{after.Id}> **({oldRank})** закончилась подписка.\n **минус** {roleToRoyalty[oldRank]} :c",
                    new Color(255, 255, 255)  // белый
                );
            }
            else
            {
                Console.WriteLine($"{ssid} НЕ ПОЛУЧИЛОСЬ УДАЛИТЬ!");
                announce(
                    $"У пользователя <@{after.Id}> закончилась подписка.\n**Но удалить не получилось!!!**",
                    new Color(255, 255, 255)
                );
            }
        }
        else
        {
            sponsorData[ssid] = newRank.ToString();
            announce(
                $"У пользователя <@{after.Id}> изменился уровень подписки:\n**{oldRank} → {newRank}**\n **Дэньги:** {roleToRoyalty[newRank]}",
                GetColor(newRank)
            );
        }



        await File.WriteAllTextAsync(_config.SponsorBDpath, sponsorData.ToString());
    }

    private Color GetColor(SponsorRank rank)
    {
        return rank switch
        {
            SponsorRank.GrayTide => new Color(128, 128, 128),      // Серый
            SponsorRank.Revolutionary => new Color(128, 0, 128),   // Фиолетовый
            SponsorRank.Syndicate => new Color(255, 0, 0),         // Красный
            SponsorRank.SpaceNinja => new Color(0, 255, 0),        // Зелёный
            SponsorRank.None => new Color(255, 255, 255),          // Белый
            _ => new Color(255, 255, 255)
        };
    }

    private async void announce(string message, Color color)
    {
        try
        {
            var logChannel = _client.GetChannel(_config.HostChannelID) as IMessageChannel;
            if (logChannel == null)
            {
                Console.WriteLine("Не удалось найти лог-канал.");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("Обновление спонсорского ранга")
                .WithDescription(message)
                .WithColor(color)
                .WithCurrentTimestamp()
                .Build();

            await logChannel.SendMessageAsync(embed: embed);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при отправке лог-сообщения: {ex.Message}");
        }
    }
    private SponsorRank GetSponsorHighestRank(IEnumerable<SocketRole> roles)
    {
        Dictionary<ulong, SponsorRank> roleToRank = new Dictionary<ulong, SponsorRank>
        {
            { _config.GrayTide, SponsorRank.GrayTide },
            { _config.Revolutionary, SponsorRank.Revolutionary },
            { _config.Syndicate, SponsorRank.Syndicate },
            { _config.SpaceNinja, SponsorRank.SpaceNinja }
        };

        SponsorRank highestRank = SponsorRank.None;
        foreach (var role in roles)
        {
            if (roleToRank.TryGetValue(role.Id, out var rank))
            {
                if (rank > highestRank)
                    highestRank = rank;
            }
        }
        return highestRank;
    }

    private async Task<string?> GetSSID(string discordId)
    {
        if (_config == null || string.IsNullOrWhiteSpace(_config.BDpath) || string.IsNullOrWhiteSpace(_config.SponsorBDpath))
        {
            Console.WriteLine("Ошибка: Конфигурация не задана или пути не указаны.");
            return null;
        }
        //читаем бдшки
        var authData = JObject.Parse(await File.ReadAllTextAsync(_config.BDpath));
        var userEntry = authData[discordId];
        if (userEntry == null)
        {
            Console.WriteLine($"Пользователь с Discord ID {discordId} не найден в auth.json");
            return null;
        }
        return userEntry["cikey"]?.ToString();
    }
    public enum SponsorRank
    {
        None = 1,
        GrayTide = 2,
        Revolutionary = 3,
        Syndicate = 4,
        SpaceNinja = 5,
    }
}
