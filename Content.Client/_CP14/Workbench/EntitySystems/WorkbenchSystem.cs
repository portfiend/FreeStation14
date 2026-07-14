using Content.Shared._CP14.Workbench;
using Content.Shared._CP14.Workbench.EntitySystems;

namespace Content.Client._CP14.Workbench.EntitySystems;

public sealed partial class WorkbenchSystem : SharedWorkbenchSystem
{
    [Dependency] private SharedUserInterfaceSystem _userInterface = default!;

    protected override void UpdateUIRecipes(Entity<WorkbenchComponent> ent)
    {
        if (_userInterface.TryGetOpenUi(ent.Owner, WorkbenchUiKey.Key, out var bui))
            bui.Update();
    }
}
