using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace VanillaBot
{
    public class EventReportCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly Config _config;

        public EventReportCommand(Config config)
        {
            _config = config;
        }

        [SlashCommand("eventreport", "Составить отчёт о проведённом ивенте")]
        public async Task SendEventReportAsync(
            [Summary("Тип", "Тип проведённого ивента")]
            [Choice("Микро", "микро")]
            [Choice("Мини", "мини")]
            [Choice("Средний", "средний")]
            [Choice("Глобальный", "глобальный")]
            string eventtype,

            [Summary("Описание", "Подробное описание ивента")]
            string eventdesc,

            [Summary("Проблемы", "Опишите проблемы, с которыми вы столкнулись (если они были)")]
            string? eventproblems = null,

            [Summary("Помощник", "Выберите одного помощника (если был)")]
            IUser? helper = null,

            [Summary("Фотокарточка", "Фоточка!")]
            IAttachment? photo = null
        )
        {
            // Получение канала для жалоб из конфигурации и номера счётчика
            var EventReportChannel = Context.Guild.GetTextChannel(_config.EventReportChannelId);
            int EventReportReportNumber = await CounterManager.GetNextCounterAsync("EventReportCounter");

            // Создание Embed
            var embed = new EmbedBuilder()
                .WithTitle($"Отчёт о событии #{EventReportReportNumber}")
                .WithColor(Color.Blue)
                .AddField("Тип ивента", eventtype, true)
                .AddField("Описание", eventdesc, false)
                .WithFooter($"Автор отчёта: {Context.User.Username} • {DateTime.UtcNow:dd.MM.yyyy HH:mm} UTC", Context.User.GetAvatarUrl())
                .WithThumbnailUrl(photo?.Url) // Если фото есть, добавляем в миниатюру
                .WithCurrentTimestamp();

            // Если указаны проблемы, добавляем поле
            if (!string.IsNullOrWhiteSpace(eventproblems))
                embed.AddField("Проблемы", eventproblems, false);

            // Если указан помощник, добавляем поле
            if (helper != null)
                embed.AddField("Помощник", helper.Mention, true);

            // Отправка Embed в указанный канал
            await EventReportChannel.SendMessageAsync(embed: embed.Build());

            // Уведомление автора команды
            await RespondAsync("Отчёт принят.", ephemeral: true);
        }
    }
}
