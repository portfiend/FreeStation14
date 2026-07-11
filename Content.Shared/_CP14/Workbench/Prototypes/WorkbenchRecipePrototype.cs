using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Workbench.Prototypes;

/// <summary>
///     Represents a crafting recipe that can be made at a workbench.
/// </summary>
[Prototype]
public sealed partial class WorkbenchRecipePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     The tag used to determine which type of workbench can produce this recipe.
    ///     For example, you might have a "cooking table" that only makes "cooking" recipes.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<TagPrototype> Tag; // TODO: HashSet

    /// <summary>
    ///     How long this recipe takes to put together.
    /// </summary>
    [DataField]
    public TimeSpan CraftTime = TimeSpan.FromSeconds(1f);

    /// <summary>
    ///     A sound effect that plays while this recipe is being crafted.
    ///     Overrides the sound defined in the workbench itself.
    /// </summary>
    [DataField]
    public SoundSpecifier? OverrideCraftSound;

    /// <summary>
    ///     States that must be fulfilled for this recipe to be craftable, such as ingredients.
    ///     If any of these are unmet, the option to craft the recipe is unavailable.
    /// </summary>
    [DataField(required: true)]
    public List<WorkbenchCraftRequirement> Requirements = new();

    /// <summary>
    ///     States that must be fulfilled for this recipe to be craftable, but you can attempt anyway.
    ///     If the condition is unmet when the recipe completes, instead of getting products,
    ///     you experience a disastrous side effect.
    /// </summary>
    [DataField]
    public List<WorkbenchCraftCondition> Conditions = new();

    /// <summary>
    ///     The entity produced by this recipe.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Result;

    /// <summary>
    ///     How many of the resulting entity are produced.
    /// </summary>
    [DataField]
    public int ResultCount = 1;

    /// <summary>
    ///     The category that this entity belongs to - used for sorting and filtering.
    /// </summary>
    [DataField]
    public ProtoId<WorkbenchRecipeCategoryPrototype>? Category;

    /// <summary>
    ///     Determines the order that this recipe will be listed in. Higher comes first.
    /// </summary>
    [DataField]
    public int Priority = 0;
}
