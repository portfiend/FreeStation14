using Content.Shared._CP14.Workbench;

namespace Content.Shared._CP14.Workbench;

public abstract partial class SharedWorkbenchSystem
{
    private void OnCraft(Entity<WorkbenchComponent> entity, ref WorkbenchUiCraftMessage args)
    {
        if (!entity.Comp.Recipes.Contains(args.Recipe))
            return;

        if (!ProtoMan.TryIndex(args.Recipe, out var prototype))
            return;

        StartCraft(entity, args.Actor, prototype);
    }

    private void UpdateUIRecipes(Entity<WorkbenchComponent> entity)
    {
        var getResource = new WorkbenchGetResourcesEvent();
        RaiseLocalEvent(entity, getResource);

        var resources = getResource.Resources;

        var recipes = new List<WorkbenchUiRecipesEntry>();
        foreach (var recipeId in entity.Comp.Recipes)
        {
            if (!ProtoMan.TryIndex(recipeId, out var indexedRecipe))
                continue;

            var canCraft = true;

            foreach (var requirement in indexedRecipe.Requirements)
            {
                if (!requirement.CheckRequirement(EntityManager, ProtoMan, resources))
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
