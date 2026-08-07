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
    private const string CatalogPath =
      "Assets/Art/Prototype/Cards_0_06/card_art_catalog_0_06.json";

    public static PlayableCardArtSet Load()
    {
      var catalogAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(CatalogPath);
      if (catalogAsset == null)
      {
        throw new InvalidOperationException("Playable card art catalog is missing: " + CatalogPath);
      }

      var catalog = JsonUtility.FromJson<CardArtCatalog>(catalogAsset.text);
      if (catalog == null)
      {
        throw new InvalidOperationException("Playable card art catalog could not be parsed: " + CatalogPath);
      }

      ValidateCatalog(catalog);
      var backTexture = LoadTexture(catalog.cardBackAssetPath, catalog.cardWidth, catalog.cardHeight);
      var halliEntries = LoadEntries(
        catalog.halliCards,
        true,
        PlayableCardArtLibrary.HalliExpectedTextureCount,
        catalog.cardWidth,
        catalog.cardHeight);
      var pokerEntries = LoadEntries(
        catalog.pokerCards,
        false,
        PlayableCardArtLibrary.PokerExpectedTextureCount,
        catalog.cardWidth,
        catalog.cardHeight);

      return new PlayableCardArtSet(
        new PlayableCardArtLibrary(
          halliEntries,
          PlayableCardArtLibrary.HalliExpectedTextureCount),
        new PlayableCardArtLibrary(
          pokerEntries,
          PlayableCardArtLibrary.PokerExpectedTextureCount),
        backTexture);
    }

    private static List<PlayableCardArtEntry> LoadEntries(
      CardArtCatalogEntry[] catalogEntries,
      bool matchSkullCount,
      int expectedCount,
      int expectedWidth,
      int expectedHeight)
    {
      if (catalogEntries == null || catalogEntries.Length != expectedCount)
      {
        var count = catalogEntries == null ? 0 : catalogEntries.Length;
        throw new InvalidOperationException(
          "Unexpected playable card art entry count. expected=" + expectedCount + ", actual=" + count);
      }

      var entries = new List<PlayableCardArtEntry>(expectedCount);
      var keys = new HashSet<string>(StringComparer.Ordinal);

      for (var index = 0; index < catalogEntries.Length; index++)
      {
        var source = catalogEntries[index];
        if (source == null)
        {
          throw new InvalidOperationException("Playable card art catalog contains a null entry.");
        }

        var suit = ParseSuit(source.suit);
        var rank = ParseRank(source.rank);
        var skullCount = matchSkullCount ? source.skullCount : 0;
        if (matchSkullCount && (skullCount < 1 || skullCount > 3))
        {
          throw new InvalidOperationException(
            "Invalid Halli skull count for " + source.assetId + ": " + skullCount);
        }

        var key = suit + ":" + rank + (matchSkullCount ? ":" + skullCount : string.Empty);
        if (!keys.Add(key))
        {
          throw new InvalidOperationException("Duplicate playable card art key: " + key);
        }

        var texture = LoadTexture(source.assetPath, expectedWidth, expectedHeight);
        entries.Add(new PlayableCardArtEntry(suit, rank, skullCount, matchSkullCount, texture));
      }

      return entries;
    }

    private static Texture2D LoadTexture(string path, int expectedWidth, int expectedHeight)
    {
      if (string.IsNullOrWhiteSpace(path))
      {
        throw new InvalidOperationException("Playable card art catalog contains an empty asset path.");
      }

      var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
      if (texture == null)
      {
        throw new InvalidOperationException("Playable card art is missing: " + path);
      }

      if (texture.width != expectedWidth || texture.height != expectedHeight)
      {
        throw new InvalidOperationException(
          "Unexpected playable card art dimensions: " + path
          + " expected=" + expectedWidth + "x" + expectedHeight
          + " actual=" + texture.width + "x" + texture.height);
      }

      return texture;
    }

    private static void ValidateCatalog(CardArtCatalog catalog)
    {
      if (!string.Equals(catalog.specRevision, "gameplay_flow_0.06", StringComparison.Ordinal))
      {
        throw new InvalidOperationException(
          "Unexpected playable card art spec revision: " + catalog.specRevision);
      }

      if (catalog.cardWidth != 64 || catalog.cardHeight != 90)
      {
        throw new InvalidOperationException(
          "Unexpected playable card dimensions: " + catalog.cardWidth + "x" + catalog.cardHeight);
      }
    }

    private static CardSuit ParseSuit(string value)
    {
      switch (value)
      {
        case "clubs": return CardSuit.Clubs;
        case "hearts": return CardSuit.Hearts;
        case "diamonds": return CardSuit.Diamonds;
        case "spades": return CardSuit.Spades;
        default: throw new InvalidOperationException("Unknown card suit in art catalog: " + value);
      }
    }

    private static CardRank ParseRank(string value)
    {
      switch (value)
      {
        case "A": return CardRank.Ace;
        case "K": return CardRank.King;
        case "Q": return CardRank.Queen;
        case "J": return CardRank.Jack;
      }

      if (int.TryParse(value, out var number)
        && number >= (int)CardRank.Two
        && number <= (int)CardRank.Ten)
      {
        return (CardRank)number;
      }

      throw new InvalidOperationException("Unknown card rank in art catalog: " + value);
    }

    [Serializable]
    private sealed class CardArtCatalog
    {
      public string specRevision;
      public int cardWidth;
      public int cardHeight;
      public string cardBackAssetPath;
      public CardArtCatalogEntry[] halliCards;
      public CardArtCatalogEntry[] pokerCards;
    }

    [Serializable]
    private sealed class CardArtCatalogEntry
    {
      public string assetId;
      public string suit;
      public string rank;
      public int skullCount;
      public string assetPath;
    }
  }
}
