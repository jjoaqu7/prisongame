using PrisonGame.Combat;
using UnityEngine;
using UnityEngine.Rendering;

namespace PrisonGame.PlayerCombat
{
    // Local-player presentation: a small upward kick of the view when this player lands a hit.
    // The offset is applied only while this camera renders and removed straight after, so the
    // transform that aiming and hit checks read is never moved. On by default, with an off
    // setting in the Escape menu (agreed: "screen shake on by default with an off switch").
    [RequireComponent(typeof(Camera))]
    public sealed class CameraKick : MonoBehaviour
    {
        public const string EnabledKey = "PrisonGame.CameraShake";
        public const float DecaySeconds = 0.15f;
        [SerializeField] private MeleeSweep _sweep;
        private Camera _camera;
        private bool _subscribed;
        private float _peakDegrees;
        private float _roll;
        private float _elapsed = DecaySeconds;
        private Quaternion _restore;
        private bool _applied;

        public static bool IsOn
        {
            get => PlayerPrefs.GetInt(EnabledKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(EnabledKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public float CurrentDegrees { get; private set; }
        public float PeakDegrees => _peakDegrees;
        public int RenderApplications { get; private set; }

        public void Configure(MeleeSweep sweep) => _sweep = sweep;

        private void Awake() => _camera = GetComponent<Camera>();

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginRendering;
            RenderPipelineManager.endCameraRendering += OnEndRendering;
            Subscribe();
        }

        private void Start() => Subscribe();

        private void Subscribe()
        {
            if (_subscribed || _sweep == null)
            {
                return;
            }
            _sweep.HitResolved += OnHit;
            _subscribed = true;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginRendering;
            RenderPipelineManager.endCameraRendering -= OnEndRendering;
            if (_subscribed)
            {
                _sweep.HitResolved -= OnHit;
                _subscribed = false;
            }
            Restore();
        }

        private void OnHit(Fighter target, DamageInfo hit)
        {
            if (!IsOn)
            {
                return;
            }
            _peakDegrees = hit.Kind == AttackKind.Shove ? 0.4f : Mathf.Lerp(0.6f, 1.5f, Mathf.InverseLerp(10f, 22f, hit.Amount));
            _roll = Random.Range(-0.3f, 0.3f) * _peakDegrees;
            _elapsed = 0f;
        }

        private void Update()
        {
            _elapsed = Mathf.Min(DecaySeconds, _elapsed + Time.deltaTime);
            float remaining = 1f - _elapsed / DecaySeconds;
            CurrentDegrees = IsOn ? _peakDegrees * remaining * remaining : 0f;
        }

        private void OnBeginRendering(ScriptableRenderContext context, Camera rendering)
        {
            if (rendering != _camera || CurrentDegrees <= 0f)
            {
                return;
            }
            _restore = transform.localRotation;
            transform.localRotation = _restore * Quaternion.Euler(-CurrentDegrees, 0f, _roll * (CurrentDegrees / Mathf.Max(0.0001f, _peakDegrees)));
            _applied = true;
            RenderApplications++;
        }

        private void OnEndRendering(ScriptableRenderContext context, Camera rendering)
        {
            if (rendering == _camera)
            {
                Restore();
            }
        }

        private void Restore()
        {
            if (_applied)
            {
                transform.localRotation = _restore;
                _applied = false;
            }
        }

        // Drawn by the prototype's Escape settings panel, like its audio and save sections.
        public static void DrawMenu()
        {
            GUILayout.Space(8);
            bool on = GUILayout.Toggle(IsOn, " Camera shake on hits");
            if (on != IsOn)
            {
                IsOn = on;
            }
        }
    }
}
