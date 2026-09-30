using Content.Shared._CP14.Workbench.EntitySystems;
using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Workbench;

/// <summary>
/// This entity can be used to craft other objects through the interface
/// </summary>
[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedWorkbenchSystem))]
public sealed partial class WorkbenchComponent : Component
{
    /// <summary>
    /// Crafting speed modifier on this workbench.
    /// </summary>
    [DataField]
    public float CraftSpeed = 1f;

    /// <summary>
    /// List of recipes available for crafting on this type of workbench
    /// </summary>
    [DataField]
    [Access(typeof(SharedWorkbenchSystem), Friend = AccessPermissions.Read, Other = AccessPermissions.None)]
    public HashSet<ProtoId<WorkbenchRecipePrototype>> Recipes = new();

    /// <summary>
    /// Auto recipe list fill based on tags
    /// </summary>
    [DataField]
    [Access(typeof(SharedWorkbenchSystem), Other = AccessPermissions.None)]
    public HashSet<ProtoId<TagPrototype>> RecipeTags = new();

    /// <summary>
    ///     All recipes from datafields. This is the "true" recipe list.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<ProtoId<WorkbenchRecipePrototype>> CombinedRecipes = new();

    /// <summary>
    /// Played during crafting. Can be overwritten by the crafting sound of a specific recipe.
    /// </summary>
    [DataField]
    public SoundSpecifier CraftSound = new SoundCollectionSpecifier("PrinterPrint");
}
