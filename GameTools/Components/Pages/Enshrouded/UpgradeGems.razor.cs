namespace GameTools.Components.Pages.Enshrouded;

public partial class UpgradeGems
{
    private int _currentGemLevel = 1;
    private bool _useSpringlandsNightSanctum = true;
    private bool _useRevelwoodNightSanctum = true;
    private bool _useNomadHighlandsNightSanctum = true;
    private bool _useKindlewastesNightSanctum = true;
    private bool _useAlbaneveSummitsNightSanctum = true;
    private bool _useVeilwaterBasinNightSanctum = true;

    private List<Upgrade> _upgrades = [];

    protected override async Task OnInitializedAsync()
    {
        await OnChanged();
    }

    private Task OnChanged()
    {
        List<UpgradePath> paths = [];

        paths.Add(_useSpringlandsNightSanctum
            ? new UpgradePath("Springlands Night Sanctum Gem Forge", 5, 1, 11, [11, 22, 32, 43, 54, 65, 75, 86, 97, 108, 118])
            : new UpgradePath("Springlands Gem Forge", 5, 1, 9, [13, 26, 39, 52, 65, 78, 91, 104, 117]));

        paths.Add(_useRevelwoodNightSanctum
            ? new UpgradePath("Revelwood Night Sanctum Gem Forge", 13, 2, 17, [8, 16, 24, 33, 41, 49, 57, 65, 73, 81, 89, 98, 106, 114, 122, 130, 138])
            : new UpgradePath("Revelwood Gem Forge", 13, 2, 14, [10, 19, 29, 39, 48, 58, 68, 77, 87, 97, 106, 116, 126, 135]));

        paths.Add(_useNomadHighlandsNightSanctum
            ? new UpgradePath("Nomad Highlands Night Sanctum Gem Forge", 18, 2, 21, [7, 14, 20, 27, 34, 41, 48, 54, 61, 68, 75, 81, 88, 95, 102, 109, 115, 122, 129, 136, 143])
            : new UpgradePath("Nomad Highlands Gem Forge", 18, 2, 19, [8, 16, 24, 32, 40, 48, 56, 64, 72, 80, 88, 96, 104, 112, 120, 128, 136, 144, 152]));

        paths.Add(_useKindlewastesNightSanctum
            ? new UpgradePath("Kindlewastes Night Sanctum Gem Forge", 25, 2, 31,
                [5, 10, 16, 21, 26, 31, 36, 42, 47, 52, 57, 62, 68, 73, 78, 83, 89, 94, 99, 104, 109, 115, 120, 125, 130, 135, 141, 146, 151, 156, 161])
            : new UpgradePath("Kindlewastes Gem Forge", 25, 2, 29, [6, 13, 19, 25, 32, 38, 44, 51, 57, 63, 70, 76, 82, 89, 95, 101, 108, 114, 120, 127, 133, 139, 146, 152, 158, 165, 171, 177, 184]));

        _upgrades.Clear();
        int currentLevel = _currentGemLevel;

        for (int index = 0; index < paths.Count; index++)
        {
            if (currentLevel >= paths[index].FromLevel && currentLevel <= paths[index].ToLevel)
            {
                int cost = 0;
                for (int upgradeIndex = currentLevel - paths[index].FromLevel; upgradeIndex < paths[index].UpgradeCosts.Length; upgradeIndex++)
                {
                    cost += paths[index].UpgradeCosts[upgradeIndex];
                }

                _upgrades.Add(new Upgrade(paths[index].GemForge, paths[index].ArchaicEssenceLevel, currentLevel, paths[index].ToLevel + 1, cost));
                currentLevel = paths[index].ToLevel + 1;
            }
        }
        return Task.CompletedTask;
    }
}