namespace PicklockSolver.Core.Models;

public record struct LockpickMove(
    byte PlateId,
    LockpickMoveType MoveType,
    int VisitedStateIndex,
    int PreviousStateIndex);
