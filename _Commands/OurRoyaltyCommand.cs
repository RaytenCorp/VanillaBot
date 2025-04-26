using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace VanillaBot
{
    public class OurRoyaltyCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("ourroyalty", "Деньги деньги деньги")]
        public async Task CalculateRoyaltyAsync()
        {
            var currentProfit = await RoyaltyManager.CalculateCurrentProfitAsync();
            var (totalProfit, totalLoss) = await RoyaltyManager.GetOverallStatsAsync();

            var embed = new EmbedBuilder()
                .WithTitle("Статистика по бабкам")
                .WithColor(Color.Gold)
                .AddField("Текущая прибыль", $"{currentProfit}₽", true)
                .AddField("Общая прибыль за всё время", $"{totalProfit-totalLoss}₽", true)
                .WithFooter(footer => footer.Text = "Rayten")
                .Build();

            await RespondAsync(embed: embed, ephemeral: true);
        }
    }
}
