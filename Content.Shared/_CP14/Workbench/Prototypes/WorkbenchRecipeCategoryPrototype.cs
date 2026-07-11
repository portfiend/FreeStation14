using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Workbench.Prototypes;

/// <summary>
///     Represents a category of <see cref="WorkbenchRecipePrototype"/>s, used for sorting and filtering.
/// </summary>
[Prototype]
public sealed partial class WorkbenchRecipeCategoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    /// <summary>
    ///     The name of the category.
    /// </summary>
    [DataField(required: true)]
    public LocId Name;

    /// <summary>
    ///     Determines the order that this recipe category will be listed in. Higher comes first.
    /// </summary>
    [DataField]
    public int Priority = 0;
}
