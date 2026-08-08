using CodexGame.Application.Playable;
using UnityEngine;
using UnityEngine.Rendering;

namespace CodexGame.Presentation.Views
{
  [DefaultExecutionOrder(-50)]
  public sealed class TableScenePresentationRig : MonoBehaviour
  {
    private const float BackdropFov = 58f;
    private const float CameraDistance = 10f;
    private const float DesignAspect = 16f / 9f;

    [SerializeField] private Camera _sceneCamera;
    [SerializeField] private PlayableDevView _view;
    [SerializeField] private Texture2D _battleBackdrop;
    [SerializeField] private Texture2D _introBackdrop;
    [SerializeField] private Shader _backdropShader;

    private MeshRenderer _backdropRenderer;
    private Mesh _backdropMesh;
    private Material _backdropMaterial;
    private Light _tableLight;
    private float _fieldOfViewVelocity;
    private float _cameraYVelocity;
    private float _lightIntensityVelocity;
    private float _spotAngleVelocity;

    public void Configure(
      Camera sceneCamera,
      PlayableDevView view,
      Texture2D battleBackdrop,
      Texture2D introBackdrop,
      Shader backdropShader)
    {
      _sceneCamera = sceneCamera;
      _view = view;
      _battleBackdrop = battleBackdrop;
      _introBackdrop = introBackdrop;
      _backdropShader = backdropShader;
    }

    private void Awake()
    {
      if (_sceneCamera == null) _sceneCamera = Camera.main;
      CreateBackdrop();
      CreateTableLight();
      ApplyPhaseImmediately(CurrentPhase);
    }

    private void Update()
    {
      if (_sceneCamera == null || _backdropRenderer == null || _tableLight == null) return;

      var phase = CurrentPhase;
      var target = GetPhasePresentation(phase);
      var deltaTime = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);

      _sceneCamera.fieldOfView = Mathf.SmoothDamp(
        _sceneCamera.fieldOfView,
        target.FieldOfView,
        ref _fieldOfViewVelocity,
        0.65f,
        Mathf.Infinity,
        deltaTime);

      var cameraPosition = _sceneCamera.transform.position;
      cameraPosition.y = Mathf.SmoothDamp(
        cameraPosition.y,
        target.CameraY,
        ref _cameraYVelocity,
        0.75f,
        Mathf.Infinity,
        deltaTime);
      _sceneCamera.transform.position = cameraPosition;

      var pulse = phase == PlayableGamePhase.Halli
        ? Mathf.Sin(Time.unscaledTime * 0.9f) * 0.08f
        : 0f;
      _tableLight.intensity = Mathf.SmoothDamp(
        _tableLight.intensity,
        target.LightIntensity + pulse,
        ref _lightIntensityVelocity,
        0.55f,
        Mathf.Infinity,
        deltaTime);
      _tableLight.spotAngle = Mathf.SmoothDamp(
        _tableLight.spotAngle,
        target.SpotAngle,
        ref _spotAngleVelocity,
        0.7f,
        Mathf.Infinity,
        deltaTime);

      var lightPosition = _tableLight.transform.position;
      lightPosition.x = Mathf.Sin(Time.unscaledTime * 0.23f) * 0.18f;
      lightPosition.y = 1.4f + Mathf.Cos(Time.unscaledTime * 0.19f) * 0.08f;
      _tableLight.transform.position = lightPosition;

      var texture = phase == PlayableGamePhase.Intro && _introBackdrop != null
        ? _introBackdrop
        : _battleBackdrop;
      if (_backdropMaterial != null && _backdropMaterial.mainTexture != texture)
      {
        _backdropMaterial.mainTexture = texture;
      }
    }

    private void OnDestroy()
    {
      if (_backdropMaterial != null) Destroy(_backdropMaterial);
      if (_backdropMesh != null) Destroy(_backdropMesh);
    }

    private PlayableGamePhase CurrentPhase => _view == null
      ? PlayableGamePhase.Intro
      : _view.CurrentPhase;

