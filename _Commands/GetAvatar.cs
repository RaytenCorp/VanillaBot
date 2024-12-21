using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace VanillaBot
{
    public class GetAvatarCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("getavatar", "Получить аватар указанного пользователя")]
        public async Task GetAvatarAsync(
            [Summary("пользователь", "Укажите пользователя, чей аватар вы хотите получить")]
            IUser user = null) // Если пользователь не указан, по умолчанию будет null
        {
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
}