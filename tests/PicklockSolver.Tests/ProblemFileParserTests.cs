using Moq;
using PicklockSolver.Core.Models;
using PicklockSolver.Core.Parser.Abstractions;
using PicklockSolver.Core.Parser;

namespace PicklockSolver.Tests;

public class ProblemFileParserTests
{
    private const string TestFilePath = "./test/file.prob";
    
    private readonly Mock<IFileSystem> _fileSystemMock = new();

    private readonly IProblemFileParser _parser;

    public ProblemFileParserTests()
    {
        _parser = new ProblemFileParser(_fileSystemMock.Object);
    }

    [Fact]
    public void Parse_ValidFileContent_ShouldReturnCorrectData()
    {
        // Arrange
        _fileSystemMock
            .Setup(fs => fs.ReadAllFileText(TestFilePath))
            .Returns("""
                     [100,
                      001]
                     
                     [010,
                      010]
                     
                     0:[1:S]
                     1:[0:O,2:S]
                     """);

        // Act
        var result = _parser.ParseFile(TestFilePath);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.InitialState.PinStates.Length);
        Assert.Equal((byte)0b100, result.Value.InitialState.PinStates[0]);
        Assert.Equal((byte)0b001, result.Value.InitialState.PinStates[1]);
        Assert.Equal(2, result.Value.TargetState.PinStates.Length);
        Assert.Equal((byte)0b010, result.Value.TargetState.PinStates[0]);
        Assert.Equal((byte)0b010, result.Value.TargetState.PinStates[1]);
        Assert.Collection(result.Value.RuleSet,
            kvp =>
            {
                Assert.Equal(0, kvp.Key);
                var rule = Assert.Single(kvp.Value);
                Assert.Equal(1, rule.PlateId);
                Assert.Equal(LockpickRuleType.Synchronized, rule.Type);
            },
            kvp =>
            {
                Assert.Equal(1, kvp.Key);
                Assert.Equal(2, kvp.Value.Length);
                var rule1 = kvp.Value[0];
                var rule2 = kvp.Value[1];
                Assert.Equal(0, rule1.PlateId);
                Assert.Equal(LockpickRuleType.Opposite, rule1.Type);
                Assert.Equal(2, rule2.PlateId);
                Assert.Equal(LockpickRuleType.Synchronized, rule2.Type);
            });
    }
}
