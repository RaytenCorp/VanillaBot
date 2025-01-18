using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Reflection;

namespace VanillaBot;
class Program
{
    private static UserJoinHandler _userJoinHandler = null!;
    private static MessageHandler _messageHandler = null!;
    private static InteractionHandler _interactionHandler = null!;

    private static DiscordSocketConfig clientConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages
        };

    private static DiscordSocketClient _client = new DiscordSocketClient(clientConfig);
    private static InteractionService _commands = new InteractionService(_client.Rest);

    /* 
    Это контейнер для управления зависимостями (Dependency Injection). 
    Он позволяет регистрировать и предоставлять экземпляры объектов, которые нужны для работы разных частей бота.
    */
    private static IServiceProvider _services = null!;

    // это тупо наш конфиг. Обрабатывается конфиг менеджером в папке _services
    private static Config? _config;

    public static async Task Main()
    {
        // Загружаем конфигурацию
        _config = await ConfigLoader.LoadConfigAsync();

        // Настройка DI (Dependency Injection)
        _services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .AddSingleton(_config)
            .BuildServiceProvider();

        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.InteractionCreated += HandleInteractionAsync;

        try
        {
            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.StartAsync();
            Console.WriteLine("Бот успешно подключен!");

            await Task.Delay(-1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при подключении: {ex.Message}");
        }
    }

    // Метод для логирования
    private static Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }

    private static async Task ReadyAsync()
    {
        if (_config == null)
        {
            Console.WriteLine("Конфигурация не инициализирована.");
            return;
        }

        await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);

        await _commands.RegisterCommandsToGuildAsync(_config.GuildId);
        Console.WriteLine("Команды зарегистрированы.");
        
        // Инициализация менеджеров
        LocalEventManager.Initialize(_client, _config);
        SanctionManager.Initialize();

        // ивент захода пользователя
        _userJoinHandler = new UserJoinHandler(_client, _config);
        _userJoinHandler.Initialize();
        // ивент сообщения
        _messageHandler = new MessageHandler(_client, _config);
        _messageHandler.Initialize();
        // ивент нажатия на кнопку
        _interactionHandler = new InteractionHandler(_client, _config);
        _interactionHandler.Initialize();  // Инициализация обработчика

        // кнопки кнопочки
        await createRoleSelectionBtns();
    }

    private static async Task createRoleSelectionBtns()
    {
        if (_config?.roleselectChannelId == null)
        {
            Console.WriteLine("ID канала для выбора ролей не задан.");
            return;
        }

        var channel = _client.GetChannel(_config.roleselectChannelId) as IMessageChannel;

        // Проверяем, что канал существует
        if (channel is null)
        {
            Console.WriteLine("Канал не найден.");
            return;
        }

        var buttons = new List<(string ButtonId, string ButtonLabel)>
        {
            ("get_role_NewsRoleID", "Новости"),
            ("get_role_EventsRoleID", "Ивенты"),
            ("get_role_HighPopRoleID", "Хайпоп"),
            ("get_role_RoundsRoleID", "Раунды")
        };

        // Создаём и отправляем эмбеды с кнопками
        await _interactionHandler.CreateAndSendButtonsAsync(
            channel,
            buttons,
            "Анонсы",
            "Здесь вы можете подписаться (или отписаться) на различные уведомления!\n\n p.s. я пока что сам не знаю что делает роль Хайпоп, потом разберёмся"
        );
    }
    //обработчик для комманд 
    private static async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        var context = new SocketInteractionContext(_client, interaction);
        await _commands.ExecuteCommandAsync(context, _services);
    }
}
