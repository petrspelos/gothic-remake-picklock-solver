using FluentResults;
using PicklockSolver.Core.Models;

namespace PicklockSolver.Core.Parser.Abstractions;

public interface IProblemFileParser
{
    public Result<LockpickProblem> ParseFile(string filePath);
}
