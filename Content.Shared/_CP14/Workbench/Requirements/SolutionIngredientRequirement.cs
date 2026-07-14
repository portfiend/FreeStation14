using Content.Shared._CP14.Workbench.EntitySystems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Workbench.Requirements;

/// <summary>
///     An ingredient requirement for a certain reagent.
/// </summary>
public sealed partial class SolutionIngredientRequirement : WorkbenchCraftRequirement
{
    /// <summary>
    ///     The reagent required.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<ReagentPrototype> Reagent = default!;

    /// <summary>
    ///     How much impurity from other reagents is allowed?
    /// </summary>
    /// <remarks>
    ///     You can think of this value as, "what percent of the solution must be this reagent?"
    ///     If a recipe requires 0.4 purity, for instance, then the reagent must be at least 40% of the
    ///     beaker's solution contents.
    /// </remarks>
    [DataField]
    public float Purity = 0f;

    /// <summary>
    ///     How much of this reagent is required.
    /// </summary>
    [DataField(required: true)]
    public FixedPoint2 Amount = 1f;

    /// <summary>
    ///     The entity to use for the requirement's icon.
    /// </summary>
    [DataField]
    public EntProtoId? DummyEntityIcon = "Beaker";

    public override bool CheckRequirement(IEntityManager entManager,
        IPrototypeManager protoManager,
        WorkbenchCraftingContext context)
    {
        var solutionSys = entManager.System<SharedSolutionContainerSystem>();
        foreach (var ent in context.Ingredients)
        {
            if (!solutionSys.TryGetDrawableSolution(ent, out var soln, out var solution))
                continue;

            var volume = solution.Volume;

            if (volume < Amount)
                continue;

            foreach (var (id, quantity) in solution.Contents)
            {
                if (id.Prototype != Reagent)
                    continue;

                //Purity check
                if (quantity / volume < Purity)
                    continue;

                return true;
            }
        }

        return false;
    }

    public override void PostCraft(IEntityManager entManager,
        IPrototypeManager protoManager,
        WorkbenchCraftingContext context)
    {
        var solutionSys = entManager.System<SharedSolutionContainerSystem>();
        foreach (var ent in context.Ingredients)
        {
            if (!solutionSys.TryGetDrawableSolution(ent, out var soln, out var solution))
                continue;

            var volume = solution.Volume;
            if (volume < Amount)
                continue;

            foreach (var (id, quantity) in solution.Contents)
            {
                if (id.Prototype != Reagent)
                    continue;

                //Purity check
                if (quantity / volume < Purity)
                    continue;

                solutionSys.Draw(ent, soln.Value, Amount);
                return;
            }
        }
    }

    public override string GetRequirementTitle(IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(Reagent, out var indexedReagent))
            return string.Empty;

        return Loc.GetString("cp14-workbench-reagent-req",
            ("reagent", indexedReagent.LocalizedName),
            ("count", Amount),
            ("purity", Purity * 100));
    }

    public override EntityPrototype? GetRequirementEntityView(IPrototypeManager protoManager)
    {
        if (DummyEntityIcon == null || !protoManager.TryIndex(DummyEntityIcon, out var indexedEnt))
            return null;

        return indexedEnt;
    }

    public override Color GetRequirementColor(IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(Reagent, out var indexedReagent))
            return Color.White;

        return indexedReagent.SubstanceColor;
    }
}
