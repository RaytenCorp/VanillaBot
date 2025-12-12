using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace VanillaBot;

public class SleepCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Random _rng = new();

    private readonly string[] _responses =
    {
        "А, что? Я не сплю!",
        "Не-не, я бодрствую! Наверное...",
        "Кто посмел разбудить Ванильку?!",
        "Я только глаза закрыла!",
        "Нет, это не храп, это вентиляция!",
        "Сплю? Я? Никогда!",
        "Проснулась... что случилось?",
        "Я вообще-то работаю! Просто очень тихо.",
        "Ну может чууть-чуть дремала...",
        "А я и не засыпала, если что!",
        "Почему все думают, что я сплю?",
        "Если бы я спала — ты бы не узнал!",
        "Я в режиме энергосбережения!",
        "Не сплю, не сплю, не сплю!",
        "Меня невозможно усыпить!",
        "Эй, я бдю!",
        "Уф… ладно, я спала. Чего надо?",
        "Я отдыхаю глазами, это другое!",
        "Сон для слабых, а я сильная!",
        "Не сплю… *зевает* …совсем…"
    };

    [SlashCommand("issleep", "привет ванилька ты спишь?")]
    public async Task WhenAsync([Summary("сообщение")] string message)
    {
        string response = _responses[_rng.Next(_responses.Length)];
        await RespondAsync(response);
    }
}
