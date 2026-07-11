using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Workbench.Prototypes;

[Prototype("CP14Recipe")]
public sealed partial class CP14WorkbenchRecipePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public ProtoId<TagPrototype> Tag;

    [DataField]
    public TimeSpan CraftTime = TimeSpan.FromSeconds(1f);

    [DataField]
    public SoundSpecifier? OverrideCraftSound;

    /// <summary>
    /// Mandatory conditions, without which the craft button will not even be active
    /// </summary>
    [DataField(required: true)]
    public List<CP14WorkbenchCraftRequirement> Requirements = new();

    /// <summary>
    /// Mandatory conditions for completion, but not blocking the craft button.
    /// Players must monitor compliance themselves.
    /// If the conditions are not met, negative effects occur.
    /// </summary>
    [DataField]
    public List<CP14WorkbenchCraftCondition> Conditions = new();

    [DataField(required: true)]
    public EntProtoId Result;

    [DataField]
    public int ResultCount = 1;

    [DataField]
    public ProtoId<CP14WorkbenchRecipeCategoryPrototype>? Category;

    [DataField]
    public int Priority = 0;  // In descending order. More means it will be first.
}
