using Discord;
using Discord.Interactions;
using System.Threading.Tasks;
namespace VanillaBot;

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
        // Получение канала для отчётов
        var EventReportChannel = Context.Guild.GetTextChannel(_config.EventReportChannelId);
        int EventReportReportNumber = await CounterManager.GetNextCounterAsync("EventReportCounter");

        // Создание Embed
        var embed = new EmbedBuilder()
            .WithTitle($"Отчёт о событии #{EventReportReportNumber}")
            .WithColor(new Color(0x9C59B6)) // Цвет: #9C59B6
            .AddField("Тип", eventtype, false)
            .AddField("Описание", $"```{eventdesc}```", false)
            .WithFooter(
                $"{Context.Guild.Name}",
                Context.Guild.IconUrl
            )
            .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
            .WithCurrentTimestamp();

        // Если указаны проблемы, добавляем поле
        if (!string.IsNullOrWhiteSpace(eventproblems))
            embed.AddField("Проблемы", $"```{eventproblems}```", false);
        
        embed.AddField("Проводящий", $"{Context.User.Mention} (Главный гейм-мастер)", false);

        if (helper != null)
            embed.AddField("Помощник", helper.Mention, true);

        // Если есть фото, добавляем его как изображение Embed
        if (photo != null)
            embed.WithImageUrl(photo.Url);

        // Отправка Embed в указанный канал
        await EventReportChannel.SendMessageAsync(embed: embed.Build());

        // Уведомление автора команды
        await RespondAsync("Отчёт принят.", ephemeral: true);
    }
}