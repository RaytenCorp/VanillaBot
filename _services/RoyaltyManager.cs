using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace VanillaBot
{
    public static class RoyaltyManager
    {
        private const decimal PeriodicLoss = 1650m;
        private static readonly string RoyaltiesFilePath = Path.Combine("data", "royalties.json");
        private static readonly string TotalsFilePath = Path.Combine("data", "royalty_totals.txt");

        public static async Task<decimal> CalculateCurrentProfitAsync()
        {
            var royalties = await LoadRoyaltiesAsync();
            decimal totalProfit = 0;
            
            foreach (var royalty in royalties)
            {
                totalProfit += royalty.Value;
            }
            totalProfit *= 0.9m;
            totalProfit -= PeriodicLoss;

            return totalProfit;
        }

        public static async Task SetProfitAsync(string itemName, decimal profit)
        {
            var royalties = await LoadRoyaltiesAsync();
            var (totalProfit, totalLoss) = await LoadTotalsAsync();

            if (!royalties.ContainsKey(itemName))
                royalties[itemName] = 0;

            decimal oldProfit = royalties[itemName];
            decimal difference = profit - oldProfit;

            royalties[itemName] = profit;
            await SaveRoyaltiesAsync(royalties);

            totalProfit += difference;
            await SaveTotalsAsync(totalProfit, totalLoss);
        }

        public static async Task ApplyPeriodicLossAsync()
        {
            var (totalProfit, totalLoss) = await LoadTotalsAsync();
            totalLoss += PeriodicLoss;
            await SaveTotalsAsync(totalProfit, totalLoss);
        }

        public static async Task<(decimal totalProfit, decimal totalLoss)> GetOverallStatsAsync()
        {
            return await LoadTotalsAsync();
        }

        private static async Task<Dictionary<string, decimal>> LoadRoyaltiesAsync()
        {
            if (File.Exists(RoyaltiesFilePath))
            {
                var json = await File.ReadAllTextAsync(RoyaltiesFilePath);
                return JsonConvert.DeserializeObject<Dictionary<string, decimal>>(json) ?? new Dictionary<string, decimal>();
            }
            else
            {
                return new Dictionary<string, decimal>();
            }
        }

        private static async Task SaveRoyaltiesAsync(Dictionary<string, decimal> royalties)
        {
            Directory.CreateDirectory("data");
            var json = JsonConvert.SerializeObject(royalties, Formatting.Indented);
            await File.WriteAllTextAsync(RoyaltiesFilePath, json);
        }

        private static async Task<(decimal totalProfit, decimal totalLoss)> LoadTotalsAsync()
        {
            if (File.Exists(TotalsFilePath))
            {
                var lines = await File.ReadAllLinesAsync(TotalsFilePath);
                decimal totalProfit = 0;
                decimal totalLoss = 0;

                foreach (var line in lines)
                {
                    var parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var value = decimal.Parse(parts[1].Trim());

                        if (key.Equals("TotalProfit", StringComparison.OrdinalIgnoreCase))
                            totalProfit = value;
                        else if (key.Equals("TotalLoss", StringComparison.OrdinalIgnoreCase))
                            totalLoss = value;
                    }
                }
                return (totalProfit, totalLoss);
            }
            else
            {
                return (0, 0);
            }
        }

        private static async Task SaveTotalsAsync(decimal totalProfit, decimal totalLoss)
        {
            Directory.CreateDirectory("data");
            var lines = new[]
            {
                $"TotalProfit: {totalProfit}",
                $"TotalLoss: {totalLoss}"
            };
            await File.WriteAllLinesAsync(TotalsFilePath, lines);
        }
    }
}
