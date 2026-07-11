using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Workbench;

// TODO: Prediction
public abstract class SharedWorkbenchSystem : EntitySystem
{ }

[Serializable, NetSerializable]
public sealed partial class WorkbenchCraftDoAfterEvent : DoAfterEvent
{
    [DataField(required: true)]
    public ProtoId<WorkbenchRecipePrototype> Recipe = default!;

    public override DoAfterEvent Clone() => this;
}
