namespace PicklockSolver.Core.Models;

public record struct LockpickProblem(
    LockpickState InitialState,
    LockpickState TargetState,
    Dictionary<byte, LockpickRule[]> RuleSet);