    private void CreateBackdrop()
    {
      if (_sceneCamera == null || _battleBackdrop == null || _backdropShader == null) return;

      _sceneCamera.clearFlags = CameraClearFlags.SolidColor;
      _sceneCamera.backgroundColor = new Color(0.006f, 0.004f, 0.003f, 1f);
      _sceneCamera.transform.position = new Vector3(0f, 0f, -CameraDistance);
      _sceneCamera.transform.rotation = Quaternion.identity;
      _sceneCamera.fieldOfView = BackdropFov;

      var backdrop = new GameObject("Lit Saloon Backdrop");
      backdrop.transform.SetParent(transform, false);
      backdrop.transform.position = Vector3.zero;

      var meshFilter = backdrop.AddComponent<MeshFilter>();
      _backdropMesh = CreateBackdropMesh();
      meshFilter.sharedMesh = _backdropMesh;
      _backdropRenderer = backdrop.AddComponent<MeshRenderer>();
      _backdropRenderer.shadowCastingMode = ShadowCastingMode.Off;
      _backdropRenderer.receiveShadows = false;

      _backdropMaterial = new Material(_backdropShader)
      {
        name = "Runtime Lit Saloon Backdrop",
        mainTexture = _introBackdrop != null ? _introBackdrop : _battleBackdrop,
        color = new Color(0.72f, 0.72f, 0.72f, 1f),
        hideFlags = HideFlags.HideAndDontSave
      };
      if (_backdropMaterial.HasProperty("_Glossiness"))
      {
        _backdropMaterial.SetFloat("_Glossiness", 0f);
      }
      if (_backdropMaterial.HasProperty("_Metallic"))
      {
        _backdropMaterial.SetFloat("_Metallic", 0f);
      }
      _backdropRenderer.sharedMaterial = _backdropMaterial;

      var height = 2f * CameraDistance * Mathf.Tan(BackdropFov * 0.5f * Mathf.Deg2Rad);
      backdrop.transform.localScale = new Vector3(height * DesignAspect, height, 1f);
    }

    private static Mesh CreateBackdropMesh()
    {
      var mesh = new Mesh { name = "Runtime Backdrop Quad" };
      mesh.vertices = new[]
      {
        new Vector3(-0.5f, -0.5f, 0f),
        new Vector3(0.5f, -0.5f, 0f),
        new Vector3(0.5f, 0.5f, 0f),
        new Vector3(-0.5f, 0.5f, 0f)
      };
      mesh.uv = new[]
      {
        new Vector2(0f, 0f),
        new Vector2(1f, 0f),
        new Vector2(1f, 1f),
        new Vector2(0f, 1f)
      };
      mesh.normals = new[]
      {
        Vector3.back,
        Vector3.back,
        Vector3.back,
        Vector3.back
      };
      mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
      mesh.RecalculateBounds();
      return mesh;
    }

    private void CreateTableLight()
    {
      var lightObject = new GameObject("Warm Table Spotlight");
      lightObject.transform.SetParent(transform, false);
      lightObject.transform.position = new Vector3(0f, 1.4f, -4.2f);
      lightObject.transform.rotation = Quaternion.identity;

      _tableLight = lightObject.AddComponent<Light>();
      _tableLight.type = LightType.Spot;
      _tableLight.color = new Color(1f, 0.66f, 0.34f, 1f);
      _tableLight.range = 18f;
      _tableLight.spotAngle = 54f;
      _tableLight.intensity = 1.25f;
      _tableLight.shadows = LightShadows.None;
      _tableLight.renderMode = LightRenderMode.ForcePixel;
    }

    private void ApplyPhaseImmediately(PlayableGamePhase phase)
    {
      if (_sceneCamera == null || _tableLight == null) return;
      var target = GetPhasePresentation(phase);
      _sceneCamera.fieldOfView = target.FieldOfView;
      var cameraPosition = _sceneCamera.transform.position;
      cameraPosition.y = target.CameraY;
      _sceneCamera.transform.position = cameraPosition;
      _tableLight.intensity = target.LightIntensity;
      _tableLight.spotAngle = target.SpotAngle;
    }

    private static PhasePresentation GetPhasePresentation(PlayableGamePhase phase)
    {
      switch (phase)
      {
        case PlayableGamePhase.Intro:
          return new PhasePresentation(56f, 0f, 1.1f, 58f);
        case PlayableGamePhase.HalliOpening:
          return new PhasePresentation(48f, 0.18f, 1.55f, 52f);
        case PlayableGamePhase.Halli:
          return new PhasePresentation(43f, 0.32f, 1.85f, 46f);
        case PlayableGamePhase.HalliTransition:
          return new PhasePresentation(47f, 0.2f, 1.45f, 50f);
        case PlayableGamePhase.PrivateSelection:
          return new PhasePresentation(49f, -0.08f, 1.35f, 54f);
        case PlayableGamePhase.PokerPrediction:
        case PlayableGamePhase.PokerResult:
          return new PhasePresentation(46f, -0.16f, 1.5f, 50f);
        case PlayableGamePhase.StageWon:
          return new PhasePresentation(52f, 0.08f, 2f, 60f);
        case PlayableGamePhase.Bar:
        case PlayableGamePhase.BattleFinished:
          return new PhasePresentation(56f, 0f, 1.15f, 62f);
        default:
          return new PhasePresentation(50f, 0f, 1.4f, 54f);
      }
    }

    private readonly struct PhasePresentation
    {
      public PhasePresentation(
        float fieldOfView,
        float cameraY,
        float lightIntensity,
        float spotAngle)
      {
        FieldOfView = fieldOfView;
        CameraY = cameraY;
        LightIntensity = lightIntensity;
        SpotAngle = spotAngle;
      }

      public float FieldOfView { get; }
      public float CameraY { get; }
      public float LightIntensity { get; }
      public float SpotAngle { get; }
    }
  }
}
