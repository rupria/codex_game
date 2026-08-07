using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class PlayableDevStyles
  {
    public PlayableDevStyles()
    {
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
    }

    public GUIStyle Title { get; }
    public GUIStyle Heading { get; }
    public GUIStyle Body { get; }
    public GUIStyle Card { get; }
    public GUIStyle SelectedCard { get; }
    public GUIStyle Status { get; }
    public GUIStyle Small { get; }
  }
}
