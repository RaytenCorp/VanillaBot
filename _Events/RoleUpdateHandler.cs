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

private async Task HandleRoleChange(Cacheable<SocketGuildUser, ulong> beforeCache, SocketGuildUser after)
{
    try
    {
        if (_config == null)
        {
            Console.WriteLine("Ошибка: _config == null");
            return;
        }

        if (string.IsNullOrWhiteSpace(_config.BDpath))
        {
            Console.WriteLine("Ошибка: Путь к auth.json (_config.BDpath) не указан");
            return;
        }

        if (string.IsNullOrWhiteSpace(_config.SponsorBDpath))
        {
            Console.WriteLine("Ошибка: Путь к sponsor.json (_config.SponsorBDpath) не указан");
            return;
        }

        var before = await beforeCache.GetOrDownloadAsync();
        if (before == null)
        {
            Console.WriteLine($"Ошибка: Не удалось получить предыдущее состояние пользователя {after.Id}");
            return;
        }
        var addedRoles = after.Roles.Except(before.Roles).ToList();
        var removedRoles = before.Roles.Except(after.Roles).ToList();

        var discordId = after.Id.ToString();
        Console.WriteLine($"Обработка изменений ролей для пользователя {discordId} ({after.Username})");

        // Чтение auth.json
        var authPath = _config.BDpath;
        if (!File.Exists(authPath))
        {
            Console.WriteLine($"Файл auth.json не найден по пути: {authPath}");
            return;
        }

        var authJson = await File.ReadAllTextAsync(authPath);
        var authData = JObject.Parse(authJson);

        var userEntry = authData[discordId];
        if (userEntry == null)
        {
            Console.WriteLine($"Пользователь с Discord ID {discordId} не найден в auth.json");
            return;
        }

        string? cikey = userEntry["cikey"]?.ToString();
        if (string.IsNullOrWhiteSpace(cikey))
        {
            Console.WriteLine($"CIKEY отсутствует или пуст для пользователя {discordId}");
            return;
        }

        var sponsorPath = _config.SponsorBDpath;
        JObject sponsorData;

        if (File.Exists(sponsorPath))
        {
            var sponsorJson = await File.ReadAllTextAsync(sponsorPath);
            sponsorData = JObject.Parse(sponsorJson);
        }
        else
        {
            Console.WriteLine($"Файл sponsor.json не найден, будет создан новый: {sponsorPath}");
            sponsorData = new JObject();
        }

        string username = after.Username ?? "Неизвестный пользователь";

        foreach (var role in addedRoles)
        {
            if (role.Id == _config.GrayTide)
            {
                sponsorData[cikey] = "GrayTide";
                announce("Грейтайд", username, cikey, false);
            }
            else if (role.Id == _config.Revolutionary)
            {
                sponsorData[cikey] = "Revolutionary";
                announce("Революционер", username, cikey, false);
            }
            else if (role.Id == _config.Syndicate)
            {
                sponsorData[cikey] = "Syndicate";
                announce("Синдикат", username, cikey, false);
            }
            else if (role.Id == _config.SpaceNinja)
            {
                sponsorData[cikey] = "SpaceNinja";
                announce("Космический ниндзя", username, cikey, false);
            }
        }

        foreach (var role in removedRoles)
        {
            if (role.Id == _config.GrayTide)
            {
                sponsorData.Remove(cikey);
                announce("GrayTide", username, cikey, true);
            }
            else if (role.Id == _config.Revolutionary)
            {
                sponsorData.Remove(cikey);
                announce("Revolutionary", username, cikey, true);
            }
            else if (role.Id == _config.Syndicate)
            {
                sponsorData.Remove(cikey);
                announce("Syndicate", username, cikey, true);
            }
            else if (role.Id == _config.SpaceNinja)
            {
                sponsorData.Remove(cikey);
                announce("SpaceNinja", username, cikey, true);
            }
        }

        await File.WriteAllTextAsync(sponsorPath, sponsorData.ToString());
        Console.WriteLine($"Спонсорские данные обновлены для CIKEY: {cikey}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при обновлении спонсорских данных: {ex}");
    }
}


    private async void announce(string sponsorRank, string username, string cikey, bool removed)
    {
        try
        {
            string message = removed
            ? $"❌ У пользователя **{username}** удалён спонсорский ранг **{sponsorRank}** (CIKEY `{cikey}`)."
            : $"✅ Пользователю **{username}** установлен ранг **{sponsorRank}** по CIKEY `{cikey}`.";

            var logChannel = _client.GetChannel(_config.HostChannelID) as IMessageChannel;
            if (logChannel == null)
            {
                Console.WriteLine("Не удалось найти лог-канал.");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("Обновление спонсорского ранга")
                .WithDescription(message)
                .WithColor(Color.DarkPurple)
                .WithCurrentTimestamp()
                .Build();

            await logChannel.SendMessageAsync(embed: embed);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при отправке лог-сообщения: {ex.Message}");
        }
    }

}
