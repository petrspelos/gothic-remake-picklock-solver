using FluentResults;

namespace PicklockSolver.Core.Parser.Abstractions;

public interface IFileSystem
{
    Result<string> ReadAllFileText(string filePath);
}
