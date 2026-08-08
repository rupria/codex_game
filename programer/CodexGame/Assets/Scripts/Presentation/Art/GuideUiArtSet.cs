using System;
using UnityEngine;

namespace CodexGame.Presentation.Art
{
  [Serializable]
  public sealed class GuideUiArtSet
  {
    [SerializeField] private Texture2D _modalBackground;
    [SerializeField] private Texture2D _flowPage;
    [SerializeField] private Texture2D _halliPage;
    [SerializeField] private Texture2D _cardsPage;
    [SerializeField] private Texture2D _resultPage;
    [SerializeField] private Texture2D _navIdle;
    [SerializeField] private Texture2D _navHover;
    [SerializeField] private Texture2D _navDisabled;
    [SerializeField] private Texture2D _pageIndicatorPlate;

    public GuideUiArtSet(
      Texture2D modalBackground,
      Texture2D flowPage,
      Texture2D halliPage,
      Texture2D cardsPage,
      Texture2D resultPage,
      Texture2D navIdle,
      Texture2D navHover,
      Texture2D navDisabled,
      Texture2D pageIndicatorPlate)
    {
      _modalBackground = modalBackground ?? throw new ArgumentNullException(nameof(modalBackground));
      _flowPage = flowPage ?? throw new ArgumentNullException(nameof(flowPage));
      _halliPage = halliPage ?? throw new ArgumentNullException(nameof(halliPage));
      _cardsPage = cardsPage ?? throw new ArgumentNullException(nameof(cardsPage));
      _resultPage = resultPage ?? throw new ArgumentNullException(nameof(resultPage));
      _navIdle = navIdle ?? throw new ArgumentNullException(nameof(navIdle));
      _navHover = navHover ?? throw new ArgumentNullException(nameof(navHover));
      _navDisabled = navDisabled ?? throw new ArgumentNullException(nameof(navDisabled));
      _pageIndicatorPlate = pageIndicatorPlate
        ?? throw new ArgumentNullException(nameof(pageIndicatorPlate));
    }

    public Texture2D ModalBackground => _modalBackground;
    public Texture2D NavIdle => _navIdle;
    public Texture2D NavHover => _navHover;
    public Texture2D NavDisabled => _navDisabled;
    public Texture2D PageIndicatorPlate => _pageIndicatorPlate;

    public Texture2D GetPageArt(int pageIndex)
    {
      return pageIndex switch
      {
        0 => _flowPage,
        1 => _halliPage,
        2 => _cardsPage,
        3 => _resultPage,
        _ => throw new ArgumentOutOfRangeException(nameof(pageIndex))
      };
    }

    public bool IsComplete => _modalBackground != null
      && _flowPage != null
      && _halliPage != null
      && _cardsPage != null
      && _resultPage != null
      && _navIdle != null
      && _navHover != null
      && _navDisabled != null
      && _pageIndicatorPlate != null;
  }
}
