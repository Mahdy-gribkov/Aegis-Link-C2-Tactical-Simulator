using System.Text.RegularExpressions;

namespace AegisLink.App.Services;

public record ParsedCommand(string Command, Dictionary<string, string?> Flags, List<string> Args);

/// <summary>
/// Parses terminal commands with flags support: scan -t 192.168.1.5 --verbose
/// </summary>
public static class CommandTokenizer
{
    private static readonly Regex FlagPattern = new(@"^--?(\w+)$", RegexOptions.Compiled);

    public static ParsedCommand? Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        
        // Sanitize: only allow alphanumeric, spaces, dashes, dots
        if (!Regex.IsMatch(input, @"^[a-zA-Z0-9\s\-\.\:]+$")) return null;

        var tokens = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0) return null;

        var command = tokens[0].ToUpperInvariant();
        var flags = new Dictionary<string, string?>();
        var args = new List<string>();

        for (int i = 1; i < tokens.Length; i++)
        {
            var token = tokens[i];
            var match = FlagPattern.Match(token);
            
            if (match.Success)
            {
                var flagName = match.Groups[1].Value.ToLowerInvariant();
                // Check if next token is value (not another flag)
                if (i + 1 < tokens.Length && !FlagPattern.IsMatch(tokens[i + 1]))
                {
                    flags[flagName] = tokens[++i];
                }
                else
                {
                    flags[flagName] = null;
                }
            }
            else
            {
                args.Add(token);
            }
        }

        return new ParsedCommand(command, flags, args);
    }
}
