using System;
using CodexGame.Application.Shop;
using CodexGame.Presentation.Art;
using CodexGame.Presentation.Localization;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class BarShopDevPanel
  {
    private static readonly Rect FullScreen = new Rect(0f, 0f, 960f, 540f);
    private static readonly Rect RerollButton = new Rect(250f, 452f, 210f, 54f);
    private static readonly Rect ContinueButton = new Rect(500f, 452f, 210f, 54f);

    public void Draw(
      BarShopSnapshot shop,
      int bullets,
      int playerHealth,
      int maximumHealth,
      PlayableDevStyles styles,
      BarShopUiArtSet art,
      LocalizationRuntime localization,
      Action reroll,
      Action continueToNextStage)
    {
      if (shop == null) throw new ArgumentNullException(nameof(shop));
      if (localization == null) throw new ArgumentNullException(nameof(localization));

      if (art?.Background != null)
      {
        GUI.DrawTexture(FullScreen, art.Background, ScaleMode.StretchToFill, true);
      }
      else
      {
        DrawFallbackBackground();
      }

      GUI.Label(new Rect(300f, 28f, 360f, 54f), localization.Get("UI_BAR_TITLE"), styles.Title);
      DrawStatusPanel(
        new Rect(74f, 82f, 236f, 58f),
        art?.BulletPanel,
        localization.Get("UI_BULLET_BALANCE", new LocalizationArgument("bullets", bullets)),
        styles.Heading);
      DrawStatusPanel(
        new Rect(650f, 82f, 236f, 58f),
        art?.HealthPanel,
        localization.Get(
          "UI_BAR_HP_STATUS",
          new LocalizationArgument("current", playerHealth),
          new LocalizationArgument("max", maximumHealth)),
        styles.Heading);

      for (var index = 0; index < shop.Slots.Count; index++)
      {
        var slot = shop.Slots[index];
        var slotRect = new Rect(78f + index * 278f, 158f, 248f, 262f);
        DrawSlot(slotRect, slot.IconKey, slot.LocalizationNameKey, art, styles, localization);
      }

      var rerollLabel = localization.Get(
        shop.CanReroll ? "UI_BAR_REROLL_FREE" : "UI_BAR_REROLL_USED");
      if (DrawButton(
        RerollButton,
        rerollLabel,
        shop.CanReroll,
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
        true,
        art?.ContinueIdle,
        art?.ContinueHover,
        art?.ContinuePressed,
        null,
        styles.Heading))
      {
        continueToNextStage();
      }
    }

    private static void DrawSlot(
      Rect rect,
      string iconKey,
      string nameKey,
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

      var iconRect = new Rect(rect.x + 66f, rect.y + 34f, 116f, 116f);
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
        new Rect(rect.x + 18f, rect.y + 160f, rect.width - 36f, 42f),
        localization.Get(nameKey),
        styles.Heading);
      GUI.enabled = false;
      GUI.Button(
        new Rect(rect.x + 48f, rect.y + 212f, rect.width - 96f, 34f),
        localization.Get("UI_BAR_PURCHASE"));
      GUI.enabled = true;
    }

    private static void DrawStatusPanel(Rect rect, Texture2D texture, string label, GUIStyle style)
    {
      if (texture != null) GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
      else GUI.Box(rect, GUIContent.none);
      GUI.Label(rect, label, style);
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
