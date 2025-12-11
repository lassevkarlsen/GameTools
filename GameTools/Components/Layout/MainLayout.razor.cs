using Microsoft.AspNetCore.Mvc.TagHelpers;

namespace GameTools.Components.Layout;

public partial class MainLayout
{
    private bool _sidebarExpanded = true;

    private string? _version;
    private string? _branch;

    private string _pageTitle = "Home";

    protected override void OnInitialized()
    {
        string idFileName = "git_id.txt";
        if (File.Exists(idFileName))
        {
            using var reader = new StreamReader(idFileName);
            _version = reader.ReadLine();
            _branch = reader.ReadLine();
        }
        else
        {
            _version = "unknown";
            _branch = "develop";
        }

        base.OnInitialized();
    }

    private void SetPageTitle(string title)
    {
        _pageTitle = title;
        StateHasChanged();
    }
}