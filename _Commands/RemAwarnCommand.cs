using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace VanillaBot;

public class RemAwarnCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Config _config;

    public RemAwarnCommand(Config config)
    {
        _config = config;
    }

    [SlashCommand("remawarn", "снять аварн досрочно")]
    public async Task AwarnAsync(
        [Summary("id", "Номер аварна, который будет снят досрочно")] int ID,
        [Summary("причина", "Причина снятия")] string reason)
    {
        if (Context.Guild == null)
        {
            await RespondAsync("Эта команда может выполняться только на сервере.", ephemeral: true);
            return;
        }

        var awarnerUser = Context.User as SocketGuildUser;
        ulong? userid = AWarnManager.GetUserByAwarnID(ID);

        if (!userid.HasValue)
        {
            await RespondAsync("Аварн не найден", ephemeral: true);
            return;
        }

        IUser user = await Context.Client.Rest.GetUserAsync(userid.Value);
        var targetUser = Context.Guild.GetUser(userid.Value);
        if (targetUser == null)
        {
            await DeferAsync(ephemeral: true);
            await Context.Guild.DownloadUsersAsync();
            targetUser = Context.Guild.GetUser(userid.Value);
            await FollowupAsync($"Выполните команду еще раз. Пользователь {targetUser} только что был закеширован");
            return;
        }

        if (awarnerUser == null || user == null || targetUser == null)
        {
            await RespondAsync($"Ошибка: {nameof(awarnerUser)} = {awarnerUser}, {nameof(user)} = {user}, {nameof(targetUser)} = {targetUser}", ephemeral: true);
            return;
        }

        // Проверка на права
        bool canWarn = false;
        foreach (var roleId in awarnerUser.Roles.Select(r => r.Id))
        {
            if (_config.RolePermissions.TryGetValue(roleId, out var allowedRoles) &&
                allowedRoles.Any(allowedRole => targetUser.Roles.Any(r => r.Id == allowedRole)))
            {
                canWarn = true;
                break;
            }
        }

        if (!canWarn)
        {
            await RespondAsync("У вас нет прав на выдачу предупреждений этому пользователю.", ephemeral: true);
            return;
        }

        // Проверка существования канала
        var channel = Context.Client.GetChannel(_config.AWarnsChannelId) as IMessageChannel;
        if (channel == null)
        {
            await RespondAsync("Канал с аварнами не найден.", ephemeral: true);
            return;
        }

        if (!AWarnManager.RemAwarn(ID))
        {
            await RespondAsync($"Аварн с ID {ID} не существует.", ephemeral: true);
            return;
        }

        // Создаем embed сообщение
        var embed = new EmbedBuilder()
            .WithTitle("🙏 Обжалование")
            .WithDescription($"Аварн {ID} снят с пользователя {user.Mention}")
            .AddField("Обжаловал", Context.User.Mention, true)
            .AddField("Причина", reason, false)
            .WithColor(Color.Green)
            .WithTimestamp(DateTimeOffset.Now)
            .Build();

        // Отправляем embed в указанный канал
        await channel.SendMessageAsync(embed: embed);

        // Уведомляем администратора об успешной отправке
        await RespondAsync("Обжалование отправлено.", ephemeral: true);
    }


}
