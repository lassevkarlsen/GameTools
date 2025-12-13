using GameTools.Database;

namespace GameTools.Components.Pages;

public class ShoppingListModel
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = "";

    public List<ShoppingListItem> Items { get; set; } = [];

    public string NewItemName { get; set; } = "";
    public int? NewItemRequired { get; set; }
    public int? NewItemCurrent { get; set; }
    public bool AllowEditName { get; set; } = false;
}