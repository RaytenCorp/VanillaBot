using Discord;
using Discord.Interactions;
using System.Linq;
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
            [Summary("Название", "Как называется ваше творение?")]
            string eventname,
            [Summary("Раунд", "укажите раунд в котором был проведён ивент")]
            int eventroundID,

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
            // Список разрешённых ролей
            var allowedRoles = new ulong[]
            {
                _config.HOSTRoleID,
                _config.AdminRoleID,
                _config.GGMRoleID,
                _config.GMRoleID,
                _config.MGMRoleID,
                _config.SMRoleID,
                _config.WardenRoleID,
                _config.MRoleID
            };

            // Проверяем, есть ли у пользователя одна из разрешённых ролей
            var userRoles = (Context.User as IGuildUser)?.RoleIds;
            if (userRoles == null || !userRoles.Any(role => allowedRoles.Contains(role)))
            {
                await RespondAsync("У вас недостаточно прав для выполнения этой команды.", ephemeral: true);
                return;
            }

            // Определяем самую высокую роль из разрешённых
            var guildRoles = Context.Guild.Roles;
            var highestRole = guildRoles
                .Where(role => userRoles.Contains(role.Id) && allowedRoles.Contains(role.Id))
                .OrderByDescending(role => role.Position)
                .FirstOrDefault();

            string eventReporterRole = highestRole?.Name ?? "Неизвестная роль";

            // Получение канала для отчётов
            var EventReportChannel = Context.Guild.GetTextChannel(_config.EventReportChannelId);

            // Создание Embed
            var embed = new EmbedBuilder()
                .WithTitle($"{eventname}")
                .WithColor(new Color(0x9C59B6)) // Цвет: #9C59B6
                .AddField("Раунд",$"```{eventroundID}```", true)
                .AddField("Тип",$"```{eventtype}```", true)
                .AddField("Описание", $"```{eventdesc}```", false)
                .WithFooter($"{Context.Guild.Name}", Context.Guild.IconUrl)
                .WithCurrentTimestamp();

            // Если указаны проблемы, добавляем поле
            if (!string.IsNullOrWhiteSpace(eventproblems))
                embed.AddField("Проблемы", $"```{eventproblems}```", false);

            // Если указан помощник, добавляем поле
            if (helper != null)
                embed.AddField("Помощник", helper.Mention, true);

            embed.AddField("Проводящий", $"{Context.User.Mention}, {eventReporterRole}", false);
            // Если есть фото, добавляем его как изображение Embed
            if (photo != null)
                embed.WithImageUrl(photo.Url);

            // Отправка Embed в указанный канал
            await EventReportChannel.SendMessageAsync(embed: embed.Build());

            // Уведомление автора команды
            await RespondAsync("Отчёт принят.", ephemeral: true);
        }
    }
}
