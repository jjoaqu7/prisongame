using UnityEngine;

namespace PrisonGame.Prototype
{
    // Presentation reads character state; it owns no input, dialogue or movement rules.
    [DefaultExecutionOrder(20)]
    [RequireComponent(typeof(Animator))]
    public sealed class InmateAnimation : MonoBehaviour
    {
        // Distance per second represented by the authored walk at playback rate 1.
        private const float AuthoredWalkSpeed = .26f / .6f;
        private static readonly int Preview = Animator.StringToHash("Preview");
        private static readonly int MoveRate = Animator.StringToHash("MoveRate");
        private InmateMotion motion;
        private Animator animator;
        private int lastTalkSequence;

        private void Awake()
        {
            motion = GetComponentInParent<InmateMotion>();
            animator = GetComponent<Animator>();
        }

        private void Update() => RefreshAnimation();

        public void RefreshAnimation()
        {
            if (motion == null) return;
            float rate = motion.Velocity.magnitude / AuthoredWalkSpeed;
            int selection = motion.IsGesturing ? 2 : rate > .001f ? 1 : 0;
            animator.SetFloat(MoveRate, Mathf.Max(0f, rate));
            animator.SetInteger(Preview, selection);
            if (lastTalkSequence != motion.TalkSequence)
            {
                lastTalkSequence = motion.TalkSequence;
                // Retriggering talk restarts the gesture, including during its existing state.
                if (selection == 2) animator.CrossFadeInFixedTime("GesturePreview", .15f, 0, 0f);
            }
        }
    }
}
