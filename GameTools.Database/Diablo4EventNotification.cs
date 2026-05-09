using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameTools.Database;

public class Diablo4EventNotification
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public required Guid ProfileId { get; init; }
    public Profile? Profile { get; set; }

    [Required]
    public required string Type { get; set; }

    [Required]
    public required string EventId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string EventText { get; set; }

    public DateTimeOffset OccursAt { get; set; }
}