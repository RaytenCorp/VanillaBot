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

    /*Это основной клиент для взаимодействия с Discord API. Он используется для обработки событий, отправки сообщений и других операций с сервером Discord.*/
    private static DiscordSocketClient _client;

    /*Это сервис, который обрабатывает команды типа Slash (slash-commands) и Context (команды контекстного меню) для бота.*/
    private static InteractionService _commands;

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

        // Настройка клиента с необходимыми Gateway Intents
        var clientConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages
        };

        _client = new DiscordSocketClient(clientConfig);
        _commands = new InteractionService(_client.Rest);

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

        // Инициализация обработчика входа пользователя
        _userJoinHandler = new UserJoinHandler(_client, _config);
        _userJoinHandler.Initialize();
    }

    //обработчик для комманд 
    private static async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        var context = new SocketInteractionContext(_client, interaction);
        await _commands.ExecuteCommandAsync(context, _services);
    }
}
