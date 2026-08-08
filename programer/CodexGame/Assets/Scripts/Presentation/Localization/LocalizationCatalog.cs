#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CodexGame.Application.Playable;

namespace CodexGame.Presentation.Localization
{
  public sealed class LocalizationCatalog
  {
    public const string DefaultLanguage = "ko";
    public const string FallbackLanguage = "en";
    public const int RequiredInitialKeyCount = 109;

    private static readonly Regex PlaceholderPattern =
      new Regex(@"\{([A-Za-z][A-Za-z0-9_]*)\}", RegexOptions.Compiled);

    private readonly Dictionary<string, Entry> _entries;
    private readonly HashSet<string> _warnedMissing = new HashSet<string>(StringComparer.Ordinal);
    private readonly Action<string> _warning;

    private LocalizationCatalog(Dictionary<string, Entry> entries, Action<string> warning)
    {
      _entries = entries;
      _warning = warning;
    }

    public int Count => _entries.Count;

    public static LocalizationCatalog Parse(string csv, Action<string>? warning = null)
    {
      if (csv == null) throw new ArgumentNullException(nameof(csv));
      var rows = ParseRows(csv);
      if (rows.Count == 0
        || rows[0].Count != 3
        || rows[0][0] != "Key"
        || rows[0][1] != "ko"
        || rows[0][2] != "en")
      {
        throw new FormatException("Localization CSV header must be exactly Key,ko,en.");
      }

      var entries = new Dictionary<string, Entry>(StringComparer.Ordinal);
      for (var rowIndex = 1; rowIndex < rows.Count; rowIndex++)
      {
        var row = rows[rowIndex];
        if (row.Count == 1 && string.IsNullOrEmpty(row[0])) continue;
        if (row.Count != 3) throw new FormatException("Localization row " + (rowIndex + 1) + " must have three columns.");
        var key = row[0].Trim();
        if (string.IsNullOrWhiteSpace(key)) throw new FormatException("Localization row " + (rowIndex + 1) + " has a blank key.");
        if (string.IsNullOrWhiteSpace(row[1]) || string.IsNullOrWhiteSpace(row[2]))
        {
          throw new FormatException("Localization key " + key + " has a blank ko/en value.");
        }
        if (!SamePlaceholders(row[1], row[2]))
        {
          throw new FormatException("Localization key " + key + " has mismatched ko/en placeholders.");
        }
        if (!entries.TryAdd(key, new Entry(row[1], row[2])))
        {
          throw new FormatException("Duplicate localization key: " + key);
        }
      }

      if (entries.Count != RequiredInitialKeyCount)
      {
        throw new FormatException(
          "Localization CSV must contain " + RequiredInitialKeyCount + " initial keys, but found " + entries.Count + ".");
      }
      return new LocalizationCatalog(entries, warning ?? (_ => { }));
    }

    public string Get(string key, string language, params LocalizationArgument[] arguments)
    {
      if (!_entries.TryGetValue(key, out var entry))
      {
        if (_warnedMissing.Add(key)) _warning("Missing localization key: " + key);
        return "[MISSING:" + key + "]";
      }

      var value = language == DefaultLanguage ? entry.Korean : entry.English;
      if (string.IsNullOrEmpty(value)) value = entry.English;
      if (string.IsNullOrEmpty(value))
      {
        if (_warnedMissing.Add(key)) _warning("Missing localization value: " + key);
        return "[MISSING:" + key + "]";
      }

      if (arguments == null) return value;
      for (var index = 0; index < arguments.Length; index++)
      {
        value = value.Replace("{" + arguments[index].Name + "}", arguments[index].Value);
      }
      return value;
    }

    public string Get(LocalizedStatus status, string language)
    {
      var arguments = new LocalizationArgument[status.Arguments.Count];
      for (var index = 0; index < arguments.Length; index++)
      {
        var source = status.Arguments[index];
        arguments[index] = new LocalizationArgument(
          source.Name,
          source.ValueIsLocalizationKey ? Get(source.Value, language) : source.Value);
      }
      return Get(status.Key, language, arguments);
    }

    private static bool SamePlaceholders(string left, string right)
    {
      var leftNames = GetPlaceholders(left);
      var rightNames = GetPlaceholders(right);
      return leftNames.SetEquals(rightNames);
    }

    private static HashSet<string> GetPlaceholders(string value)
    {
      var result = new HashSet<string>(StringComparer.Ordinal);
      var matches = PlaceholderPattern.Matches(value);
      for (var index = 0; index < matches.Count; index++) result.Add(matches[index].Groups[1].Value);
      return result;
    }

    private static List<List<string>> ParseRows(string csv)
    {
      var rows = new List<List<string>>();
      var row = new List<string>();
      var field = new StringBuilder();
      var quoted = false;
      for (var index = 0; index < csv.Length; index++)
      {
        var character = csv[index];
        if (quoted)
        {
          if (character == '"')
          {
            if (index + 1 < csv.Length && csv[index + 1] == '"')
            {
              field.Append('"');
              index++;
            }
            else quoted = false;
          }
          else field.Append(character);
        }
        else if (character == '"') quoted = true;
        else if (character == ',')
        {
          row.Add(field.ToString());
          field.Clear();
        }
        else if (character == '\n')
        {
          row.Add(TrimCarriageReturn(field.ToString()));
          field.Clear();
          rows.Add(row);
          row = new List<string>();
        }
        else field.Append(character);
      }
      if (quoted) throw new FormatException("Localization CSV has an unterminated quoted field.");
      if (field.Length > 0 || row.Count > 0)
      {
        row.Add(TrimCarriageReturn(field.ToString()));
        rows.Add(row);
      }
      return rows;
    }

    private static string TrimCarriageReturn(string value)
    {
      return value.EndsWith("\r", StringComparison.Ordinal)
        ? value.Substring(0, value.Length - 1)
        : value;
    }

    private readonly struct Entry
    {
      public Entry(string korean, string english)
      {
        Korean = korean;
        English = english;
      }
      public string Korean { get; }
      public string English { get; }
    }
  }

  public readonly struct LocalizationArgument
  {
    public LocalizationArgument(string name, object value)
    {
      Name = name ?? throw new ArgumentNullException(nameof(name));
      Value = value == null
        ? string.Empty
        : Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }
    public string Name { get; }
    public string Value { get; }
  }
}
