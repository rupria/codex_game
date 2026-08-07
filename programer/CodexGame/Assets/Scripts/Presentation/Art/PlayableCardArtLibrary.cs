using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using UnityEngine;

namespace CodexGame.Presentation.Art
{
  [Serializable]
  public sealed class PlayableCardArtEntry
  {
    [SerializeField]
    private CardSuit _suit;

    [SerializeField]
    private CardRank _rank;

    [SerializeField]
    private int _skullCount;

    [SerializeField]
    private Texture2D _texture;

    public PlayableCardArtEntry(
      CardSuit suit,
      CardRank rank,
      int skullCount,
      Texture2D texture)
    {
      _suit = suit;
      _rank = rank;
      _skullCount = skullCount;
      _texture = texture;
    }

    public bool Matches(Card card)
    {
      return _suit == card.Suit
        && _rank == card.Rank
        && _skullCount == card.SkullCount;
    }

    public Texture2D Texture => _texture;
  }

  [Serializable]
  public sealed class PlayableCardArtLibrary
  {
    public const int ExpectedTextureCount = 156;

    [SerializeField]
    private List<PlayableCardArtEntry> _entries = new List<PlayableCardArtEntry>();

    public PlayableCardArtLibrary(IReadOnlyList<PlayableCardArtEntry> entries)
    {
      if (entries == null)
      {
        throw new ArgumentNullException(nameof(entries));
      }

      _entries = new List<PlayableCardArtEntry>(entries);
    }

    public int Count => _entries?.Count ?? 0;

    public bool IsComplete => Count == ExpectedTextureCount;

    public bool TryGetTexture(Card card, out Texture2D texture)
    {
      if (_entries != null)
      {
        for (var index = 0; index < _entries.Count; index++)
        {
          var entry = _entries[index];
          if (entry != null && entry.Matches(card) && entry.Texture != null)
          {
            texture = entry.Texture;
            return true;
          }
        }
      }

      texture = null;
      return false;
    }
  }
}
