using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;

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
        _client.UserJoined += CheckMute;
        _client.UserJoined += Subscribe;
        // _client.UserJoined += CheckAuth;
        Console.WriteLine("UserJoinHandler инициализирован.");
    }

    private async Task Subscribe(SocketGuildUser user)
    {
        try
        {
            await user.AddRoleAsync(user.Guild.GetRole(_config.EventsRoleID));//ивенты
            await user.AddRoleAsync(user.Guild.GetRole(_config.HighPopRoleID));//хайпоп
            await user.AddRoleAsync(user.Guild.GetRole(_config.NewsRoleID));//новости
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке пользователя {user.Username}: {ex.Message}");
        }
    }

    private async Task CheckAuth(SocketGuildUser user)
    {
        try
        {
            // Проверяем, существует ли файл базы данных
            if (!File.Exists(_config.BDpath))
            {
                Console.WriteLine($"База данных не найдена по указанному пути: {_config.BDpath}");
                return;
            }

            // Считываем JSON и парсим его
            var jsonData = File.ReadAllText(_config.BDpath);
            var db = JObject.Parse(jsonData);

            // Проверяем наличие пользователя в базе по ключу
            if (db.ContainsKey(user.Id.ToString()))
                await user.AddRoleAsync(user.Guild.GetRole(_config.AuthRoleID));// выдаем роль авторизованного
            else
                await user.AddRoleAsync(user.Guild.GetRole(_config.NotAuthRoleID));//иначе выдаем роль не авторизованного
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при проверке авторизации: {ex.Message}");
        }
    }


    private async Task CheckMute(SocketGuildUser user)
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
