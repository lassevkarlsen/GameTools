using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameTools.Database;

public class ProfileSettings
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public Guid ProfileId { get; set; }

    [MaxLength(64)]
    public string? PushoverUserKey { get; set; }
}