using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace VanillaBot;
public class RoleCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Config _config;

    public RoleCommand(Config config)
    {
        _config = config;
    }

    [SlashCommand("role", "Добавить или снять роль у пользователя")]
    public async Task ManageRoleAsync(
        [Summary("пользователь", "Укажите пользователя, которому хотите добавить/снять роль")] IGuildUser targetUser,
        [Summary("роль", "Укажите роль, которую хотите добавить/снять")] IRole role)
    {
        // Проверка: Может ли вызывающий пользователь управлять указанной ролью
        var caller = Context.User as IGuildUser;
        if (!CanManageRole(caller, role))
        {
            await RespondAsync("У вас нет прав для управления этой ролью.", ephemeral: true);
            return;
        }

        // Проверка, имеет ли пользователь уже эту роль
        if (targetUser.RoleIds.Contains(role.Id))
        {
            // Удаление роли
            await targetUser.RemoveRoleAsync(role);
            await RespondAsync($"Роль {role.Mention} успешно снята с пользователя {targetUser.Mention}.");
        }
        else
        {
            // Добавление роли
            await targetUser.AddRoleAsync(role);
            await RespondAsync($"Роль {role.Mention} успешно добавлена пользователю {targetUser.Mention}.");
        }
    }

    private bool CanManageRole(IGuildUser? user, IRole role)
    {
        if (user == null)
            return false;

        // Проверяем роли вызывающего пользователя
        foreach (var callerRoleId in user.RoleIds)
        {
            if (_config.RoleManagementPermissions.TryGetValue(callerRoleId, out var manageableRoles) && manageableRoles.Contains(role.Id))
                return true;
        }

        return false;
    }

}
