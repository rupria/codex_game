#if UNITY_EDITOR || ENABLE_GAMEPLAY_CHEATS
using System;
using CodexGame.Application.Development;
using CodexGame.Application.Playable;
using CodexGame.Core.Items;
using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class DevelopmentCheatPanel
  {
    public void Draw(
      PlayableGameSnapshot snapshot,
      PlayableDevStyles styles,
      Action completeStage,
      Action<GameItemId> grantItem,
      Action<PokerCheatPreset> setPoker,
      Action close)
    {
      var previous = GUI.color;
      GUI.color = new Color(0f, 0f, 0f, 0.94f);
      GUI.DrawTexture(new Rect(0f, 0f, 960f, 540f), Texture2D.whiteTexture);
      GUI.color = previous;

      GUI.Box(new Rect(24f, 20f, 912f, 500f), GUIContent.none);
      GUI.Label(new Rect(48f, 34f, 720f, 32f), "DEVELOPMENT CHEATS — F10", styles.Heading);
      GUI.Label(
        new Rect(48f, 70f, 840f, 24f),
        $"STAGE {snapshot.StageNumber} / ROUND {snapshot.CombatRoundNumber} / {snapshot.Phase} / HP {snapshot.Health.Player}:{snapshot.Health.Ai}",
        styles.Small);
      if (GUI.Button(new Rect(48f, 108f, 190f, 38f), "COMPLETE STAGE")) completeStage();
      if (GUI.Button(new Rect(746f, 34f, 150f, 38f), "CLOSE")) close();

      GUI.Label(new Rect(48f, 158f, 240f, 24f), "GRANT UNIQUE ITEM", styles.Small);
      var itemValues = (GameItemId[])Enum.GetValues(typeof(GameItemId));
      for (var index = 0; index < itemValues.Length; index++)
      {
        if (GUI.Button(new Rect(48f + index * 150f, 184f, 140f, 34f), itemValues[index].ToString()))
        {
          grantItem(itemValues[index]);
        }
      }

      GUI.Label(new Rect(48f, 232f, 240f, 24f), "SET EXACT POKER HAND", styles.Small);
      var presets = (PokerCheatPreset[])Enum.GetValues(typeof(PokerCheatPreset));
      for (var index = 0; index < presets.Length; index++)
      {
        var column = index % 4;
        var row = index / 4;
        if (GUI.Button(
          new Rect(48f + column * 190f, 258f + row * 34f, 180f, 30f),
          presets[index].ToString()))
        {
          setPoker(presets[index]);
        }
      }

      GUI.Label(new Rect(48f, 400f, 220f, 24f), "RECENT COMMANDS (LAST 20)", styles.Small);
      var start = Math.Max(0, snapshot.CheatHistory.Count - 4);
      for (var index = start; index < snapshot.CheatHistory.Count; index++)
      {
        var entry = snapshot.CheatHistory[index];
        GUI.Label(
          new Rect(48f, 424f + (index - start) * 20f, 840f, 20f),
          $"{entry.Command} {entry.Input} -> {entry.Result}",
          styles.Small);
      }
    }
  }
}
#endif
