using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameTools.Database;

public class NoMansSkyGuildSystemReward
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public required Guid ProfileId { get; init; }
    public Profile? Profile { get; set; }

    [Required]
    public required int GuildSystemId { get; set; }
    public NoMansSkyGuildSystem? GuildSystem { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    public DateTimeOffset? LastRedeemed { get; set; }
}