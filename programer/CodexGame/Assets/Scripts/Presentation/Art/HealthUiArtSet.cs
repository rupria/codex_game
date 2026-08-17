using System;
using UnityEngine;

namespace CodexGame.Presentation.Art
{
  [Serializable]
  public sealed class HealthUiArtSet
  {
    [SerializeField] private Texture2D _playerFilled;
    [SerializeField] private Texture2D _playerEmpty;
    [SerializeField] private Texture2D _aiFilled;
    [SerializeField] private Texture2D _aiEmpty;
    [SerializeField] private Texture2D _playerDamage;
    [SerializeField] private Texture2D _aiDamage;

    public HealthUiArtSet(
      Texture2D playerFilled,
      Texture2D playerEmpty,
      Texture2D aiFilled,
      Texture2D aiEmpty,
      Texture2D playerDamage = null,
      Texture2D aiDamage = null)
    {
      _playerFilled = playerFilled ?? throw new ArgumentNullException(nameof(playerFilled));
      _playerEmpty = playerEmpty ?? throw new ArgumentNullException(nameof(playerEmpty));
      _aiFilled = aiFilled ?? throw new ArgumentNullException(nameof(aiFilled));
      _aiEmpty = aiEmpty ?? throw new ArgumentNullException(nameof(aiEmpty));
      _playerDamage = playerDamage;
      _aiDamage = aiDamage;
    }

    public Texture2D PlayerFilled => _playerFilled;
    public Texture2D PlayerEmpty => _playerEmpty;
    public Texture2D AiFilled => _aiFilled;
    public Texture2D AiEmpty => _aiEmpty;
    public Texture2D PlayerDamage => _playerDamage ?? _playerFilled;
    public Texture2D AiDamage => _aiDamage ?? _aiFilled;

    public bool IsComplete => _playerFilled != null
      && _playerEmpty != null
      && _aiFilled != null
      && _aiEmpty != null;
  }
}
