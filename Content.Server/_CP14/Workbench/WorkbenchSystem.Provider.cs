using Content.Shared.Placeable;

namespace Content.Server._CP14.Workbench;

public sealed partial class WorkbenchSystem
{
    private void InitProviders()
    {
        SubscribeLocalEvent<WorkbenchPlaceableProviderComponent, WorkbenchGetResourcesEvent>(OnGetResource);
    }

    private void OnGetResource(Entity<WorkbenchPlaceableProviderComponent> ent, ref WorkbenchGetResourcesEvent args)
    {
        if (!TryComp<ItemPlacerComponent>(ent, out var placer))
            return;

        args.AddResources(placer.PlacedEntities);
    }
}

public sealed class WorkbenchGetResourcesEvent : EntityEventArgs
{
    public HashSet<EntityUid> Resources { get; private set; } = new();

    public void AddResource(EntityUid resource)
    {
        Resources.Add(resource);
    }

    public void AddResources(IEnumerable<EntityUid> resources)
    {
        foreach (var resource in resources)
        {
            Resources.Add(resource);
        }
    }
}
