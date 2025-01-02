using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VanillaBot;

// Интерфейс для всех обработчиков кнопок
public interface IButtonHandler
{
    Task HandleAsync(SocketMessageComponent component);
}

// Класс для регистрации и вызова обработчиков кнопок
public class ButtonHandlerRegistry
{
    private readonly Dictionary<string, IButtonHandler> _handlers;

    public ButtonHandlerRegistry()
    {
        _handlers = new Dictionary<string, IButtonHandler>();
    }

    public void RegisterHandler(string buttonId, IButtonHandler handler)
    {
        _handlers[buttonId] = handler;
    }

    public async Task HandleButtonAsync(SocketMessageComponent component)
    {
        var buttonId = component.Data.CustomId;

        if (_handlers.TryGetValue(buttonId, out var handler))
        {
            await handler.HandleAsync(component);
        }
        else
        {
            await component.RespondAsync($"Неизвестная кнопка: {buttonId}", ephemeral: true);
        }
    }
}