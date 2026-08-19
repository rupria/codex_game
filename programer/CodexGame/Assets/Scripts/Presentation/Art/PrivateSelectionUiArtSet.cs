using System;
using UnityEngine;

namespace CodexGame.Presentation.Art
{
  [Serializable]
  public sealed class PrivateSelectionUiArtSet
  {
    [SerializeField] private Texture2D _modalDim;
    [SerializeField] private Texture2D _modalPanel;
    [SerializeField] private Texture2D _publicFrame;
    [SerializeField] private Texture2D _candidateIdle;
    [SerializeField] private Texture2D _candidateHover;
    [SerializeField] private Texture2D _candidateSelected;
    [SerializeField] private Texture2D _candidateConfirmed;
    [SerializeField] private Texture2D _candidateDisabled;
    [SerializeField] private Texture2D _confirmIdle;
    [SerializeField] private Texture2D _confirmActive;
    [SerializeField] private Texture2D _confirmDisabled;

    public PrivateSelectionUiArtSet(
      Texture2D modalDim,
      Texture2D modalPanel,
      Texture2D publicFrame,
      Texture2D candidateIdle,
      Texture2D candidateHover,
      Texture2D candidateSelected,
      Texture2D candidateConfirmed,
      Texture2D candidateDisabled,
      Texture2D confirmIdle,
      Texture2D confirmActive,
      Texture2D confirmDisabled)
    {
      _modalDim = Require(modalDim, nameof(modalDim));
      _modalPanel = Require(modalPanel, nameof(modalPanel));
      _publicFrame = Require(publicFrame, nameof(publicFrame));
      _candidateIdle = Require(candidateIdle, nameof(candidateIdle));
      _candidateHover = Require(candidateHover, nameof(candidateHover));
      _candidateSelected = Require(candidateSelected, nameof(candidateSelected));
      _candidateConfirmed = Require(candidateConfirmed, nameof(candidateConfirmed));
      _candidateDisabled = Require(candidateDisabled, nameof(candidateDisabled));
      _confirmIdle = Require(confirmIdle, nameof(confirmIdle));
      _confirmActive = Require(confirmActive, nameof(confirmActive));
      _confirmDisabled = Require(confirmDisabled, nameof(confirmDisabled));
    }

    public Texture2D ModalDim => _modalDim;
    public Texture2D ModalPanel => _modalPanel;
    public Texture2D PublicFrame => _publicFrame;
    public Texture2D CandidateIdle => _candidateIdle;
    public Texture2D CandidateHover => _candidateHover;
    public Texture2D CandidateSelected => _candidateSelected;
    public Texture2D CandidateConfirmed => _candidateConfirmed;
    public Texture2D CandidateDisabled => _candidateDisabled;
    public Texture2D ConfirmIdle => _confirmIdle;
    public Texture2D ConfirmActive => _confirmActive;
    public Texture2D ConfirmDisabled => _confirmDisabled;
    public bool IsComplete => _modalDim != null
      && _modalPanel != null
      && _publicFrame != null
      && _candidateIdle != null
      && _candidateHover != null
      && _candidateSelected != null
      && _candidateConfirmed != null
      && _candidateDisabled != null
      && _confirmIdle != null
      && _confirmActive != null
      && _confirmDisabled != null;

    private static Texture2D Require(Texture2D texture, string parameterName)
    {
      return texture != null
        ? texture
        : throw new ArgumentNullException(parameterName);
    }
  }
}
