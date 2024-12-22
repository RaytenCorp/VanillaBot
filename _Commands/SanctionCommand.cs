using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace VanillaBot;

public class SanctionCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Config _config;

    // Конструктор принимает Config для настройки
    public SanctionCommand(Config config)
    {
        _config = config;
    }

    [SlashCommand("sanction", "Issues a sanction to a user.")]
    public async Task SanctionAsync(
        [Summary("user", "The user to sanction.")] IUser user,
        [Summary("description", "The reason for the sanction.")] string description)
    {
        //прежде всего очищаем старые санкции
        SanctionManager.RemoveExpiredSanctions();
        
        // Проверка, имеет ли пользователь разрешение на выполнение команды
        var SanctionerUser = Context.User as SocketGuildUser;
        if (SanctionerUser == null || !SanctionerUser.Roles.Any(role => _config.RoleSanctionPermissions.Contains(role.Id)))
        {
            await RespondAsync("Данную команду может выполнять только караульный, часовой, начальник караула и администратор.", ephemeral: true);
            return;
        }

        // берем данные мутов пользователя
        var userId = user.Id;
        var userSanctions = SanctionManager.GetSanctionsForUser(userId);

        SanctionType sanctionType;
        DateTime? muteExpiry = null;

        var guildUser = Context.Guild.GetUser(userId);
        if (guildUser == null)
            return;

        switch (userSanctions.Count)
        {
            case 0:
                sanctionType = SanctionType.Warn;
                SanctionManager.AddSanction(userId, description, SanctionType.Warn);
                break;
            case 1:
                sanctionType = SanctionType.Mute;
                muteExpiry = DateTime.UtcNow.AddDays(3);
                SanctionManager.AddSanction(userId, description, SanctionType.Mute, muteExpiry);
                break;
            case 2:
                sanctionType = SanctionType.Mute;
                muteExpiry = DateTime.UtcNow.AddDays(5);
                SanctionManager.AddSanction(userId, description, SanctionType.Mute, muteExpiry);
                break;
            case 3:
                sanctionType = SanctionType.Mute;
                muteExpiry = DateTime.UtcNow.AddDays(8);
                SanctionManager.AddSanction(userId, description, SanctionType.Mute, muteExpiry);
                break;
            case 4:
                sanctionType = SanctionType.Mute;
                muteExpiry = DateTime.UtcNow.AddDays(11);
                SanctionManager.AddSanction(userId, description, SanctionType.Mute, muteExpiry);
                await guildUser.AddRoleAsync(Context.Guild.GetRole(_config.PoopRoleID)); //на 5 раз выдаем роль грязнули
                break;
            case 5:
                sanctionType = SanctionType.Mute;
                muteExpiry = DateTime.UtcNow.AddDays(19);
                SanctionManager.AddSanction(userId, description, SanctionType.Mute, muteExpiry);
                break;
            default:
                await RespondAsync($"У {user.Mention} Уже перебор по наказаниям. Почему он все еще не в бане?", ephemeral: true);
                return;
        }

        await RespondAsync($"Выдано {userSanctions.Count + 1} наказание {user.Mention}: {sanctionType}. Причина: {description}", ephemeral: true);

        // Если мут - мутим
        if (sanctionType == SanctionType.Mute && muteExpiry.HasValue)
                await guildUser.AddRoleAsync(Context.Guild.GetRole(_config.MuteRoleID));
    }
}
