using System.ComponentModel.DataAnnotations;

namespace GameTools.Components.Pages;

public class ProfileConfigurationModel
{
    [MaxLength(100)]
    public string Name { get; set; } = "";

    [MaxLength(64)]
    public string PushoverUserKey { get; set; } = "";
}