using System.Linq;
using Content.Shared._CP14.Workbench;
using Content.Shared._CP14.Workbench.EntitySystems;
using Content.Shared._CP14.Workbench.Prototypes;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._CP14.Workbench;

public sealed partial class WorkbenchBoundUserInterface : BoundUserInterface
{
    [Dependency] private IPrototypeManager _protoMan = default!;
    [Dependency] private IPlayerManager _player = default!;
    private WorkbenchWindow? _window;

    public WorkbenchBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    { }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<WorkbenchWindow>();
        _window.OnCraft += entry => SendMessage(new WorkbenchUiCraftMessage(entry.ProtoId));
    }

    public override void Update()
    {
        base.Update();
        UpdateRecipes();
    }

    private void UpdateRecipes()
    {
        if (_window == null || !EntMan.TryGetComponent<WorkbenchComponent>(Owner, out var workbench))
            return;

        var getResource = new WorkbenchGetResourcesEvent();
        EntMan.EventBus.RaiseLocalEvent(Owner, getResource);

        var context = new WorkbenchCraftingContext(User: _player.LocalEntity,
            Workbench: Owner,
            Ingredients: getResource.Resources.ToArray());

        var recipes = new List<WorkbenchUiRecipesEntry>();
        foreach (var recipeId in workbench.CombinedRecipes)
        {
            if (!_protoMan.TryIndex(recipeId, out var indexedRecipe))
                continue;

            var canCraft = CanCraftRecipe(indexedRecipe, context);
            var entry = new WorkbenchUiRecipesEntry(recipeId, canCraft);
            recipes.Add(entry);
        }

        _window?.UpdateRecipes(recipes);
    }

    private bool CanCraftRecipe(WorkbenchRecipePrototype recipe, WorkbenchCraftingContext context)
    {
        foreach (var requirement in recipe.Requirements)
        {
            if (!requirement.CheckRequirement(EntMan, _protoMan, context))
                return false;
        }

        return true;
    }
}
