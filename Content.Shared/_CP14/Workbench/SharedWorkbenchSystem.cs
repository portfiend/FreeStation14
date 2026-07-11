using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Workbench;

// TODO: Prediction

/// <summary>
///     System logic for a workbench, an entity that can be used to produce crafting recipes
///     by placing ingredients near the bench and selecting a recipe.
/// </summary>
public abstract class SharedWorkbenchSystem : EntitySystem
{ }

/// <summary>
///     Fired when a player is attempting to craft a recipe.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class WorkbenchCraftDoAfterEvent : DoAfterEvent
{
    [DataField(required: true)]
    public ProtoId<WorkbenchRecipePrototype> Recipe = default!;

    public override DoAfterEvent Clone()
    {
        return this;
    }
}
