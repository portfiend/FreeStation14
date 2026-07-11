using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Workbench.Prototypes;

[Prototype("CP14RecipeCategory")]
public sealed partial class CP14WorkbenchRecipeCategoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    [DataField(required: true)]
    public LocId Name;

    [DataField]
    public int Priority = 0; // In descending order. More means it will be first.
}
