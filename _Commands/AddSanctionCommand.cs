using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace VanillaBot;

public class SanctionCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Config _config;

    // Конструктор принимает Config для настройки
    public SanctionCommand(Config config)
    {
        _config = config;
    }

    [SlashCommand("addsanction", "Жёстко наказать пользователя")]
    public async Task SanctionAsync(
        [Summary("user", "Пользователь, которого наказываем")] IUser user,
        [Summary("description", "Описание, обязательно заполните его НОРМАЛЬНО")] string description)
    {

        //прежде всего очищаем старые санкции
        SanctionManager.RemoveExpiredSanctions();

        // Проверка, имеет ли пользователь разрешение на выполнение команды
        var SanctionerUser = Context.User as SocketGuildUser;
        if (SanctionerUser == null || !SanctionerUser.Roles.Any(role => _config.RoleSanctionPermissions.Contains(role.Id)))
        {
            await RespondAsync("Данную команду может выполнять только караульный, часовой, начальник караула и администратор.", ephemeral: true);
            return;
        }


        var userId = user.Id;
        Console.WriteLine($"мутим {userId}");
        var guildUser = Context.Guild.GetUser(userId);
        if (guildUser == null)
        {
            Console.WriteLine($"Пользователь не найден в гильдии");
            return;
        }


        // берем данные мутов пользователя
        var userSanctions = SanctionManager.GetSanctionsForUser(userId);

        SanctionType sanctionType;
        DateTime? muteExpiry = null;

        string sanctionDetails = "";
        int sanctionNumber = await CounterManager.GetNextCounterAsync("sanctionCounter");
        switch (userSanctions.Count)
        {
            case 0:
                sanctionType = SanctionType.Warn;
                SanctionManager.AddSanction(userId, description, SanctionType.Warn, sanctionNumber);
                sanctionDetails = "предупреждение";
                break;
            case 1:
                sanctionType = SanctionType.Mute;
                muteExpiry = DateTime.UtcNow.AddDays(3);
                SanctionManager.AddSanction(userId, description, SanctionType.Mute, sanctionNumber, muteExpiry);
                sanctionDetails = "Мут на 3 дня";
                break;
            case 2:
                sanctionType = SanctionType.Mute;
                muteExpiry = DateTime.UtcNow.AddDays(5);
                SanctionManager.AddSanction(userId, description, SanctionType.Mute, sanctionNumber, muteExpiry);
                sanctionDetails = "Мут на 5 дней";
                break;
            case 3:
                sanctionType = SanctionType.Mute;
                muteExpiry = DateTime.UtcNow.AddDays(8);
                SanctionManager.AddSanction(userId, description, SanctionType.Mute, sanctionNumber, muteExpiry);
                sanctionDetails = "Мут на 8 дней";
                break;
            case 4:
                sanctionType = SanctionType.Mute;
                muteExpiry = DateTime.UtcNow.AddDays(13);
                SanctionManager.AddSanction(userId, description, SanctionType.Mute, sanctionNumber, muteExpiry);
                await guildUser.AddRoleAsync(Context.Guild.GetRole(_config.PoopRoleID)); //на 5 раз выдаем роль грязнули
                sanctionDetails = "Мут на 13 дней, а также особая роль! Следующее наказание приведёт к бану.";
                break;
            case 5:
                sanctionType = SanctionType.Mute;
                muteExpiry = DateTime.UtcNow.AddDays(21);
                SanctionManager.AddSanction(userId, description, SanctionType.Mute, sanctionNumber, muteExpiry);
                sanctionDetails = "Мут на 21 день. На рассмотрении об исключении из сообщества.";
                break;
            default:
                await RespondAsync($"У {user.Mention} Уже перебор по наказаниям. Почему он все еще не в бане? Сообщите начальнику караула", ephemeral: true);
                return;
        }

        // Если мут - мутим
        if (sanctionType == SanctionType.Mute && muteExpiry.HasValue)
                await guildUser.AddRoleAsync(Context.Guild.GetRole(_config.MuteRoleID));

        Color embedColor;

        switch (userSanctions.Count)
        {
            case 0:
                embedColor = new Color(0, 0, 255); // Синий
                break;
            case 1:
            case 2:
                embedColor = new Color(255, 255, 0); // Жёлтый
                break;
            case 3:
            case 4:
            case 5:
                int greenValue = (int)(255 - (userSanctions.Count - 2) * 51.5); //постепенное покраснение
                embedColor = new Color(255, greenValue, 0);
                break;
            case 6:
                embedColor = new Color(255, 0, 0); //красный
                break;
            default:
                embedColor = new Color(0, 0, 255); //этого цвета не должно быть
                break;
        }
        await RespondAsync($"Выдано {userSanctions.Count + 1} наказание {user.Mention}: {sanctionType}. Причина: {description}", ephemeral: true);


        // Создание embed-сообщения для отправки в канал ReportChannelId
        var embed = new EmbedBuilder()
            .WithTitle($"Наказание № {sanctionNumber}")
            .WithDescription($"{user.Mention} получил **{userSanctions.Count + 1}** наказание")
            .AddField("Причина:", $"{description}")
            .WithColor(embedColor)
            .WithTimestamp(DateTime.UtcNow)
            .AddField("Наказание:", $"{sanctionDetails}")
            .AddField("Исполнивший:", SanctionerUser?.Mention)
            .Build();

        // Отправка сообщения в канал ReportChannelId
        var reportChannel = Context.Guild.GetTextChannel(_config.SanctionChannelID);
        await reportChannel.SendMessageAsync(embed: embed);
    }
}
