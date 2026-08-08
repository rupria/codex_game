using System;
using System.Collections.Generic;

namespace CodexGame.Application.Playable
{
  public sealed class LocalizedStatus
  {
    private static readonly IReadOnlyList<LocalizedStatusArgument> EmptyArguments =
      Array.AsReadOnly(Array.Empty<LocalizedStatusArgument>());

    public LocalizedStatus(string key, params LocalizedStatusArgument[] arguments)
    {
      if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("A status key is required.", nameof(key));
      Key = key;
      Arguments = arguments == null || arguments.Length == 0
        ? EmptyArguments
        : Array.AsReadOnly((LocalizedStatusArgument[])arguments.Clone());
    }

    public string Key { get; }
    public IReadOnlyList<LocalizedStatusArgument> Arguments { get; }

    public static LocalizedStatus Of(string key)
    {
      return new LocalizedStatus(key);
    }
  }

  public readonly struct LocalizedStatusArgument
  {
    public LocalizedStatusArgument(string name, string value, bool valueIsLocalizationKey = false)
    {
      if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("An argument name is required.", nameof(name));
      Name = name;
      Value = value ?? throw new ArgumentNullException(nameof(value));
      ValueIsLocalizationKey = valueIsLocalizationKey;
    }

    public string Name { get; }
    public string Value { get; }
    public bool ValueIsLocalizationKey { get; }
  }
}
