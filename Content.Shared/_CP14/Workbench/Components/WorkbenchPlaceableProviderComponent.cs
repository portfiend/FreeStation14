using Content.Shared._CP14.Workbench;

namespace Content.Shared._CP14.Workbench;

/// <summary>
/// Provides resources to the workbench located on ItemPlacer
/// </summary>
[RegisterComponent]
[Access(typeof(SharedWorkbenchSystem))]
public sealed partial class WorkbenchPlaceableProviderComponent : Component
{ }
