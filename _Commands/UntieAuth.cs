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
public class UntieAuthCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Config _config;

    public UntieAuthCommand(Config config)
    {
        _config = config;
    }

    [SlashCommand("untieauth", "Отвязать аккаунт")]
    public async Task UntieAuthAsync(
        [Summary("аккаунт", "у кого отвязываем?")]
        IUser account
    )
    {
        var userSender = Context.User as SocketGuildUser;
        if (userSender == null || 
            (!userSender.Roles.Any(role => role.Id == _config.HOSTRoleID) &&
             !userSender.Roles.Any(role => role.Id == _config.ChiefOfGuardRoleID)))
        {
            await RespondAsync("У вас нет прав для выполнения этой команды.", ephemeral: true);
            return;
        }

        try
        {
            if (!File.Exists(_config.BDpath))
            {
                Console.WriteLine($"База данных не найдена по указанному пути: {_config.BDpath}");
                await RespondAsync("База данных не найдена.", ephemeral: true);
                return;
            }

            // Считываем JSON и парсим его
            var jsonData = File.ReadAllText(_config.BDpath);
            var db = JObject.Parse(jsonData);

            if (db.ContainsKey(account.Id.ToString()))
            {
                db.Remove(account.Id.ToString());
                File.WriteAllText(_config.BDpath, db.ToString(Formatting.Indented));

                // Снимаем роль и добавляем новую
                var guildUser = Context.Guild.GetUser(account.Id);
                if (guildUser != null)
                {
                    var addedRole = Context.Guild.GetRole(_config.AuthRoleID);
                    var rmRole = Context.Guild.GetRole(_config.NotAuthRoleID); 

                    if (addedRole != null && guildUser.Roles.Contains(addedRole))
                    {
                        await guildUser.RemoveRoleAsync(addedRole);
                    }

                    if (rmRole != null && !guildUser.Roles.Contains(rmRole))
                    {
                        await guildUser.AddRoleAsync(rmRole);
                    }
                }

                await RespondAsync($"Аккаунт <@{account.Id}> успешно отвязан, роль обновлена.");
            }
            else
            {
                await RespondAsync("Аккаунт не привязан.", ephemeral: true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при отвязке аккаунта: {ex.Message}");
            await RespondAsync("Произошла ошибка при отвязке аккаунта.", ephemeral: true);
        }
    }
}
