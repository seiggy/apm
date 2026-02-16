using System.Text;
using System.Text.RegularExpressions;

namespace Apm.Cli.Utils;

/// <summary>
/// Minimal AOT-compatible TOML parser and serializer.
/// Supports the subset used by Codex config: string/bool/integer values,
/// nested tables (sections), and basic arrays.
/// </summary>
public static partial class SimpleToml
{
    /// <summary>
    /// Parse a TOML string into a nested dictionary.
    /// </summary>
    public static Dictionary<string, object?> Parse(string text)
    {
        var root = new Dictionary<string, object?>();
        var currentTable = root;
        var lines = text.Split('\n');

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd('\r').Trim();

            // Skip empty lines and comments
            if (string.IsNullOrEmpty(line) || line.StartsWith('#'))
                continue;

            // Table header: [section] or [section.subsection]
            var headerMatch = TableHeaderRegex().Match(line);
            if (headerMatch.Success)
            {
                var path = headerMatch.Groups[1].Value;
                currentTable = EnsureTablePath(root, path);
                continue;
            }

            // Key = Value
            var eqIndex = line.IndexOf('=');
            if (eqIndex < 0) continue;

            var key = line[..eqIndex].Trim();
            var valuePart = line[(eqIndex + 1)..].Trim();

            // Strip inline comments (not inside strings)
            var parsedValue = ParseValue(valuePart);
            currentTable[key] = parsedValue;
        }

