namespace PicklockSolver.Core.Models;

public record struct LockpickRule(
    byte PlateId,
    LockpickRuleType Type);
