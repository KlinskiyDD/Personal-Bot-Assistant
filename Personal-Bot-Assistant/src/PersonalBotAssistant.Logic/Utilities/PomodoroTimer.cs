using System;
using System.Timers;
using Timer = System.Timers.Timer;

namespace PersonalBotAssistant.Logic.Utilities
{
    public class ResultPomodoro
    {
        public long UserId { get; set; }
        public string Message { get; set; }
    }
    public class PomodoroTimer: IDisposable
    {
        private Timer timer;
        private int workDurationMinutes=0; // Длительность работы в минутах
        private int breakDurationMinutes=0; // Длительность перерыва в минутах
        private int minutesRemaining = 0;
        private bool isWorking;
        private long userId;

        public event Action<ResultPomodoro> TimerTick; // Событие, вызываемое на каждом тике таймера

        public PomodoroTimer()
        {

        }

        public PomodoroTimer(string workDuration, string breakDuration, long userId)
        {
            workDurationMinutes = ParseTimeToMinutes(workDuration) * 60;
            breakDurationMinutes = ParseTimeToMinutes(breakDuration) * 60;
            minutesRemaining = workDurationMinutes;
            isWorking = true;
            this.userId = userId;
            timer = new System.Timers.Timer(1000); // Таймер с интервалом в 1 секунду (1 000 миллисекунд)
            timer.Elapsed += TimerElapsed;
        }

        private int ParseTimeToMinutes(string timeString)
        {
            if (TimeSpan.TryParse(timeString, out TimeSpan timeSpan))
            {
                return (int)timeSpan.TotalMinutes;
            }
            return 0; // Вернуть 0 в случае некорректного формата времени
        }

        public void Start()
        {
            minutesRemaining = isWorking ? workDurationMinutes : breakDurationMinutes;
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        public void Configure(string workDuration, string breakDuration)
        {
            workDurationMinutes = ParseTimeToMinutes(workDuration);
            breakDurationMinutes = ParseTimeToMinutes(breakDuration);
            minutesRemaining = workDurationMinutes;
            isWorking = true;
            Stop();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            minutesRemaining--;

            if (minutesRemaining <= 0)
            {
                if (isWorking)
                {
                    isWorking = false;
                    minutesRemaining = breakDurationMinutes;
                }
                else
                {
                    isWorking = true;
                    minutesRemaining = workDurationMinutes;
                }
            }

            //string message = isWorking ? "Работа" : "Перерыв";
            //var result = new ResultPomodoro
            //{
            //    UserId = userId,
            //    Message = $"{message}: {minutesRemaining} мин."
            //};
            //TimerTick?.Invoke(result);
        }

        public int GetRemaining()
        {
            return minutesRemaining;
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Освобождаем управляемые ресурсы
                    timer.Dispose();
                }

                // Освобождаем неуправляемые ресурсы
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
