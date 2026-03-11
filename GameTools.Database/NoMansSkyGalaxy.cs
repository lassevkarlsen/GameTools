using System.ComponentModel.DataAnnotations;

namespace GameTools.Database;

public class NoMansSkyGalaxy
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(32)]
    public string Type { get; set; } = "Normal";
}