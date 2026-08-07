using System;
using System.Collections.Generic;
using CodexGame.Application.Playable;
using CodexGame.Core.Cards;
using CodexGame.Presentation.Art;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  public sealed class PlayableDevView : MonoBehaviour
  {
    [SerializeField]
    private Texture2D _boardTexture;

    [SerializeField]
    private PlayableCardArtLibrary _cardArt;

    private PrototypeHalliSnapshot _snapshot;
    private GUIStyle _titleStyle;
    private GUIStyle _headingStyle;
    private GUIStyle _bodyStyle;
    private GUIStyle _cardStyle;
    private GUIStyle _statusStyle;

    public event Action StartRequested;
    public event Action AdvanceRequested;
    public event Action LeftBellRequested;
    public event Action RightBellRequested;
    public event Action RestartRequested;

    public void Configure(Texture2D boardTexture, PlayableCardArtLibrary cardArt)
    {
      _boardTexture = boardTexture;
      _cardArt = cardArt;
    }

    public void Present(PrototypeHalliSnapshot snapshot)
    {
      _snapshot = snapshot;
    }

    private void Update()
    {
      if (_snapshot == null)
      {
        return;
      }

      if (_snapshot.Phase == PrototypeSessionPhase.Intro)
      {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
          StartRequested?.Invoke();
        }

        return;
      }

      if (_snapshot.Phase == PrototypeSessionPhase.Finished)
      {
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return))
        {
          RestartRequested?.Invoke();
        }

        return;
      }

      if (_snapshot.Phase == PrototypeSessionPhase.BellOpen
        && Input.GetKeyDown(KeyCode.LeftArrow))
      {
        LeftBellRequested?.Invoke();
      }
      else if (_snapshot.Phase == PrototypeSessionPhase.BellOpen
        && Input.GetKeyDown(KeyCode.RightArrow))
      {
        RightBellRequested?.Invoke();
      }
      else if (Input.GetKeyDown(KeyCode.UpArrow)
        || Input.GetKeyDown(KeyCode.Space)
        || (_snapshot.Phase == PrototypeSessionPhase.Review && Input.GetKeyDown(KeyCode.W)))
      {
        AdvanceRequested?.Invoke();
      }
    }

    private void OnGUI()
    {
      if (_snapshot == null)
      {
        return;
      }

      EnsureStyles();
      var scale = Mathf.Min(Screen.width / 960f, Screen.height / 600f);
      GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

      if (_boardTexture != null)
      {
        GUI.DrawTexture(
          new Rect(0f, 0f, 960f, 540f),
          _boardTexture,
          ScaleMode.StretchToFill,
          true);
      }

      GUILayout.BeginArea(new Rect(20f, 15f, 920f, 570f));
      GUILayout.Label("CODEX HALLI - PLAYABLE DEV SLICE", _titleStyle);
      GUILayout.Label("Goal: Halli targets by combat round are 3 / 2 / 1 wins. Same-suit skull 1+2 is valid; any skull 3 is a single-card exception.", _bodyStyle);
      GUILayout.Label("Controls: UP/SPACE = flip, W/UP/SPACE = continue review, LEFT/RIGHT = ring that pile, R = restart.", _bodyStyle);
      GUILayout.Space(8f);

      if (_snapshot.Phase == PrototypeSessionPhase.Intro)
      {
        GUILayout.FlexibleSpace();
        var artStatus = _boardTexture == null
          ? "Board art is missing."
          : _cardArt != null && _cardArt.IsComplete
            ? "Prototype board art and all 156 card images loaded."
            : "Prototype board art loaded. Missing cards use text fallback.";
        GUILayout.Label(artStatus, _statusStyle);

        if (GUILayout.Button("START  [ENTER / SPACE]", GUILayout.Height(64f)))
        {
          StartRequested?.Invoke();
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
        return;
      }

      DrawScoreboard();
      GUILayout.Space(6f);
      GUILayout.BeginHorizontal();
      DrawPublicCard();
      DrawPile("LEFT PILE", _snapshot.LeftPile);
      DrawPile("RIGHT PILE", _snapshot.RightPile);
      GUILayout.EndHorizontal();
      GUILayout.Space(10f);
      GUILayout.Label(_snapshot.StatusMessage, _statusStyle, GUILayout.Height(54f));
      DrawAcquisitionReview();
      DrawActions();
      GUILayout.EndArea();
    }

    private void DrawScoreboard()
    {
      var timer = _snapshot.RemainingMicroseconds > 0
        ? Math.Ceiling(_snapshot.RemainingMicroseconds / 1_000_000d).ToString("0") + "s"
        : "--";

      GUILayout.BeginHorizontal();
      GUILayout.Label($"ROUND {_snapshot.CombatRoundNumber}", _headingStyle);
      GUILayout.Label($"PLAYER {_snapshot.PlayerWins} / {_snapshot.WinTarget}", _headingStyle);
      GUILayout.Label($"AI {_snapshot.AiWins} / {_snapshot.WinTarget}", _headingStyle);
      GUILayout.Label($"FLIPS {_snapshot.FlipCount}/25", _headingStyle);
      GUILayout.Label($"DECK {_snapshot.RemainingDeckCards}", _headingStyle);
      GUILayout.Label($"TIMER {timer}", _headingStyle);
      GUILayout.EndHorizontal();
    }

    private void DrawAcquisitionReview()
    {
      if (_snapshot.LastAcquirer == PrototypeAcquirer.None
        || _snapshot.LastAcquiredCards.Count == 0)
      {
        return;
      }

      var owner = _snapshot.LastAcquirer == PrototypeAcquirer.Player ? "PLAYER" : "AI";
      var cards = string.Empty;

      for (var index = 0; index < _snapshot.LastAcquiredCards.Count; index++)
      {
        if (index > 0)
        {
          cards += " + ";
        }

        cards += FormatCardInline(_snapshot.LastAcquiredCards[index]);
      }

      GUILayout.Label($"LAST ACQUIRED — {owner}: {cards}", _bodyStyle, GUILayout.Height(30f));
    }

    private void DrawPublicCard()
    {
      GUILayout.BeginVertical(GUILayout.Width(210f));
      GUILayout.Label("PUBLIC CARD", _headingStyle);

      if (_snapshot.FirstPublicCard.HasValue)
      {
        DrawCard(_snapshot.FirstPublicCard.Value, 190f, 160f);
      }

      GUILayout.EndVertical();
    }

    private void DrawPile(string label, IReadOnlyList<Card> cards)
    {
      GUILayout.BeginVertical(GUILayout.Width(330f));
      GUILayout.Label(label, _headingStyle);
      GUILayout.BeginHorizontal();

      for (var index = 0; index < 2; index++)
      {
        if (index < cards.Count)
        {
          DrawCard(cards[index], 150f, 160f);
        }
        else
        {
          GUILayout.Box("EMPTY", _cardStyle, GUILayout.Width(150f), GUILayout.Height(160f));
        }
      }

      GUILayout.EndHorizontal();
      GUILayout.EndVertical();
    }

    private void DrawCard(Card card, float width, float height)
    {
      var rect = GUILayoutUtility.GetRect(
        width,
        height,
        GUILayout.Width(width),
        GUILayout.Height(height));

      GUI.Box(rect, GUIContent.none, _cardStyle);
      if (_cardArt != null && _cardArt.TryGetTexture(card, out var texture))
      {
        var inset = new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, rect.height - 10f);
        GUI.DrawTexture(inset, texture, ScaleMode.ScaleToFit, true);
        return;
      }

      GUI.Label(rect, FormatCard(card), _cardStyle);
    }

    private void DrawActions()
    {
      GUILayout.BeginHorizontal();

      if (_snapshot.Phase == PrototypeSessionPhase.Finished)
      {
        if (GUILayout.Button("RESTART  [R / ENTER]", GUILayout.Height(54f)))
        {
          RestartRequested?.Invoke();
        }
      }
      else
      {
        GUI.enabled = _snapshot.Phase == PrototypeSessionPhase.BellOpen;

        if (GUILayout.Button("LEFT BELL  [LEFT]", GUILayout.Height(54f)))
        {
          LeftBellRequested?.Invoke();
        }

        if (GUILayout.Button("RIGHT BELL  [RIGHT]", GUILayout.Height(54f)))
        {
          RightBellRequested?.Invoke();
        }

        GUI.enabled = true;
        var advanceLabel = _snapshot.Phase == PrototypeSessionPhase.Review
          ? "CONTINUE  [W / UP / SPACE]"
          : "FLIP / SKIP BELL  [UP / SPACE]";

        if (GUILayout.Button(advanceLabel, GUILayout.Height(54f)))
        {
          AdvanceRequested?.Invoke();
        }
      }

      GUILayout.EndHorizontal();
    }

    private void EnsureStyles()
    {
      if (_titleStyle != null)
      {
        return;
      }

      _titleStyle = new GUIStyle(GUI.skin.label)
      {
        fontSize = 26,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter
      };
      _headingStyle = new GUIStyle(GUI.skin.label)
      {
        fontSize = 17,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter
      };
      _bodyStyle = new GUIStyle(GUI.skin.label)
      {
        fontSize = 15,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = true
      };
      _cardStyle = new GUIStyle(GUI.skin.box)
      {
        fontSize = 20,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = true
      };
      _statusStyle = new GUIStyle(GUI.skin.box)
      {
        fontSize = 17,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = true
      };
    }

    private static string FormatCard(Card card)
    {
      return $"{RankText(card.Rank)} {SuitText(card.Suit)}\nSKULL {card.SkullCount}";
    }

    private static string FormatCardInline(Card card)
    {
      return $"{RankText(card.Rank)} {SuitText(card.Suit)} / SKULL {card.SkullCount}";
    }

    private static string RankText(CardRank rank)
    {
      switch (rank)
      {
        case CardRank.Ace: return "A";
        case CardRank.King: return "K";
        case CardRank.Queen: return "Q";
        case CardRank.Jack: return "J";
        default: return ((int)rank).ToString();
      }
    }

    private static string SuitText(CardSuit suit)
    {
      switch (suit)
      {
        case CardSuit.Spades: return "SPADE";
        case CardSuit.Diamonds: return "DIAMOND";
        case CardSuit.Hearts: return "HEART";
        default: return "CLUB";
      }
    }
  }
}
