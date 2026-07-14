using Content.Shared._CP14.Workbench.EntitySystems;
using Content.Shared.Materials;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Workbench.Requirements;

/// <summary>
///     An ingredient requirement for a certain amount of total materials.
/// </summary>
public sealed partial class MaterialIngredientRequirement : WorkbenchCraftRequirement
{
    /// <summary>
    ///     The required material.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<MaterialPrototype> Material;

    /// <summary>
    ///     How much of this material is required.
    /// </summary>
    [DataField]
    public int Count = 1;

    public override bool CheckRequirement(
        IEntityManager entManager,
        IPrototypeManager protoManager,
        WorkbenchCraftingContext context)
    {
        var count = 0;
        foreach (var ent in context.Ingredients)
        {
            if (!entManager.TryGetComponent<PhysicalCompositionComponent>(ent, out var material))
                continue;

            entManager.TryGetComponent<StackComponent>(ent, out var stack);

            foreach (var (key, value) in material.MaterialComposition)
            {
                if (key != Material)
                    continue;

                if (stack is null)
                    count += value;
                else
                    count += value * stack.Count;
            }
        }

        if (count < Count)
            return false;

        return true;
    }

    public override void PostCraft(IEntityManager entManager,
        IPrototypeManager protoManager,
        WorkbenchCraftingContext context)
    {
        var stackSystem = entManager.System<SharedStackSystem>();
        var requiredCount = Count;

        foreach (var placedEntity in context.Ingredients)
        {
            if (!entManager.TryGetComponent<PhysicalCompositionComponent>(placedEntity, out var material))
                continue;

            entManager.TryGetComponent<StackComponent>(placedEntity, out var stack);

            foreach (var mat in material.MaterialComposition)
            {
                if (mat.Key != Material)
                    continue;

                if (requiredCount <= 0)
                    return;

                if (stack is null)
                {
                    var value = (int)MathF.Min(requiredCount, mat.Value);
                    requiredCount -= value;
                    entManager.DeleteEntity(placedEntity);
                }
                else
                {
                    var materialValue = mat.Value * stack.Count;
                    var countToRemove = (int)MathF.Min(requiredCount, materialValue);
                    var newStackCount = (int)MathF.Ceiling((materialValue - countToRemove) / (float)mat.Value);

                    if (newStackCount <= 0)
                        entManager.DeleteEntity(placedEntity);
                    else
                        stackSystem.SetCount((placedEntity, stack), newStackCount);

                    requiredCount -= countToRemove;
                }
            }
        }
    }

    public override string GetRequirementTitle(IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(Material, out var indexedMaterial))
            return string.Empty;

        return Loc.GetString("workbench-material-requirement-hint",
            ("ingredient", Loc.GetString(indexedMaterial.Name)),
            ("count", Count));
    }

    public override SpriteSpecifier? GetRequirementTexture(IPrototypeManager protoManager)
    {
        return !protoManager.TryIndex(Material, out var indexedMaterial) ? null : indexedMaterial.Icon;
    }
}
