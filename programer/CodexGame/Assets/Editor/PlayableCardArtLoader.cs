using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using CodexGame.Presentation.Art;
using UnityEditor;
using UnityEngine;

namespace CodexGame.Editor
{
  internal static class PlayableCardArtLoader
  {
    private const string CardVariantRoot = "Assets/Art/Prototype/Cards/deck_variants/";

    private static readonly CardSuit[] Suits =
    {
      CardSuit.Clubs,
      CardSuit.Hearts,
      CardSuit.Diamonds,
      CardSuit.Spades
    };

    private static readonly CardRank[] Ranks =
    {
      CardRank.Two,
      CardRank.Three,
      CardRank.Four,
      CardRank.Five,
      CardRank.Six,
      CardRank.Seven,
      CardRank.Eight,
      CardRank.Nine,
      CardRank.Ten,
      CardRank.Jack,
      CardRank.Queen,
      CardRank.King,
      CardRank.Ace
    };

    public static PlayableCardArtLibrary Load()
    {
      var entries = new List<PlayableCardArtEntry>(PlayableCardArtLibrary.ExpectedTextureCount);

      foreach (var suit in Suits)
      {
        foreach (var rank in Ranks)
        {
          for (var skullCount = 1; skullCount <= 3; skullCount++)
          {
            var path = BuildAssetPath(suit, rank, skullCount);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null)
            {
              throw new InvalidOperationException("Playable card art is missing: " + path);
            }

            entries.Add(new PlayableCardArtEntry(suit, rank, skullCount, texture));
          }
        }
      }

      return new PlayableCardArtLibrary(entries);
    }

    private static string BuildAssetPath(CardSuit suit, CardRank rank, int skullCount)
    {
      return CardVariantRoot
        + "card_" + SuitName(suit)
        + "_" + RankName(rank)
        + "_skull_" + skullCount.ToString("00")
        + ".png";
    }

    private static string SuitName(CardSuit suit)
    {
      switch (suit)
      {
        case CardSuit.Clubs: return "clubs";
        case CardSuit.Hearts: return "hearts";
        case CardSuit.Diamonds: return "diamonds";
        case CardSuit.Spades: return "spades";
        default: throw new ArgumentOutOfRangeException(nameof(suit));
      }
    }

    private static string RankName(CardRank rank)
    {
      switch (rank)
      {
        case CardRank.Ace: return "a";
        case CardRank.King: return "k";
        case CardRank.Queen: return "q";
        case CardRank.Jack: return "j";
        default: return ((int)rank).ToString();
      }
    }
  }
}
