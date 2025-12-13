using GameTools.Database;
using GameTools.Workers.Events;

using LVK.Events;

using Microsoft.EntityFrameworkCore;

using Radzen;

namespace GameTools.Components.Pages;

public partial class ShoppingList : IAsyncDisposable
{
    private readonly IDbContextFactory<GameToolsDbContext> _dbContextFactory;
    private readonly IEventBus _eventBus;
    private readonly DialogService _dialogService;

    private List<ShoppingListModel> _categories = [];

    private string _newCategoryName = "";
    private IDisposable? _subscription;

    public ShoppingList(IDbContextFactory<GameToolsDbContext> dbContextFactory, IEventBus eventBus, DialogService dialogService)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        await ReloadList();
        _subscription ??= _eventBus.Subscribe<ShoppingListEditerForUserEvent>(OnShoppingListEditerForUserEvent);
    }

    private async Task OnShoppingListEditerForUserEvent(ShoppingListEditerForUserEvent arg)
    {
        if (arg.ProfileId == ProfileId!.Value)
        {
            await InvokeAsync(async () =>
            {
                await ReloadList();
                StateHasChanged();
            });
        }
    }

    private async Task ReloadList()
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();

        List<ShoppingListCategory> categories = await dbContext.ShoppingListCategories.Where(cat => cat.ProfileId == ProfileId!.Value).OrderBy(cat => cat.Name).ToListAsync();
        List<ShoppingListItem> items = await dbContext.ShoppingListItems.Where(item => item.ProfileId == ProfileId!.Value).OrderBy(item => item.Name).ToListAsync();

        var models = new List<ShoppingListModel>();
        foreach (ShoppingListCategory category in categories)
        {
            var model = new ShoppingListModel
            {
                Id = category.Id, CategoryName = category.Name,
            };

            model.Items.AddRange(items.Where(item => item.ShoppingListCategoryId == category.Id));
            models.Add(model);
        }

        _categories = models;
    }

    private async Task AddItemToCategory(ShoppingListModel category)
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        var item = new ShoppingListItem
        {
            ProfileId = ProfileId!.Value,
            ShoppingListCategoryId = category.Id,
            Name = category.NewItemName,
            Required = category.NewItemRequired ?? 1,
            Current = category.NewItemCurrent ?? 0, };

        dbContext.ShoppingListItems.Add(item);
        await dbContext.SaveChangesAsync();
        await _eventBus.PublishAsync(new ShoppingListEditerForUserEvent
        {
            ProfileId = ProfileId!.Value
        });
    }

    private async Task UpdateItemName(ShoppingListItem item, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            return;
        }

        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        ShoppingListItem? foundItem = dbContext.ShoppingListItems.FirstOrDefault(i => i.Id == item.Id);
        if (foundItem != null)
        {
            foundItem.Name = newName;
            await dbContext.SaveChangesAsync();
            await _eventBus.PublishAsync(new ShoppingListEditerForUserEvent
            {
                ProfileId = ProfileId!.Value
            });
        }
    }

    private async Task UpdateItemRequired(ShoppingListItem item, int newRequired)
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        ShoppingListItem? foundItem = dbContext.ShoppingListItems.FirstOrDefault(i => i.Id == item.Id);
        if (foundItem != null)
        {
            foundItem.Required = newRequired;
            await dbContext.SaveChangesAsync();
            await _eventBus.PublishAsync(new ShoppingListEditerForUserEvent
            {
                ProfileId = ProfileId!.Value
            });
        }
    }

    private async Task UpdateItemCurrent(ShoppingListItem item, int newCurrent)
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        ShoppingListItem? foundItem = dbContext.ShoppingListItems.FirstOrDefault(i => i.Id == item.Id);
        if (foundItem != null)
        {
            foundItem.Current = newCurrent;
            await dbContext.SaveChangesAsync();
            await _eventBus.PublishAsync(new ShoppingListEditerForUserEvent
            {
                ProfileId = ProfileId!.Value
            });
        }
    }

    private async Task DeleteCategory(ShoppingListModel category)
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        ShoppingListCategory? foundCategory = dbContext.ShoppingListCategories.FirstOrDefault(cat => cat.Id == category.Id);
        if (foundCategory != null)
        {
            bool? response = await _dialogService.Confirm($"Do you want to delete category '{category.CategoryName}'?", "Delete category?", new()
            {
                OkButtonText = "Delete", Icon = "warning",
            });

            if (response ?? false)
            {
                IQueryable<ShoppingListItem> itemsToDelete = dbContext.ShoppingListItems.Where(item => item.ShoppingListCategoryId == category.Id);
                dbContext.ShoppingListItems.RemoveRange(itemsToDelete);
                dbContext.ShoppingListCategories.Remove(foundCategory);
                await dbContext.SaveChangesAsync();
                await _eventBus.PublishAsync(new ShoppingListEditerForUserEvent
                {
                    ProfileId = ProfileId!.Value
                });
            }
        }
    }

    private async Task DeleteItem(ShoppingListItem item)
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        ShoppingListItem? foundItem = dbContext.ShoppingListItems.FirstOrDefault(i => i.Id == item.Id);
        if (foundItem != null)
        {
            bool? response = await _dialogService.Confirm($"Do you want to delete item '{item.Name}'?", "Delete item?", new()
            {
                OkButtonText = "Delete", Icon = "warning",
            });

            if (response ?? false)
            {
                dbContext.ShoppingListItems.Remove(foundItem);
                await dbContext.SaveChangesAsync();
                await _eventBus.PublishAsync(new ShoppingListEditerForUserEvent
                {
                    ProfileId = ProfileId!.Value
                });
            }
        }
    }

    private async Task UpdateCategoryName(ShoppingListModel category, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            return;
        }

        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        ShoppingListCategory? foundCategory = dbContext.ShoppingListCategories.FirstOrDefault(cat => cat.Id == category.Id);
        if (foundCategory != null)
        {
            foundCategory.Name = newName;
            await dbContext.SaveChangesAsync();
            await _eventBus.PublishAsync(new ShoppingListEditerForUserEvent
            {
                ProfileId = ProfileId!.Value
            });
        }
    }

    private async Task AddNewCategory()
    {
        if (string.IsNullOrWhiteSpace(_newCategoryName))
        {
            return;
        }

        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        dbContext.ShoppingListCategories.Add(new ShoppingListCategory
        {
            ProfileId = ProfileId!.Value,
            Name = _newCategoryName,
        });
        await dbContext.SaveChangesAsync();
        _newCategoryName = "";
        await _eventBus.PublishAsync(new ShoppingListEditerForUserEvent
        {
            ProfileId = ProfileId!.Value
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (_subscription is IAsyncDisposable subscriptionAsyncDisposable)
        {
            await subscriptionAsyncDisposable.DisposeAsync();
        }
        else if (_subscription != null)

        {
            _subscription.Dispose();
        }
    }
}