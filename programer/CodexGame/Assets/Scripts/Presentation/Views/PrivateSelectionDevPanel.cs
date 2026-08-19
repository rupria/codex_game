using System;
using System.Collections.Generic;
using CodexGame.Application.Distribution;
using CodexGame.Core.Cards;
using CodexGame.Presentation.Art;
using CodexGame.Presentation.Localization;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class PrivateSelectionDevPanel
  {
    private static readonly Rect PanelRect = new Rect(50f, 42f, 860f, 456f);
    private static readonly Rect TitleRect = new Rect(68f, 58f, 824f, 28f);
    private static readonly Rect GuideRect = new Rect(68f, 86f, 824f, 24f);
    private static readonly Rect PublicRect = new Rect(68f, 124f, 190f, 226f);
    private static readonly Rect StatusRect = new Rect(68f, 362f, 190f, 42f);
    private static readonly Rect ConfirmRect = new Rect(73f, 418f, 180f, 52f);
    private const float GridX = 270f;
    private const float GridY = 124f;
    private const float CellWidth = 112f;
    private const float CellHeight = 150f;
    private const float GapX = 20f;
    private const float GapY = 10f;
    private readonly PrivateSelectionJokerRevealState _jokerReveal =
      new PrivateSelectionJokerRevealState();

    public void Observe(
      long selectionSessionSerial,
      PrivateCardSelectionSnapshot snapshot,
      double nowSeconds)
    {
      _jokerReveal.Observe(
        CreateSessionKey(selectionSessionSerial, snapshot),
        FindJokerIndex(snapshot.WinnerCandidates) >= 0,
        nowSeconds);
    }

    public bool IsInputLocked(double nowSeconds)
    {
      return _jokerReveal.IsInputLocked(nowSeconds);
    }

    public void Draw(
      long selectionSessionSerial,
      PrivateCardSelectionSnapshot snapshot,
      int focusedIndex,
      PlayableDevStyles styles,
      PlayableCardRenderer cards,
      PrivateSelectionUiArtSet selectionArt,
      JokerRevealUiArtSet jokerArt,
      LocalizationRuntime localization,
      Action<int> focus,
      Action<CardId> toggle,
      Action confirm)
    {
      var now = Time.unscaledTime;
      Observe(selectionSessionSerial, snapshot, now);
      var inputLocked = IsInputLocked(now);
      var jokerIndex = FindJokerIndex(snapshot.WinnerCandidates);
      var seconds = Math.Ceiling(snapshot.RemainingMicroseconds / 1_000_000d);

      DrawTexture(new Rect(0f, 0f, 960f, 540f), selectionArt?.ModalDim);
      DrawTexture(PanelRect, selectionArt?.ModalPanel);
      if (_jokerReveal.IsActive(now))
      {
        DrawTexture(new Rect(0f, 0f, 960f, 540f), jokerArt?.FocusVignette);
      }

      GUI.Label(TitleRect, localization.Get("UI_PRIVATE_SELECTION_TITLE"), styles.Heading);
      GUI.Label(
        GuideRect,
        localization.Get(
          "UI_PRIVATE_SELECTION_GUIDE",
          new LocalizationArgument("required", snapshot.RequiredSelectionCount),
          new LocalizationArgument("seconds", seconds.ToString("0"))),
        styles.Small);
      DrawPublicCards(snapshot, cards, selectionArt, styles, localization);

      GUI.Label(
        StatusRect,
        localization.Get(
          "UI_PRIVATE_CONFIRM",
          new LocalizationArgument("selected", snapshot.SelectedCards.Count),
          new LocalizationArgument("required", snapshot.RequiredSelectionCount)),
        styles.Small);

      for (var index = 0; index < snapshot.WinnerCandidates.Count && index < 8; index++)
      {
        DrawCandidate(
          snapshot,
          index,
          jokerIndex,
          focusedIndex,
          inputLocked,
          now,
          cards,
          selectionArt,
          jokerArt,
          focus,
          toggle);
      }

      DrawConfirm(snapshot, inputLocked, selectionArt, styles, localization, confirm);
    }

    private void DrawCandidate(
      PrivateCardSelectionSnapshot snapshot,
      int index,
      int jokerIndex,
      int focusedIndex,
      bool inputLocked,
      double now,
      PlayableCardRenderer cards,
      PrivateSelectionUiArtSet selectionArt,
      JokerRevealUiArtSet jokerArt,
      Action<int> focus,
      Action<CardId> toggle)
    {
      var cell = CandidateCell(index);
      var cardRect = new Rect(cell.x + 14f, cell.y + 13f, 84f, 117f);
      var card = snapshot.WinnerCandidates[index];
      var selected = Contains(snapshot.SelectedCards, card.Id);
      var hovered = cell.Contains(Event.current.mousePosition);
      var disabled = inputLocked || snapshot.Phase != PrivateCardSelectionPhase.AwaitingSelection;
      var frame = disabled
        ? selectionArt?.CandidateDisabled
        : snapshot.Phase == PrivateCardSelectionPhase.Completed && selected
          ? selectionArt?.CandidateConfirmed
          : selected
            ? selectionArt?.CandidateSelected
            : hovered || index == focusedIndex
              ? selectionArt?.CandidateHover
              : selectionArt?.CandidateIdle;
      DrawTexture(cell, frame);

      var revealThisCard = index == jokerIndex && _jokerReveal.IsActive(now);
      if (revealThisCard)
      {
        DrawJokerReveal(card, cardRect, now, cards, jokerArt);
      }
      else
      {
        cards.DrawAt(cardRect, card, false, false);
      }

      if (!disabled && hovered && index != focusedIndex)
      {
        focus(index);
      }
      if (!disabled && GUI.Button(cell, GUIContent.none, GUIStyle.none))
      {
        focus(index);
        toggle(card.Id);
      }
    }

    private void DrawJokerReveal(
      Card joker,
      Rect cardRect,
      double now,
      PlayableCardRenderer cards,
      JokerRevealUiArtSet art)
    {
      var elapsed = _jokerReveal.ElapsedSeconds(now);
      var step = _jokerReveal.Step(now);
      if (step == PrivateSelectionJokerRevealStep.Focus)
      {
        cards.DrawBackAt(cardRect, 0f, false);
        return;
      }

      if (step == PrivateSelectionJokerRevealStep.Flip)
      {
        var progress = Mathf.Clamp01((float)((elapsed - 0.15d) / 0.20d));
        CardFlipMotion.Draw(cards, joker, cardRect, cardRect, progress, false, false);
        DrawSheetFrame(
          art?.ArcTrailSheet,
          6,
          progress,
          Centered(cardRect.center, 96f, 96f));
        return;
      }

      cards.DrawAt(cardRect, joker, false, false);
      if (step == PrivateSelectionJokerRevealStep.Accent)
      {
        var progress = Mathf.Clamp01((float)((elapsed - 0.35d) / 0.50d));
        DrawSheetFrame(
          art?.GunsightRingSheet,
          8,
          progress,
          Centered(cardRect.center, 144f, 144f));
        DrawSheetFrame(
          art?.MuzzleFlashSheet,
          8,
          progress,
          Centered(cardRect.center, 120f, 120f));
        DrawSheetFrame(
          art?.CardGlintSheet,
          6,
          progress,
          Centered(cardRect.center, 112f, 156f));
      }
      else if (step == PrivateSelectionJokerRevealStep.Settle)
      {
        var progress = Mathf.Clamp01((float)((elapsed - 0.85d) / 0.15d));
        DrawSheetFrame(
          art?.SettleGlintSheet,
          5,
          progress,
          Centered(cardRect.center, 64f, 64f));
      }
    }

    private static void DrawPublicCards(
      PrivateCardSelectionSnapshot snapshot,
      PlayableCardRenderer cards,
      PrivateSelectionUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      DrawTexture(new Rect(PublicRect.x + 12f, PublicRect.y + 12f, 166f, 198f), art?.PublicFrame);
      GUI.Label(
        new Rect(PublicRect.x + 18f, PublicRect.y + 16f, 154f, 22f),
        localization.Get("UI_POKER_PUBLIC"),
        styles.Small);
      if (snapshot.FirstPublicCard.HasValue)
      {
        cards.DrawAt(
          new Rect(PublicRect.x + 18f, PublicRect.y + 52f, 68f, 94f),
          snapshot.FirstPublicCard.Value,
          false,
          false);
      }
      if (snapshot.SecondPublicCard.HasValue)
      {
        cards.DrawAt(
          new Rect(PublicRect.x + 104f, PublicRect.y + 52f, 68f, 94f),
          snapshot.SecondPublicCard.Value,
          false,
          false);
      }
    }

    private static void DrawConfirm(
      PrivateCardSelectionSnapshot snapshot,
      bool inputLocked,
      PrivateSelectionUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization,
      Action confirm)
    {
      var enabled = snapshot.CanConfirm && !inputLocked;
      var hovered = ConfirmRect.Contains(Event.current.mousePosition);
      DrawTexture(
        ConfirmRect,
        enabled
          ? hovered ? art?.ConfirmActive : art?.ConfirmIdle
          : art?.ConfirmDisabled);
      GUI.Label(
        ConfirmRect,
        localization.Get(
          "UI_PRIVATE_CONFIRM",
          new LocalizationArgument("selected", snapshot.SelectedCards.Count),
          new LocalizationArgument("required", snapshot.RequiredSelectionCount)),
        styles.Small);
      if (enabled && GUI.Button(ConfirmRect, GUIContent.none, GUIStyle.none))
      {
        confirm();
      }
    }

    private static Rect CandidateCell(int index)
    {
      var column = index % 4;
      var row = index / 4;
      return new Rect(
        GridX + column * (CellWidth + GapX),
        GridY + row * (CellHeight + GapY),
        CellWidth,
        CellHeight);
    }

    private static long CreateSessionKey(
      long selectionSessionSerial,
      PrivateCardSelectionSnapshot snapshot)
    {
      unchecked
      {
        long hash = 1469598103934665603L;
        hash = (hash ^ selectionSessionSerial) * 1099511628211L;
        hash = (hash ^ snapshot.CombatRoundNumber) * 1099511628211L;
        hash = (hash ^ (int)snapshot.Winner) * 1099511628211L;
        hash = (hash ^ snapshot.RequiredSelectionCount) * 1099511628211L;
        for (var index = 0; index < snapshot.WinnerCandidates.Count; index++)
        {
          hash = (hash ^ snapshot.WinnerCandidates[index].Id.Value) * 1099511628211L;
        }
        return hash;
      }
    }

    private static int FindJokerIndex(IReadOnlyList<Card> cards)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        if (cards[index].IsJoker) return index;
      }
      return -1;
    }

    private static bool Contains(IReadOnlyList<Card> cards, CardId id)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        if (cards[index].Id == id) return true;
      }
      return false;
    }

    private static Rect Centered(Vector2 center, float width, float height)
    {
      return new Rect(center.x - width * 0.5f, center.y - height * 0.5f, width, height);
    }

    private static void DrawTexture(Rect rect, Texture2D texture)
    {
      if (texture != null)
      {
        GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
      }
    }

    private static void DrawSheetFrame(
      Texture2D sheet,
      int frameCount,
      float progress,
      Rect rect)
    {
      if (sheet == null || frameCount <= 0) return;
      var frame = Mathf.Clamp(
        Mathf.FloorToInt(Mathf.Clamp01(progress) * frameCount),
        0,
        frameCount - 1);
      GUI.DrawTextureWithTexCoords(
        rect,
        sheet,
        new Rect((float)frame / frameCount, 0f, 1f / frameCount, 1f),
        true);
    }
  }
}
