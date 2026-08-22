using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class PlayableDevStyles
  {
    public PlayableDevStyles()
    {
      var runtimeFont = Resources.Load<Font>("Fonts/NotoSansKR");
      if (runtimeFont != null) GUI.skin.font = runtimeFont;
      Title = new GUIStyle(GUI.skin.label)
      {
        fontSize = 25,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter
      };
      Heading = new GUIStyle(GUI.skin.label)
      {
        fontSize = 17,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter
      };
      Body = new GUIStyle(GUI.skin.label)
      {
        fontSize = 14,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = true
      };
      Card = new GUIStyle(GUI.skin.box)
      {
        fontSize = 17,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = true
      };
      SelectedCard = new GUIStyle(Card);
      SelectedCard.normal.background = Texture2D.whiteTexture;
      SelectedCard.normal.textColor = new Color(0.08f, 0.12f, 0.18f);
      Status = new GUIStyle(GUI.skin.box)
      {
        fontSize = 16,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = true
      };
      Small = new GUIStyle(GUI.skin.label)
      {
        fontSize = 12,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = true
      };
      PredictionButton = new GUIStyle(GUI.skin.label)
      {
        fontSize = 16,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = false
      };
      PredictionButton.normal.textColor = new Color(0.96f, 0.91f, 0.76f);
      PredictionButton.hover.textColor = Color.white;
      PredictionButton.active.textColor = new Color(1f, 0.78f, 0.34f);
      PredictionMetric = new GUIStyle(GUI.skin.label)
      {
        fontSize = 13,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleLeft,
        wordWrap = false
      };
      PredictionMetric.normal.textColor = new Color(1f, 0.82f, 0.42f);
      IntroButton = new GUIStyle(GUI.skin.label)
      {
        fontSize = 28,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter
      };
      IntroButton.normal.textColor = new Color(0.96f, 0.92f, 0.82f);
      IntroButton.hover.textColor = Color.white;
      IntroButton.active.textColor = new Color(1f, 0.76f, 0.34f);
    }

    public GUIStyle Title { get; }
    public GUIStyle Heading { get; }
    public GUIStyle Body { get; }
    public GUIStyle Card { get; }
    public GUIStyle SelectedCard { get; }
    public GUIStyle Status { get; }
    public GUIStyle Small { get; }
    public GUIStyle PredictionButton { get; }
    public GUIStyle PredictionMetric { get; }
    public GUIStyle IntroButton { get; }
  }
}
