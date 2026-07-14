using System.Linq;
using Content.Shared._CP14.Workbench;
using Content.Shared._CP14.Workbench.EntitySystems;
using Robust.Client.Player;

namespace Content.Client._CP14.Workbench.EntitySystems;

public sealed partial class WorkbenchSystem : SharedWorkbenchSystem
{
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private SharedUserInterfaceSystem _userInterface = default!;

    protected override void UpdateUIRecipes(Entity<WorkbenchComponent> entity)
    {
        var getResource = new WorkbenchGetResourcesEvent();
        RaiseLocalEvent(entity, getResource);

        var context = new WorkbenchCraftingContext(User: _player.LocalEntity,
            Workbench: entity.Owner,
            Ingredients: getResource.Resources.ToArray());

        var recipes = new List<WorkbenchUiRecipesEntry>();
        foreach (var recipeId in entity.Comp.Recipes)
        {
            if (!ProtoMan.TryIndex(recipeId, out var indexedRecipe))
                continue;

            var canCraft = true;

            foreach (var requirement in indexedRecipe.Requirements)
            {
                if (!requirement.CheckRequirement(EntityManager, ProtoMan, context))
                {
                    canCraft = false;
                    break;
                }
            }

            var entry = new WorkbenchUiRecipesEntry(recipeId, canCraft);
            recipes.Add(entry);
        }

        _userInterface.SetUiState(entity.Owner, WorkbenchUiKey.Key, new WorkbenchUiRecipesState(recipes));
    }
}
