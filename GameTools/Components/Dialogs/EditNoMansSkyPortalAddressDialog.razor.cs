using GameTools.Database;

using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

using Radzen;

namespace GameTools.Components.Dialogs;

public partial class EditNoMansSkyPortalAddressDialog
{
    private readonly DialogService _dialogService;
    private readonly IDbContextFactory<GameToolsDbContext> _dbContextFactory;

    [Parameter]
    public ViewModel Model { get; set; } = new();

    private List<NoMansSkyGalaxy> _galaxies = [];

    public EditNoMansSkyPortalAddressDialog(DialogService dialogService, IDbContextFactory<GameToolsDbContext> dbContextFactory)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        _galaxies = await dbContext.NoMansSkyGalaxies.OrderBy(g => g.Id).ToListAsync();
    }

    private void SaveInformation()
    {
        // Validation
        _dialogService.Close(true);
    }

    private void CancelEdit()
    {
        _dialogService.Close(false);
    }
}