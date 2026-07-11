using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Workbench;

/// <summary>
///     A state that must be met for this recipe to be craftable.
///     You can attempt to craft it anyway, but the recipe will not complete,
///     and possibly produce a disastrous side effect.
/// </summary>
/// <remarks>
///     Similar to <see cref="WorkbenchCraftRequirement"/>, but unlike requirements,
///     you can still click the "craft" button even if Conditions are not met.
/// </remarks>
[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class WorkbenchCraftCondition
{
    /// <summary>
    ///     Check whether or not this condition can be met.
    ///     Nothing should be transformed in this function - this is just for checks.
    /// </summary>
    public abstract bool CheckCondition(
        EntityManager entManager,
        IPrototypeManager protoManager,
        EntityUid workbench,
        EntityUid user);

    /// <summary>
    ///     The mechanical effects of a recipe being crafted under this condition.
    ///     This is where resources are spent, items are deleted, et cetera.
    /// </summary>
    public virtual void PostCraft(
        EntityManager entManager,
        IPrototypeManager protoManager,
        EntityUid workbench,
        EntityUid user)
    { }

    /// <summary>
    ///     What happens if you attempt to craft the recipe when this condition is unmet.
    /// </summary>
    public abstract void FailedEffect(
        EntityManager entManager,
        IPrototypeManager protoManager,
        EntityUid workbench,
        EntityUid user);

    /// <summary>
    /// This text will be displayed in the description of the craft conditions.
    /// Write something like 'The workbench must be filled to 100% mana.' here
    /// </summary>
    public virtual string GetConditionTitle(
        EntityManager entManager,
        IPrototypeManager protoManager,
        EntityUid workbench,
        EntityUid user)
    {
        return string.Empty;
    }

    /// <summary>
    /// You can specify the texture directly. Return null to disable.
    /// </summary>
    public virtual SpriteSpecifier? GetConditionTexture(IPrototypeManager protoManager)
    {
        return null;
    }
}
