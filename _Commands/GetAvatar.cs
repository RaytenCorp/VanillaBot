using Discord;
using Discord.Interactions;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace VanillaBot;

public class GetAvatarCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Config _config;
    public GetAvatarCommand(Config config)
    {
        _config = config;
    }

    [SlashCommand("getavatar", "Получить аватар указанного пользователя")]
    public async Task GetAvatarAsync(
        [Summary("пользователь", "Укажите пользователя, чей аватар вы хотите получить")]
        IUser? user = null) 
    {

        var usersender = Context.User as SocketGuildUser;
        if (usersender == null || !usersender.Roles.Any(role => role.Id == _config.HOSTRoleID))
        {
            await RespondAsync("Мой хозяин запретил мне показывать его аватарки... <:O_cutte:1310747922689949716>");
            return;
        }        

        // Если пользователь не передан, использовать автора команды
        user ??= Context.User;

        // Получение URL аватара
        string avatarUrl = user.GetAvatarUrl(ImageFormat.Auto, 1024) ?? user.GetDefaultAvatarUrl();

        // Ответ с аватаром
        var embed = new EmbedBuilder()
            .WithTitle($"Аватар пользователя {user.Username}")
            .WithImageUrl(avatarUrl)
            .WithColor(Color.Blue)
            .Build();

        await RespondAsync(embed: embed);
    }
}

