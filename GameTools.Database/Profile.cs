using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameTools.Database;

public class Profile
{
    public const string DefaultProfileName = "Unnamed profile";

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = DefaultProfileName;

    [MaxLength(64)]
    public string? PushoverUserKey { get; set; }
}