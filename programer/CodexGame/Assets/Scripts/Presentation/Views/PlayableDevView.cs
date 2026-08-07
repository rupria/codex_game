using System;
using System.Collections.Generic;
using CodexGame.Application.Playable;
using CodexGame.Core.Cards;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  public sealed class PlayableDevView : MonoBehaviour
  {
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

      if (Input.GetKeyDown(KeyCode.LeftArrow))
      {
        LeftBellRequested?.Invoke();
      }
      else if (Input.GetKeyDown(KeyCode.RightArrow))
      {
        RightBellRequested?.Invoke();
      }
      else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
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

      GUILayout.BeginArea(new Rect(20f, 15f, 920f, 570f));
      GUILayout.Label("CODEX HALLI - PLAYABLE DEV SLICE", _titleStyle);
      GUILayout.Label("Goal: reach 3 Halli wins. Same-suit skull 1+2 is valid; any skull 3 is a single-card exception.", _bodyStyle);
      GUILayout.Label("Controls: UP/SPACE = flip or continue, LEFT/RIGHT = ring that pile, R = restart.", _bodyStyle);
      GUILayout.Space(8f);

      if (_snapshot.Phase == PrototypeSessionPhase.Intro)
      {
        GUILayout.FlexibleSpace();
        GUILayout.Label("No art assets are bound in this build. Cards use code placeholders.", _statusStyle);

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
      DrawActions();
      GUILayout.EndArea();
    }

    private void DrawScoreboard()
    {
      var timer = _snapshot.RemainingMicroseconds > 0
        ? Math.Ceiling(_snapshot.RemainingMicroseconds / 1_000_000d).ToString("0") + "s"
        : "--";

      GUILayout.BeginHorizontal();
      GUILayout.Label($"PLAYER {_snapshot.PlayerWins} / {_snapshot.WinTarget}", _headingStyle);
      GUILayout.Label($"AI {_snapshot.AiWins} / {_snapshot.WinTarget}", _headingStyle);
      GUILayout.Label($"FLIPS {_snapshot.FlipCount}/25", _headingStyle);
      GUILayout.Label($"DECK {_snapshot.RemainingDeckCards}", _headingStyle);
      GUILayout.Label($"TIMER {timer}", _headingStyle);
      GUILayout.EndHorizontal();
    }

    private void DrawPublicCard()
    {
      GUILayout.BeginVertical(GUILayout.Width(210f));
      GUILayout.Label("PUBLIC CARD", _headingStyle);

      if (_snapshot.FirstPublicCard.HasValue)
      {
        GUILayout.Box(FormatCard(_snapshot.FirstPublicCard.Value), _cardStyle, GUILayout.Height(160f));
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
        var text = index < cards.Count ? FormatCard(cards[index]) : "EMPTY";
        GUILayout.Box(text, _cardStyle, GUILayout.Width(150f), GUILayout.Height(160f));
      }

      GUILayout.EndHorizontal();
      GUILayout.EndVertical();
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
          ? "CONTINUE  [UP / SPACE]"
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
