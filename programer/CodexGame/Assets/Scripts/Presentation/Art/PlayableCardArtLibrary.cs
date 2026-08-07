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
    private bool _matchSkullCount;

    [SerializeField]
    private Texture2D _texture;

    public PlayableCardArtEntry(
      CardSuit suit,
      CardRank rank,
      int skullCount,
      bool matchSkullCount,
      Texture2D texture)
    {
      _suit = suit;
      _rank = rank;
      _skullCount = skullCount;
      _matchSkullCount = matchSkullCount;
      _texture = texture;
    }

    public bool Matches(Card card)
    {
      return _suit == card.Suit
        && _rank == card.Rank
        && (!_matchSkullCount || _skullCount == card.SkullCount);
    }

    public Texture2D Texture => _texture;
  }

  [Serializable]
  public sealed class PlayableCardArtLibrary
  {
    public const int HalliExpectedTextureCount = 156;
    public const int PokerExpectedTextureCount = 52;

    [SerializeField]
    private List<PlayableCardArtEntry> _entries = new List<PlayableCardArtEntry>();

    [SerializeField]
    private int _expectedTextureCount;

    public PlayableCardArtLibrary(
      IReadOnlyList<PlayableCardArtEntry> entries,
      int expectedTextureCount)
    {
      if (entries == null)
      {
        throw new ArgumentNullException(nameof(entries));
      }

      if (expectedTextureCount <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(expectedTextureCount));
      }

      _entries = new List<PlayableCardArtEntry>(entries);
      _expectedTextureCount = expectedTextureCount;
    }

    public int Count => _entries?.Count ?? 0;

    public bool IsComplete => Count == _expectedTextureCount;

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

  [Serializable]
  public sealed class PlayableCardArtSet
  {
    [SerializeField]
    private PlayableCardArtLibrary _halli;

    [SerializeField]
    private PlayableCardArtLibrary _poker;

    [SerializeField]
    private Texture2D _backTexture;

    public PlayableCardArtSet(
      PlayableCardArtLibrary halli,
      PlayableCardArtLibrary poker,
      Texture2D backTexture)
    {
      _halli = halli ?? throw new ArgumentNullException(nameof(halli));
      _poker = poker ?? throw new ArgumentNullException(nameof(poker));
      _backTexture = backTexture ?? throw new ArgumentNullException(nameof(backTexture));
    }

    public PlayableCardArtLibrary Halli => _halli;

    public PlayableCardArtLibrary Poker => _poker;

    public Texture2D BackTexture => _backTexture;

    public bool IsComplete => _halli != null
      && _halli.IsComplete
      && _poker != null
      && _poker.IsComplete
      && _backTexture != null;
  }
}