        return root;
    }

    /// <summary>
    /// Serialize a nested dictionary to a TOML string.
    /// </summary>
    public static string Serialize(Dictionary<string, object?> data)
    {
        var sb = new StringBuilder();
        WriteTable(sb, data, "");
        return sb.ToString();
    }

    private static void WriteTable(StringBuilder sb, Dictionary<string, object?> table, string prefix)
    {
        // Write simple key-value pairs first
        foreach (var kvp in table)
        {
            if (kvp.Value is Dictionary<string, object?>)
                continue; // handled after

            if (kvp.Value is List<object?> list)
            {
                sb.Append(kvp.Key);
                sb.Append(" = ");
                WriteArray(sb, list);
                sb.AppendLine();
            }
            else
            {
                sb.Append(kvp.Key);
                sb.Append(" = ");
                WriteValue(sb, kvp.Value);
                sb.AppendLine();
            }
        }

        // Write nested tables
        foreach (var kvp in table)
        {
            if (kvp.Value is not Dictionary<string, object?> nested)
                continue;

            var fullKey = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";
            sb.AppendLine();
            sb.AppendLine($"[{fullKey}]");
            WriteTable(sb, nested, fullKey);
        }
    }

    private static void WriteValue(StringBuilder sb, object? value)
    {
        switch (value)
        {
            case null:
                sb.Append("\"\"");
                break;
            case bool b:
                sb.Append(b ? "true" : "false");
                break;
            case int i:
                sb.Append(i);
                break;
            case long l:
                sb.Append(l);
                break;
            case double d:
                sb.Append(d.ToString(System.Globalization.CultureInfo.InvariantCulture));
                break;
            case string s:
                sb.Append('"');
                sb.Append(EscapeString(s));
                sb.Append('"');
                break;
            default:
                sb.Append('"');
                sb.Append(EscapeString(value.ToString() ?? ""));
                sb.Append('"');
                break;
        }
    }

    private static void WriteArray(StringBuilder sb, List<object?> list)
    {
        sb.Append('[');
        for (var i = 0; i < list.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            WriteValue(sb, list[i]);
        }
        sb.Append(']');
    }

    private static object? ParseValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        // Boolean
        if (value == "true") return true;
        if (value == "false") return false;

        // String (basic quoted)
        if (value.StartsWith('"'))
            return ParseBasicString(value);

        // String (literal quoted)
        if (value.StartsWith('\''))
            return ParseLiteralString(value);

        // Array
        if (value.StartsWith('['))
            return ParseArray(value);

        // Integer
        if (long.TryParse(value, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var l))
            return l;

        // Float
        if (double.TryParse(value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var d))
            return d;

        // Strip inline comment and return as string
        var commentIdx = value.IndexOf('#');
        if (commentIdx > 0)
            return ParseValue(value[..commentIdx].Trim());

        return value;
    }

    private static string ParseBasicString(string value)
    {
        // Find the closing quote, handling escape sequences
        var sb = new StringBuilder();
        var i = 1; // skip opening quote
        while (i < value.Length)
        {
            if (value[i] == '\\' && i + 1 < value.Length)
            {
                switch (value[i + 1])
                {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case 'n': sb.Append('\n'); break;
                    case 't': sb.Append('\t'); break;
                    case 'r': sb.Append('\r'); break;
                    default: sb.Append(value[i + 1]); break;
                }
                i += 2;
            }
            else if (value[i] == '"')
            {
                break;
            }
            else
            {
                sb.Append(value[i]);
                i++;
            }
        }
        return sb.ToString();
    }

    private static string ParseLiteralString(string value)
    {
        var end = value.IndexOf('\'', 1);
        return end < 0 ? value[1..] : value[1..end];
    }

    private static List<object?> ParseArray(string value)
    {
        var list = new List<object?>();
        // Strip outer brackets
        var inner = value[1..];
        var closeBracket = FindMatchingBracket(inner);
        if (closeBracket >= 0)
            inner = inner[..closeBracket];
        else
            inner = inner.TrimEnd(']');

        foreach (var item in SplitArrayItems(inner))
        {
            var trimmed = item.Trim();
            if (!string.IsNullOrEmpty(trimmed))
                list.Add(ParseValue(trimmed));
        }

        return list;
    }

    private static int FindMatchingBracket(string s)
    {
        var depth = 0;
        var inString = false;
        var stringChar = '\0';

        for (var i = 0; i < s.Length; i++)
        {
            if (inString)
            {
                if (s[i] == '\\' && i + 1 < s.Length)
                    i++; // skip escaped char
                else if (s[i] == stringChar)
                    inString = false;
            }
            else
            {
                switch (s[i])
                {
                    case '"' or '\'':
                        inString = true;
                        stringChar = s[i];
                        break;
                    case '[':
                        depth++;
                        break;
                    case ']' when depth == 0:
                        return i;
                    case ']':
                        depth--;
                        break;
                }
            }
        }
        return -1;
    }

    private static IEnumerable<string> SplitArrayItems(string inner)
    {
        var current = new StringBuilder();
        var depth = 0;
        var inString = false;
        var stringChar = '\0';

        foreach (var ch in inner)
        {
            if (inString)
            {
                current.Append(ch);
                if (ch == '\\')
                    continue;
                if (ch == stringChar)
                    inString = false;
            }
            else
            {
                switch (ch)
                {
                    case '"' or '\'':
                        inString = true;
                        stringChar = ch;
                        current.Append(ch);
                        break;
                    case '[':
                        depth++;
                        current.Append(ch);
                        break;
                    case ']':
                        depth--;
                        current.Append(ch);
                        break;
                    case ',' when depth == 0:
                        yield return current.ToString();
                        current.Clear();
                        break;
                    default:
                        current.Append(ch);
                        break;
                }
            }
        }

        if (current.Length > 0)
            yield return current.ToString();
    }

    private static Dictionary<string, object?> EnsureTablePath(Dictionary<string, object?> root, string path)
    {
        var parts = path.Split('.');
        var current = root;

        foreach (var part in parts)
        {
            if (!current.TryGetValue(part, out var existing) || existing is not Dictionary<string, object?> nested)
            {
                nested = new Dictionary<string, object?>();
                current[part] = nested;
            }
            current = nested;
        }

        return current;
    }

    private static string EscapeString(string s)
    {
        return s
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    [GeneratedRegex(@"^\[([^\[\]]+)\]$")]
    private static partial Regex TableHeaderRegex();
}
