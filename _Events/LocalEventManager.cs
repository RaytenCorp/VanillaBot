using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using System.Threading.Tasks;

namespace VanillaBot
{
    public static class LocalEventManager
    {
        private static DiscordSocketClient? _client;
        private static Config? _config;

        public static void Initialize(DiscordSocketClient client, Config config)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client), "DiscordSocketClient не может быть null.");
            _config = config ?? throw new ArgumentNullException(nameof(config), "Config не может быть null.");
            SubscribeToEvents();
            Console.WriteLine("LocalEventManager инициализирован.");
        }

        // Подписка на события из других менеджеров
        private static void SubscribeToEvents()
        {
            SanctionManager.MuteExpired += OnMuteExpired;
        }

        // При вызове этого события снимается роль заглушён
        private static async void OnMuteExpired(ulong userId)
        {
            if (_client == null || _config == null)
                return;

            // Получаем гильдию по ID
            var guild = _client.GetGuild(_config.GuildId);
            var muteRole = guild.GetRole(_config.MuteRoleID);
            
            if (guild == null)
                return;

            // Получаем пользователя по ID
            var guildUser = guild.GetUser(userId);
            if (guildUser == null)
            {
                Console.WriteLine($"Хотели бы мы его размутить, но пользователя нет на сервере");
                return;
            }

            // Удаляем роль
            await guildUser.RemoveRoleAsync(muteRole);
            Console.WriteLine($"Role 'Mute' has been removed from user {userId}");
        }

    }
}
