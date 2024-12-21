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
    /*Это основной клиент для взаимодействия с Discord API. Он используется для обработки событий, отправки сообщений и других операций с сервером Discord.*/
    private static DiscordSocketClient _client = new DiscordSocketClient();

    /*Это сервис, который обрабатывает команды типа Slash (slash-commands) и Context (команды контекстного меню) для бота.*/
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

        // Проверка, что конфигурация загружена и токен не пустой
        if (_config?.Token == null)
        {
            Console.WriteLine("Токен не найден в конфигурации. Пожалуйста, укажите токен.");
            return;
        }

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

    // Метод, который вызывается, когда клиент готов
    private static async Task ReadyAsync()
    {
        // Регистрация команд через InteractionService
        await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _services); // Используем _commands для регистрации команд
        
        if (_config?.GuildId != null)
            await _commands.RegisterCommandsToGuildAsync(_config.GuildId);
        else
            Console.WriteLine("GuildId не указан в конфигурации. Команды не зарегистрированы.");

        Console.WriteLine("Команды зарегистрированы.");
    }

    //обработчик для комманд 
    private static async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        var context = new SocketInteractionContext(_client, interaction);
        await _commands.ExecuteCommandAsync(context, _services);
    }
}
