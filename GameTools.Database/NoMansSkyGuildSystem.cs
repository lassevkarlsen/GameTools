using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameTools.Database;

public class NoMansSkyGuildSystem
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
    [MaxLength(50)]
    public required string Guild { get; set; }

    public ICollection<NoMansSkyGuildSystemReward>? Rewards { get; set; }
}