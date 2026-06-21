using System.Text;
using FluentResults;
using PicklockSolver.Core.Models;
using PicklockSolver.Core.Parser.Abstractions;

namespace PicklockSolver.Core.Parser;

public class ProblemFileParser(
    IFileSystem fileSystem)
    : IProblemFileParser
{
    public Result<LockpickProblem> ParseFile(string filePath)
    {
        var contentResult = fileSystem.ReadAllFileText(filePath);
        if (contentResult.IsFailed)
        {
            return Result.Fail(contentResult.Errors);
        }
        
        var content = contentResult.Value;
        var lastPos = content.Length - 1;
        
        var pos = 0;
        EatAllWhiteSpace(content, ref pos);

        // Start parsing initial state
        if (pos == lastPos || content[pos] != '[')
        {
            return Result.Fail($"Failed to parse problem content. Expected character at position {pos} to be '['.");
        }
        
        // Start parsing plate state until end of definition
        var initialStateResult = ParseBinaryByteArray(content, ref pos);
        if (initialStateResult.IsFailed)
        {
            return Result.Fail(initialStateResult.Errors);
        }
        
        EatAllWhiteSpace(content, ref pos);
        if (pos >= lastPos)
        {
            return Result.Fail("Unexpected end of file reached.");
        }
        
        // Start parsing target state
        if (pos == lastPos || content[pos] != '[')
        {
            return Result.Fail($"Failed to parse problem content. Expected character at position {pos} to be '['.");
        }

        var targetStateResult = ParseBinaryByteArray(content, ref pos);
        if (targetStateResult.IsFailed)
        {
            return Result.Fail(targetStateResult.Errors);
        }
        
        EatAllWhiteSpace(content, ref pos);
        if (pos >= lastPos)
        {
            // No rule set, technically a valid problem.
            return Result.Ok(new LockpickProblem(
                InitialState: new LockpickState(initialStateResult.Value.ToArray()),
                TargetState: new LockpickState(targetStateResult.Value.ToArray()),
                RuleSet: []));
        }

        Dictionary<byte, LockpickRule[]> ruleSet = [];
        while (pos < lastPos)
        {
            if (!char.IsAsciiDigit(content[pos]))
            {
                return Result.Fail($"Expected a digit at position {pos} for a rule-set definition.");
            }

            var plateId = byte.Parse(content[pos].ToString());

            pos++;
            if (pos >= lastPos || content[pos] != ':')
            {
                return Result.Fail($"Expected ':' at position {pos} as part of a rule-set definition.");
            }

            pos++;
            if (pos > lastPos || content[pos] != '[')
            {
                return Result.Fail($"Expected '[' at position {pos} as part of a rule-set definition.");
            }

            var ruleSetResult = ParseRules(content, ref pos);
            if (ruleSetResult.IsFailed)
            {
                return Result.Fail(ruleSetResult.Errors);
            }
            
            ruleSet.Add(plateId, ruleSetResult.Value.ToArray());
            EatAllWhiteSpace(content, ref pos);
        }
        
        return Result.Ok(new LockpickProblem(
            InitialState: new LockpickState(initialStateResult.Value.ToArray()),
            TargetState: new LockpickState(targetStateResult.Value.ToArray()),
            RuleSet: ruleSet));
    }

    private static Result<List<byte>> ParseBinaryByteArray(string content, ref int pos)
    {
        var lastPos = content.Length - 1;
        List<byte> array = [];
        while (content[pos] != ']')
        {
            pos++;
            EatAllWhiteSpace(content, ref pos);
            
            byte value = 0;

            while (pos < lastPos && (content[pos] == '0' || content[pos] == '1'))
            {
                value <<= 1;

                if (content[pos] == '1')
                {
                    value |= 0b_1;
                }

                pos++;
            }
            
            if (pos >= lastPos)
            {
                return Result.Fail("Unexpected end of file reached.");
            }
            
            array.Add(value);
        }
        
        pos++;
        return array;
    }

    private static Result<List<LockpickRule>> ParseRules(string content, ref int pos)
    {
        var lastPos = content.Length - 1;
        List<LockpickRule> rules = [];
        while (content[pos] != ']')
        {
            pos++;
            if (pos >= lastPos || !char.IsAsciiDigit(content[pos]))
            {
                if (content[pos] == ',' && pos + 1 < lastPos)
                {
                    pos++;
                }
                else
                {
                    return Result.Fail($"Expected a plate ID digit at pos {pos} as part of a rule definition.");
                }
            }

            var plateId = byte.Parse(content[pos].ToString());

            pos++;
            if (pos >= lastPos || content[pos] != ':')
            {
                return Result.Fail(
                    $"Expected ':' at position {pos} as part of a rule definition.");
            }
            
            pos++;
            if (pos >= lastPos || !(content[pos] == 'O' || content[pos] == 'S'))
            {
                return Result.Fail(
                    $"Expected a lockpick rule type (O or S) at position {pos} as part of a rule definition.");
            }

            var type = content[pos] == 'O'
                ? LockpickRuleType.Opposite
                : LockpickRuleType.Synchronized;
            
            rules.Add(new(plateId, type));

            pos++;
        }

        if (pos < lastPos)
        {
            pos++;
        }
        return rules;
    }
    
    private static void EatAllWhiteSpace(string content, ref int pos)
    {
        if (pos >= content.Length)
            return;
        
        while (char.IsWhiteSpace(content[pos]))
        {
            pos++;
            if (pos >= content.Length)
                return;
        }
    }
}
