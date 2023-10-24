using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalBotAssistant.Logic.Utilities
{
    public static class Validator
    {
        public static bool IsValidTimeFormat(string timeString)
        {
            if (string.IsNullOrEmpty(timeString)) return false;
            if (timeString.Contains(":"))
            {
                if (TimeSpan.TryParse(timeString, out TimeSpan timeSpan))
                {
                    timeString = timeSpan.TotalMinutes.ToString();
                }
            }

            if (int.TryParse(timeString, out var min))
            {
                if (min is >= 0 and < 1440)
                {
                    return true;
                }
            }

            return false;
        }

        public static string ConvertTime(string timeString)
        {
            if (timeString.Contains(":"))
            {
                timeString = TimeSpan.TryParse(timeString, out TimeSpan timeSpan) ? timeSpan.ToString(@"hh\:mm") : "Некорректный формат";
            }
            else
            {
                timeString = int.TryParse(timeString, out var min) ? $"{min / 60:D2}:{min % 60:D2}" : "Некорректный формат";
            }

            return timeString;
        }
    }
}
