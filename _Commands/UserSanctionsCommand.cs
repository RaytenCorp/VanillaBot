using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace VanillaBot;
public class UserSanctionCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Config _config;

    public UserSanctionCommand(Config config)
    {
        _config = config;
    }

    [SlashCommand("usersanctions", "Посмотреть наказания игрока")]
    public async Task UsersanctionsAsync(
        [Summary("Пользователь", "Тот, чьи наказания мы смотрим")] IUser user)
    {
        // Прежде всего очищаем старые санкции
        SanctionManager.RemoveExpiredSanctions();
        
        // Получаем все санкции для пользователя
        var userId = user.Id;
        var userSanctions = SanctionManager.GetSanctionsForUser(userId);

        // Если нет санкций, отправляем Embed с сообщением о чистой репутации
        if (userSanctions == null || !userSanctions.Any())
        {
            var cleanReputationEmbed = new EmbedBuilder()
                .WithTitle($"Список нарушений")
                .WithDescription($"Репутация {user.Mention} чиста. Ни одного нарушения.")
                .WithColor(Color.Green)
                .Build();
            await RespondAsync(embed: cleanReputationEmbed, ephemeral: true);
            return;
        }

        // Создаем EmbedBuilder для санкций
        var embedBuilder = new EmbedBuilder()
            .WithTitle($"Список нарушений")
            .WithDescription($"Нарушения {user.Mention}")
            .WithColor(Color.Red);

        // Получаем московскую временную зону
        TimeZoneInfo mskTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");

        // Добавляем санкции в Embed
        foreach (var sanction in userSanctions.Select((value, index) => new { index, value }))
        {
            var expiryDateUtc = sanction.value.DateIssued.AddDays(90);
            var expiryDateMsk = TimeZoneInfo.ConvertTimeFromUtc(expiryDateUtc, mskTimeZone).ToString("yyyy-MM-dd HH:mm:ss");

            // Форматируем данные в строку
            var sanctionInfo = $"**Причина**: {sanction.value.Reason}\n" +
                            $"**Истекает**: {expiryDateMsk} (МСК)";

            embedBuilder.AddField($"Нарушение №{sanction.index + 1}", sanctionInfo, false);
        }

        // Строим Embed для санкций
        var embed = embedBuilder.Build();

        // Отправляем Embed с санкциями
        await RespondAsync(embed: embed, ephemeral: true);
    }
}
