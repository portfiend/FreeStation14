using System.Numerics;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared._CP14.Workbench;
using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Placeable;
using Content.Shared.UserInterface;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Workbench;

public sealed partial class WorkbenchSystem : SharedWorkbenchSystem
{
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private DoAfterSystem _doAfter = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private UserInterfaceSystem _userInterface = default!;
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
        foreach (var recipe in _proto.EnumeratePrototypes<WorkbenchRecipePrototype>())
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

    private void OnCraftFinished(Entity<WorkbenchComponent> ent, ref WorkbenchCraftDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!_proto.TryIndex(args.Recipe, out var recipe))
            return;

        var getResource = new WorkbenchGetResourcesEvent();
        RaiseLocalEvent(ent.Owner, getResource);

        var resources = getResource.Resources;

        if (!CanCraftRecipe(recipe, resources, args.User))
        {
            _popup.PopupEntity(Loc.GetString("cp14-workbench-cant-craft"), ent, args.User);
            return;
        }

        //Check conditions
        var passConditions = true;
        foreach (var condition in recipe.Conditions)
        {
            if (!condition.CheckCondition(EntityManager, _proto, ent, args.User))
            {
                condition.FailedEffect(EntityManager, _proto, ent, args.User);
                passConditions = false;
            }
            condition.PostCraft(EntityManager, _proto, ent, args.User);
        }

        foreach (var req in recipe.Requirements)
        {
            req.PostCraft(EntityManager, _proto, resources);
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
                _transform.SetCoordinates(resultEntity, Transform(ent).Coordinates.Offset(new Vector2(_random.NextFloat(-0.25f, 0.25f), _random.NextFloat(-0.25f, 0.25f))));
            }
        }

        UpdateUIRecipes(ent);
        args.Handled = true;
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

    private bool CanCraftRecipe(WorkbenchRecipePrototype recipe, HashSet<EntityUid> entities, EntityUid user)
    {
        foreach (var req in recipe.Requirements)
        {
            if (!req.CheckRequirement(EntityManager, _proto, entities))
                return false;
        }

        return true;
    }
}
