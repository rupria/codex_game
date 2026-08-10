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

      DrawPurchaseMotion(shop.Purchase, bullets, art);
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
      int currentBulletCount,
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

      var displayedBulletCount = currentBulletCount;
      if (purchase != null
        && purchase.InputLocked
        && purchase.Phase != BarShopPurchasePhase.Rejected)
      {
        displayedBulletCount = purchase.ElapsedMicroseconds < GameRules.BarShopPouchCoverMicroseconds
          ? purchase.BulletCountBefore
          : purchase.BulletCountAfter;
      }

      DrawAmmoPouch(pouchRect, displayedBulletCount, art);
      DrawPouchHandCover(pouchRect, purchase, art);

      if (purchase == null
        || !purchase.InputLocked
        || purchase.Product == null
        || (purchase.Phase != BarShopPurchasePhase.Tossing
          && purchase.Phase != BarShopPurchasePhase.Completed)) return;

      var price = purchase.Product.Price;
      var paymentElapsed = Math.Max(
        0,
        purchase.ElapsedMicroseconds - GameRules.BarShopPouchCoverMicroseconds);
      var paymentDuration = price <= 2
        ? GameRules.BarShopCoinFlipDurationMicroseconds
        : GameRules.BarShopBulletPourDurationMicroseconds;
      var progress = Mathf.Clamp01(paymentElapsed / (float)paymentDuration);
      if (price <= 2) DrawCoinFlipPayment(price, progress, art);
      else DrawPourPayment(price, progress, art);
    }

    private static void DrawAmmoPouch(Rect pouchRect, int bulletCount, BarShopUiArtSet art)
    {
      var pouch = art?.AmmoPouchEmpty ?? art?.AmmoPouch;
      if (pouch != null)
      {
        GUI.DrawTexture(pouchRect, pouch, ScaleMode.ScaleToFit, true);
      }
      else
      {
        var previous = GUI.color;
        GUI.color = new Color(0.24f, 0.12f, 0.035f, 0.94f);
        GUI.DrawTexture(pouchRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
        GUI.color = previous;
      }

      if (art?.AmmoPouchBullet == null || bulletCount <= 0) return;
      if (bulletCount <= 5)
      {
        var anchors = new[] { 43f, 60f, 77f, 94f, 111f };
        for (var index = 0; index < bulletCount; index++)
        {
          GUI.DrawTexture(
            new Rect(pouchRect.x + anchors[index], pouchRect.y + 34f, 24f, 40f),
            art.AmmoPouchBullet,
            ScaleMode.ScaleToFit,
            true);
        }
        return;
      }

      for (var index = 0; index < bulletCount; index++)
      {
        var column = index % 6;
        var row = index / 6;
        GUI.DrawTexture(
          new Rect(
            pouchRect.x + 44f + column * 13f,
            pouchRect.y + 31f + row * 13f,
            10f,
            18f),
          art.AmmoPouchBullet,
          ScaleMode.ScaleToFit,
          true);
      }
    }

    private static void DrawPouchHandCover(
      Rect pouchRect,
      BarShopPurchaseSnapshot purchase,
      BarShopUiArtSet art)
    {
      if (purchase == null
        || !purchase.InputLocked
        || purchase.Phase == BarShopPurchasePhase.Rejected
        || art?.AmmoPouchHandCover == null) return;
      var duration = GameRules.BarShopPouchCoverMicroseconds;
      var elapsed = purchase.ElapsedMicroseconds;
      if (elapsed >= duration * 2) return;
      var offset = elapsed < duration
        ? Mathf.Lerp(180f, 0f, elapsed / (float)duration)
        : Mathf.Lerp(0f, 180f, (elapsed - duration) / (float)duration);
      var handRect = new Rect(
        pouchRect.x - 20f,
        pouchRect.y - 15f + offset,
        220f,
        180f);
      GUI.DrawTexture(handRect, art.AmmoPouchHandCover, ScaleMode.ScaleToFit, true);
    }

    private static void DrawCoinFlipPayment(int price, float progress, BarShopUiArtSet art)
    {
      for (var index = 0; index < price; index++)
      {
        var localProgress = Mathf.Clamp01(progress * 1.15f - index * 0.15f);
        var start = new Vector2(520f + index * 24f, 560f);
        var control = new Vector2(520f + index * 38f, 260f);
        var end = new Vector2(500f + index * 42f, 330f);
        var inverse = 1f - localProgress;
        var point = (inverse * inverse * start)
          + (2f * inverse * localProgress * control)
          + (localProgress * localProgress * end);
        var bulletRect = new Rect(point.x - 16f, point.y - 32f, 32f, 64f);
        var sheet = art?.BulletCoinFlipSheet ?? art?.BulletTossSheet;
        if (sheet != null)
        {
          var frameCount = art?.BulletCoinFlipSheet != null ? 8 : 6;
          var frameIndex = Mathf.Min(
            frameCount - 1,
            Mathf.FloorToInt(localProgress * frameCount));
          GUI.DrawTextureWithTexCoords(
            bulletRect,
            sheet,
            new Rect(frameIndex / (float)frameCount, 0f, 1f / frameCount, 1f),
            true);
        }
        else if (art?.AmmoPouchBullet != null)
        {
          var previousMatrix = GUI.matrix;
          GUIUtility.RotateAroundPivot(localProgress * 540f, point);
          GUI.DrawTexture(bulletRect, art.AmmoPouchBullet, ScaleMode.ScaleToFit, true);
          GUI.matrix = previousMatrix;
        }
      }
    }

    private static void DrawPourPayment(int price, float progress, BarShopUiArtSet art)
    {
      if (art?.BulletPourSheet != null)
      {
        const int frameCount = 8;
        var frameIndex = Mathf.Min(frameCount - 1, Mathf.FloorToInt(progress * frameCount));
        GUI.DrawTextureWithTexCoords(
          new Rect(400f, 350f, 160f, 120f),
          art.BulletPourSheet,
          new Rect(frameIndex / (float)frameCount, 0f, 1f / frameCount, 1f),
          true);
      }

      if (art?.AmmoPouchBullet == null || progress < 0.55f) return;
      for (var index = 0; index < price; index++)
      {
        var x = 444f + ((index * 37) % 122);
        var y = 392f + ((index * 19) % 44);
        var point = new Vector2(x, y);
        var previousMatrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(-38f + (index * 29) % 84, point);
        GUI.DrawTexture(
          new Rect(x - 6f, y - 10f, 12f, 20f),
          art.AmmoPouchBullet,
          ScaleMode.ScaleToFit,
          true);
        GUI.matrix = previousMatrix;
      }
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
