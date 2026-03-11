namespace GameTools.Components.Pages.NoMansSky;

public class NewGuildSystemModel
{
    public string SystemName { get; set; } = "";
    public string GuildName { get; set; } = "";

    public string ErrorMessage { get; set; } = "";

    public void Clear()
    {
        SystemName = "";
        GuildName = "";
        ErrorMessage = "";
    }
}