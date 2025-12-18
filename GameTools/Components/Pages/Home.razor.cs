using Microsoft.AspNetCore.Components;

namespace GameTools.Components.Pages;

public partial class Home
{
    [Inject]
    public NavigationManager? NavigationManager { get; set; }

    private string? _version;
    private string? _branch;

    protected override Task OnInitializedAsync()
    {
        base.OnInitializedAsync();
        SetPageTitle?.Invoke("Home");

        string idFileName = "git_id.txt";
        if (File.Exists(idFileName))
        {
            using var reader = new StreamReader(idFileName);
            _version = reader.ReadLine();
            _branch = reader.ReadLine();
        }
        else
        {
            _version = "0000000000000000";
            _branch = "develop";
        }

        return Task.CompletedTask;
    }

    private Task CreateSession()
    {
        NavigationManager!.NavigateTo($"/{Guid.NewGuid()}/", forceLoad: true);
        return Task.CompletedTask;
    }
}