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
    var user = Context.User as SocketGuildUser;
    if (user == null || !user.Roles.Any(role => role.Id == _config.HOSTRoleID))
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
            await FollowupAsync("Ошибка: база данных не найдена.", ephemeral: true);
            return;
        }

        var jsonData = File.ReadAllText(_config.BDpath);
        var db = JObject.Parse(jsonData);
        var guild = Context.Guild;
        var authRole = guild.GetRole(_config.AuthRoleID);
        var notAuthRole = guild.GetRole(_config.NotAuthRoleID);
        
        var allUsers = await guild.GetUsersAsync().FlattenAsync();
        Console.WriteLine($"Пользователей на сервере: {allUsers.Count()}");
        
        foreach (var guildUser in allUsers)
        {
            bool isAuthorized = db.ContainsKey(guildUser.Id.ToString());
            Console.WriteLine($"Пользователь {guildUser.Username} ({guildUser.Id}): {(isAuthorized ? "авторизован" : "не авторизован")}");

            if (isAuthorized)
            {
                if (!guildUser.Roles.Contains(authRole))
                {
                    Console.WriteLine($"Выдача роли авторизованного пользователю {guildUser.Username}");
                    await guildUser.AddRoleAsync(authRole);
                }
                if (guildUser.Roles.Contains(notAuthRole))
                {
                    Console.WriteLine($"Удаление роли неавторизованного у {guildUser.Username}");
                    await guildUser.RemoveRoleAsync(notAuthRole);
                }
            }
            else
            {
                if (!guildUser.Roles.Contains(notAuthRole))
                {
                    Console.WriteLine($"Выдача роли неавторизованного пользователю {guildUser.Username}");
                    await guildUser.AddRoleAsync(notAuthRole);
                }
                if (guildUser.Roles.Contains(authRole))
                {
                    Console.WriteLine($"Удаление роли авторизованного у {guildUser.Username}");
                    await guildUser.RemoveRoleAsync(authRole);
                }
            }
        }

        Console.WriteLine("Проверка авторизации завершена.");
        await FollowupAsync("Проверка авторизации завершена.", ephemeral: true);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при проверке авторизации: {ex.Message}");
        await FollowupAsync("Произошла ошибка при выполнении команды.", ephemeral: true);
    }
}


}
