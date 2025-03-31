using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VanillaBot;
public class UserAwarnsCommand : InteractionModuleBase<SocketInteractionContext>
{

    [SlashCommand("userawarns", "Показать все аварны указанного пользователя и их даты сгорания.")]
    public async Task UserAwarnsAsync([Summary("пользователь", "Укажите пользователя для проверки варнов.")] IUser user)
    {
        // Получаем все аварны пользователя
        var userWarns = AWarnManager.GetAwarnsForUser(user.Id);

        if (userWarns.Count == 0)
        {
            await RespondAsync($"{user.Username} не имеет аварнов.", ephemeral: true);
            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle($"Аварны пользователя {user.Username}")
            .WithColor(Color.Blue)
            .WithTimestamp(DateTimeOffset.Now);

        // Добавляем информацию о каждом аварне
        foreach (var warn in userWarns)
        {
            var expirationDate = warn.AwarnDate.AddDays(90);

           string Typeofawarn = warn.Type == AwarnType.FullWarn ? "Аварн" : "Полуварн";

            embed.AddField($"Аварн от {warn.AwarnDate:dd MMM yyyy}",
                $"Дата окончания: {expirationDate:dd MMM yyyy}",
                false);
            embed.AddField($"Причина: {warn.Reason}",
                $"Тип: {Typeofawarn}",
                false);
        }

        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }
}
