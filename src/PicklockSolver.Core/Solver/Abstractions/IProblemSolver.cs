using FluentResults;
using PicklockSolver.Core.Models;

namespace PicklockSolver.Core.Solver.Abstractions;

public interface IProblemSolver
{
    Result<IReadOnlyCollection<LockpickSolutionStep>> Solve(LockpickProblem problem);
}
