using Microsoft.AspNetCore.Mvc.TagHelpers;

namespace GameTools.Components.Layout;

public partial class MainLayout
{
    private bool _sidebarExpanded = true;

    private string? _version;
    private string? _branch;

    private string _pageTitle = "Home";
    private Guid? _userId;

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
            _version = "0000000000000000";
            _branch = "develop";
        }

        base.OnInitialized();
    }

    private void SetPageTitle(string title)
    {
        _pageTitle = title;
        StateHasChanged();
    }

    private void SetUserId(Guid userId)
    {
        _userId = userId;
        StateHasChanged();
    }
}