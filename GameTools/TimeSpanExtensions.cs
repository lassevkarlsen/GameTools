using Humanizer;

namespace GameTools;

public static class TimeSpanExtensions
{
    extension(TimeSpan ts)
    {
        public string Format()
        {
            if (ts.TotalDays >= 1)
            {
                return ts.Humanize(3, minUnit: TimeUnit.Hour);
            }

            if (ts.TotalHours >= 1)
            {
                return ts.Humanize(3, minUnit: TimeUnit.Minute);
            }

            if (ts.TotalMinutes >= 1)
            {
                return ts.Humanize(2, minUnit: TimeUnit.Second);
            }

            return ts.Humanize();
        }
    }
}