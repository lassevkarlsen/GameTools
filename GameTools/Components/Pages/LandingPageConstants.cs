namespace GameTools.Components.Pages;

public static class LandingPageConstants
{
    public const string PreferenceKey = "landing-page";

    public static class Values
    {
        public const string Home = "home";
        public const string Timers = "timers";
        public const string ShoppingList = "shopping-list";
        public const string ProfileConfiguration = "profile-configuration";
        public const string UpgradeGems = "upgrade-gems";
        public const string Diablo4EventTimers = "diablo4-eventtimers";
        public const string NoMansSkyPortalAddresses = "nms-portal-addresses";
        public const string NoMansSkyGuildRewards = "nms-guild-rewards";
    }

    public const string DefaultValue = Values.Home;

    public static string ToRelativePath(string? value)
    {
        return value switch
        {
            Values.Home => string.Empty,
            Values.Timers => "timers",
            Values.ShoppingList => "shopping",
            Values.ProfileConfiguration => "profile",
            Values.UpgradeGems => "enshrouded/gems",
            Values.Diablo4EventTimers => "diablo4/eventtimers",
            Values.NoMansSkyPortalAddresses => "nms/portaladdresses",
            Values.NoMansSkyGuildRewards => "nms/guildrewards",
            _ => string.Empty,
        };
    }
}