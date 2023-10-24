using GraphBot.BotHost.Interfaces;
using GraphBot.Logic;
using GraphBot.MessageContents;
using GraphBot.Telegram;
using Microsoft.Extensions.Configuration;
using PersonalBotAssistant.Logic.Utilities;
using Splat;
using Telegram.Bot.Types;

namespace PersonalBotAssistant.Logic
{
    public class Logic : ILogic
    {
        private PomodoroTimer pomodoroTimer { get; set; }
        private TelegramAdapter tgAdapter { get; set; }
        public void BindFunctions(ChatAdapter botAdapter)
        {
            var configuration = Locator.Current.GetService<IConfiguration>();
            pomodoroTimer = new PomodoroTimer();
            tgAdapter = botAdapter as TelegramAdapter;
            //if (tgAdapter != null)
            //{
            //    tgAdapter.
            //}

            //botAdapter.Functions.Add("Проверка настроек", (userId, message) =>
            //{
            //    ResetSettings(botAdapter, userId);
            //    return "";//1 times
            //});

            botAdapter.Functions.Add("Проверка настроек", (userId, message) =>
            {
                var workTime = botAdapter.ChatBotState.GetUserData(userId, "Время работы");
                var delayTime = botAdapter.ChatBotState.GetUserData(userId, "Время отдыха");
                var btnPomidor = botAdapter.ChatBotState.GetUserData(userId, "Кнопка помидора");
                var statusPomidor = botAdapter.ChatBotState.GetUserData(userId, "Статус помидора");

                if (Validator.IsValidTimeFormat(workTime)
                    && Validator.IsValidTimeFormat(delayTime)
                    && (btnPomidor is "Включить" or "Выключить")
                    && (statusPomidor is "Включен" or "Выключен"))
                {

                    botAdapter.ChatBotState.SetUserData(userId, "Статус помидора", statusPomidor);
                    botAdapter.ChatBotState.SetUserData(userId, "Кнопка помидора", btnPomidor);
                    botAdapter.ChatBotState.SetUserData(userId, "Время работы", Validator.ConvertTime(workTime));
                    botAdapter.ChatBotState.SetUserData(userId, "Время отдыха", Validator.ConvertTime(delayTime));
                    return "Да";
                }
                return "Нет";
            });

            botAdapter.Functions.Add("Вкл-Выкл помидор", (userId, message) =>
            {
                // -> AnswerButton "Включить\Выключить"
                var status = botAdapter.ChatBotState.GetUserData(userId, "Статус помидора");
                if (!string.IsNullOrEmpty(status))
                {
                    var flag = status switch
                    {
                        "Выключен" => false,
                        "Включен" => true,
                        _ => false
                    };

                    if (flag)
                    {
                        botAdapter.ChatBotState.SetUserData(userId, "Статус помидора", "Выключен");
                        botAdapter.ChatBotState.SetUserData(userId, "Кнопка помидора", "Включить");

                    }
                    else
                    { 
                        botAdapter.ChatBotState.SetUserData(userId, "Статус помидора", "Включен");
                        botAdapter.ChatBotState.SetUserData(userId, "Кнопка помидора", "Выключить");


                        //Запускаем помидор
                        var workTime = botAdapter.ChatBotState.GetUserData(userId, "Время работы");
                        var delayTime = botAdapter.ChatBotState.GetUserData(userId, "Время отдыха");
                        pomodoroTimer = new PomodoroTimer(workTime, delayTime, userId);
                        pomodoroTimer.TimerTick += PomodoroTimer_TimerTick;
                        pomodoroTimer.Start();

                        var formattedTime = FormattedTime(pomodoroTimer.GetRemaining());

                        botAdapter.ChatBotState.SetUserData(userId, "оставшиеся время", formattedTime);
                    }
                }
                return "Да";//1 times
            });
            
            botAdapter.Functions.Add("Таймер обновление", (userId, message) =>
            {
                var formattedTime = FormattedTime(pomodoroTimer.GetRemaining());

                botAdapter.ChatBotState.SetUserData(userId, "оставшиеся время", formattedTime);
                return "Да";//1 times
            });



            botAdapter.Functions.Add("Ввод времени", (userId, message) =>
            {
                var workTime = Validator.IsValidTimeFormat(botAdapter.ChatBotState.GetUserData(userId, "Время работы"));
                var delayTime = Validator.IsValidTimeFormat(botAdapter.ChatBotState.GetUserData(userId, "Время отдыха"));

                if (!workTime)
                {
                    botAdapter.ChatBotState.SetUserData(userId, "Время работы", "00:30");
                    return "Плохое время работы";//1 times
                }
                else if (!delayTime)
                {
                    botAdapter.ChatBotState.SetUserData(userId, "Время отдыха", "00:05");
                    return "Плохое время отдыха";//1 times
                }

                ResetSettings(botAdapter, userId);
                return "Обнуление";//1 times

            });

            botAdapter.Functions.Add("Сброс настроек", (userId, message) =>
            {
                ResetSettings(botAdapter, userId);
                return "";//1 times
            });

            botAdapter.Functions.Add("Авторизация", (userId, message) =>
            {
                var userName = botAdapter.ChatBotState.GetUserData(userId, "UserName");
                return string.IsNullOrEmpty(userName) ? "Нет" : "Да";
            });

           

        }

        private string FormattedTime(int seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            var formattedTime = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            return formattedTime;
        }

        private void PomodoroTimer_TimerTick(ResultPomodoro obj)
        {
            IMessageContent message = new MessageContent()
            {
                Text = obj.Message,
            };
            tgAdapter.SendMessage(obj.UserId, message, null);
        }

        private static void ResetSettings(ChatAdapter botAdapter, long userId)
        {
            botAdapter.ChatBotState.SetUserData(userId, "Время работы", "00:30");
            botAdapter.ChatBotState.SetUserData(userId, "Время отдыха", "00:05");
        }

    }
}