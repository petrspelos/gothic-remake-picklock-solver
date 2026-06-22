namespace PicklockSolver.Core.Models;

public record struct LockpickSolutionStep(
    byte PlateId,
    LockpickMoveType MoveType);
    