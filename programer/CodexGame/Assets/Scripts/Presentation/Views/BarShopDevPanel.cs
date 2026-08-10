using System;
using System.Collections.Generic;
using CodexGame.Application.Shop;
using CodexGame.Core.Items;
using CodexGame.Presentation.Art;
using CodexGame.Presentation.Localization;
using CodexGame.Core.Shared;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class BarShopDevPanel
  {
    private static readonly Rect FullScreen = new Rect(0f, 0f, 960f, 540f);
    private static readonly Rect BulletPanel = new Rect(40f, 28f, 200f, 58f);
    private static readonly Rect HealthPanel = new Rect(720f, 28f, 200f, 58f);
    private static readonly Rect RerollButton = new Rect(38f, 462f, 190f, 48f);
    private static readonly Rect ContinueButton = new Rect(382f, 462f, 210f, 48f);
    private static readonly Rect AmmoPouch = new Rect(776f, 384f, 180f, 150f);
    private static readonly float[] SlotXPositions = { 20f, 250f, 480f, 710f };

    public void Draw(
      BarShopSnapshot shop,
      int bullets,
      int playerHealth,
      int maximumHealth,
      IReadOnlyList<GameItemId> inventory,
      PlayableDevStyles styles,
      BarShopUiArtSet art,
      LocalizationRuntime localization,
      Action reroll,
      Action<int> purchase,
      Action continueToNextStage,
      bool drawBackground = true)
    {
      if (shop == null) throw new ArgumentNullException(nameof(shop));
      if (localization == null) throw new ArgumentNullException(nameof(localization));

      if (drawBackground && art?.Background != null)
      {
        GUI.DrawTexture(FullScreen, art.Background, ScaleMode.StretchToFill, true);
      }
      else if (drawBackground)
      {
        DrawFallbackBackground();
      }

      DrawStatusPanel(
        BulletPanel,
        new Rect(98f, 45f, 126f, 24f),
        art?.BulletPanel,
        localization.Get("UI_BULLET_BALANCE", new LocalizationArgument("bullets", bullets)),
        styles.Small);
      DrawStatusPanel(
        HealthPanel,
        new Rect(850f, 45f, 58f, 24f),
        art?.HealthPanel,
        localization.Get(
          "UI_BAR_HP_STATUS",
          new LocalizationArgument("current", playerHealth),
          new LocalizationArgument("max", maximumHealth)),
        styles.Small);
      for (var index = 0; index < shop.Slots.Count; index++)
      {
        var slot = shop.Slots[index];
        if (index >= SlotXPositions.Length) break;
        var slotRect = new Rect(SlotXPositions[index], 146f, 190f, 174f);
        if (DrawSlot(
          slotRect,
          slot.IconKey,
          slot.LocalizationNameKey,
          slot.Price,
          !(shop.Purchase?.InputLocked ?? false),
          art,
          styles,
          localization))
        {
          purchase(index);
        }
      }

      var rerollLabel = localization.Get(
        shop.CanReroll ? "UI_BAR_REROLL_FREE" : "UI_BAR_REROLL_USED");
      if (DrawButton(
        RerollButton,
        rerollLabel,
        shop.CanReroll && !(shop.Purchase?.InputLocked ?? false),
        art?.RerollIdle,
        art?.RerollHover,
        art?.RerollPressed,
        art?.RerollDisabled,
        styles.Heading))
      {
        reroll();
      }

      if (DrawButton(
        ContinueButton,
        localization.Get("UI_BAR_CONTINUE"),
        !(shop.Purchase?.InputLocked ?? false),
        art?.ContinueIdle,
        art?.ContinueHover,
        art?.ContinuePressed,
        null,
        styles.Heading))
      {
        continueToNextStage();
      }

      DrawPurchaseMotion(shop.Purchase, art);
      DrawPurchaseFailure(shop.Purchase, styles, localization);
    }

    private static bool DrawSlot(
      Rect rect,
      string iconKey,
      string nameKey,
      int price,
      bool enabled,
      BarShopUiArtSet art,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      if (art?.SlotFrame != null)
      {
        GUI.DrawTexture(rect, art.SlotFrame, ScaleMode.StretchToFill, true);
      }
      else
      {
        var previous = GUI.color;
        GUI.color = new Color(0.19f, 0.11f, 0.055f, 0.96f);
        GUI.Box(rect, GUIContent.none);
        GUI.color = previous;
      }

      var iconRect = new Rect(rect.x + 67f, rect.y + 14f, 56f, 56f);
      var icon = art?.FindProductIcon(iconKey);
      if (icon != null)
      {
        GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
      }
      else
      {
        DrawFallbackIcon(iconRect, iconKey);
      }

      GUI.Label(
        new Rect(rect.x + 12f, rect.y + 84f, 166f, 20f),
        localization.Get(nameKey),
        styles.Small);
      GUI.Label(
        new Rect(rect.x + 24f, rect.y + 108f, 142f, 18f),
        localization.Get("UI_BAR_PRICE", new LocalizationArgument("price", price)),
        styles.Small);
      GUI.enabled = enabled;
      var clicked = GUI.Button(
        new Rect(rect.x + 24f, rect.y + 138f, 142f, 28f),
        localization.Get("UI_BAR_PURCHASE"));
      GUI.enabled = true;
      return clicked;
    }

    private static void DrawPurchaseMotion(
      BarShopPurchaseSnapshot purchase,
      BarShopUiArtSet art)
    {
      var pouchRect = AmmoPouch;
      if (purchase != null && purchase.InputLocked)
      {
        if (purchase.Phase == BarShopPurchasePhase.Rejected)
        {
          var shake = Mathf.Sin(purchase.ElapsedMicroseconds / 120000f * Mathf.PI * 4f) * 8f;
          pouchRect.x += shake;
        }
        else if (purchase.ElapsedMicroseconds < 60000)
        {
          pouchRect.y += 6f;
        }
      }

      if (art?.AmmoPouch != null)
      {
        GUI.DrawTexture(pouchRect, art.AmmoPouch, ScaleMode.ScaleToFit, true);
      }
      else
      {
        var previous = GUI.color;
        GUI.color = new Color(0.24f, 0.12f, 0.035f, 0.94f);
        GUI.DrawTexture(pouchRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
        GUI.color = previous;
      }

      if (purchase == null
        || (purchase.Phase != BarShopPurchasePhase.Tossing
          && purchase.Phase != BarShopPurchasePhase.Completed)) return;

      var progress = Mathf.Clamp01(
        purchase.ElapsedMicroseconds / (float)GameRules.BarShopPurchaseContactMicroseconds);
      var start = new Vector2(868f, 468f);
      var control = new Vector2(735f, 305f);
      var end = new Vector2(494f, 182f);
      var inverse = 1f - progress;
      var point = (inverse * inverse * start)
        + (2f * inverse * progress * control)
        + (progress * progress * end);
      var scale = progress < 0.5f
        ? Mathf.Lerp(1f, 1.15f, progress * 2f)
        : Mathf.Lerp(1.15f, 0.8f, (progress - 0.5f) * 2f);
      var size = new Vector2(24f, 40f) * scale;
      var bulletRect = new Rect(point.x - size.x * 0.5f, point.y - size.y * 0.5f, size.x, size.y);
      var previousMatrix = GUI.matrix;
      GUIUtility.RotateAroundPivot(progress * 540f, point);
      if (art?.BulletTossSheet != null)
      {
        const int frameCount = 6;
        var frameIndex = Mathf.Min(frameCount - 1, Mathf.FloorToInt(progress * frameCount));
        GUI.DrawTextureWithTexCoords(
          bulletRect,
          art.BulletTossSheet,
          new Rect(frameIndex / (float)frameCount, 0f, 1f / frameCount, 1f),
          true);
      }
      else
      {
        var frame = FindBulletFrame(art, progress);
        if (frame != null) GUI.DrawTexture(bulletRect, frame, ScaleMode.ScaleToFit, true);
        else
        {
          var previous = GUI.color;
          GUI.color = new Color(0.78f, 0.52f, 0.16f, 1f);
          GUI.DrawTexture(bulletRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
          GUI.color = previous;
        }
      }
      GUI.matrix = previousMatrix;

      if (purchase.ElapsedMicroseconds >= GameRules.BarShopPurchaseContactMicroseconds
        && purchase.ElapsedMicroseconds
          <= GameRules.BarShopPurchaseContactMicroseconds + 80000)
      {
        var sparkRect = new Rect(end.x - 30f, end.y - 30f, 60f, 60f);
        if (art?.BrassSpark != null)
        {
          GUI.DrawTexture(sparkRect, art.BrassSpark, ScaleMode.ScaleToFit, true);
        }
        else
        {
          var previous = GUI.color;
          GUI.color = new Color(1f, 0.74f, 0.22f, 0.8f);
          GUI.DrawTexture(sparkRect, Texture2D.whiteTexture, ScaleMode.ScaleToFit, true);
          GUI.color = previous;
        }
      }
    }

    private static Texture2D FindBulletFrame(BarShopUiArtSet art, float progress)
    {
      if (art?.BulletTossFrames == null || art.BulletTossFrames.Count == 0) return null;
      var index = Mathf.Min(
        art.BulletTossFrames.Count - 1,
        Mathf.FloorToInt(progress * art.BulletTossFrames.Count));
      return art.BulletTossFrames[index];
    }

    private static void DrawPurchaseFailure(
      BarShopPurchaseSnapshot purchase,
      PlayableDevStyles styles,
      LocalizationRuntime localization)
    {
      if (purchase == null || purchase.Phase != BarShopPurchasePhase.Rejected) return;
      var key = purchase.Failure == BarShopPurchaseFailure.InsufficientBullets
        ? "UI_BAR_NOT_ENOUGH"
        : purchase.Failure == BarShopPurchaseFailure.DuplicateItem
          ? "UI_BAR_DUPLICATE_ITEM"
          : purchase.Failure == BarShopPurchaseFailure.InventoryFull
            ? "UI_BAR_INVENTORY_FULL"
            : "UI_BAR_PURCHASE_BLOCKED";
      GUI.Label(new Rect(250f, 420f, 460f, 28f), localization.Get(key), styles.Heading);
    }

    private static void DrawStatusPanel(
      Rect panelRect,
      Rect labelRect,
      Texture2D texture,
      string label,
      GUIStyle style)
    {
      if (texture != null) GUI.DrawTexture(panelRect, texture, ScaleMode.StretchToFill, true);
      else GUI.Box(panelRect, GUIContent.none);
      GUI.Label(labelRect, label, style);
    }

    private static void DrawFallbackBackground()
    {
      var previous = GUI.color;
      GUI.color = new Color(0.055f, 0.028f, 0.012f, 0.98f);
      GUI.DrawTexture(FullScreen, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
      GUI.color = previous;
    }

    private static void DrawFallbackIcon(Rect rect, string iconKey)
    {
      var checksum = 0;
      for (var index = 0; index < iconKey.Length; index++) checksum += iconKey[index];
      var previous = GUI.color;
      GUI.color = new Color(
        0.42f + (checksum % 3) * 0.08f,
        0.24f + (checksum % 5) * 0.035f,
        0.08f,
        1f);
      GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
      GUI.color = previous;
    }

    private static bool DrawButton(
      Rect rect,
      string label,
      bool enabled,
      Texture2D idle,
      Texture2D hover,
      Texture2D pressed,
      Texture2D disabled,
      GUIStyle labelStyle)
    {
      var hovered = rect.Contains(Event.current.mousePosition);
      var selected = !enabled
        ? disabled
        : hovered && Input.GetMouseButton(0)
          ? pressed
          : hovered
            ? hover
            : idle;

      GUI.enabled = enabled;
      bool clicked;
      if (selected != null)
      {
        GUI.DrawTexture(rect, selected, ScaleMode.StretchToFill, true);
        GUI.Label(rect, label, labelStyle);
        clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);
      }
      else
      {
        clicked = GUI.Button(rect, label);
      }
      GUI.enabled = true;
      return clicked;
    }
  }
}
