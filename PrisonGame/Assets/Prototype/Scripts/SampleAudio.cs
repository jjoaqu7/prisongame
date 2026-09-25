using UnityEngine;

namespace PrisonGame.Prototype
{
    [RequireComponent(typeof(PlayerInteraction), typeof(FirstPersonController))]
    public sealed class SampleAudio : MonoBehaviour
    {
        [SerializeField] private AudioClip[] footsteps;
        [SerializeField] private AudioClip[] cues;
        [SerializeField] private AudioClip roomTone;
        private AudioSource[] voices;
        private AudioSource ambience;
        private GameObject voiceRoot;
        private PlayerInteraction actor;
        private FirstPersonController movement;
        private CharacterController controller;
        private PrisonClock clock;
        private Vector3 lastPosition;
        private float distance;
        private int nextVoice, nextStep;
        private bool paused, ambienceStarted;
        private float effectsVolume, ambienceVolume;
        public int PlayedSounds { get; private set; }
        public int FootstepsPlayed { get; private set; }
        public bool Paused => paused;
        public float EffectsVolume => effectsVolume;
        public float AmbienceVolume => ambienceVolume;

        private void Awake()
        {
            actor=GetComponent<PlayerInteraction>();movement=GetComponent<FirstPersonController>();
            controller=GetComponent<CharacterController>();clock=GetComponent<SnackRequest>().Clock;
            effectsVolume=Mathf.Clamp01(PlayerPrefs.GetFloat("PrisonGame.EffectsVolume",.65f));
            ambienceVolume=Mathf.Clamp01(PlayerPrefs.GetFloat("PrisonGame.AmbienceVolume",.35f));
            voiceRoot=new GameObject("Sample audio voices");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(voiceRoot,gameObject.scene);
            voices=new AudioSource[8];
            for(int i=0;i<voices.Length;i++)
            {
                var obj=new GameObject("Sample sound voice "+i);obj.transform.SetParent(voiceRoot.transform,false);
                var source=obj.AddComponent<AudioSource>();source.playOnAwake=false;source.dopplerLevel=0;
                source.rolloffMode=AudioRolloffMode.Linear;source.minDistance=1.5f;source.maxDistance=10;
                voices[i]=source;
            }
            ambience=gameObject.AddComponent<AudioSource>();ambience.playOnAwake=false;ambience.loop=true;
            ambience.spatialBlend=0;ambience.clip=roomTone;ambience.volume=ambienceVolume*.4f;
            paused=true;lastPosition=transform.position;
        }
        private void OnEnable()
        {
            SampleSoundEvents.Played+=OnCue;
            paused=true;ambienceStarted=false;
        }
        private void OnDestroy() { if(voiceRoot!=null)Destroy(voiceRoot); }
        internal void ResetAfterLoad()
        {
            foreach(var voice in voices)voice.Stop();
            distance=0;lastPosition=transform.position;SetPaused(true);
        }
        private void OnDisable()
        {
            SampleSoundEvents.Played-=OnCue;
            if(voices!=null)foreach(var v in voices)if(v!=null)v.Stop();
            if(ambience!=null)ambience.Stop();
            ambienceStarted=false;
        }
        private void LateUpdate()
        {
            bool shouldPause=clock.Paused || !movement.ControlsActive;
            SetPaused(shouldPause);
            MeasureSteps(transform.position,controller.isGrounded,shouldPause);
        }
        private void SetPaused(bool value)
        {
            if(value==paused)return;
            paused=value;
            foreach(var v in voices) { if(value)v.Pause();else v.UnPause(); }
            if(value)ambience.Pause();
            else if(!ambienceStarted) { ambience.Play();ambienceStarted=true; }
            else ambience.UnPause();
        }
        private void MeasureSteps(Vector3 position,bool grounded,bool stopped)
        {
            Vector3 travel=position-lastPosition;lastPosition=position;travel.y=0;
            float metres=travel.magnitude;
            if(stopped || !grounded || metres>1) {distance=0;return;}
            if(metres<.0001f)return;
            distance+=metres;
            if(distance<1.35f)return;
            distance-=1.35f;
            if(footsteps.Length==0)return;
            Play(footsteps[nextStep++%footsteps.Length],position,.42f,false,1);
            FootstepsPlayed++;
        }
        private void OnCue(PlayerInteraction actingPlayer,SampleSound cue,Vector3 position)
        {
            if(clock.Paused || (actingPlayer!=null && actingPlayer.gameObject.scene!=gameObject.scene))return;
            int index=(int)cue;
            if(index<0 || index>=cues.Length)return;
            // Small stable pitch variation; gameplay randomness is not consumed by presentation.
            float pitch=1f+((PlayedSounds%3)-1)*.025f;
            Play(cues[index],position,cue==SampleSound.Door?.75f:.65f,true,pitch);
        }
        private void Play(AudioClip clip,Vector3 position,float gain,bool spatial,float pitch)
        {
            if(clip==null || effectsVolume<=0)return;
            var voice=voices[nextVoice++%voices.Length];voice.Stop();
            voice.transform.position=position;voice.clip=clip;voice.spatialBlend=spatial?1:0;
            voice.pitch=pitch;voice.volume=effectsVolume*gain;voice.Play();PlayedSounds++;
        }
        public void SetVolumes(float effects,float ambient,bool persist=true)
        {
            effectsVolume=Mathf.Clamp01(effects);ambienceVolume=Mathf.Clamp01(ambient);
            foreach(var voice in voices)voice.Stop();
            ambience.volume=ambienceVolume*.4f;
            if(persist)
            {
                PlayerPrefs.SetFloat("PrisonGame.EffectsVolume",effectsVolume);
                PlayerPrefs.SetFloat("PrisonGame.AmbienceVolume",ambienceVolume);PlayerPrefs.Save();
            }
        }
        public void DrawMenu(GUIStyle style)
        {
            GUILayout.Space(6);
            GUILayout.Label("Sound effects: "+Mathf.RoundToInt(effectsVolume*100)+"%",style);
            float effects=GUILayout.HorizontalSlider(effectsVolume,0,1);
            GUILayout.Label("Room ambience: "+Mathf.RoundToInt(ambienceVolume*100)+"%",style);
            float ambient=GUILayout.HorizontalSlider(ambienceVolume,0,1);
            if(effects!=effectsVolume || ambient!=ambienceVolume)SetVolumes(effects,ambient);
        }
    }
}
