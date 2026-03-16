using System.Text.RegularExpressions;

using GameTools.Database;

using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

using Radzen;

namespace GameTools.Components.Dialogs;

public partial class EditNoMansSkyPortalAddressDialog
{
    private readonly DialogService _dialogService;
    private readonly IDbContextFactory<GameToolsDbContext> _dbContextFactory;
    private readonly IJSRuntime _jsRuntime;

    [Parameter]
    public ViewModel Model { get; set; } = new();

    private List<NoMansSkyGalaxy> _galaxies = [];

    public EditNoMansSkyPortalAddressDialog(DialogService dialogService, IDbContextFactory<GameToolsDbContext> dbContextFactory, IJSRuntime jsRuntime)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
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

    private void AppendGlyph(char glyph)
    {
        if (Model.Address.Length < 12)
        {
            Model.Address += glyph;
        }
    }

    private void RemoveLastGlyph()
    {
        if (Model.Address.Length > 0)
        {
            Model.Address = Model.Address[..^1];
        }
    }

    private async Task PasteFromClipboard()
    {
        string text = await _jsRuntime.InvokeAsync<string>("clipboardInterop.readText");

        if (ParseTextAsHexAddress(text))
        {
            return;
        }

        ParseTextAsDiscordGlyphs(text);
    }

    private bool ParseTextAsHexAddress(string text)
    {
        NoMansSkyGalaxy? galaxy = TryParseGalaxy(ref text);

        if (text.ToLowerInvariant().StartsWith("0x"))
        {
            text = text[2..];
        }

        text = text.Trim().Trim(',').ToUpperInvariant();

        if (!Regex.IsMatch(text, "^[A-F0-9]{12}$"))
        {
            return false;
        }

        Model.Address = text;
        if (galaxy != null)
        {
            Model.Galaxy = galaxy;
            Model.GalaxyId = galaxy.Id;
        }

        return true;
    }

    private NoMansSkyGalaxy? TryParseGalaxy(ref string text)
    {
        int colonIndex = text.IndexOf(':');
        if (colonIndex > 0)
        {
            string possibleGalaxyName = text[..colonIndex].Trim();

            NoMansSkyGalaxy? galaxy = _galaxies.FirstOrDefault(g => StringComparer.InvariantCultureIgnoreCase.Equals(g.Name, possibleGalaxyName));
            if (galaxy != null)
            {
                text = text[(colonIndex + 1)..].Trim();
                return galaxy;
            }
        }

        string galaxiesPattern = $@"(^|\s+)in\s+(?<galaxy>{string.Join("|", _galaxies.Select(g => g.Name))})";
        Match match = Regex.Match(text, galaxiesPattern, RegexOptions.IgnoreCase);
        if (match.Success)
        {
            text = text[0..match.Index].Trim() + " " + text[(match.Index + match.Length)..].Trim();
            return _galaxies.FirstOrDefault(g => StringComparer.InvariantCultureIgnoreCase.Equals(g.Name, match.Groups["galaxy"].Value));
        }

        return null;
    }

    private bool ParseTextAsDiscordGlyphs(string text)
    {
        NoMansSkyGalaxy? galaxy = TryParseGalaxy(ref text);

        text = text.Trim().ToLowerInvariant();
        MatchCollection matches = Regex.Matches(text, ":portal(?<glyph>[0-9a-f]):");
        if (matches.Count != 12)
        {
            return false;
        }

        Model.Address = string.Join("", matches.Select(m => m.Groups["glyph"].Value)).ToUpperInvariant();
        if (galaxy != null)
        {
            Model.Galaxy = galaxy;
            Model.GalaxyId = galaxy.Id;
        }
        return true;
    }
}