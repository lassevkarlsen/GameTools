using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameTools.Database;

public class GameTimer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public required Guid ProfileId { get; init; }

    public Profile? Profile { get; set; }

    [Required]
    public required TimeSpan Duration { get; set; }

    [MaxLength(50)]
    public required string Name { get; set; }

    public DateTimeOffset? ElapsesAt { get; set; }
    public TimeSpan? Remaining { get; set; }

    public bool CompletionProcessed { get; set; }
}