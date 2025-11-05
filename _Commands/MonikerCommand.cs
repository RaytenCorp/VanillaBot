using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VanillaBot;

public class MonikerCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly string poolPath = "data/moniker/monikerpool.txt";
    private readonly string jsonPath = "data/moniker/monikers.json";

    [SlashCommand("moniker", "Получить или узнать прозвище")]
    public async Task GetMonikerAsync(
        [Summary("user", "пользователь (необязательно)")] IUser? targetUser = null)
    {
        var requester = Context.User;
        var target = targetUser ?? requester;
        var targetId = target.Id.ToString();

        // Готовим пути
        Directory.CreateDirectory("data/moniker");

        // Загружаем json
        Dictionary<string, string> assigned = new();
        if (File.Exists(jsonPath))
        {
            var json = await File.ReadAllTextAsync(jsonPath);
            if (!string.IsNullOrWhiteSpace(json))
                assigned = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
        }

        // Если указан другой пользователь — просто показать его прозвище
        if (targetUser != null)
        {
            if (assigned.TryGetValue(targetId, out var existing))
                await RespondAsync($"Прозвище пользователя {target.Mention}: **{existing}**");
            else
                await RespondAsync($"У пользователя {target.Mention} пока нет прозвища.");
            return;
        }

        // Проверяем — есть ли уже у вызвавшего пользователя
        if (assigned.ContainsKey(targetId))
        {
            await RespondAsync($"У тебя уже есть прозвище: **{assigned[targetId]}**");
            return;
        }

        // Загружаем пул
        if (!File.Exists(poolPath))
        {
            await RespondAsync("Прозвища закончились! Приходите позже", ephemeral: true);
            return;
        }

        var pool = File.ReadAllLines(poolPath)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (pool.Count == 0)
        {
            await RespondAsync("Прозвища закончились! Приходите позже", ephemeral: true);
            return;
        }

        // Выбираем случайное имя
        var random = new Random();
        var moniker = pool[random.Next(pool.Count)];

        // Удаляем выбранное из пула
        pool.Remove(moniker);
        await File.WriteAllLinesAsync(poolPath, pool);

        // Обратный отсчёт с прогресс-баром
        int countdown = 10; // секунд до выдачи
        var message = await ReplyAsync(CreateCountdownMessage(requester, countdown));

        for (int i = countdown - 1; i >= 0; i--)
        {
            await Task.Delay(1000);
            await message.ModifyAsync(m => m.Content = CreateCountdownMessage(requester, i));
        }

        // Сохраняем в JSON
        assigned[targetId] = moniker;
        var updatedJson = JsonSerializer.Serialize(assigned, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(jsonPath, updatedJson);

        // Финальное сообщение
        await message.ModifyAsync(m => m.Content = $"✨ {requester.Mention}, твоё новое прозвище: **{moniker}** ✨");
    }

    private string CreateCountdownMessage(IUser user, int secondsRemaining)
    {
        int totalBars = 20;
        int filledBars = (int)Math.Round((double)(totalBars * (10 - secondsRemaining)) / 10);
        int emptyBars = totalBars - filledBars;

        var progressBar = new StringBuilder("[");
        progressBar.Append(new string('█', filledBars));
        progressBar.Append(new string('░', emptyBars));
        progressBar.Append(']');

        return $"Назначаю прозвище для {user.Mention}...\n" +
               $"⏳ Осталось: {secondsRemaining}s\n" +
               $"{progressBar}";
    }
}
