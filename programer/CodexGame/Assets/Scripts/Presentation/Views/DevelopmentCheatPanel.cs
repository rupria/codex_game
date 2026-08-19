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
    private ItemQaPreset _selectedItemPreset;

    public void Draw(
      PlayableGameSnapshot snapshot,
      PlayableDevStyles styles,
      Action completeStage,
      Action<bool> setJokerAwardGuaranteed,
      Action<GameItemId> grantItem,
      Action<PokerCheatPreset> setPoker,
      Action<ItemQaPreset> runItemPreset,
      Action close)
    {
      var previous = GUI.color;
      GUI.color = new Color(0f, 0f, 0f, 0.94f);
      GUI.DrawTexture(new Rect(0f, 0f, 960f, 540f), Texture2D.whiteTexture);
      GUI.color = previous;

      GUI.Box(new Rect(24f, 20f, 912f, 500f), GUIContent.none);
      GUI.Label(new Rect(48f, 34f, 720f, 32f), "DEVELOPMENT CHEATS — `", styles.Heading);
      GUI.Label(
        new Rect(48f, 70f, 840f, 24f),
        $"STAGE {snapshot.StageNumber} / ROUND {snapshot.CombatRoundNumber} / {snapshot.Phase} / HP {snapshot.Health.Player}:{snapshot.Health.Ai}",
        styles.Small);
      if (GUI.Button(new Rect(48f, 108f, 150f, 38f), "COMPLETE STAGE")) completeStage();
      var jokerCheatLabel = snapshot.JokerAwardCheatEnabled
        ? "JOKER 100%: ON"
        : "JOKER 100%: OFF";
      if (GUI.Button(new Rect(208f, 108f, 190f, 38f), jokerCheatLabel))
      {
        setJokerAwardGuaranteed(!snapshot.JokerAwardCheatEnabled);
      }
      if (GUI.Button(new Rect(408f, 108f, 38f, 38f), "<")) MoveItemPreset(-1);
      GUI.Label(new Rect(452f, 108f, 220f, 38f), _selectedItemPreset.ToString(), styles.Small);
      if (GUI.Button(new Rect(678f, 108f, 38f, 38f), ">")) MoveItemPreset(1);
      if (GUI.Button(new Rect(726f, 108f, 170f, 38f), "RUN ITEM QA"))
      {
        runItemPreset(_selectedItemPreset);
      }
      if (GUI.Button(new Rect(746f, 34f, 150f, 38f), "CLOSE")) close();

      GUI.Label(new Rect(48f, 158f, 240f, 24f), "GRANT UNIQUE ITEM", styles.Small);
      var itemValues = (GameItemId[])Enum.GetValues(typeof(GameItemId));
      for (var index = 0; index < itemValues.Length; index++)
      {
        var column = index % 4;
        var row = index / 4;
        if (GUI.Button(
          new Rect(48f + column * 190f, 184f + row * 36f, 180f, 32f),
          itemValues[index].ToString()))
        {
          grantItem(itemValues[index]);
        }
      }

      GUI.Label(new Rect(48f, 264f, 240f, 24f), "SET EXACT POKER HAND", styles.Small);
      var presets = (PokerCheatPreset[])Enum.GetValues(typeof(PokerCheatPreset));
      for (var index = 0; index < presets.Length; index++)
      {
        var column = index % 4;
        var row = index / 4;
        if (GUI.Button(
          new Rect(48f + column * 190f, 290f + row * 30f, 180f, 28f),
          presets[index].ToString()))
        {
          setPoker(presets[index]);
        }
      }

      if (snapshot.LastItemQaPresetResult != null)
      {
        var qa = snapshot.LastItemQaPresetResult;
        GUI.Label(
          new Rect(48f, 438f, 840f, 20f),
          $"ITEM QA {(qa.Passed ? "PASS" : "FAIL")} {qa.Preset} seed={qa.Seed} expected={qa.Expected} actual={qa.Actual}",
          styles.Small);
        GUI.Label(
          new Rect(48f, 458f, 840f, 20f),
          $"hand={qa.PlayerHand} / public={qa.PublicCards} / items={qa.Items}",
          styles.Small);
      }
      GUI.Label(new Rect(48f, 480f, 220f, 18f), "RECENT COMMANDS", styles.Small);
      var start = Math.Max(0, snapshot.CheatHistory.Count - 1);
      for (var index = start; index < snapshot.CheatHistory.Count; index++)
      {
        var entry = snapshot.CheatHistory[index];
        GUI.Label(
          new Rect(48f, 498f + (index - start) * 14f, 840f, 16f),
          $"{entry.Command} {entry.Input} -> {entry.Result}",
          styles.Small);
      }
    }

    private void MoveItemPreset(int delta)
    {
      var values = (ItemQaPreset[])Enum.GetValues(typeof(ItemQaPreset));
      var next = ((int)_selectedItemPreset + delta + values.Length) % values.Length;
      _selectedItemPreset = values[next];
    }
  }
}
#endif
