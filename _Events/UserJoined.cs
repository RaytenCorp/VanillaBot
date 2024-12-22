using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace VanillaBot;

public class UserJoinHandler
{
    private readonly DiscordSocketClient _client;
    private readonly Config _config;

    public UserJoinHandler(DiscordSocketClient client, Config config)
    {
        _client = client;
        _config = config;
    }

    public void Initialize()
    {
        _client.UserJoined += OnUserJoinedAsync;
        Console.WriteLine("UserJoinHandler инициализирован.");
    }

    private async Task OnUserJoinedAsync(SocketGuildUser user)
    {
        try
        {
            // Проверяем, есть ли у пользователя активный мут
            var sanctions = SanctionManager.GetSanctionsForUser(user.Id);
            
            // Ищем активный мут
            var activeMute = sanctions.FirstOrDefault(s =>
                s.Type == SanctionType.Mute &&
                s.MuteExpiry.HasValue &&
                s.MuteExpiry > DateTime.UtcNow);

            //если есть активный мут - мутим при заходе
            if (activeMute != null)
                await user.AddRoleAsync(user.Guild.GetRole(_config.MuteRoleID));

            //если количество нарушение переваливает за 4 - выдаём роль грязнули
            if (sanctions.Count > 5)
            {
                await user.AddRoleAsync(user.Guild.GetRole(_config.PoopRoleID));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке пользователя {user.Username}: {ex.Message}");
        }
    }

}
