using FluentResults;
using PicklockSolver.Core.Parser.Abstractions;

namespace PicklockSolver.Core.Parser;

public class SystemFileSystem : IFileSystem
{
    public Result<string> ReadAllFileText(string filePath)
    {
        try
        {
            var fileInfo = new FileInfo(filePath.Trim());

            if (!fileInfo.Exists)
            {
                return Result.Fail($"File '{fileInfo.FullName}' does not exist.");
            }
            
            using var sr = fileInfo.OpenText();
            
            return sr.ReadToEnd();
        }
        catch (Exception e)
        {
            return Result.Fail($"Failed to read file '{filePath}'. Reason: {e.Message}");
        }
    }
}
