using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace VanillaBot;

public class AwarnCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Config _config;

    public AwarnCommand(Config config)
    {
        _config = config;
    }

    [SlashCommand("awarn", "Дать по жопе администратору")]
    public async Task AwarnAsync(
        [Summary("пользователь", "Пользователь, которому выдать предупреждение")] IUser user,
        [Summary("причина", "Причина предупреждения")] string reason = "Не указана")
    {
        var AwarnerUser = Context.User as SocketGuildUser;
        var targetUser = user as SocketGuildUser;

        if (AwarnerUser == null || targetUser == null)
        {
            await RespondAsync("Эта команда может быть выполнена только на сервере.", ephemeral: true);
            return;
        }

        // Проверка на права
        bool canWarn = false;
        foreach (var roleId in AwarnerUser.Roles.Select(r => r.Id))
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
        //проерка на существование канала
        var channel = Context.Client.GetChannel(_config.AWarnsChannelId) as IMessageChannel;
        if (channel == null)
        {
            await RespondAsync("Канал для предупреждений не найден.", ephemeral: true);
            return;
        }

        // Получаем список аварнов пользователя
        var activeWarnCount = await AWarnManager.GetActiveWarnCountAsync(targetUser.Id);
        // Получаем номер аварна
        int warningNumber = await CounterManager.GetNextCounterAsync("warningCounter");
        // Добавляем новый аварн
        await AWarnManager.AddWarnAsync(targetUser.Id);

        // Получаем следующий номер предупреждения
        var warnNumber = activeWarnCount + 1;  // Это новый аварн для пользователя

        // Дата сгорания аварна (через 90 дней)
        var expirationDate = DateTime.UtcNow.AddDays(1);

        Color embedColor;
        switch(warnNumber)
        {
        case 1:
            embedColor = new Color(0, 0, 255);  // Первый аварн - синий
            break;
        case 2:
            embedColor = new Color(255, 255, 0);  // Второй аварн - жёлтый
            break;
        case 3:
            embedColor = new Color(255, 0, 0);  // Третий аварн - красный
            break;
        default:
            embedColor = new Color(169, 169, 169);  // этого цвета не должно быть. обычно...
            break;
        }


        // Создаем embed сообщение
        var embed = new EmbedBuilder()
            .WithTitle($"⚠️ Админпредупреждение #{warningNumber}")
            .WithDescription($"Пользователь {user.Mention} получил {warnNumber} из 3 предупрждений.")
            .AddField("Выдал предупреждение", $"{Context.User.Mention}", true)
            .AddField("Причина", $"{reason}", false)
            .AddField("Истечёт", expirationDate.ToString("dd MMM yyyy"), true)
            .WithColor(embedColor)
            .WithTimestamp(DateTimeOffset.Now)
            .Build();


        // Отправляем embed в указанный канал
        await channel.SendMessageAsync(embed: embed);

        // Уведомляем администратора об успешной отправке
        await RespondAsync("Предупреждение отправлено.", ephemeral: true);
    }

}
