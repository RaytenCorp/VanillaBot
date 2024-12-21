using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace VanillaBot
{
    public static class CounterManager
    {

        // Метод для получения следующего значения счётчика
        public static async Task<int> GetNextCounterAsync(string counterName)
        {
            var counters = await LoadCountersAsync(); //получаем счётчики

            if (!counters.ContainsKey(counterName)) //ищем нужный по переданному параметру
                counters[counterName] = 1;
            else
                counters[counterName]++;

            // Сохраняем обновлённые счётчики
            await SaveCountersAsync(counters);
            return counters[counterName];
        }


        private static readonly string CountersFilePath = Path.Combine("data", "counters.json");
        private static async Task<Dictionary<string, int>> LoadCountersAsync()
        {
            if (File.Exists(CountersFilePath))
            {
                var json = await File.ReadAllTextAsync(CountersFilePath);
                return JsonConvert.DeserializeObject<Dictionary<string, int>>(json) ?? new Dictionary<string, int>();
            }
            else
            {
                return new Dictionary<string, int>();
            }
        }

        // Метод для сохранения всех счётчиков
        private static async Task SaveCountersAsync(Dictionary<string, int> counters)
        {
            Directory.CreateDirectory("data");//создание директориии если её не было
            var json = JsonConvert.SerializeObject(counters, Formatting.Indented); //сохраняем счётчики
            await File.WriteAllTextAsync(CountersFilePath, json); //записываем в файл
        }
    }
}
