using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameTools.Database;

public class NoMansSkyPortalAddress
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public required Guid ProfileId { get; init; }
    public Profile? Profile { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(100)]
    public required string SystemName { get; set; }

    [Required]
    [MaxLength(100)]
    public required string PlanetName { get; set; }

    [Required]
    public required int GalaxyId { get; set; }
    public NoMansSkyGalaxy? Galaxy { get; set; }

    [Required]
    [MaxLength(65536)]
    public required string Description { get; set; }

    public decimal? CoordinatesX { get; set; }
    public decimal? CoordinatesY { get; set; }

    [Required]
    [MaxLength(12)]
    public required string Address { get; set; }
}