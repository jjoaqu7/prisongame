using PrisonGame.Combat;
using UnityEngine;

namespace PrisonGame.PlayerCombat
{
    // Local-player fight readout: the health bar, shown only during a fight and briefly after; a faint
    // red screen-edge flash when hit (weaker through a block); "Timed block" when a timed block lands;
    // and the fade to black when a fight is lost, then back in after the checkpoint restore.
    // Placeholder IMGUI like the rest of the sandbox HUD. Presentation only: it never changes rules state.
    public sealed class FightHud : MonoBehaviour
    {
        public const float FlashSeconds = 0.35f;
        public const float FlashAlpha = 0.45f;
        public const float BlockedFlashAlpha = 0.18f;
        public const float FadeInSeconds = 0.6f;
        public const float TimedBlockTextSeconds = 0.8f;
        [SerializeField] private FightController _fight;
        [SerializeField] private Fighter _fighter;
        private Texture2D _edge;
        private GUIStyle _style;
        private bool _subscribed;
        private float _flashRemaining;
        private float _flashPeak;
        private float _fadeInRemaining;
        private float _timedBlockRemaining;

        public bool HealthBarVisible => _fight != null && _fight.Session != null && _fight.Session.ChallengerHealthVisible;
        public float FlashAlphaNow => _flashPeak * Mathf.Clamp01(_flashRemaining / FlashSeconds);
        public float FadeAlpha { get; private set; }
        public bool ShowsTimedBlock => _timedBlockRemaining > 0f;

        public void Configure(FightController fight, Fighter fighter)
        {
            _fight = fight;
            _fighter = fighter;
        }

        private void OnEnable() => Subscribe();
        private void Start() => Subscribe();

        private void Subscribe()
        {
            if (_subscribed || _fight == null || _fighter == null || _fighter.Defense == null)
            {
                return;
            }
            _fighter.Defense.HitTaken += OnHitTaken;
            _fight.CheckpointRestored += OnCheckpointRestored;
            _subscribed = true;
        }

        private void OnDisable()
        {
            if (_subscribed)
            {
                _fighter.Defense.HitTaken -= OnHitTaken;
                _fight.CheckpointRestored -= OnCheckpointRestored;
                _subscribed = false;
            }
        }

        private void OnDestroy()
        {
            if (_edge != null)
            {
                Destroy(_edge);
            }
        }

        private void OnHitTaken(DamageInfo hit, DamageResult result)
        {
            if (result.Block == BlockOutcome.TimedBlock)
            {
                _timedBlockRemaining = TimedBlockTextSeconds;
                return;
            }
            if (result.Ignored)
            {
                return;
            }
            _flashPeak = result.Block == BlockOutcome.Blocked ? BlockedFlashAlpha : FlashAlpha;
            _flashRemaining = FlashSeconds;
        }

        private void OnCheckpointRestored() => _fadeInRemaining = FadeInSeconds;

        private void Update()
        {
            _flashRemaining = Mathf.Max(0f, _flashRemaining - Time.deltaTime);
            _fadeInRemaining = Mathf.Max(0f, _fadeInRemaining - Time.deltaTime);
            _timedBlockRemaining = Mathf.Max(0f, _timedBlockRemaining - Time.deltaTime);
            FightSession session = _fight == null ? null : _fight.Session;
            if (session != null && session.Phase == FightPhase.Lost)
            {
                FadeAlpha = Mathf.Clamp01(session.TimeInPhase / FightSession.LossFadeSeconds);
            }
            else
            {
                FadeAlpha = _fadeInRemaining / FadeInSeconds;
            }
        }

        private void OnGUI()
        {
            if (_fighter == null || _fighter.Health == null)
            {
                return;
            }
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
            }
            var screen = new Rect(0f, 0f, Screen.width, Screen.height);
            Color previous = GUI.color;
            float flash = FlashAlphaNow;
            if (flash > 0f)
            {
                GUI.color = new Color(0.75f, 0f, 0f, flash);
                GUI.DrawTexture(screen, EdgeTexture());
            }
            if (HealthBarVisible)
            {
                DrawHealthBar();
            }
            if (ShowsTimedBlock)
            {
                GUI.color = Color.white;
                GUI.Label(new Rect(Screen.width * 0.5f - 100f, Screen.height * 0.5f + 40f, 200f, 24f), "Timed block", _style);
            }
            if (FadeAlpha > 0f)
            {
                GUI.color = new Color(0f, 0f, 0f, FadeAlpha);
                GUI.DrawTexture(screen, Texture2D.whiteTexture);
            }
            GUI.color = previous;
        }

        private void DrawHealthBar()
        {
            Health health = _fighter.Health;
            float fraction = Mathf.Clamp01(health.Current / health.Max);
            var frame = new Rect(Screen.width * 0.5f - 150f, 18f, 300f, 20f);
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(frame, Texture2D.whiteTexture);
            GUI.color = Color.Lerp(new Color(0.8f, 0.15f, 0.1f), new Color(0.35f, 0.75f, 0.3f), fraction);
            GUI.DrawTexture(new Rect(frame.x + 2f, frame.y + 2f, (frame.width - 4f) * fraction, frame.height - 4f),
                Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(frame, $"Health {health.Current:0}/{health.Max:0}", _style);
        }

        // Transparent in the middle, opaque toward the corners; built once in code, not an imported asset.
        private Texture2D EdgeTexture()
        {
            if (_edge != null)
            {
                return _edge;
            }
            const int Size = 64;
            _edge = new Texture2D(Size, Size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[Size * Size];
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float u = (x + 0.5f) / Size * 2f - 1f;
                    float v = (y + 0.5f) / Size * 2f - 1f;
                    float alpha = Mathf.Clamp01((Mathf.Sqrt(u * u + v * v) - 0.6f) / 0.6f);
                    pixels[y * Size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }
            _edge.SetPixels32(pixels);
            _edge.Apply();
            return _edge;
        }
    }
}
