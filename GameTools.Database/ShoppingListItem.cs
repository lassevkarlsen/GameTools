using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameTools.Database;

public class ShoppingListItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public required Guid ProfileId { get; init; }
    public Profile? Profile { get; set; }

    public int ShoppingListCategoryId { get; set; }
    public ShoppingListCategory? ShoppingListCategory { get; set; }

    [MaxLength(100)]
    public required string Name { get; set; }

    [Range(0, int.MaxValue)]
    public int Required { get; set; }

    [Range(0, int.MaxValue)]
    public int Current { get; set; }
}