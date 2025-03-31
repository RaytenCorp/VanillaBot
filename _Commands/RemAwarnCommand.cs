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
        var AwarnerUser = Context.User as SocketGuildUser;
        ulong? userid = AWarnManager.GetUserByAwarnID(ID);

        if (!userid.HasValue)
        {
            await RespondAsync("Аварн не найден", ephemeral: true);
            return;
        }

        IUser user = await Context.Client.Rest.GetUserAsync(userid.Value);
        var target = user as SocketGuildUser;

        if (AwarnerUser == null || user == null || target == null)
        {
            await RespondAsync($"Эта команда может быть выполнена только на сервере. {AwarnerUser}; {user}; {target}", ephemeral: true);
            return;
        }
        // Проверка на права
        bool canWarn = false;
        foreach (var roleId in AwarnerUser.Roles.Select(r => r.Id))
        {
            if (_config.RolePermissions.TryGetValue(roleId, out var allowedRoles) &&
                allowedRoles.Any(allowedRole => target.Roles.Any(r => r.Id == allowedRole)))
            {
                canWarn = true;
                break;
            }
        }

        if (!canWarn)
        {
            await RespondAsync("У вас нет прав на снятия аварнов у этого пользователя.", ephemeral: true);
            return;
        }

        //проерка на существование канала
        var channel = Context.Client.GetChannel(_config.AWarnsChannelId) as IMessageChannel;
        if (channel == null)
        {
            await RespondAsync("Канал с аварнами не найден.", ephemeral: true);
            return;
        }


        if (!AWarnManager.RemAwarn(ID))
        {
            await RespondAsync($"аварна с id {ID} не существует.", ephemeral: true);
            return;
        }

        // Создаем embed сообщение
        var embed = new EmbedBuilder()
            .WithTitle($"🙏 Обжалование")
            .WithDescription($"Аварн {ID} снят с пользователя {user.Mention}")
            .AddField("Обжаловал", $"{Context.User.Mention}", true)
            .AddField("Причина", $"{reason}", false)
            .WithColor(Color.Green)
            .WithTimestamp(DateTimeOffset.Now)
            .Build();


        // Отправляем embed в указанный канал
        await channel.SendMessageAsync(embed: embed);

        // Уведомляем администратора об успешной отправке
        await RespondAsync("Обжалование отправлено.", ephemeral: true);
    }

}
