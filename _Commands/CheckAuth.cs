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
            
            foreach (var guildUser in guild.Users)
            {
                if (db.ContainsKey(guildUser.Id.ToString()))
                {
                    if (!guildUser.Roles.Contains(authRole))
                        await guildUser.AddRoleAsync(authRole);
                    if (guildUser.Roles.Contains(notAuthRole))
                        await guildUser.RemoveRoleAsync(notAuthRole);
                }
                else
                {
                    if (!guildUser.Roles.Contains(notAuthRole))
                        await guildUser.AddRoleAsync(notAuthRole);
                    if (guildUser.Roles.Contains(authRole))
                        await guildUser.RemoveRoleAsync(authRole);
                }
            }

            await FollowupAsync("Проверка авторизации завершена.", ephemeral: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при проверке авторизации: {ex.Message}");
            await FollowupAsync("Произошла ошибка при выполнении команды.", ephemeral: true);
        }
    }
}
