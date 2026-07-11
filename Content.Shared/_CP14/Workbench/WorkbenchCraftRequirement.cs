using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Workbench;

/// <summary>
///     A state that must be met in order for this recipe to be craftable.
///     For example: Ingredient costs are a type of crafting requirement.
/// </summary>
/// <remarks>
///     If this requirement is not met, then the "craft" button should be unavailable.
///     Similar to <see cref="WorkbenchCraftCondition"/>, but unlike requirements,
///     you can still click the "craft" button even if Conditions are not met.
/// </remarks>
[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class WorkbenchCraftRequirement
{
    /// <summary>
    /// Here a check is made that the recipe as a whole can be fulfilled at the current moment.
    /// Do not add anything that affects gameplay here, and only perform checks here.
    /// </summary>
    /// <returns></returns>
    public abstract bool CheckRequirement(IEntityManager entManager,
        IPrototypeManager protoManager,
        HashSet<EntityUid> placedEntities);

    /// <summary>
    /// An event that is triggered after crafting. This is the mechanical effect of this
    /// crafting requirement - such as subtracting resources when they are used in a recipe.
    /// </summary>
    public virtual void PostCraft(IEntityManager entManager,
        IPrototypeManager protoManager,
        HashSet<EntityUid> placedEntities)
    { }

    /// <summary>
    /// This text will be displayed in the description of the craft recipe. Write something
    /// like ‘Wooden planks: х10’ here
    /// </summary>
    public virtual string GetRequirementTitle(IPrototypeManager protoManager)
    {
        return string.Empty;
    }

    /// <summary>
    /// You can specify an icon generated from an entity. It will support layering, colour
    /// changes and other layer options. Return null to disable.
    /// </summary>
    public virtual EntityPrototype? GetRequirementEntityView(IPrototypeManager protoManager)
    {
        return null;
    }

    /// <summary>
    /// You can specify the texture directly. Return null to disable.
    /// </summary>
    public virtual SpriteSpecifier? GetRequirementTexture(IPrototypeManager protoManager)
    {
        return null;
    }

    /// <summary>
    /// Optional, allows you to repaint the icon.
    /// </summary>
    public virtual Color GetRequirementColor(IPrototypeManager protoManager)
    {
        return Color.White;
    }
}
