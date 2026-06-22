namespace PicklockSolver.Core.Models;

public readonly record struct LockpickState(byte[] PinStates)
{
    public bool Equals(LockpickState other) =>
        PinStates.AsSpan().SequenceEqual(other.PinStates);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var v in PinStates)
            hash.Add(v);
        return hash.ToHashCode();
    }
    
    public LockpickState MakeCopy() => new LockpickState(PinStates: PinStates.ToArray());
}
