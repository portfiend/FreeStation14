using System.Linq;
using System.Numerics;
using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Placeable;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Content.Shared.UserInterface;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Workbench.EntitySystems;

// TODO: Prediction

/// <summary>
///     System logic for a workbench, an entity that can be used to produce crafting recipes
///     by placing ingredients near the bench and selecting a recipe.
/// </summary>
public abstract partial class SharedWorkbenchSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitProviders();

        SubscribeLocalEvent<WorkbenchComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<WorkbenchComponent, ItemPlacedEvent>(OnItemPlaced);
        SubscribeLocalEvent<WorkbenchComponent, ItemRemovedEvent>(OnItemRemoved);
        SubscribeLocalEvent<WorkbenchComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpen);
        SubscribeLocalEvent<WorkbenchComponent, WorkbenchUiCraftMessage>(OnCraft);
        SubscribeLocalEvent<WorkbenchComponent, WorkbenchCraftDoAfterEvent>(OnCraftFinished);

        ProtoMan.PrototypesReloaded += OnPrototypesReloaded;
    }

    private void OnMapInit(Entity<WorkbenchComponent> ent, ref MapInitEvent args)
    {
        PopulateRecipes(ent);
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<WorkbenchRecipePrototype>() || args.WasModified<WorkbenchRecipeCategoryPrototype>())
            CacheRecipes();
    }

    private void CacheRecipes()
    {
        var query = EntityQueryEnumerator<WorkbenchComponent>();
        while (query.MoveNext(out var uid, out var workbench))
            PopulateRecipes((uid, workbench));
    }

    private void PopulateRecipes(Entity<WorkbenchComponent> ent)
    {
        var recipes = ent.Comp.Recipes;
        if (ent.Comp.RecipeTags.Count > 0)
        {
            var taggedRecipes = ProtoMan.EnumeratePrototypes<WorkbenchRecipePrototype>()
                .Where(p => ent.Comp.RecipeTags.Contains(p.Tag))
                .Select(p => (ProtoId<WorkbenchRecipePrototype>)p.ID)
                .ToHashSet();

            recipes.UnionWith(taggedRecipes);
        }

        ent.Comp.CombinedRecipes = recipes.ToList();
        Dirty(ent);
        UpdateUIRecipes(ent);
    }

    private void OnItemRemoved(Entity<WorkbenchComponent> ent, ref ItemRemovedEvent args)
    {
        UpdateUIRecipes(ent);
    }

    private void OnItemPlaced(Entity<WorkbenchComponent> ent, ref ItemPlacedEvent args)
    {
        UpdateUIRecipes(ent);
    }

    private void OnBeforeUIOpen(Entity<WorkbenchComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        UpdateUIRecipes(ent);
    }

    private bool CanCraftRecipe(WorkbenchRecipePrototype recipe, WorkbenchCraftingContext context)
    {
        foreach (var req in recipe.Requirements)
        {
            if (!req.CheckRequirement(EntityManager, ProtoMan, context))
                return false;
        }

        return true;
    }

    private void StartCraft(Entity<WorkbenchComponent> workbench,
        EntityUid user,
        WorkbenchRecipePrototype recipe)
    {
        var craftDoAfter = new WorkbenchCraftDoAfterEvent
        {
            Recipe = recipe.ID,
        };

        var doAfterArgs = new DoAfterArgs(EntityManager,
            user,
            recipe.CraftTime * workbench.Comp.CraftSpeed,
            craftDoAfter,
            workbench,
            workbench)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        _audio.PlayPredicted(recipe.OverrideCraftSound ?? workbench.Comp.CraftSound, workbench, user);
    }

    private void OnCraftFinished(Entity<WorkbenchComponent> ent, ref WorkbenchCraftDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!ProtoMan.TryIndex(args.Recipe, out var recipe))
            return;

        var getResource = new WorkbenchGetResourcesEvent();
        RaiseLocalEvent(ent.Owner, getResource);

        var context = new WorkbenchCraftingContext(User: args.User,
            Workbench: ent.Owner,
            Ingredients: getResource.Resources.ToArray());

        // Check requirements
        if (!CanCraftRecipe(recipe, context))
        {
            _popup.PopupPredicted(Loc.GetString("workbench-cant-craft-error"), ent, args.User);
            return;
        }

        // Check conditions
        var passConditions = true;
        foreach (var condition in recipe.Conditions)
        {
            if (!condition.CheckCondition(EntityManager, ProtoMan, context))
            {
                condition.FailedEffect(EntityManager, ProtoMan, context);
                passConditions = false;
            }

            condition.PostCraft(EntityManager, ProtoMan, context);
        }

        // Perform post-craft requirement effects (e.g. spending resources)
        foreach (var req in recipe.Requirements)
            req.PostCraft(EntityManager, ProtoMan, context);

        // Spawn entities, if all conditions are right
        if (passConditions)
        {
            var random = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(ent.Owner));
            var coords = Transform(ent).Coordinates;
            var resultEntities = new HashSet<EntityUid>();

            for (var i = 0; i < recipe.ResultCount; i++)
            {
                var offset = new Vector2(random.NextFloat(-0.25f, 0.25f), random.NextFloat(-0.25f, 0.25f));
                var resultEntity = PredictedSpawnAtPosition(recipe.Result, coords.Offset(offset));
                resultEntities.Add(resultEntity);
            }
        }

        UpdateUIRecipes(ent);
        args.Handled = true;
    }
}

/// <summary>
///     Fired when a player is attempting to craft a recipe.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class WorkbenchCraftDoAfterEvent : DoAfterEvent
{
    [DataField(required: true)]
    public ProtoId<WorkbenchRecipePrototype> Recipe = default!;

    public override DoAfterEvent Clone()
    {
        return this;
    }
}

/// <summary>
///     A list of shared parameters for methods involving workbench crafting.
/// </summary>
public record WorkbenchCraftingContext(EntityUid? User, EntityUid Workbench, EntityUid[] Ingredients)
{
    /// <summary>
    ///     The entity initiating this craft operation.
    /// </summary>
    public EntityUid? User = User;

    /// <summary>
    ///     The workbench entity.
    /// </summary>
    public EntityUid Workbench = Workbench;

    /// <summary>
    ///     A list of valid, usable crafting ingredients.
    /// </summary>
    public EntityUid[] Ingredients = Ingredients;
}
