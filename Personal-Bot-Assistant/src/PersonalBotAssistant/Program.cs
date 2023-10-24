using GraphBot.BotHost;
using GraphBot.BotHost.Modules.Loggers;
using GraphBot.Telegram;
using TimeAccelerator;

namespace PersonalBotAssistant
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //TODO Подключить конфиг c ИД бота (5943435074:AAHlrivwQkWr1Og33SqIUpZx47ljdH9Ftc8)
            var botHost = new BotHostBuilder<TelegramAdapter>()
                .AddDefaultTelegramBotHost()
                .SetLogger(new DefaultSerilog(new RealDateTimeOffset()))
                .SetLogic(new Logic.Logic())
                .Build();

            // Нужен для того чтобы быстро получить ИД чатов для Ошибок и статусов пользователей.
            botHost.BotAdapter.OnBotMemberStatusChanged += (_, statusArgs) =>
            {
                var chatId = statusArgs.ChatId; //TODO в конфиг добавить ИД чатов
            };

            botHost.Run();
        }
    }
}