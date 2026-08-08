using System;
using UnityEngine;

namespace CodexGame.Presentation.Art
{
  [Serializable]
  public sealed class PokerUiArtSet
  {
    [SerializeField] private Texture2D _winIdle;
    [SerializeField] private Texture2D _winHover;
    [SerializeField] private Texture2D _loseIdle;
    [SerializeField] private Texture2D _loseHover;
    [SerializeField] private Texture2D _itemSlot;

    public PokerUiArtSet(
      Texture2D winIdle,
      Texture2D winHover,
      Texture2D loseIdle,
      Texture2D loseHover,
      Texture2D itemSlot)
    {
      _winIdle = winIdle ?? throw new ArgumentNullException(nameof(winIdle));
      _winHover = winHover ?? throw new ArgumentNullException(nameof(winHover));
      _loseIdle = loseIdle ?? throw new ArgumentNullException(nameof(loseIdle));
      _loseHover = loseHover ?? throw new ArgumentNullException(nameof(loseHover));
      _itemSlot = itemSlot ?? throw new ArgumentNullException(nameof(itemSlot));
    }

    public Texture2D WinIdle => _winIdle;
    public Texture2D WinHover => _winHover;
    public Texture2D LoseIdle => _loseIdle;
    public Texture2D LoseHover => _loseHover;
    public Texture2D ItemSlot => _itemSlot;

    public bool IsComplete => _winIdle != null
      && _winHover != null
      && _loseIdle != null
      && _loseHover != null
      && _itemSlot != null;
  }
}
