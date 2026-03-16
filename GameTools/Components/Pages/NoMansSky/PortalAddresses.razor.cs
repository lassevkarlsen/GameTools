using System.Globalization;
using System.Text;

using GameTools.Components.Dialogs;
using GameTools.Database;

using LVK.Events;

using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

using Radzen;
using Radzen.Blazor;

namespace GameTools.Components.Pages.NoMansSky;

public partial class PortalAddresses
{
    private readonly IDbContextFactory<GameToolsDbContext> _dbContextFactory;
    private readonly IEventBus _eventBus;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly DialogService _dialogService;
    private readonly ILogger<PortalAddresses> _logger;
    private readonly IJSRuntime _jsRuntime;
    private readonly NotificationService _notificationService;
    private List<NoMansSkyPortalAddress> _addresses = [];
    private string _filter = "";

    private CancellationTokenSource? _filterDebounceCts;

    public PortalAddresses(IDbContextFactory<GameToolsDbContext> dbContextFactory, IEventBus eventBus,
        IHostApplicationLifetime hostApplicationLifetime, DialogService dialogService, ILogger<PortalAddresses> logger,
        IJSRuntime jsRuntime,
        NotificationService notificationService)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _hostApplicationLifetime = hostApplicationLifetime ?? throw new ArgumentNullException(nameof(hostApplicationLifetime));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await ReloadAddresses();

        // _ = PeriodicRefresh();
    }

    private async Task ReloadAddresses()
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        await dbContext.EnsureProfileExistAsync(ProfileId!.Value);
        _addresses = await dbContext.NoMansSkyPortalAddresses.Where(a => a.ProfileId == ProfileId!.Value)
           .Include(a => a.Profile)
           .Include(a => a.Galaxy)
           .OrderBy(a => a.GalaxyId)
           .ThenBy(a => a.Name)
           .ToListAsync();

        if (string.IsNullOrWhiteSpace(_filter))
        {
            return;
        }

        string[] words = _filter.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(w => w.ToLowerInvariant()).ToArray();
        var allAddresses = _addresses.ToList();
        _addresses = [];

        foreach (NoMansSkyPortalAddress address in allAddresses)
        {
            string allText = address.GetAllText().ToLowerInvariant();
            if (words.All(w => allText.Contains(w)))
            {
                _addresses.Add(address);
            }
        }
    }

    private string AddressAsDiscordGlyphs(string address)
    {
        var addressText = new StringBuilder();
        foreach (char c in address.ToLowerInvariant())
        {
            addressText.Append(":portal").Append(c).Append(':');
        }

        return addressText.ToString();
    }

    private async Task CopyAddressForDiscord(NoMansSkyPortalAddress address)
    {
        await _jsRuntime.InvokeVoidAsync("copyToClipboard", $"{address.Galaxy!.Name}: {AddressAsDiscordGlyphs(address.Address)}");
        _notificationService.Notify(NotificationSeverity.Success, "Portal address copied to clipboard");
    }

    private async Task CopyEntireText(NoMansSkyPortalAddress address)
    {
        var builder = new StringBuilder();
        builder.Append("## ").AppendLine(address.Name);

        // copy rest in a structured manner
        builder.Append("**Galaxy: **").AppendLine(address.Galaxy!.Name);
        builder.Append("**System: **").AppendLine(address.SystemName);
        builder.Append("**Planet: **").AppendLine(address.PlanetName);
        if (address is { CoordinatesX: not null, CoordinatesY: not null })
        {
            builder.Append("**Coordinates: **").AppendLine($"{address.CoordinatesX.Value.ToString("0.00")}, {address.CoordinatesY.Value.ToString("0.00")}");
        }
        builder.Append("**Address: **").AppendLine(AddressAsDiscordGlyphs(address.Address));
        builder.AppendLine();
        builder.AppendLine(address.Description);

        await _jsRuntime.InvokeVoidAsync("copyToClipboard", builder.ToString());
        _notificationService.Notify(NotificationSeverity.Success, "Portal information copied to clipboard");
    }

    private async Task OnButtonClick(RadzenSplitButtonItem? args, NoMansSkyPortalAddress address)
    {
        switch (args?.Value)
        {
            case null:
                await CopyEntireText(address);
                break;
            case "copy_address_discord":
                await CopyAddressForDiscord(address);
                break;

            case "edit":
                await EditAddress(new EditNoMansSkyPortalAddressDialog.ViewModel(address));
                break;

            case "delete":
                await DeletePortalAddress(address);
                break;
        }
    }

    private async Task EditAddress(EditNoMansSkyPortalAddressDialog.ViewModel viewModel)
    {
        bool? save = await _dialogService.OpenAsync<EditNoMansSkyPortalAddressDialog>(viewModel.IsNew ? "Add portal address" : "Edit portal address", new Dictionary<string, object?>
        {
            ["Model"] = viewModel,
        });

        if (save ?? false)
        {
            await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();

            NoMansSkyPortalAddress address;
            if (viewModel.IsNew)
            {
                address = new()
                {
                    ProfileId = ProfileId!.Value,
                    Name = viewModel.Name,
                    SystemName = viewModel.SystemName,
                    PlanetName = viewModel.PlanetName,
                    Description = viewModel.Description,
                    GalaxyId = viewModel.GalaxyId,
                    CoordinatesX = viewModel.CoordinatesX,
                    CoordinatesY = viewModel.CoordinatesY,
                    Address = viewModel.Address,
                };
                dbContext.NoMansSkyPortalAddresses.Add(address);
            }
            else
            {
                NoMansSkyPortalAddress? existingAddress = await dbContext.NoMansSkyPortalAddresses.FindAsync(viewModel.Id!.Value);
                if (existingAddress == null)
                {
                    _logger.LogError("Could not find address to edit");
                    _notificationService.Notify(NotificationSeverity.Error, "Could not find address to edit");
                    return;
                }

                address = existingAddress;
                address.Name = viewModel.Name;
                address.SystemName = viewModel.SystemName;
                address.PlanetName = viewModel.PlanetName;
                address.Description = viewModel.Description;
                address.GalaxyId = viewModel.GalaxyId;
                address.CoordinatesX = viewModel.CoordinatesX;
                address.CoordinatesY = viewModel.CoordinatesY;
                address.Address = viewModel.Address;
            }

            await dbContext.SaveChangesAsync();
            await ReloadAddresses();
            _notificationService.Notify(NotificationSeverity.Success, "Portal address was saved");
        }
    }

    private async Task DeletePortalAddress(NoMansSkyPortalAddress address)
    {
        bool? result = await _dialogService.Confirm(title: "Delete portal address?", message: $"Confirm that you really want to delete the portal address '{address.Name}'", options: new ConfirmOptions
        {
            Icon = "delete",
            CancelButtonText = "Cancel",
            OkButtonText = "Delete",
        });

        if (result == true)
        {
            await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
            dbContext.NoMansSkyPortalAddresses.Remove(address);
            await dbContext.SaveChangesAsync();
            await ReloadAddresses();
            _notificationService.Notify(NotificationSeverity.Success, "Portal address was deleted");
        }
    }

    private async Task FilterChange()
    {
        _filterDebounceCts?.Cancel();
        _filterDebounceCts = new CancellationTokenSource();

        try
        {
            await Task.Delay(1000, _filterDebounceCts.Token);
            await ReloadAddresses();
        }
        catch (TaskCanceledException)
        {
        }
    }
}