using System.Linq;
using Content.Shared._CP14.Workbench;

namespace Content.Shared._CP14.Workbench.EntitySystems;

public abstract partial class SharedWorkbenchSystem
{
    private void OnCraft(Entity<WorkbenchComponent> entity, ref WorkbenchUiCraftMessage args)
    {
        if (!entity.Comp.CombinedRecipes.Contains(args.Recipe))
            return;

        if (!ProtoMan.TryIndex(args.Recipe, out var prototype))
            return;

        StartCraft(entity, args.Actor, prototype);
    }

    protected virtual void UpdateUIRecipes(Entity<WorkbenchComponent> entity)
    { }
}
