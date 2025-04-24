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
            var before = await beforeCache.GetOrDownloadAsync();
            var addedRoles = after.Roles.Except(before.Roles).ToList();
            var removedRoles = before.Roles.Except(after.Roles).ToList();

            // Получаем Discord ID
            var discordId = after.Id.ToString();

            // Загружаем auth.json
            var authPath = _config.BDpath;
            var authData = JObject.Parse(await File.ReadAllTextAsync(authPath));

            if (!authData.ContainsKey(discordId))
            {
                Console.WriteLine($"Пользователь с Discord ID {discordId} не найден в {authPath}, видимо он не авторизован");
                return;
            }

            string? cikey = authData[discordId]?["cikey"]?.ToString();

            if (string.IsNullOrWhiteSpace(cikey))
            {
                Console.WriteLine($"CIKEY не найден или пустой для пользователя {discordId}");
                return;
            }

            var sponsorPath = _config.SponsorBDpath;
            var sponsorData = File.Exists(sponsorPath)
                ? JObject.Parse(await File.ReadAllTextAsync(sponsorPath))
                : new JObject();

            if (!File.Exists(sponsorPath))
            {
                Console.WriteLine($"Файл данных спонсоров не найден по пути: {sponsorPath}");
                return;
            }
            
            foreach (var role in addedRoles)
            {
                if (role.Id == _config.GrayTide)
                {
                    sponsorData[cikey] = "GrayTide";
                    announce("Грейтайд", after.Username, cikey, false);
                }
                if (role.Id == _config.Revolutionary)
                {
                    sponsorData[cikey] = "Revolutionary";
                    announce("Революционер", after.Username, cikey, false);
                }
                if (role.Id == _config.Syndicate)
                {
                    sponsorData[cikey] = "Syndicate";
                    announce("Синдикат", after.Username, cikey, false);
                }
                if (role.Id == _config.SpaceNinja)
                {
                    sponsorData[cikey] = "SpaceNinja";
                    announce("Космический ниндзя", after.Username, cikey, false);
                }
            }

            foreach (var role in removedRoles)
            {
                if (role.Id == _config.GrayTide)
                {
                    sponsorData.Remove(cikey);
                    announce("GrayTide", after.Username, cikey, true);
                }
                if (role.Id == _config.Revolutionary)
                {
                    sponsorData.Remove(cikey);
                    announce("Revolutionary", after.Username, cikey, true);
                }
                if (role.Id == _config.Syndicate)
                {
                    sponsorData.Remove(cikey);
                    announce("Syndicate", after.Username, cikey, true);
                }
                if (role.Id == _config.SpaceNinja)
                {
                    sponsorData.Remove(cikey);
                    announce("SpaceNinja", after.Username, cikey, true);
                }
            }

            await File.WriteAllTextAsync(sponsorPath, sponsorData.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обновлении спонсорских данных: {ex.Message}");
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
