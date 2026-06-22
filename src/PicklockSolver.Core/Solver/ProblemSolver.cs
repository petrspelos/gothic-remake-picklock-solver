using FluentResults;
using PicklockSolver.Core.Models;
using PicklockSolver.Core.Solver.Abstractions;

namespace PicklockSolver.Core.Solver;

public class ProblemSolver : IProblemSolver
{
    public Result<IReadOnlyCollection<LockpickSolutionStep>> Solve(LockpickProblem problem)
    {
        if (problem.InitialState == problem.TargetState)
            return Result.Ok<IReadOnlyCollection<LockpickSolutionStep>>([]);

        var processQueue = new Queue<LockpickState>([problem.InitialState]);
        var visited = new HashSet<LockpickState> { problem.InitialState };
        var cameFrom = new Dictionary<LockpickState, (LockpickMove Move, LockpickState Parent)>();

        IReadOnlyList<(LockpickMove, LockpickState)> GeneratePossibleMovesFromState(LockpickState state)
        {
            var foundPossibleMoves = state.PinStates.SelectMany((pins, plateId) =>
            {
                List<LockpickRule> rules = [];
                if (problem.RuleSet.Any(rs => rs.Key == plateId))
                {
                    rules.AddRange(problem.RuleSet[(byte)plateId]);
                }

                var maxLeft = 0b1 << problem.PlateWidth - 1;

                List<(LockpickMove, LockpickState)> validMoves = [];

                // Generate Left move (shifts pins to the right)
                if (pins != 0b1)
                {
                    if (rules.Count == 0 || rules.All(rule =>
                        {
                            var linkedState = state.PinStates[rule.PlateId];
                            if (rule.Type == LockpickRuleType.Synchronized)
                                return linkedState != 0b1;
                            return linkedState != maxLeft;
                        }))
                    {
                        var leftState = state.MakeCopy();
                        leftState.PinStates[plateId] >>= 1;

                        foreach (var rule in rules)
                        {
                            if (rule.Type == LockpickRuleType.Synchronized)
                                leftState.PinStates[rule.PlateId] >>= 1;
                            else
                                leftState.PinStates[rule.PlateId] <<= 1;
                        }

                        validMoves.Add((new LockpickMove((byte)plateId, LockpickMoveType.Left, 0, 0), leftState));
                    }
                }

                // Generate Right move (shifts pins to the left)
                if (pins != maxLeft)
                {
                    if (rules.Count == 0 || rules.All(rule =>
                        {
                            var linkedState = state.PinStates[rule.PlateId];
                            if (rule.Type == LockpickRuleType.Synchronized)
                                return linkedState != maxLeft;
                            return linkedState != 0b1;
                        }))
                    {
                        var rightState = state.MakeCopy();
                        rightState.PinStates[plateId] <<= 1;

                        foreach (var rule in rules)
                        {
                            if (rule.Type == LockpickRuleType.Synchronized)
                                rightState.PinStates[rule.PlateId] <<= 1;
                            else
                                rightState.PinStates[rule.PlateId] >>= 1;
                        }

                        validMoves.Add((new LockpickMove((byte)plateId, LockpickMoveType.Right, 0, 0), rightState));
                    }
                }

                return validMoves;
            });

            return foundPossibleMoves.ToList().AsReadOnly();
        }

        while (processQueue.Count > 0)
        {
            var currentState = processQueue.Dequeue();

            foreach (var (move, newState) in GeneratePossibleMovesFromState(currentState))
            {
                if (!visited.Add(newState))
                    continue;

                cameFrom[newState] = (move, currentState);

                if (newState == problem.TargetState)
                {
                    List<LockpickSolutionStep> solutionSteps = [];
                    var backCurrentState = newState;

                    while (backCurrentState != problem.InitialState)
                    {
                        var (backMove, parent) = cameFrom[backCurrentState];
                        solutionSteps.Add(new(backMove.PlateId, backMove.MoveType));
                        backCurrentState = parent;
                    }

                    solutionSteps.Reverse();
                    return solutionSteps;
                }

                processQueue.Enqueue(newState);
            }
        }

        return Result.Fail("Failed to determine the problem solution.");
    }
}