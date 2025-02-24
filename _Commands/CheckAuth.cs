using Discord;  
using Discord.Interactions;  
using Discord.WebSocket;  
using Newtonsoft.Json.Linq;  
using System;  
using System.IO;  
using System.Linq;  
using System.Threading.Tasks;

namespace VanillaBot;
public class CheckAuthCommand : InteractionModuleBase<SocketInteractionContext>
{

    private readonly Config _config;
    public CheckAuthCommand(Config config)
    {
        _config = config;
    }
[SlashCommand("checkauth", "ПРОВЕРИТЬ ВСЕХ НА АВТОРИЗАЦИЮ!")]
public async Task CheckAuthAsync()
{
    var usersender = Context.User as SocketGuildUser;
    if (usersender == null || !usersender.Roles.Any(role => role.Id == _config.HOSTRoleID))
    {
        await RespondAsync("У вас нет прав для выполнения этой команды.", ephemeral: true);
        return;
    }
    
    await DeferAsync(); // Заглушка для избежания таймаута
    Console.WriteLine("Команда checkauth запущена.");
    
    try
    {
        if (!File.Exists(_config.BDpath))
        {
            Console.WriteLine($"База данных не найдена по указанному пути: {_config.BDpath}");
            return;
        }
        // Считываем JSON и парсим его
        var jsonData = File.ReadAllText(_config.BDpath);
        var db = JObject.Parse(jsonData);
        var guild = Context.Guild;
        await guild.DownloadUsersAsync();
        int authed=0;
        int unauthed=0;
        foreach (var user in guild.Users)
        {
            Console.WriteLine($"проверяем {user.Id.ToString()}");
            // Проверяем наличие пользователя в базе по ключу
            if (db.ContainsKey(user.Id.ToString()))
            {
                authed++;
                Console.WriteLine($" {user.Id.ToString()} - авторизован");
                await user.AddRoleAsync(user.Guild.GetRole(_config.AuthRoleID));
            }
            else
            {
                unauthed++;
                Console.WriteLine($" {user.Id.ToString()} - НЕ авторизован");
                await user.AddRoleAsync(user.Guild.GetRole(_config.NotAuthRoleID)); // иначе выдаем роль не авторизованного
            }
        }
        await FollowupAsync($"Обновлено {authed + unauthed} пользователей. Авторизованы - {authed}, не авторизованы - {unauthed}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при проверке авторизации: {ex.Message}");
    }
}



}
