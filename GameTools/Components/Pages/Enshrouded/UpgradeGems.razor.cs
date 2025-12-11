using Microsoft.AspNetCore.Components;

namespace GameTools.Components.Pages.Enshrouded;

public partial class UpgradeGems
{
    private int _currentGemLevel = 1;
    private bool _useSpringlandsNightSanctum = false;
    private bool _useRevelwoodNightSanctum = false;
    private bool _useNomadHighlandsNightSanctum = false;
    private bool _useKindlewastesNightSanctum = false;
    private bool _useAlbaneveSummitsNightSanctum = false;
    private bool _useVeilwaterBasinNightSanctum = false;

    [CascadingParameter]
    public Action<string>? SetPageTitle { get; set; }

    private List<Upgrade> _upgrades = [];

    protected override async Task OnInitializedAsync()
    {
        await OnChanged();
        await base.OnInitializedAsync();

        SetPageTitle?.Invoke("Enshrouded :: Upgrade Gems");
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

        paths.Add(_useAlbaneveSummitsNightSanctum
            ? new UpgradePath("Albaneve Summits Night Sanctum Gem Forge", 33, 2, 39,
                [4, 9, 13, 18, 22, 26, 31, 35, 40, 44, 48, 53, 57, 62, 66, 70, 75, 79, 84, 88, 92, 97, 101, 106, 110, 114, 119, 123, 128, 132, 136, 141, 145, 150, 154, 158, 163, 167, 172])
            : new UpgradePath("Albaneve Summits Gem Forge", 33, 2, 34,
                [6, 12, 18, 23, 29, 35, 41, 47, 53, 59, 64, 70, 76, 82, 88, 94, 100, 105, 111, 117, 123, 129, 135, 141, 146, 152, 158, 164, 170, 176, 182, 187, 193, 199]));

        paths.Add(_useVeilwaterBasinNightSanctum
            ? new UpgradePath("Veilwater Basin Night Sanctum Gem Forge", 40, 2, 49,
            [
                4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48, 52, 56, 60, 64, 68, 72, 76, 80, 84, 88, 92, 96, 100, 104, 108, 112, 116, 120, 124, 128, 132, 136, 140, 144, 148, 152, 156, 160, 164, 168,
                172, 176, 180, 184, 188, 192, 200
            ])
            : new UpgradePath("Veilwater Basin Gem Forge", 40, 2, 49,
            [
                4, 10, 15, 20, 25, 30, 36, 41, 46, 51, 56, 62, 67, 72, 82, 88, 93, 98, 103, 108, 114, 119, 124, 129, 134, 140, 145, 150, 155, 160, 166, 171, 176, 181, 186, 192, 197, 202, 207, 212,
                218, 223, 228, 233, 238, 244, 249, 254
            ]));

        List<Upgrade> upgrades = [];
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

                upgrades.Add(new Upgrade(paths[index].GemForge, paths[index].ArchaicEssenceLevel, currentLevel, paths[index].ToLevel + 1, cost));
                currentLevel = paths[index].ToLevel + 1;
            }
        }

        _upgrades = upgrades;
        return Task.CompletedTask;
    }
}