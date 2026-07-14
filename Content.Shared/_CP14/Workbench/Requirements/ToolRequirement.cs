using Content.Shared._CP14.Workbench.EntitySystems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Tools;
using Content.Shared.Tools.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Workbench.Requirements;

/// <summary>
///     Requires a specific tool quality to be available.
/// </summary>
public sealed partial class ToolRequirement : WorkbenchCraftRequirement
{
    /// <summary>
    ///     The required tool quality.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<ToolQualityPrototype> Quality;

    /// <summary>
    ///     Whether or not the placed entities on the workbench are valid to use as tools.
    /// </summary>
    /// <remarks>
    ///     In most cases this should be fine. But if an ingredient used in the recipe itself
    ///     might contain the tool, you might want to consider turning this off.
    /// </remarks>
    [DataField]
    public bool AllowToolInIngredients = true;

    /// <summary>
    ///     Sprite specifier used to override this requirement's tool quality icon.
    /// </summary>
    [DataField("icon")]
    public SpriteSpecifier? IconOverride = null;

    public override bool CheckRequirement(IEntityManager entManager,
        IPrototypeManager protoManager,
        WorkbenchCraftingContext context)
    {
        var toolSystem = entManager.System<SharedToolSystem>();

        // Workbench is a tool
        if (toolSystem.HasQuality(context.Workbench, Quality))
            return true;

        if (context.User != null)
        {
            // User *is* the tool
            if (toolSystem.HasQuality(context.User.Value, Quality))
                return true;

            // Search user's hands
            var handsSystem = entManager.System<SharedHandsSystem>();
            foreach (var held in handsSystem.EnumerateHeld(context.User.Value))
                if (toolSystem.HasQuality(held, Quality))
                    return true;
        }

        // Search ingredient list
        if (AllowToolInIngredients)
        {
            foreach (var ingredient in context.Ingredients)
                if (toolSystem.HasQuality(ingredient, Quality))
                    return true;
        }

        return false;
    }

    public override string GetRequirementTitle(IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(Quality, out var qualityProto))
            return string.Empty;

        return Loc.GetString("workbench-tool-requirement-hint",
            ("quality", Loc.GetString(qualityProto.Name)));
    }

    public override SpriteSpecifier? GetRequirementTexture(IPrototypeManager protoManager)
    {
        if (IconOverride != null)
            return IconOverride;

        if (protoManager.TryIndex(Quality, out var qualityProto))
            return qualityProto.Icon;

        return null;
    }
}
