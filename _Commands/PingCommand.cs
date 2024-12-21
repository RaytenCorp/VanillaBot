using Discord.Interactions;
using System.Threading.Tasks;

namespace VanillaBot;

public class PingCommand : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Понг!")]
    public async Task PingAsync()
    {
        await RespondAsync($"Понг! Задержка: {Context.Client.Latency} мс");
    }
}

