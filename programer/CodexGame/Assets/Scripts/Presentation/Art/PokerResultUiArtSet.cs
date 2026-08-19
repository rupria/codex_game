using System;
using CodexGame.Presentation.Views;
using UnityEngine;

namespace CodexGame.Presentation.Art
{
  public enum PokerResultPanelVisualState
  {
    Neutral,
    Success,
    Failure
  }

  [Serializable]
  public sealed class PokerResultUiArtSet
  {
    [SerializeField] private Texture2D _successCompact;
    [SerializeField] private Texture2D _successStandard;
    [SerializeField] private Texture2D _successExpanded;
    [SerializeField] private Texture2D _failureCompact;
    [SerializeField] private Texture2D _failureStandard;
    [SerializeField] private Texture2D _failureExpanded;
    [SerializeField] private Texture2D _neutralCompact;
    [SerializeField] private Texture2D _neutralStandard;
    [SerializeField] private Texture2D _neutralExpanded;
    [SerializeField] private Texture2D _itemStatusChip;

    public PokerResultUiArtSet()
    {
    }

    public PokerResultUiArtSet(
      Texture2D successCompact,
      Texture2D successStandard,
      Texture2D successExpanded,
      Texture2D failureCompact,
      Texture2D failureStandard,
      Texture2D failureExpanded,
      Texture2D neutralCompact,
      Texture2D neutralStandard,
      Texture2D neutralExpanded,
      Texture2D itemStatusChip)
    {
      _successCompact = RequireTexture(successCompact, nameof(successCompact));
      _successStandard = RequireTexture(successStandard, nameof(successStandard));
      _successExpanded = RequireTexture(successExpanded, nameof(successExpanded));
      _failureCompact = RequireTexture(failureCompact, nameof(failureCompact));
      _failureStandard = RequireTexture(failureStandard, nameof(failureStandard));
      _failureExpanded = RequireTexture(failureExpanded, nameof(failureExpanded));
      _neutralCompact = RequireTexture(neutralCompact, nameof(neutralCompact));
      _neutralStandard = RequireTexture(neutralStandard, nameof(neutralStandard));
      _neutralExpanded = RequireTexture(neutralExpanded, nameof(neutralExpanded));
      _itemStatusChip = RequireTexture(itemStatusChip, nameof(itemStatusChip));
    }

    public Texture2D ItemStatusChip => _itemStatusChip;

    internal Texture2D FindPanel(
      PokerResultPanelVisualState state,
      PokerResultPanelSize size)
    {
      switch (state)
      {
        case PokerResultPanelVisualState.Success:
          return size == PokerResultPanelSize.Compact
            ? _successCompact
            : size == PokerResultPanelSize.Standard
              ? _successStandard
              : _successExpanded;
        case PokerResultPanelVisualState.Failure:
          return size == PokerResultPanelSize.Compact
            ? _failureCompact
            : size == PokerResultPanelSize.Standard
              ? _failureStandard
              : _failureExpanded;
        default:
          return size == PokerResultPanelSize.Compact
            ? _neutralCompact
            : size == PokerResultPanelSize.Standard
              ? _neutralStandard
              : _neutralExpanded;
      }
    }

    private static Texture2D RequireTexture(Texture2D texture, string parameterName)
    {
      return texture != null
        ? texture
        : throw new ArgumentNullException(parameterName);
    }
  }
}
