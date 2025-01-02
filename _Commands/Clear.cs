using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace VanillaBot;

public class ClearCommand : InteractionModuleBase<SocketInteractionContext>
{

    private readonly Config _config;

    public ClearCommand(Config config)
    {
        _config = config;
    }

    [SlashCommand("clear", "Очистить чат")]
    public async Task ClearChatAsync(
        [Summary("количество", "Количество сообщений на очистку")] int amount = 10) 
    {

        var user = Context.User as SocketGuildUser;
        if (user == null || !user.Roles.Any(role => role.Id == _config.HOSTRoleID))
        {
            await RespondAsync("У вас нет прав для выполнения этой команды.", ephemeral: true);
            return;
        }

        // Ограничиваем количество сообщений (максимум 100)
        if (amount < 1 || amount > 200)
        {
            await RespondAsync("КУДАААААА. Максимум 200.", ephemeral: true);
            return;
        }


        // Получаем последние сообщения из канала
        var messages = await Context.Channel.GetMessagesAsync(amount).FlattenAsync();

        // Удаляем сообщения
        await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);

        // Отправляем подтверждение
        await RespondAsync($"Успешно удалено {messages.Count()} сообщений.", ephemeral: true);
    }
}
