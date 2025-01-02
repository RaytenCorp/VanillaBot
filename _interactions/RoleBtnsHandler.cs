using Discord;
using Discord.WebSocket;


namespace VanillaBot;

public class RoleButtonHandler : IButtonHandler
{
    private readonly ulong _roleId;
    private readonly DiscordSocketClient _client;
    private readonly ulong _guildId;

    public RoleButtonHandler(ulong roleId, DiscordSocketClient client, ulong guildId)
    {
        _roleId = roleId;
        _client = client;
        _guildId = guildId;
    }

    public async Task HandleAsync(SocketMessageComponent component)
    {
        var role = _client.GetGuild(_guildId)?.GetRole(_roleId);
        if (role == null)
        {
            await component.RespondAsync("Роль не найдена.", ephemeral: true);
            return;
        }

        var user = component.User as SocketGuildUser;
        if (user != null)
        {
            if (user.Roles.Contains(role))
            {
                await user.RemoveRoleAsync(role);
                await component.RespondAsync($"Роль {role.Name} была снята с вас.", ephemeral: true);
            }
            else
            {
                await user.AddRoleAsync(role);
                await component.RespondAsync($"Вам была выдана роль: {role.Name}!", ephemeral: true);
            }
        }
    }
}
