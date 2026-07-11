using Content.Shared._CP14.Workbench.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Workbench;

[Serializable, NetSerializable]
public enum WorkbenchUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class WorkbenchUiCraftMessage(ProtoId<WorkbenchRecipePrototype> recipe)
    : BoundUserInterfaceMessage
{
    public readonly ProtoId<WorkbenchRecipePrototype> Recipe = recipe;
}


[Serializable, NetSerializable]
public sealed class WorkbenchUiRecipesState(List<WorkbenchUiRecipesEntry> recipes) : BoundUserInterfaceState
{
    public readonly List<WorkbenchUiRecipesEntry> Recipes = recipes;
}

[Serializable, NetSerializable]
public readonly struct WorkbenchUiRecipesEntry(ProtoId<WorkbenchRecipePrototype> protoId, bool craftable)
    : IEquatable<WorkbenchUiRecipesEntry>
{
    public readonly ProtoId<WorkbenchRecipePrototype> ProtoId = protoId;
    public readonly bool Craftable = craftable;

    public int CompareTo(WorkbenchUiRecipesEntry other)
    {
        return Craftable.CompareTo(other.Craftable);
    }

    public override bool Equals(object? obj)
    {
        return obj is WorkbenchUiRecipesEntry other && Equals(other);
    }

    public bool Equals(WorkbenchUiRecipesEntry other)
    {
        return ProtoId.Id == other.ProtoId.Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ProtoId, Craftable);
    }

    public override string ToString()
    {
        return $"{ProtoId} ({Craftable})";
    }

    public static int CompareTo(WorkbenchUiRecipesEntry left, WorkbenchUiRecipesEntry right)
    {
        return right.CompareTo(left);
    }
}
