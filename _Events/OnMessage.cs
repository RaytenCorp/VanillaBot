using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace VanillaBot;
public class MessageHandler
{
    private readonly DiscordSocketClient _client;
    private readonly Config _config;

    public MessageHandler(DiscordSocketClient client, Config config)
    {
        _client = client;
        _config = config;
    }

    public void Initialize()
    {
        _client.MessageReceived += CreateBranch;
        _client.MessageReceived += AddEmoji;
        Console.WriteLine("MessageHandler инициализирован.");
    }
    private async Task AddEmoji(SocketMessage message)
    {
        // Проверяем, что сообщение не от бота
        if (message.Author.IsBot)
            return;

        // Проверяем, что сообщение в нужном канале
        if (message.Channel.Id != _config.arrivalChannelId)
            return;

        // Создаём объект эмодзи с кастомным ID
        var emote = Emote.Parse("<:o_zdarova:1257350434729885777>");

        // Добавляем реакцию к сообщению
        await message.AddReactionAsync(emote);
    }
    private async Task CreateBranch(SocketMessage message)
    {
        // Проверяем, что сообщение не от бота
        if (message.Author.IsBot)
        {
            await CreateBranchForEventReportChannel(message);
            return;
        }

        // Проверяем, что сообщение не находится ни в одном из указанных каналов
        if (message.Channel.Id != _config.PhotocardsChannelId &&
            message.Channel.Id != _config.HelpChannelId &&
            message.Channel.Id != _config.VideoChannelId &&
            message.Channel.Id != _config.AudioChannelId &&
            message.Channel.Id != _config.MemesChannelId &&
            message.Channel.Id != _config.DrawingChannelId &&
            message.Channel.Id != _config.QuentaChannelId &&
            message.Channel.Id != _config.ReviewsChannelId &&
            message.Channel.Id != _config.HeadHuntChannelId &&
            message.Channel.Id != _config.TimeChannelId &&
            message.Channel.Id != _config.SS14ReportsChannelId &&
            message.Channel.Id != _config.AppealChannelId &&
            message.Channel.Id != _config.EventReportChannelId &&
            message.Channel.Id != _config.AdminReportsChannelId)
            return;


        if (message.Channel is ITextChannel textChannel && message is IUserMessage userMessage)
        {
            // Получаем никнейм пользователя или глобальное имя
            var guildUser = message.Author as SocketGuildUser;
            string threadName = guildUser?.Nickname ?? message.Author.Username;

            // Создаём ветку, привязанную к сообщению
            var thread = await textChannel.CreateThreadAsync(
                threadName, 
                ThreadType.PublicThread, 
                ThreadArchiveDuration.OneDay, 
                userMessage);
        }
    }
    private async Task CreateBranchForEventReportChannel(SocketMessage message)
    {
        // Проверяем, что сообщение находится в канале для отчетов
        if (message.Channel.Id != _config.EventReportChannelId)
            return;

        // Проверяем, что сообщение от бота
        if (!message.Author.IsBot)
            return;

        // Проверяем, что сообщение является IUserMessage (сообщение от бота)
        if (message is IUserMessage userMessage)
        {
            // Находим первый embed в сообщении
            var embed = userMessage.Embeds.FirstOrDefault();
            if (embed != null)
            {
                // Название ветки будет взято из embed.title
                string threadName = embed.Title ?? "Отчёт о событии";

                var textChannel = message.Channel as ITextChannel;

                // Создаём ветку, привязанную к сообщению с эмбедами
                var thread = await textChannel.CreateThreadAsync(
                    threadName,
                    ThreadType.PublicThread,
                    ThreadArchiveDuration.OneDay,
                    userMessage // Привязываем ветку к сообщению
                );

                Console.WriteLine($"Создана ветка с названием: {threadName}");
            }
        }
    }


}
