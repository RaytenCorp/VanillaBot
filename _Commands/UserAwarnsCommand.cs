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
        var targetUser = user;
        var userId = targetUser.Id;

        // Получаем все аварны пользователя
        var userWarns = await WarnManager.GetUserWarnsAsync(userId);

        if (userWarns == null || userWarns.Count == 0)
        {
            await RespondAsync($"{targetUser.Username} не имеет аварнов.", ephemeral: true);
            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle($"Аварны пользователя {targetUser.Username}")
            .WithColor(Color.Blue)
            .WithTimestamp(DateTimeOffset.Now);

        // Добавляем информацию о каждом аварне
        foreach (var warn in userWarns)
        {
            // Дата сгорания - через 3 месяца после даты получения аварна
            var expirationDate = warn.WarnDate.AddDays(90);

            embed.AddField($"Аварн от {warn.WarnDate:dd MMM yyyy}",
                $"Дата окончания: {expirationDate:dd MMM yyyy}",
                false);
        }

        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }
}
