using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VanillaBot;

public class SayCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Config _config;
    public SayCommand(Config config)
    {
        _config = config;
    }

    [SlashCommand("say", "Скажи что-нибудь")]
    public async Task WhenAsync(
        [Summary("Текст", "Текст")] string messageText,
        [Summary("сообщение", "Ссылка на сообщение (необязательно)")] string? messageLink = null
    )
    {
        var userSender = Context.User as SocketGuildUser;
        if (userSender == null || (!userSender.Roles.Any(role => role.Id == _config.HOSTRoleID)))
        {
            await RespondAsync($"Мне запрещено вам отвечать.");
        }
        if (!string.IsNullOrEmpty(messageLink))
        {
            var parts = messageLink.Split('/');
            if (parts.Length >= 3)
            {
                ulong channelId = ulong.Parse(parts[^2]);
                ulong messageId = ulong.Parse(parts[^1]);

                var channel = Context.Client.GetChannel(channelId) as IMessageChannel;
                if (channel != null)
                {
                    var message = await channel.GetMessageAsync(messageId);
                    if (message is IUserMessage userMessage)
                    {
                        await userMessage.ReplyAsync(messageText);
                        return;
                    }
                }
            }
        }
    }
}
