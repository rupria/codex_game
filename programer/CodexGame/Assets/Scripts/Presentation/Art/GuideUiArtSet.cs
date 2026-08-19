using System;
using UnityEngine;

namespace CodexGame.Presentation.Art
{
  [Serializable]
  public sealed class GuideNavButtonArtSet
  {
    [SerializeField] private Texture2D _idle;
    [SerializeField] private Texture2D _hover;
    [SerializeField] private Texture2D _pressed;
    [SerializeField] private Texture2D _disabled;

    public GuideNavButtonArtSet(
      Texture2D idle,
      Texture2D hover,
      Texture2D pressed,
      Texture2D disabled)
    {
      _idle = idle ?? throw new ArgumentNullException(nameof(idle));
      _hover = hover ?? throw new ArgumentNullException(nameof(hover));
      _pressed = pressed ?? throw new ArgumentNullException(nameof(pressed));
      _disabled = disabled ?? throw new ArgumentNullException(nameof(disabled));
    }

    public Texture2D GetTexture(bool enabled, bool hovered, bool pressed)
    {
      if (!enabled) return _disabled;
      if (pressed) return _pressed;
      return hovered ? _hover : _idle;
    }

    public bool IsComplete => _idle != null
      && _hover != null
      && _pressed != null
      && _disabled != null;
  }

  [Serializable]
  public sealed class GuideUiArtSet
  {
    [SerializeField] private Texture2D _modalBackground;
    [SerializeField] private Texture2D _flowPage;
    [SerializeField] private Texture2D _halliPage;
    [SerializeField] private Texture2D _cardsPage;
    [SerializeField] private Texture2D _resultPage;
    [SerializeField] private Texture2D _navRail;
    [SerializeField] private Texture2D _pageIndicatorPlate;
    [SerializeField] private GuideNavButtonArtSet _previousButton;
    [SerializeField] private GuideNavButtonArtSet _nextButton;
    [SerializeField] private GuideNavButtonArtSet _closeButton;

    public GuideUiArtSet(
      Texture2D modalBackground,
      Texture2D flowPage,
      Texture2D halliPage,
      Texture2D cardsPage,
      Texture2D resultPage,
      Texture2D navRail,
      Texture2D pageIndicatorPlate,
      GuideNavButtonArtSet previousButton,
      GuideNavButtonArtSet nextButton,
      GuideNavButtonArtSet closeButton)
    {
      _modalBackground = modalBackground ?? throw new ArgumentNullException(nameof(modalBackground));
      _flowPage = flowPage ?? throw new ArgumentNullException(nameof(flowPage));
      _halliPage = halliPage ?? throw new ArgumentNullException(nameof(halliPage));
      _cardsPage = cardsPage ?? throw new ArgumentNullException(nameof(cardsPage));
      _resultPage = resultPage ?? throw new ArgumentNullException(nameof(resultPage));
      _navRail = navRail ?? throw new ArgumentNullException(nameof(navRail));
      _pageIndicatorPlate = pageIndicatorPlate
        ?? throw new ArgumentNullException(nameof(pageIndicatorPlate));
      _previousButton = previousButton ?? throw new ArgumentNullException(nameof(previousButton));
      _nextButton = nextButton ?? throw new ArgumentNullException(nameof(nextButton));
      _closeButton = closeButton ?? throw new ArgumentNullException(nameof(closeButton));
    }

    public Texture2D ModalBackground => _modalBackground;
    public Texture2D NavRail => _navRail;
    public Texture2D PageIndicatorPlate => _pageIndicatorPlate;
    public GuideNavButtonArtSet PreviousButton => _previousButton;
    public GuideNavButtonArtSet NextButton => _nextButton;
    public GuideNavButtonArtSet CloseButton => _closeButton;

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
      && _navRail != null
      && _pageIndicatorPlate != null
      && _previousButton != null
      && _previousButton.IsComplete
      && _nextButton != null
      && _nextButton.IsComplete
      && _closeButton != null
      && _closeButton.IsComplete;
  }
}
