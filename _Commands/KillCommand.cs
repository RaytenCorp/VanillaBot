using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace VanillaBot;

public class KillCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Config _config;

    // Конструктор принимает Config для настройки
    public KillCommand(Config config)
    {
        _config = config;
    }

    [SlashCommand("kill", "выдать пермач")]
    public async Task SanctionAsync(
        [Summary("user", "Кому?")] IUser targetUser,
        [Summary("description", "Причина")] string description)
    {
        var user = Context.User as SocketGuildUser;
        if (user == null || !user.Roles.Any(role => role.Id == _config.HOSTRoleID))
        {
            await RespondAsync("У вас нет прав для выполнения этой команды.", ephemeral: true);
            return;
        }

        // Отправляем начальное сообщение с обратным отсчетом
        var countdown = 60; // Время в секундах
        var message = await ReplyAsync(CreateCountdownMessage(targetUser, countdown));

        // Запускаем обратный отсчет
        for (int i = countdown - 1; i >= 0; i--)
        {
            await Task.Delay(1000); // Ждем 1 секунду
            await message.ModifyAsync(msg => msg.Content = CreateCountdownMessage(targetUser, i));
        }

        // Финальное действие
        await message.ModifyAsync(msg => msg.Content = $":skull: Пользователь {targetUser.Mention} был убит. Причина: {description}");
    }

    private string CreateCountdownMessage(IUser user, int secondsRemaining)
    {
        // Создаем прогресс-бар
        int totalBars = 20;
        int filledBars = (int)Math.Round((double)(secondsRemaining * totalBars) / 60);
        int emptyBars = totalBars - filledBars;

        var progressBar = new StringBuilder();
        progressBar.Append('[');
        progressBar.Append(new string('█', filledBars));
        progressBar.Append(new string('░', emptyBars));
        progressBar.Append(']');

        return $"Пользователь {user.Mention} будет убит через: {TimeSpan.FromSeconds(secondsRemaining):mm\\:ss}\n{progressBar}";
    }
}
