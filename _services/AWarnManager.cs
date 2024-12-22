using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VanillaBot
{
    public static class AWarnManager
    {
        private static readonly string AwarnsFilePath = Path.Combine("data", "awarns.json");

        // Метод для загрузки всех аварнов
        private static async Task<Dictionary<ulong, UserWarnData>> LoadAwarnsAsync()
        {
            if (File.Exists(AwarnsFilePath))
            {
                var json = await File.ReadAllTextAsync(AwarnsFilePath);
                return JsonConvert.DeserializeObject<Dictionary<ulong, UserWarnData>>(json) ?? new Dictionary<ulong, UserWarnData>();
            }
            else
            {
                return new Dictionary<ulong, UserWarnData>();
            }
        }

        // Метод для сохранения всех аварнов
        private static async Task SaveAwarnsAsync(Dictionary<ulong, UserWarnData> awarns)
        {
            Directory.CreateDirectory("data");
            var json = JsonConvert.SerializeObject(awarns, Formatting.Indented);
            await File.WriteAllTextAsync(AwarnsFilePath, json);
        }

        // Метод для добавления нового аварна
        public static async Task AddWarnAsync(ulong userId)
        {
            var awarns = await LoadAwarnsAsync();
            var currentDate = DateTime.UtcNow;

            // Если данных для пользователя нет, создаем новый список аварнов
            if (!awarns.ContainsKey(userId))
            {
                awarns[userId] = new UserWarnData { Warns = new List<Warn>() };
            }

            var userWarnData = awarns[userId];

            // Добавляем новый аварн с текущей датой
            userWarnData.Warns.Add(new Warn { WarnDate = currentDate });

            // Удаляем старые аварны (если им больше 3 месяцев)
            userWarnData.Warns = userWarnData.Warns.Where(warn => (currentDate - warn.WarnDate).TotalDays <= 90).ToList();

            // Сохраняем изменения
            await SaveAwarnsAsync(awarns);
        }

        // Метод для получения количества активных аварнов
        public static async Task<int> GetActiveWarnCountAsync(ulong userId)
        {
            var awarns = await LoadAwarnsAsync();
            if (awarns.ContainsKey(userId))
            {
                var userWarnData = awarns[userId];
                var currentDate = DateTime.UtcNow;

                // Удаляем все аварны, срок действия которых истёк (больше 3 месяцев)
                userWarnData.Warns = userWarnData.Warns.Where(warn => (currentDate - warn.WarnDate).TotalDays <= 90).ToList();

                // Сохраняем обновления
                await SaveAwarnsAsync(awarns);

                // Возвращаем количество активных аварнов
                return userWarnData.Warns.Count;
            }
            else
            {
                // Если нет аварнов, то возвращаем 0
                return 0;
            }
        }
        //метод, который возвращает список всех аварнов пользователя
        public static async Task<List<WarnDetails>> GetUserWarnsAsync(ulong userId)
        {
            var awarns = await LoadAwarnsAsync(); //смотрим все аварны на сервере
            if (awarns.ContainsKey(userId))
            {
                var userWarnData = awarns[userId];
                var currentDate = DateTime.UtcNow;

                var warnDetailsList = new List<WarnDetails>();

                // Удаляем все аварны, срок действия которых истёк (больше 3 месяцев)
                userWarnData.Warns = userWarnData.Warns.Where(warn => (currentDate - warn.WarnDate).TotalDays <= 90).ToList();

                // Добавляем только активные аварны в список
                foreach (var warn in userWarnData.Warns)
                {
                    var expirationDate = warn.WarnDate.AddDays(90);
                    warnDetailsList.Add(new WarnDetails
                    {
                        WarnDate = warn.WarnDate,
                        ExpirationDate = expirationDate
                    });
                }
                await SaveAwarnsAsync(awarns);

                return warnDetailsList;
            }

            return new List<WarnDetails>();
        }


    }

    // Класс для хранения данных о пользователе
    public class UserWarnData
    {
        public List<Warn> Warns { get; set; } = new List<Warn>(); // Инициализация списка
    }

    // Класс для хранения данных об отдельном аварне
    public class Warn
    {
        public DateTime WarnDate { get; set; } // Дата получения аварна
    }
    public class WarnDetails
    {
        public DateTime WarnDate { get; set; }   // Дата получения аварна
        public DateTime ExpirationDate { get; set; }  // Дата сгорания аварна
    }

}
