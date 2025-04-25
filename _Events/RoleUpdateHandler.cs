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
            if (_config == null || string.IsNullOrWhiteSpace(_config.BDpath) || string.IsNullOrWhiteSpace(_config.SponsorBDpath))
            {
                Console.WriteLine("Ошибка: Конфигурация не задана или пути не указаны.");
                return;
            }

            var before = await beforeCache.GetOrDownloadAsync();
            if (before == null)
            {
                Console.WriteLine($"Ошибка: Не удалось получить предыдущее состояние пользователя {after.Id}");
                return;
            }

            var discordId = after.Id.ToString();
            Console.WriteLine($"Обработка изменений ролей для пользователя {discordId} ({after.Username})");

            var authJson = await File.ReadAllTextAsync(_config.BDpath);
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

            JObject sponsorData;
            if (File.Exists(_config.SponsorBDpath))
            {
                sponsorData = JObject.Parse(await File.ReadAllTextAsync(_config.SponsorBDpath));
            }
            else
            {
                Console.WriteLine($"Файл sponsor.json не найден, будет создан новый.");
                sponsorData = new JObject();
            }

            string username = after.Username ?? "Неизвестный пользователь";

            // Словарь соответствия ролей и enum-значений
            var roleToRank = new Dictionary<ulong, SponsorRank>
            {
                { _config.GrayTide, SponsorRank.GrayTide },
                { _config.Revolutionary, SponsorRank.Revolutionary },
                { _config.Syndicate, SponsorRank.Syndicate },
                { _config.SpaceNinja, SponsorRank.SpaceNinja }
            };
            bool issponsorrole = false;
            // Определение самой высокой роли
            SponsorRank highestRank = SponsorRank.None;
            foreach (var role in after.Roles)
            {
                if (roleToRank.TryGetValue(role.Id, out var rank))
                {
                    issponsorrole = true;
                    if (rank > highestRank)
                        highestRank = rank;
                }
            }

            if(!issponsorrole)
            {
                return;
            }

            if (highestRank == SponsorRank.None)
            {
                if (sponsorData.Remove(cikey))
                    Console.WriteLine($"Удалён {cikey} из sponsor.json (нет спонсорских ролей).");
                announce(highestRank.ToString(), username, cikey, true);
            }
            else
            {
                var oldRank = sponsorData[cikey]?.ToString();
                if (oldRank != highestRank.ToString())
                {
                    sponsorData[cikey] = highestRank.ToString();
                    announce(highestRank.ToString(), username, cikey, false);
                    Console.WriteLine($"Обновлен ранг {cikey} -> {highestRank}");
                }
            }

            await File.WriteAllTextAsync(_config.SponsorBDpath, sponsorData.ToString());
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
            ? $"❌ У пользователя **{username}** удалён спонсорский ранг (CIKEY `{cikey}`)."
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
public enum SponsorRank
{
    None = 1,
    GrayTide = 2,
    Revolutionary = 3,
    Syndicate = 4,
    SpaceNinja = 5,
}
}
