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

    private const int CooldownSeconds = 3600; // 60 минут на всю команду

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
            var left = CooldownSeconds - diff;
            await RespondAsync($"Подожди ещё {Math.Ceiling(left)} сек. перед следующим вызовом.", ephemeral: true);
            return;
        }

        // Обновляем кулдаун
        _lastCallUtc = DateTime.UtcNow;

        // Формируем текст
        string roleMention = $"<@&{_config.PingRoleID}>";
        string callText = $"{user.Mention} приглашает сыграть всех на <a:pepe_hyped:1276604737839435786> Rayten <a:pepe_hyped:1276604737839435786> {roleMention}";

        if (!string.IsNullOrWhiteSpace(message))
            callText += $"\n\n{message}";

        callText += $"\n\n-# если хотите отписаться/подписаться на уведомления, сделать вы это можете [здесь](https://discord.com/channels/{Context.Guild.Id}/{_config.roleselectChannelId})";

        // Отправляем с разрешённым пингом
        await RespondAsync(callText, allowedMentions: new AllowedMentions
        {
            RoleIds = new List<ulong> { _config.PingRoleID }
        });
    }
}
