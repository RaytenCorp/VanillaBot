using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace VanillaBot;

    public class ReportCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly Config _config;

        public ReportCommand(Config config)
        {
            _config = config;
        }

        [SlashCommand("report", "Отправить жалобу на пользователя")]
        public async Task ReportAsync(
            [Summary("человек", "Укажите пользователя, который должен получить по жопе")] IUser targetUser,
            [Summary("причина", "Укажите причину жалобы")] string reason)
        {
            // Получение канала для жалоб из конфигурации и номера счётчика
            var reportChannel = Context.Guild.GetTextChannel(_config.ReportChannelId);
            int reportNumber = await CounterManager.GetNextCounterAsync("ReportCounter");

            // Создание embed-сообщения с жалобой
            var embed = new EmbedBuilder()
                .WithTitle($"Жалоба #{reportNumber}")
                .AddField("Кто отправил:", Context.User.Mention, true)
                .AddField("На кого:", targetUser.Mention, true)
                .AddField("Причина:", reason, false)
                .WithColor(Color.Red)
                .WithTimestamp(System.DateTimeOffset.Now)
                .Build();

            // Отправка embed в канал для жалоб
            await reportChannel.SendMessageAsync(embed: embed);

            // Уведомление автора команды
            await RespondAsync("Ваша жалоба успешно отправлена.", ephemeral: true);
        }
    }

