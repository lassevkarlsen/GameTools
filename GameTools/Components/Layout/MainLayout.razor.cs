namespace GameTools.Components.Layout;

public partial class MainLayout
{
    private string? _version;

    protected override void OnInitialized()
    {
        string idFileName = "git_id.txt";
        if (File.Exists(idFileName))
        {
            _version = File.ReadAllText(idFileName);
        }

        base.OnInitialized();
    }
}