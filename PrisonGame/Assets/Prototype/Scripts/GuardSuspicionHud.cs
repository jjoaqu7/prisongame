using UnityEngine;

namespace PrisonGame.Prototype
{
    [RequireComponent(typeof(GuardSuspicion), typeof(PlayerInteraction))]
    public sealed class GuardSuspicionHud : MonoBehaviour
    {
        private GuardSuspicion suspicion;
        private PlayerInteraction actor;
        private FirstPersonController movement;
        private bool hadSuspicion, warned, ordered;
        private GUIStyle title, body;

        private void Awake()
        {
            suspicion = GetComponent<GuardSuspicion>();
            actor = GetComponent<PlayerInteraction>();
            movement = GetComponent<FirstPersonController>();
        }

        public string Heading => suspicion.OrderedOut ? "HARRIS: LEAVE NOW" : suspicion.Rising ? "HARRIS IS WATCHING" : suspicion.Level > 0 ? "SUSPICION FALLING" : "STAFF-ONLY CORNER";
        public string Status
        {
            get
            {
                if (suspicion.Rising) return "Cause: Harris sees you in the staff-only area.\n" +
                    (suspicion.OrderedOut ? "He has ordered you to leave.\n" : "Suspicion rising; at 100 he orders you out.\n") + "Step outside the marked line to lower it.";
                if (suspicion.Level > 0) return (suspicion.Inside ? "Harris lost sight of you." : "You left the staff-only area.") + "\nSuspicion falling. Stay outside the marked line until it clears.";
                if (suspicion.Inside) return "You are in the staff-only area, outside Harris's view. Leave before he sees you.";
                return "Harris watches the marked corner at the far right of the common room. Being seen inside raises suspicion.";
            }
        }

        internal void ResetAfterLoad()
        { hadSuspicion = suspicion.Level > 0; warned = suspicion.Level >= 40; ordered = suspicion.OrderedOut; }

        private void Update()
        {
            if (suspicion.Level <= 0)
            {
                if (hadSuspicion) actor.ShowMessage("Harris's suspicion has cleared.");
                hadSuspicion = warned = ordered = false;
                return;
            }
            if (suspicion.OrderedOut && !ordered) { ordered = warned = true; actor.ShowMessage("Harris: Leave the staff-only area. Now! Step outside the marked line.", 6f); }
            else if (suspicion.Level >= 40 && !warned) { warned = true; actor.ShowMessage("Harris: You're in a staff-only area. Leave before this goes further.", 6f); }
            else if (!hadSuspicion) actor.ShowMessage("Harris sees you in the staff-only area. Suspicion is rising.");
            hadSuspicion = true;
        }

        private void OnGUI()
        {
            if (movement == null || !movement.ControlsActive) return;
            if (title == null)
            {
                title = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, wordWrap = true };
                body = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };
            }
            var old = GUI.matrix;
            float scale = Mathf.Clamp(Screen.height / 900f, .75f, 2.5f);
            GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
            float width = 290, x = Screen.width / scale - width - 18;
            string status = Status;
            float height = body.CalcHeight(new GUIContent(status), width - 24);
            var panel = new Rect(x, 220, width, height + 94);
            GUI.Box(panel, GUIContent.none);
            GUI.Label(new Rect(x + 12, 228, width - 24, 28), Heading, title);
            GUI.Label(new Rect(x + 12, 260, width - 24, 24), "Officer Harris: " + Mathf.CeilToInt(suspicion.Level) + " / 100", body);
            GUI.Box(new Rect(x + 12, 288, width - 24, 6), GUIContent.none);
            var color = GUI.color; GUI.color = new Color(1f, .72f, .28f);
            GUI.DrawTexture(new Rect(x + 12, 288, (width - 24) * suspicion.Level / 100, 6), Texture2D.whiteTexture);
            GUI.color = color;
            GUI.Label(new Rect(x + 12, 304, width - 24, height), status, body);
            GUI.matrix = old;
        }
    }
}
