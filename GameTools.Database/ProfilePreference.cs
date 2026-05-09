using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameTools.Database;

public class ProfilePreference
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public required Guid ProfileId { get; init; }
    public Profile? Profile { get; set; }

    [Required]
    [MaxLength(64)]
    public required string Key { get; set; }

    [Required]
    [MaxLength(64)]
    public required string Value { get; set; }
}