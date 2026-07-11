using Content.Shared._CP14.Workbench;
using Robust.Client.UserInterface;

namespace Content.Client._CP14.Workbench;

public sealed class WorkbenchBoundUserInterface : BoundUserInterface
{
    private WorkbenchWindow? _window;

    public WorkbenchBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    { }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<WorkbenchWindow>();

        _window.OnCraft += entry => SendMessage(new WorkbenchUiCraftMessage(entry.ProtoId));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        switch (state)
        {
            case WorkbenchUiRecipesState recipesState:
                _window?.UpdateState(recipesState);
                break;
        }
    }
}
