using Discord;
using Discord.Interactions;
using System.Threading.Tasks;
using System.Text;

namespace VanillaBot
{
    public class OurRoyaltyCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("ourroyalty", "Деньги деньги деньги")]
        public async Task CalculateRoyaltyAsync()
        {
            var currentProfit = await RoyaltyManager.CalculateCurrentProfitAsync();
            var (totalProfit, totalLoss) = await RoyaltyManager.GetOverallStatsAsync();

            // Получаем отсортированные Discord ID по убыванию денег
            var sortedDiscordIds = await RoyaltyManager.GetSortedDiscordIdsByRoyaltiesAsync();

            // Создаем строку с пингами всех донатеров
            var donorsString = new StringBuilder();

            foreach (var discordId in sortedDiscordIds)
            {
                var user = Context.Guild.GetUser(ulong.Parse(discordId));
                if (user != null)
                {
                    donorsString.AppendLine($"<@{user.Id}>");
                }
            }
            var embed = new EmbedBuilder()
                .WithTitle("Статистика по бабкам")
                .WithColor(Color.Gold)
                .AddField("Текущая прибыль", $"{currentProfit}₽", true)
                .AddField("Общая прибыль за всё время", $"{totalProfit-totalLoss}₽", true)
                .AddField("Огромная благодарность этим людям:", donorsString.ToString() ?? "А их нет блять", false)
                .WithFooter(footer => footer.Text = "Rayten")
                .Build();

            await RespondAsync(embed: embed, ephemeral: true);
        }
    }
}
