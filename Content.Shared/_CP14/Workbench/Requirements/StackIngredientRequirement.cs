using Content.Shared._CP14.Workbench.EntitySystems;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Workbench.Requirements;

/// <summary>
///     An ingredient requirement for a certain amount of entity stacks.
/// </summary>
public sealed partial class StackIngredientRequirement : WorkbenchCraftRequirement
{
    /// <summary>
    ///     The entity stack required.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<StackPrototype> Stack;

    /// <summary>
    ///     How many stacks are needed.
    /// </summary>
    [DataField]
    public int Count = 1;

    public override bool CheckRequirement(IEntityManager entManager,
        IPrototypeManager protoManager,
        WorkbenchCraftingContext context)
    {
        var count = 0;
        foreach (var ent in context.Ingredients)
        {
            if (!entManager.TryGetComponent<StackComponent>(ent, out var stack))
                continue;

            if (stack.StackTypeId != Stack)
                continue;

            count += stack.Count;
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
            if (!entManager.TryGetComponent<StackComponent>(placedEntity, out var stack))
                continue;

            if (stack.StackTypeId != Stack)
                continue;

            var count = (int)MathF.Min(requiredCount, stack.Count);

            if (stack.Count - count <= 0)
                entManager.DeleteEntity(placedEntity);
            else
                stackSystem.SetCount((placedEntity, stack), stack.Count - count);

            requiredCount -= count;
        }
    }

    public override string GetRequirementTitle(IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(Stack, out var indexedStack))
            return string.Empty;

        return Loc.GetString("workbench-material-requirement-hint",
            ("ingredient", Loc.GetString(indexedStack.Name)),
            ("count", Count));
    }

    public override SpriteSpecifier? GetRequirementTexture(IPrototypeManager protoManager)
    {
        return !protoManager.TryIndex(Stack, out var indexedStack) ? null : indexedStack.Icon;
    }
}
