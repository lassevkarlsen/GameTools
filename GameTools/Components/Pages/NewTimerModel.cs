using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GameTools.Components.Pages;

public class NewTimerModel
{
    private static readonly Regex _secondsPattern = new Regex(@"^(?<value>\d+)\s*s(ec(onds?)?)?$", RegexOptions.IgnoreCase);
    private static readonly Regex _minutesPatterns = new Regex(@"^(?<value>\d+)\s*m(in(utes?)?)?$", RegexOptions.IgnoreCase);
    private static readonly Regex _hoursPattern = new Regex(@"^(?<value>\d+)\s*h(ours?)?$", RegexOptions.IgnoreCase);
    private static readonly Regex _daysPattern = new Regex(@"^(?<value>\d+)\s*d(ays?)?$", RegexOptions.IgnoreCase);
    private static readonly Regex _weeksPattern = new Regex(@"^(?<value>\d+)\s*w(eeks?)?$", RegexOptions.IgnoreCase);

    [Required]
    [MaxLength(100)]
    public string Input { get; set; } = "";

    public bool TryParse(out TimeSpan duration, out string name)
    {
        duration = TimeSpan.Zero;
        name = "";

        if (string.IsNullOrWhiteSpace(Input))
        {
            return false;
        }

        string durationString = Input.Trim();
        string finalName = "";

        int nameEndIndex = durationString.IndexOf(':');
        if (nameEndIndex >= 0)
        {
            finalName = durationString[0..nameEndIndex].Trim();
            durationString = durationString[(nameEndIndex + 1)..].Trim();
        }

        string[] parts = durationString.Split(' ', ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        TimeSpan totalDuration = TimeSpan.Zero;

        (Regex pattern, Action<Match> action)[] matchers = [
            (_secondsPattern, m => totalDuration += TimeSpan.FromSeconds(int.Parse(m.Groups["value"].Value))),
            (_minutesPatterns, m => totalDuration += TimeSpan.FromMinutes(int.Parse(m.Groups["value"].Value))),
            (_hoursPattern, m => totalDuration += TimeSpan.FromHours(int.Parse(m.Groups["value"].Value))),
            (_daysPattern, m => totalDuration += TimeSpan.FromDays(int.Parse(m.Groups["value"].Value))),
            (_weeksPattern, m => totalDuration += TimeSpan.FromDays(7 * int.Parse(m.Groups["value"].Value))),
        ];

        foreach (string part in parts)
        {
            bool wasMatched = false;
            foreach ((Regex pattern, Action<Match> action) matcher in matchers)
            {
                Match match = matcher.pattern.Match(part);
                if (match.Success)
                {
                    matcher.action(match);
                    wasMatched = true;
                    break;
                }
            }

            if (!wasMatched)
            {
                // TODO: How to signal what was wrong back to the user?
                return false;
            }
        }

        if (totalDuration == TimeSpan.Zero)
        {
            // TODO: How to signal what was wrong back to the user?
            return false;
        }

        duration = totalDuration;
        name = finalName;
        return true;
    }

    public void Clear()
    {
        Input = "";
    }
}