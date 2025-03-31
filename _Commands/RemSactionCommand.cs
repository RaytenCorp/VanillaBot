using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace VanillaBot;

public class RemSanctionCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Config _config;

    // Конструктор принимает Config для настройки
    public RemSanctionCommand(Config config)
    {
        _config = config;
    }

    [SlashCommand("remsanction", "удалить санкцию")]
    public async Task SanctionAsync(
        [Summary("id", "id санкции, можно посмотреть в канале с наказаниями")] int id,
        [Summary("description", "Описание, обязательно заполните его НОРМАЛЬНО")] string description)
    {
        // Очищаем старые санкции
        SanctionManager.RemoveExpiredSanctions();
        
        // Проверка прав
        if (Context.User is not SocketGuildUser sanctionerUser || 
            !sanctionerUser.Roles.Any(role => _config.RoleSanctionPermissions.Contains(role.Id)))
        {
            await RespondAsync("Данную команду может выполнять только караульный, часовой, начальник караула и администратор.", 
                            ephemeral: true);
            return;
        }

        ulong? userid = SanctionManager.GetUserBySanctionID(id);
        if(userid == null)
        {
            await RespondAsync("Ошибка: пользователь не найден.", ephemeral: true);
            return;
        }

        // Получаем пользователя
        IUser? user = await Context.Client.GetUserAsync(userid.Value);
        if (user == null)
        {
            await RespondAsync("Ошибка: пользователь не найден.", ephemeral: true);
            return;
        }

        // Удаляем санкцию
        if (SanctionManager.RemSanction(id))
        {
            var embed = new EmbedBuilder()
                .WithTitle("Снятие наказания")
                .WithDescription($"С {user.Mention} снято наказание №{id}")
                .AddField("Причина:", description)
                .WithColor(Color.Green)
                .WithCurrentTimestamp()
                .AddField("Исполнитель:", sanctionerUser.Mention)
                .Build();

            await RespondAsync($"Наказание {id} успешно снято с {user.Mention}", ephemeral: true);
            
            // Отправляем в лог-канал
            var reportChannel = Context.Guild.GetTextChannel(_config.SanctionChannelID);

            await reportChannel.SendMessageAsync(embed: embed);
        }
        else
        {
            await RespondAsync($"Ошибка: санкция с ID {id} не существует.", ephemeral: true);
        }
    }
}
