using UnityEngine;

namespace PrisonGame.Combat
{
    // Placeholder hit sounds for the hits this fighter lands, played at the contact point.
    // A charged punch layers a body thud under a heavier impact. Presentation only.
    [RequireComponent(typeof(MeleeSweep))]
    public sealed class FighterHitSounds : MonoBehaviour
    {
        // Shared with the prototype's settings panel, so its effects slider applies here too.
        public const string EffectsVolumeKey = "PrisonGame.EffectsVolume";
        public const float DefaultEffectsVolume = 0.65f;

        [SerializeField] private AudioClip[] _quick = new AudioClip[0];
        [SerializeField] private AudioClip[] _charged = new AudioClip[0];
        [SerializeField] private AudioClip[] _chargedLayer = new AudioClip[0];
        [SerializeField] private AudioClip[] _shove = new AudioClip[0];
        private MeleeSweep _sweep;
        private AudioSource _impact;
        private AudioSource _layer;
        private bool _subscribed;

        public AudioClip LastImpact { get; private set; }
        public AudioClip LastLayer { get; private set; }

        public void Configure(AudioClip[] quick, AudioClip[] charged, AudioClip[] chargedLayer, AudioClip[] shove)
        {
            _quick = quick;
            _charged = charged;
            _chargedLayer = chargedLayer;
            _shove = shove;
        }

        private void Awake()
        {
            _sweep = GetComponent<MeleeSweep>();
            _impact = MakeSource("HitImpactAudio");
            _layer = MakeSource("HitLayerAudio");
        }

        private void OnEnable() => Subscribe();
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
            if (_subscribed)
            {
                _sweep.HitResolved -= OnHit;
                _subscribed = false;
            }
        }

        private void OnHit(Fighter target, DamageInfo hit)
        {
            float volume = Mathf.Clamp01(PlayerPrefs.GetFloat(EffectsVolumeKey, DefaultEffectsVolume));
            LastLayer = null;
            switch (hit.Kind)
            {
                case AttackKind.Shove:
                    LastImpact = Play(_impact, _shove, hit.Point, volume * 0.8f);
                    break;
                case AttackKind.Charged:
                    LastImpact = Play(_impact, _charged, hit.Point, volume);
                    LastLayer = Play(_layer, _chargedLayer, hit.Point, volume * 0.6f);
                    break;
                default:
                    LastImpact = Play(_impact, _quick, hit.Point, volume * 0.85f);
                    break;
            }
        }

        private static AudioClip Play(AudioSource source, AudioClip[] clips, Vector3 point, float volume)
        {
            if (clips == null || clips.Length == 0)
            {
                return null;
            }
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            if (clip == null)
            {
                return null;
            }
            source.transform.position = point;
            source.pitch = Random.Range(0.94f, 1.06f);
            source.PlayOneShot(clip, volume);
            return clip;
        }

        private AudioSource MakeSource(string name)
        {
            var holder = new GameObject(name);
            holder.transform.SetParent(transform, false);
            var source = holder.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.minDistance = 1f;
            source.maxDistance = 20f;
            return source;
        }
    }
}
