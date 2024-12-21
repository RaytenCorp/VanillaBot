using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace VanillaBot
{
    public class WikiCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("Wiki", "получить ссылку на вики")]
        public async Task SendTextMessageAsync()
        {
            string link = "https://vanilla-station.ru/";

            await RespondAsync($"{link}");
        }
    }
}
