using Discord;
using Discord.Interactions;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace VanillaBot;

public class CallCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Config _config;
    public CallCommand(Config config)
    {
        _config = config;
    }

    [SlashCommand("call", "Позвать всех на Rayten!")]
    public async Task CallPlayersAsync(
        [Summary("текст", "Дополнительное сообщение (необязательно)")] string? message = null)
    {
        var user = Context.User as SocketGuildUser;

        // Только для хоста
        if (user == null || !user.Roles.Any(role => role.Id == _config.HOSTRoleID))
        {
            await RespondAsync("Команда доступна только хосту", ephemeral: true);
            return;
        }

        // Формируем текст
        string roleMention = $"<@&{_config.PingRoleID}>";
        string callText = $"{user.Mention} приглашает сыграть всех на <a:pepe_hyped:1276604737839435786> Rayten <a:pepe_hyped:1276604737839435786> {roleMention}";

        if (!string.IsNullOrWhiteSpace(message))
            callText += $"\n\n{message}";

        // Отправляем с разрешённым пингом
        await RespondAsync(callText, allowedMentions: new AllowedMentions
        {
            RoleIds = new List<ulong> { _config.PingRoleID }
        });
    }
}

