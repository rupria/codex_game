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
    public const int RequiredKeyCount = 197;

    private static readonly string[] RequiredJokerKeys =
    {
      "UI_POKER_JOKER_CHOICE_TITLE",
      "UI_POKER_JOKER_CHOICE_GUIDE"
    };

    private static readonly string[] RequiredGuideKeys =
    {
      "UI_GUIDE_PAGE_FLOW_TITLE",
      "UI_GUIDE_PAGE_FLOW_BODY",
      "UI_GUIDE_PAGE_HALLI_TITLE",
      "UI_GUIDE_PAGE_HALLI_BODY",
      "UI_GUIDE_PAGE_CARDS_TITLE",
      "UI_GUIDE_PAGE_CARDS_BODY",
      "UI_GUIDE_PAGE_RESULT_TITLE",
      "UI_GUIDE_PAGE_RESULT_BODY",
      "UI_GUIDE_PREV",
      "UI_GUIDE_NEXT",
      "UI_GUIDE_PAGE_INDICATOR",
      "UI_GUIDE_MODAL_HINT"
    };

    private static readonly string[] RequiredStageFlowKeys =
    {
      "UI_STAGE_REWARD_TITLE",
      "UI_STAGE_REWARD_FORMULA",
      "UI_STAGE_REWARD_DETAIL",
      "UI_BULLET_BALANCE",
      "UI_BAR_TITLE",
      "UI_BAR_CONTINUE",
      "UI_TRANSITION_LEAVING",
      "UI_TRANSITION_ENTERING",
      "UI_TRANSITION_OPPONENT",
      "UI_TRANSITION_SIT_START",
      "UI_TRANSITION_APPROACHING_TAVERN",
      "UI_TRANSITION_OPEN_SALOON_DOOR"
    };

    private static readonly string[] RequiredPresentation0124Keys =
    {
      "UI_STAGE_ENTRY",
      "UI_STAGE_ENTRY_SKIP",
      "UI_THREE_CALL_ENTRY",
      "UI_SHOWDOWN_ENTRY",
      "UI_ITEM_LIMIT_ACTIVE",
      "UI_ITEM_LIMIT_REMAINING"
    };

    private static readonly string[] RequiredPresentation0125Keys =
    {
      "UI_GUIDE_TUTORIAL_SKIP",
      "UI_INACTIVITY_RETURN_MESSAGE",
      "UI_COMMON_CONFIRM",
      "UI_RUN_COMPLETE",
      "UI_ITEM_USING",
      "UI_THREE_CALL_FLIP_READY"
    };

    private static readonly string[] RequiredItemExpansion0125Keys =
    {
      "UI_ITEM_RELOAD_DESC",
      "UI_ITEM_BOTTOM_DEAL_DESC",
      "UI_ITEM_HYPE_MAN_DESC",
      "UI_ITEM_HEALTH_RECOVERY_DESC",
      "UI_ITEM_WILD_INK",
      "UI_ITEM_WILD_INK_DESC",
      "UI_ITEM_BARREL",
      "UI_ITEM_BARREL_DESC",
      "UI_ITEM_PREDICTION_INSURANCE",
      "UI_ITEM_PREDICTION_INSURANCE_DESC",
      "UI_ITEM_MERCENARY",
      "UI_ITEM_MERCENARY_DESC",
      "UI_ITEM_EXCHANGE_LOCK_AFTER_INK",
      "UI_ITEM_INSURANCE_APPLIED",
      "UI_ITEM_NO_VALID_REPLACEMENT_PAIR",
      "UI_ITEM_CONFIRM_TIMER",
      "UI_ITEM_CONFIRM_TIMEOUT",
      "UI_PREDICTION_ACTUAL_COUNT",
      "UI_PREDICTION_INSURED_COUNT",
      "UI_PREDICTION_CHARGES",
      "UI_BARREL_DAMAGE_PREVENTED"
    };

    private static readonly string[] RequiredBarShopKeys =
    {
      "UI_BAR_REROLL_FREE",
      "UI_BAR_REROLL_USED",
      "UI_BAR_PURCHASE",
      "UI_BAR_DUMMY_ITEM_01",
      "UI_BAR_DUMMY_ITEM_02",
      "UI_BAR_DUMMY_ITEM_03",
      "UI_BAR_DUMMY_ITEM_04",
      "UI_BAR_DUMMY_ITEM_05",
      "UI_BAR_DUMMY_ITEM_06",
      "UI_ITEM_RELOAD",
      "UI_ITEM_BOTTOM_DEAL",
      "UI_ITEM_HYPE_MAN",
      "UI_ITEM_HEALTH_RECOVERY",
      "UI_ITEM_WINDOW_TITLE",
      "UI_ITEM_CHOOSE_CARD",
      "UI_ITEM_CONFIRM_HAND",
      "UI_ITEM_INVENTORY",
      "UI_ITEM_EMPTY_SLOT",
      "UI_ITEM_ACTION_FAILED",
      "UI_BAR_DUPLICATE_ITEM",
      "UI_BAR_INVENTORY_FULL",
      "UI_BAR_PURCHASE_BLOCKED"
    };

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

      if (entries.Count != RequiredKeyCount)
      {
        throw new FormatException(
          "Localization CSV must contain " + RequiredKeyCount + " keys, but found " + entries.Count + ".");
      }
      for (var index = 0; index < RequiredGuideKeys.Length; index++)
      {
        if (!entries.ContainsKey(RequiredGuideKeys[index]))
        {
          throw new FormatException("Missing required guide localization key: " + RequiredGuideKeys[index]);
        }
      }
      for (var index = 0; index < RequiredStageFlowKeys.Length; index++)
      {
        if (!entries.ContainsKey(RequiredStageFlowKeys[index]))
        {
          throw new FormatException(
            "Missing required stage-flow localization key: " + RequiredStageFlowKeys[index]);
        }
      }
      for (var index = 0; index < RequiredPresentation0124Keys.Length; index++)
      {
        if (!entries.ContainsKey(RequiredPresentation0124Keys[index]))
        {
          throw new FormatException(
            "Missing required 0.1.2.4 presentation localization key: "
              + RequiredPresentation0124Keys[index]);
        }
      }
      for (var index = 0; index < RequiredPresentation0125Keys.Length; index++)
      {
        if (!entries.ContainsKey(RequiredPresentation0125Keys[index]))
        {
          throw new FormatException(
            "Missing required 0.1.2.5 presentation localization key: "
              + RequiredPresentation0125Keys[index]);
        }
      }
      for (var index = 0; index < RequiredItemExpansion0125Keys.Length; index++)
      {
        if (!entries.ContainsKey(RequiredItemExpansion0125Keys[index]))
        {
          throw new FormatException(
            "Missing required 0.1.2.5 item localization key: "
              + RequiredItemExpansion0125Keys[index]);
        }
      }
      for (var index = 0; index < RequiredBarShopKeys.Length; index++)
      {
        if (!entries.ContainsKey(RequiredBarShopKeys[index]))
        {
          throw new FormatException(
            "Missing required bar-shop localization key: " + RequiredBarShopKeys[index]);
        }
      }
      for (var index = 0; index < RequiredJokerKeys.Length; index++)
      {
        if (!entries.ContainsKey(RequiredJokerKeys[index]))
        {
          throw new FormatException(
            "Missing required Joker localization key: " + RequiredJokerKeys[index]);
        }
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
