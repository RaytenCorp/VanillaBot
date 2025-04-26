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
            var currentProfit = await RoyaltyManager.CalculateTotalProfitAsync();
            var (totalProfit, totalLoss) = await RoyaltyManager.GetOverallStatsAsync();

            var embed = new EmbedBuilder()
                .WithTitle("Статистика по бабкам")
                .WithColor(Color.Gold)
                .AddField("Текущая прибыль", $"{currentProfit}₽", true)
                .AddField("Общая прибыль за всё время", $"{totalProfit}₽", true)
                .AddField("Общий убыток за всё время", $"{totalLoss}₽", true)
                .WithFooter(footer => footer.Text = "Vanilla Station Royalty Manager")
                .Build();

            await RespondAsync(embed: embed);
        }
    }
}
