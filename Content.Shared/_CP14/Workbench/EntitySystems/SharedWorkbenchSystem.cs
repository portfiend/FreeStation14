using System.Numerics;
using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Placeable;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

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
    [Dependency] private SharedUserInterfaceSystem _userInterface = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private IRobustRandom _random = default!;

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
    }

    private void OnMapInit(Entity<WorkbenchComponent> ent, ref MapInitEvent args)
    {
        foreach (var recipe in ProtoMan.EnumeratePrototypes<WorkbenchRecipePrototype>())
        {
            if (ent.Comp.Recipes.Contains(recipe))
                continue;

            if (!ent.Comp.RecipeTags.Contains(recipe.Tag))
                continue;

            ent.Comp.Recipes.Add(recipe);
        }
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
        _audio.PlayPvs(recipe.OverrideCraftSound ?? workbench.Comp.CraftSound, workbench);
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
            Ingredients: getResource.Resources);

        if (!CanCraftRecipe(recipe, context))
        {
            _popup.PopupEntity(Loc.GetString("cp14-workbench-cant-craft"), ent, args.User);
            return;
        }

        //Check conditions
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

        foreach (var req in recipe.Requirements)
        {
            req.PostCraft(EntityManager, ProtoMan, context);
        }

        if (passConditions)
        {
            var resultEntities = new HashSet<EntityUid>();
            for (var i = 0; i < recipe.ResultCount; i++)
            {
                var resultEntity = Spawn(recipe.Result);
                resultEntities.Add(resultEntity);
            }

            //We teleport result to workbench AFTER craft.
            foreach (var resultEntity in resultEntities)
            {
                var coords = Transform(ent).Coordinates;
                var offset = new Vector2(_random.NextFloat(-0.25f, 0.25f),
                    _random.NextFloat(-0.25f, 0.25f));

                _transform.SetCoordinates(resultEntity, coords.Offset(offset));
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
public record WorkbenchCraftingContext(EntityUid? User, EntityUid Workbench, HashSet<EntityUid> Ingredients)
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
    public HashSet<EntityUid> Ingredients = Ingredients;
}
