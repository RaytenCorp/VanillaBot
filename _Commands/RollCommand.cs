using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace VanillaBot
{
    public class RollCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("roll", "Возвращает случайное значение от 1 до 100 или до заданного параметра.")]
        public async Task RollAsync(
            [Summary("максимум", "Максимальное значение (по умолчанию 100).")]
            int max = 100)
        {
            // Проверяем, чтобы максимальное значение было больше 1
            if (max < 1)
            {
                await RespondAsync("Максимальное значение должно быть больше 0.", ephemeral: true);
                return;
            }

            // Генерируем случайное число
            Random random = new Random();
            int result = random.Next(1, max + 1);

            // Формируем сообщение
            var user = Context.User as SocketUser;
            string response = $" 🎲 | {user.Username} выбросил {result} из {max}!";

            // Отправляем результат
            await RespondAsync(response, ephemeral: true);
        }
    }
}
