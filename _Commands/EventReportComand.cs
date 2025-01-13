using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;

namespace VanillaBot
{
    public class EventReportCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("eventreport", "Составить отчёт о проведённом ивенте")]
        public async Task SendEventReportAsync()
        {
            Console.WriteLine("команда поступила");
            await RespondAsync($"успех");
        }

    }
}
