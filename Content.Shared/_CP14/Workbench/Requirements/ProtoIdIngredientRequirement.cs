using Content.Shared._CP14.Workbench.EntitySystems;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Workbench.Requirements;

/// <summary>
///     An ingredient requirement for a specific entity.
/// </summary>
public sealed partial class ProtoIdIngredientRequirement : WorkbenchCraftRequirement
{
    /// <summary>
    ///     The entity required.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId ProtoId;

    /// <summary>
    ///     How much of this entity is required.
    /// </summary>
    [DataField]
    public int Count = 1;

    public override bool CheckRequirement(IEntityManager entManager,
        IPrototypeManager protoManager,
        WorkbenchCraftingContext context)
    {
        var indexedIngredients = IndexIngredients(entManager, context.Ingredients);

        return indexedIngredients.TryGetValue(ProtoId, out var availableQuantity) && availableQuantity >= Count;
    }

    public override void PostCraft(IEntityManager entManager,
        IPrototypeManager protoManager,
        WorkbenchCraftingContext context)
    {
        var requiredCount = Count;

        foreach (var placedEntity in context.Ingredients)
        {
            if (!entManager.TryGetComponent<MetaDataComponent>(placedEntity, out var metaData))
                continue;

            if (metaData.EntityPrototype is null)
                continue;

            var placedProto = metaData.EntityPrototype.ID;
            if (placedProto != ProtoId || requiredCount <= 0)
                continue;

            requiredCount--;
            entManager.DeleteEntity(placedEntity);
        }
    }

    public override string GetRequirementTitle(IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(ProtoId, out var indexedProto))
            return string.Empty;

        return Loc.GetString("workbench-protoid-requirement-hint",
            ("ingredient", indexedProto.Name),
            ("count", Count));
    }

    public override EntityPrototype? GetRequirementEntityView(IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(ProtoId, out var indexedProto))
            return null;

        return indexedProto;
    }

    private Dictionary<EntProtoId, int> IndexIngredients(IEntityManager entManager, EntityUid[] ingredients)
    {
        var indexedIngredients = new Dictionary<EntProtoId, int>();

        foreach (var ingredient in ingredients)
        {
            if (!entManager.TryGetComponent<MetaDataComponent>(ingredient, out var metaData))
                continue;

            var protoId = metaData.EntityPrototype?.ID;
            if (protoId == null)
                continue;

            if (indexedIngredients.ContainsKey(protoId))
                indexedIngredients[protoId]++;
            else
                indexedIngredients[protoId] = 1;
        }

        return indexedIngredients;
    }
}
