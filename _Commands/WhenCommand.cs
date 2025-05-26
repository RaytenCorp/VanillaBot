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
public class WhenCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Config _config;

    public WhenCommand(Config config)
    {
        _config = config;
    }

    [SlashCommand("when", "Узнать когда")]
    public async Task WhenAsync(
        [Summary("сообщение", "Ссылка на сообщение (необязательно)")] string? messageLink = null
    )
    {
        var userSender = Context.User as SocketGuildUser;
        if (userSender == null || (!userSender.Roles.Any(role => role.Id == _config.HOSTRoleID)))
        {
            await RespondAsync("У вас нет прав для выполнения этой команды.", ephemeral: true);
            return;
        }
        if (!string.IsNullOrEmpty(messageLink))
        {
            try
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
                        if (message != null)
                        {
                            await message.ReplyAsync("Hello world");
                            await RespondAsync("Ответ отправлен на сообщение.", ephemeral: true);
                            return;
                        }
                    }
                }
                await RespondAsync("Не удалось найти сообщение по ссылке.", ephemeral: true);
            }
            catch
            {
                await RespondAsync("Неверный формат ссылки на сообщение.", ephemeral: true);
            }
        }
        else
        {
            await RespondAsync("Hello world");
        }
    }


}
