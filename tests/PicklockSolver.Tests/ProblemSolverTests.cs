using Moq;
using PicklockSolver.Core.Models;
using PicklockSolver.Core.Parser;
using PicklockSolver.Core.Parser.Abstractions;
using PicklockSolver.Core.Solver;
using PicklockSolver.Core.Solver.Abstractions;

namespace PicklockSolver.Tests;

public class ProblemSolverTests
{
    private readonly IProblemSolver _solver = new ProblemSolver();

    [Fact]
    public void Comparisons_SanityCheck()
    {
        var stateA = new LockpickState([0b100, 0b001]);
        var stateB = new LockpickState([0b100, 0b001]);
        var stateC = new LockpickState([0b010, 0b001]);

        List<LockpickState> list = [stateA];
        
        Assert.True(stateA.Equals(stateB));
        Assert.False(stateA.Equals(stateC));
        
        Assert.Contains(stateA, list);
        Assert.DoesNotContain(stateC, list);
    }
    
    [Fact]
    public void Solve_ValidProblem_ShouldSolve()
    {
        // Arrange
        var ruleSet = new Dictionary<byte, LockpickRule[]>();
        ruleSet.Add(0, [ new(1, LockpickRuleType.Synchronized) ]);
        
        var problem = new LockpickProblem(
            InitialState: new([0b100, 0b001]),
            TargetState: new([0b010, 0b010]),
            RuleSet: new(ruleSet),
            PlateWidth: 3);
        
        // Act
        var solutionStepsResult = _solver.Solve(problem);
        
        // Assert
        Assert.True(solutionStepsResult.IsSuccess);
        var solutionSteps = solutionStepsResult.Value;
        Assert.Equal(3, solutionSteps.Count);
    }
    
    [Fact]
    public void Solve_ComplexProblem_ShouldSolve()
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystem>();
        fileSystemMock
            .Setup(fs => fs.ReadAllFileText(It.IsAny<string>()))
            .Returns("""
                     [0100000,
                      0001000,
                      0000100,
                      0000001,
                      0000100,
                      0000100]
                     
                     [0001000,
                      0001000,
                      0001000,
                      0001000,
                      0001000,
                      0001000]
                     
                     0:[1:O,2:S]
                     2:[0:S,4:O]
                     3:[1:O,2:O,5:S]
                     4:[5:S,3:O]
                     5:[0:S]
                     """);
        var parser = new ProblemFileParser(fileSystemMock.Object);
        var problemResult = parser.ParseFile("foo");

        var problem = problemResult.Value;
        problem.PlateWidth = 7;
        
        // Act
        var solutionStepsResult = _solver.Solve(problem);
        
        var todo = string.Join("\n", solutionStepsResult.Value.Select(s => $"[Plate {s.PlateId + 1} {s.MoveType}]"));
        
        // Assert
        Assert.True(solutionStepsResult.IsSuccess);
    }
}