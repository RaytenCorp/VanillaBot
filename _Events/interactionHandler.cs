using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace VanillaBot;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly ButtonHandlerRegistry _registry;
    private readonly Config _config;

    public InteractionHandler(DiscordSocketClient client, Config config)
    {
        _client = client;
        _config = config;
        _registry = new ButtonHandlerRegistry();
    }

    public void Initialize()
    {
        _registry.RegisterHandler("get_role_NewsRoleID", new RoleButtonHandler(_config.NewsRoleID, _client,_config.GuildId)); 
        _registry.RegisterHandler("get_role_EventsRoleID", new RoleButtonHandler(_config.EventsRoleID, _client,_config.GuildId));
        _registry.RegisterHandler("get_role_HighPopRoleID", new RoleButtonHandler(_config.HighPopRoleID, _client,_config.GuildId)); 
        _registry.RegisterHandler("get_role_RoundsRoleID", new RoleButtonHandler(_config.RoundsRoleID, _client,_config.GuildId)); 
        // Подписка на события взаимодействий
        _client.InteractionCreated += async (interaction) =>
        {
            if (interaction is not SocketMessageComponent component) return;

            // Обработка нажатия кнопок через реестр
            await _registry.HandleButtonAsync(component);
        };

        Console.WriteLine("InteractionHandler инициализирован.");
    }

    public async Task CreateAndSendButtonsAsync(
        IMessageChannel channel,
        List<(string ButtonId, string ButtonLabel)> buttons,
        string embedTitle,
        string embedDescription)
    {
        var componentBuilder = new ComponentBuilder();
        var embedAlreadyExists = false;

        // Проверка на существование эмбеда
        var messages = await channel.GetMessagesAsync(10).FlattenAsync();
        foreach (var message in messages)
        {
            if (message.Embeds.Count > 0)
            {
                var messageEmbed = message.Embeds.First(); // Переименовываем переменную

                if (messageEmbed.Title == embedTitle && messageEmbed.Description == embedDescription)
                {
                    embedAlreadyExists = true;
                    break;
                }
            }
        }

        if (embedAlreadyExists)
        {
            Console.WriteLine("Эмбед с такими кнопками уже существует. Не создаем новый.");
            return;
        }

        // Создание кнопок
        foreach (var (buttonId, buttonLabel) in buttons)
        {
            componentBuilder.WithButton(buttonLabel, buttonId, ButtonStyle.Primary);
        }

        // Отправка эмбеда с кнопками
        var embed = new EmbedBuilder()
            .WithTitle(embedTitle)
            .WithDescription(embedDescription)
            .WithColor(Color.Blue)
            .Build();

        await channel.SendMessageAsync(embed: embed, components: componentBuilder.Build());
    }
}
