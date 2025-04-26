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
        // Словарь соответствия ролей и enum-значений
        var roleToRank = new Dictionary<ulong, SponsorRank>
        {
            { _config.GrayTide, SponsorRank.GrayTide },
            { _config.Revolutionary, SponsorRank.Revolutionary },
            { _config.Syndicate, SponsorRank.Syndicate },
            { _config.SpaceNinja, SponsorRank.SpaceNinja }
        };
        // Словарь соответствия ролей и деняк
        var roleToRoyalty = new Dictionary<SponsorRank, decimal>
        {
            { SponsorRank.GrayTide, 200m},
            { SponsorRank.Revolutionary, 400m},
            { SponsorRank.Syndicate, 800m},
            { SponsorRank.SpaceNinja, 1200m}
        };
        if (_config == null || string.IsNullOrWhiteSpace(_config.BDpath) || string.IsNullOrWhiteSpace(_config.SponsorBDpath))
        {
            Console.WriteLine("Ошибка: Конфигурация не задана или пути не указаны.");
            return;
        }

        try
        {
            var discordId = after.Id.ToString();
            Console.WriteLine($"Обработка изменений ролей для пользователя {after.Username}");

            var before = await beforeCache.GetOrDownloadAsync();
            if (before == null)
            {
                Console.WriteLine($"Ошибка: Не удалось получить предыдущее состояние пользователя {after.Id}");
                return;
            }
            //читаем бдшки
            var authData = JObject.Parse(await File.ReadAllTextAsync(_config.BDpath));
            var sponsorData = JObject.Parse(await File.ReadAllTextAsync(_config.SponsorBDpath));


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

            string username = after.Username ?? "Неизвестный пользователь";

            // Определение самой высокой роли
            SponsorRank highestRank = SponsorRank.None;
            bool wassponsor = false;

            foreach (var role in after.Roles)
            {
                if (roleToRank.TryGetValue(role.Id, out var rank))
                {
                    if (rank > highestRank)
                        highestRank = rank;
                }
            }

            foreach (var role in before.Roles)
            {
                if (roleToRank.TryGetValue(role.Id, out var rank))
                {
                    wassponsor = true;
                }
            }

            if (highestRank == SponsorRank.None)
            {
                if(wassponsor)
                {
                    await RoyaltyManager.SetProfitAsync(cikey, 0);
                    if (sponsorData.Remove(cikey))
                        Console.WriteLine($"Удалён {cikey} из sponsor.json (нет спонсорских ролей).");
                    announce(highestRank.ToString(), username, cikey, true);
                }
            }
            else
            {
                var oldRank = sponsorData[cikey]?.ToString();
                if (oldRank != highestRank.ToString())
                {
                    sponsorData[cikey] = highestRank.ToString();

                    await RoyaltyManager.SetProfitAsync(cikey, roleToRoyalty[highestRank]);

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
            ? $"❌ У пользователя **{username}** удалён спонсорский ранг."
            : $"✅ Пользователю **{username}** установлен ранг **{sponsorRank}**.";

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
