using Discord;
using Discord.Interactions;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace VanillaBot;

public class CallCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Config _config;
    private static DateTime _lastCallUtc = DateTime.MinValue;

    private static readonly TimeZoneInfo MoscowTZ = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
    private static readonly TimeSpan StartTime = new(9, 0, 0);
    private static readonly TimeSpan EndTime = new(22, 0, 0);

    private const int CooldownSeconds = 5400; // 90 минут на всю команду

    public CallCommand(Config config)
    {
        _config = config;
    }

    [SlashCommand("call", "Позвать всех на Rayten!")]
    public async Task CallPlayersAsync(
        [Summary("текст", "Дополнительное сообщение (необязательно)")] string? message = null)
    {
        var user = Context.User as SocketGuildUser;

        // Проверка времени (МСК)
        var nowMoscow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, MoscowTZ);
        if (nowMoscow.TimeOfDay < StartTime || nowMoscow.TimeOfDay > EndTime)
        {
            await RespondAsync("Команду можно использовать только с 9:00 до 22:00 (МСК).", ephemeral: true);
            return;
        }

        // Проверка кулдауна
        var diff = (DateTime.UtcNow - _lastCallUtc).TotalSeconds;
        if (diff < CooldownSeconds)
        {
            var left = TimeSpan.FromSeconds(CooldownSeconds - diff);

            string leftText;
            if (left.TotalHours >= 1)
                leftText = $"{(int)left.TotalHours} ч. {left.Minutes} мин.";
            else if (left.TotalMinutes >= 1)
                leftText = $"{left.Minutes} мин. {left.Seconds} сек.";
            else
                leftText = $"{left.Seconds} сек.";

            await RespondAsync($"Подожди ещё {leftText} перед следующим вызовом.", ephemeral: true);
            return;
        }


        // Обновляем кулдаун
        _lastCallUtc = DateTime.UtcNow;

        // Формируем текст
        string roleMention = $"<@&{_config.PingRoleID}>";
        string caller = user != null ? user.Mention : "кто-то";
        string callText = $"{caller} приглашает сыграть всех на Rayten <a:pepe_hyped:1276604737839435786> {roleMention}";

        if (!string.IsNullOrWhiteSpace(message))
            callText += $"\n\n{caller}: {message}";

        // Отправляем с разрешённым пингом
        await RespondAsync(callText, allowedMentions: new AllowedMentions
        {
            RoleIds = new List<ulong> { _config.PingRoleID }
        });
    }
}
